
using UnityEngine;

public class MainCharacter : MonoBehaviour
{
    CharacterController controller;

    [Header("Movement")]
    public float walkSpeed = 1.0f;
    public float runSpeed = 3.0f;
    public float gravity = -9.8f;

    [Header("Camera")]
    public Transform playerTransform;
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;

    [Header("상호작용 설정")]
    [Tooltip("레이캐스트가 뻗어나가는 총 거리입니다.")]
    public float interactDistance = 3.0f; 

    [Tooltip("이 거리보다 가까이 가야만 [E] 안내 문구가 화면에 나타납니다.")]
    public float maxPromptDistance = 1.3f; 

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
        if ((InventoryUI.Instance != null && InventoryUI.Instance.inventoryBaseUI.activeSelf) ||
            (TalkManager.Instance != null && TalkManager.Instance.ShouldFreezePlayer) ||
            (QuestManager.Instance != null && QuestManager.Instance.IsPopUpActive))
        {
            animator.SetFloat("Speed", 0f);
            return; 
        }

        if (TalkManager.Instance != null && TalkManager.Instance.talkUI.activeSelf)
        {
            animator.SetFloat("Speed", 0f);
            return;
        }

        if (PlayerHealth.Instance != null && PlayerHealth.Instance.IsExhausted)
        {
            // 애니메이션 속도를 0으로 만들어 제자리에 숨 고르는 자세로 만듭니다.
            animator.SetFloat("Speed", 0f); 
            return; // ◀ 중요! 여기서 꺾어서 아래 이동 함수들로 못 내려가게 차단합니다.
        }

        // 탈진이 아닐 때만 정상 조작 가능
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
        if (Camera.main == null)
            return;

        // 화면 정중앙(크로스헤어)에서 레이저를 쏩니다.
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;

        // 1차 검사: 일단 앞에 상호작용 가능한 물체가 있는지 레이저로 조준했는가?
        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            
            // 2차 검사: 조준한 물체가 상호작용 대상이 맞고, '실제 거리(hit.distance)'가 설정한 최소 거리 이내인가?
            if (interactable != null && hit.distance <= maxPromptDistance)
            {
                // 모든 조건 만족 시 오브젝트 위에 UI 표시
                if (InteractionUI.Instance != null)
                {
                    InteractionUI.Instance.Show(interactable.GetPromptText(), hit.collider.transform);
                }
                
                // E키 입력 처리
                if (Input.GetKeyDown(KeyCode.E))
                {
                    interactable.Interact();
                }
                
                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.green);
                return; // UI를 계속 켜두기 위해 함수를 여기서 종료(리턴)합니다.
            }
        }
        
        // 조건이 하나라도 안 맞으면 (고개를 돌렸거나, 거리가 멀어졌거나) 즉시 UI를 숨깁니다.
        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.Hide();
        }
        
        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.red);
    }
}
