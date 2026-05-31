using UnityEngine;

public class MainCharacter : MonoBehaviour
{
    CharacterController controller;

    [Header("Movement - Normal (장비 미장착)")]
    public float normalWalkSpeed = 5f;
    public float normalRunSpeed = 9f;

    [Header("Movement - Heavy (25kg 장비 장착)")]
    public float heavyWalkSpeed = 2.5f;
    public float heavyRunSpeed = 4.5f;

    [HideInInspector] public float gravity = -9.8f; // 기존 값 유지 (-9.8f)

    [Header("Camera")]
    public Transform playerTransform;
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;

    [Header("Interaction")]
    public float interactDistance = 5f;

    [Header("Scene 2 Conditions")]
    // 이 체크박스가 켜져 있으면 무거운 속도가 적용됩니다. (씬2 시작 시 체크 켜두기)
    public bool hasHeavyEquipment = true; 

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
<<<<<<< Updated upstream
=======
        // 1. 대화창이나 인벤토리가 열려 있을 때 멈춤
        if (InventoryUI.Instance != null && InventoryUI.Instance.inventoryBaseUI.activeSelf)
        {
            animator.SetFloat("Speed", 0f);
            return; 
        }

        if (TalkManager.Instance != null && TalkManager.Instance.talkUI.activeSelf)
        {
            animator.SetFloat("Speed", 0f);
            return;
        }

        // 2. [추가 조건] 체력이 다해 탈진(Exhausted) 상태일 때 강제로 멈춤
        if (PlayerHealth.Instance != null && PlayerHealth.Instance.IsExhausted)
        {
            animator.SetFloat("Speed", 0f);
            return;
        }

        // 인벤토리도 안 켜져 있고 탈진도 아닐 때만 정상 조작 가능
>>>>>>> Stashed changes
        PlayerMovement();
        CameraLook();
        DetectObject();
    }

    void PlayerMovement()
    {
        float keyX = Input.GetAxis("Horizontal");
        float keyZ = Input.GetAxis("Vertical");

        // [추가 조건 1] 장비 착용 여부에 따라 베이스 속도를 삼항 연산자로 결정합니다.
        float walkSpeed = hasHeavyEquipment ? heavyWalkSpeed : normalWalkSpeed;
        float runSpeed = hasHeavyEquipment ? heavyRunSpeed : normalRunSpeed;

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