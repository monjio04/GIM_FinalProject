using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(CharacterController))]
public sealed class LookDevCameraController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float baseMoveSpeed = 8f;
    [SerializeField] private float sprintMultiplier = 4f;
    [SerializeField] private float slowMultiplier = 0.25f;
    [SerializeField] private float speedStep = 1.25f;
    [SerializeField] private float minMoveSpeed = 0.05f;
    [SerializeField] private float maxMoveSpeed = 200f;

    [Header("Character Collision")]
    [SerializeField] private float controllerHeight = 1.8f;
    [SerializeField] private float controllerRadius = 0.3f;
    [SerializeField] private float gravity = -24f;
    [SerializeField] private float groundedStickForce = -2f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private bool allowJump = true;
    [SerializeField] private float maxFallSpeed = 80f;

    [Header("Grounding")]
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private bool snapToGroundOnStart = true;
    [SerializeField] private float initialGroundSnapDistance = 100f;
    [SerializeField] private float movingGroundSnapDistance = 0.35f;

    [Header("Look")]
    [SerializeField] private float lookSensitivity = 0.12f;
    [SerializeField] private bool invertY;
    [SerializeField] private bool lockCursorWhileLooking = true;

    [Header("Lookdev Helpers")]
    [SerializeField] private LayerMask focusMask = ~0;
    [SerializeField] private float focusPadding = 2.25f;
    [SerializeField] private float defaultFocusDistance = 8f;
    [SerializeField] private bool ignoreTriggerColliders = true;

    private Vector3 startPosition;
    private Quaternion startRotation;
    private CharacterController characterController;
    private float verticalVelocity;
    private float pitch;
    private float yaw;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        ConfigureCharacterController();
        startPosition = transform.position;
        startRotation = transform.rotation;
        ReadRotation();
    }

    private void Start()
    {
        if (snapToGroundOnStart)
        {
            SnapToGround(initialGroundSnapDistance);
        }
    }

    private void OnValidate()
    {
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null)
        {
            characterController = controller;
            ConfigureCharacterController();
        }
    }

    private void OnDisable()
    {
        if (lockCursorWhileLooking)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void Update()
    {
        HandleCursor();
        HandleLook();
        HandleSpeedControl();
        HandleMove();
        HandleShortcuts();
    }

    private void HandleCursor()
    {
        if (!lockCursorWhileLooking)
        {
            return;
        }

        bool looking = IsRightMouseHeld();
        Cursor.lockState = looking ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !looking;
    }

    private void HandleLook()
    {
        if (!IsRightMouseHeld())
        {
            return;
        }

        Vector2 delta = GetMouseDelta();
        yaw += delta.x * lookSensitivity;
        pitch += delta.y * lookSensitivity * (invertY ? 1f : -1f);
        pitch = Mathf.Clamp(pitch, -89f, 89f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void HandleSpeedControl()
    {
        float wheel = GetMouseWheel();
        if (Mathf.Approximately(wheel, 0f))
        {
            return;
        }

        float multiplier = wheel > 0f ? speedStep : 1f / speedStep;
        baseMoveSpeed = Mathf.Clamp(baseMoveSpeed * multiplier, minMoveSpeed, maxMoveSpeed);
    }

    private void HandleMove()
    {
        Vector3 input = Vector3.zero;

        if (IsKeyHeld(KeyCode.W)) input += Vector3.forward;
        if (IsKeyHeld(KeyCode.S)) input += Vector3.back;
        if (IsKeyHeld(KeyCode.D)) input += Vector3.right;
        if (IsKeyHeld(KeyCode.A)) input += Vector3.left;
        if (input.sqrMagnitude <= 0f)
        {
            ApplyGravityAndMove(Vector3.zero);
            return;
        }

        float speed = baseMoveSpeed;
        if (IsKeyHeld(KeyCode.LeftShift) || IsKeyHeld(KeyCode.RightShift)) speed *= sprintMultiplier;
        if (IsKeyHeld(KeyCode.LeftControl) || IsKeyHeld(KeyCode.RightControl)) speed *= slowMultiplier;

        Vector3 move = GetPlanarMoveDirection(input.normalized);
        ApplyGravityAndMove(move * speed);
    }

    private void ApplyGravityAndMove(Vector3 horizontalVelocity)
    {
        if (characterController.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedStickForce;
        }

        if (allowJump && characterController.isGrounded && WasKeyPressed(KeyCode.Space))
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.unscaledDeltaTime;
        verticalVelocity = Mathf.Max(verticalVelocity, -maxFallSpeed);

        Vector3 velocity = horizontalVelocity + Vector3.up * verticalVelocity;
        CollisionFlags flags = characterController.Move(velocity * Time.unscaledDeltaTime);

        if ((flags & CollisionFlags.Below) != 0 && verticalVelocity < 0f)
        {
            verticalVelocity = groundedStickForce;
        }
        else if (verticalVelocity <= groundedStickForce && horizontalVelocity.sqrMagnitude > 0f)
        {
            SnapToGround(movingGroundSnapDistance);
        }
    }

    private Vector3 GetPlanarMoveDirection(Vector3 input)
    {
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;
        return (forward * input.z + right * input.x).normalized;
    }

    private void HandleShortcuts()
    {
        if (WasKeyPressed(KeyCode.F))
        {
            FocusForward();
        }

        if (WasKeyPressed(KeyCode.R))
        {
            ResetToStart();
        }
    }

    private void FocusForward()
    {
        Camera cam = GetComponent<Camera>();
        Ray ray = new Ray(transform.position, transform.forward);
        QueryTriggerInteraction triggerInteraction = ignoreTriggerColliders
            ? QueryTriggerInteraction.Ignore
            : QueryTriggerInteraction.Collide;

        if (Physics.Raycast(ray, out RaycastHit hit, cam.farClipPlane, focusMask, triggerInteraction))
        {
            float distance = Mathf.Max(hit.distance / focusPadding, cam.nearClipPlane + 0.01f);
            SetPosition(hit.point - transform.forward * distance);
            return;
        }

        SetPosition(transform.position + transform.forward * defaultFocusDistance);
    }

    private void ResetToStart()
    {
        SetPositionAndRotation(startPosition, startRotation);
        verticalVelocity = 0f;
        ReadRotation();
    }

    private void ConfigureCharacterController()
    {
        controllerHeight = Mathf.Max(controllerHeight, 0.1f);
        controllerRadius = Mathf.Clamp(controllerRadius, 0.01f, controllerHeight * 0.5f);
        maxFallSpeed = Mathf.Max(maxFallSpeed, 1f);
        initialGroundSnapDistance = Mathf.Max(initialGroundSnapDistance, 0f);
        movingGroundSnapDistance = Mathf.Max(movingGroundSnapDistance, 0f);

        characterController.height = controllerHeight;
        characterController.radius = controllerRadius;
        characterController.center = Vector3.down * (controllerHeight * 0.5f);
    }

    private bool SnapToGround(float maxDistance)
    {
        if (maxDistance <= 0f)
        {
            return false;
        }

        Vector3 origin = transform.position + Vector3.up * 0.05f;
        QueryTriggerInteraction triggerInteraction = ignoreTriggerColliders
            ? QueryTriggerInteraction.Ignore
            : QueryTriggerInteraction.Collide;

        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, maxDistance + controllerHeight, groundMask, triggerInteraction))
        {
            return false;
        }

        float desiredHeight = controllerHeight + characterController.skinWidth + 0.01f;
        SetPosition(hit.point + Vector3.up * desiredHeight);
        verticalVelocity = groundedStickForce;
        return true;
    }

    private void SetPosition(Vector3 position)
    {
        SetPositionAndRotation(position, transform.rotation);
    }

    private void SetPositionAndRotation(Vector3 position, Quaternion rotation)
    {
        characterController.enabled = false;
        transform.SetPositionAndRotation(position, rotation);
        characterController.enabled = true;
    }

    private void ReadRotation()
    {
        Vector3 euler = transform.rotation.eulerAngles;
        pitch = NormalizeAngle(euler.x);
        yaw = NormalizeAngle(euler.y);
    }

    private static float NormalizeAngle(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }

    private static bool IsRightMouseHeld()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null && Mouse.current.rightButton.isPressed;
#else
        return Input.GetMouseButton(1);
#endif
    }

    private static Vector2 GetMouseDelta()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
#else
        return new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * 10f;
#endif
    }

    private static float GetMouseWheel()
    {
#if ENABLE_INPUT_SYSTEM
        return Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;
#else
        return Input.mouseScrollDelta.y;
#endif
    }

    private static bool IsKeyHeld(KeyCode key)
    {
#if ENABLE_INPUT_SYSTEM
        Key control = ToInputSystemKey(key);
        return control != Key.None && Keyboard.current != null && Keyboard.current[control].isPressed;
#else
        return Input.GetKey(key);
#endif
    }

    private static bool WasKeyPressed(KeyCode key)
    {
#if ENABLE_INPUT_SYSTEM
        Key control = ToInputSystemKey(key);
        return control != Key.None && Keyboard.current != null && Keyboard.current[control].wasPressedThisFrame;
#else
        return Input.GetKeyDown(key);
#endif
    }

#if ENABLE_INPUT_SYSTEM
    private static Key ToInputSystemKey(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.W: return Key.W;
            case KeyCode.A: return Key.A;
            case KeyCode.S: return Key.S;
            case KeyCode.D: return Key.D;
            case KeyCode.F: return Key.F;
            case KeyCode.R: return Key.R;
            case KeyCode.Space: return Key.Space;
            case KeyCode.LeftShift: return Key.LeftShift;
            case KeyCode.RightShift: return Key.RightShift;
            case KeyCode.LeftControl: return Key.LeftCtrl;
            case KeyCode.RightControl: return Key.RightCtrl;
            default: return Key.None;
        }
    }
#endif
}
