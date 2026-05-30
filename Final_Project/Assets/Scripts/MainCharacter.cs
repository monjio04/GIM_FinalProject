using UnityEngine;

using UnityEngine;

public class MainCharacter : MonoBehaviour
{
    CharacterController controller;

    [Header("Movement")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float gravity = -9.8f;

    [Header("Camera")]
    public Transform playerTransform;
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;

    [Header("Interaction")]
    public float interactDistance = 5f;

    float cameraXRotation = 0f;
    float yVelocity = 0f;
	
	Animator animator;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        animator = GetComponent<Animator>();
    }

    void Update()
    {
        PlayerMovement();
        CameraLook();
        DetectObject();
    }

    void PlayerMovement()
    {
        float keyX = Input.GetAxis("Horizontal");
        float keyZ = Input.GetAxis("Vertical");

        float currentSpeed = walkSpeed;

        // Shift 누르면 달리기
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed = runSpeed;
        }

        Vector3 move =
            playerTransform.right * keyX +
            playerTransform.forward * keyZ;

        // 바닥 체크
        if (controller.isGrounded && yVelocity < 0)
        {
            yVelocity = -2f;
        }

        // 중력
        yVelocity += gravity * Time.deltaTime;

        Vector3 finalMove =
            move * currentSpeed +
            Vector3.up * yVelocity;

        float animationSpeed = 0f;

        if (move.magnitude > 0.1f){
            animationSpeed = 0.5f;
        }

        if(Input.GetKey(KeyCode.LeftShift) && move.magnitude > 0.1f ){
            animationSpeed = 1f;
        }

        animator.SetFloat("Speed", animationSpeed);

        controller.Move(finalMove * Time.deltaTime);
    }

    void CameraLook()
    {
        float mouseX =
            Input.GetAxis("Mouse X") * mouseSensitivity;

        float mouseY =
            Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 플레이어 좌우 회전
        playerTransform.Rotate(Vector3.up * mouseX);

        // 카메라 상하 회전
        cameraXRotation -= mouseY;
        cameraXRotation = Mathf.Clamp(cameraXRotation, -90f, 90f);

        cameraTransform.localRotation =
            Quaternion.Euler(cameraXRotation, 0f, 0f);
    }

    void DetectObject()
    {
        Ray ray = Camera.main.ViewportPointToRay(
            new Vector3(0.5f, 0.5f, 0f)
        );

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            Debug.DrawRay(
                ray.origin,
                ray.direction * hit.distance,
                Color.green
            );

            // 예시
            // Debug.Log(hit.collider.name);
        }
        else
        {
            Debug.DrawRay(
                ray.origin,
                ray.direction * interactDistance,
                Color.red
            );
        }
    }
}
