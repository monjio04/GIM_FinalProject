using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("HP Drain (장비 장착 상태)")]
    public float walkHpDrain = 3f;    // 걸을 때 초당 감소량
    public float runHpDrain = 10f;    // 뛸 때 초당 감소량 (빠름)

    [Header("Exhausted Settings (탈진)")]
    public float restDuration = 3f;   // 체력 0일 때 강제로 쉬어야 하는 시간 (3초)
    private float restTimer = 0f;
    private bool isExhausted = false;

    private bool isUIInitialized = false;

    // 다른 스크립트에서 플레이어가 지쳤는지 확인할 수 있는 프로퍼티
    public bool IsExhausted => isExhausted;

    private CharacterController controller;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        currentHealth = maxHealth;
        controller = GetComponent<CharacterController>();

    }

    void Update()
    {
        if (!isUIInitialized && HealthUI.Instance != null)
        {
            UpdateHealthUI();
            isUIInitialized = true;
        }

        // 1. 탈진 상태라면 타이머 진행 후 리턴 (이동 및 추가 체력 소모 차단)
        if (isExhausted)
        {
            restTimer -= Time.deltaTime;
            if (restTimer <= 0f)
            {
                EndRest();
            }
            return; 
        }

        // 2. 대화창이 켜져 있을 때는 체력 소모를 일시 정지
        if (TalkManager.Instance != null && TalkManager.Instance.talkUI.activeSelf) return;

        // 3. 실제 이동 중일 때만 체력 감소 계산
        bool isMoving = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;

        if (isMoving && controller != null && controller.isGrounded)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                TakeDamage(runHpDrain * Time.deltaTime);
            }
            else
            {
                TakeDamage(walkHpDrain * Time.deltaTime);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateHealthUI();

        if (currentHealth <= 0f && !isExhausted)
        {
            StartRest();
        }
    }

    // 강제 휴식 시작
    void StartRest()
    {
        isExhausted = true;
        restTimer = restDuration;
        Debug.Log("다리에 힘이 풀렸습니다! 제자리에 멈춰 숨을 고릅니다...");
        
        // 여기에 헐떡이는 거친 숨소리 오디오 재생 코드를 넣으면 좋습니다.
    }

    // 휴식 끝, 체력 소량 회복 후 재기출발
    void EndRest()
    {
        isExhausted = false;
        currentHealth = 20f; // 쉴 만큼 쉬었으니 체력 20 보너스 제공
        UpdateHealthUI();
        Debug.Log("다시 달릴 준비가 되었습니다!");
    }

    private void UpdateHealthUI()
    {
        if (HealthUI.Instance != null)
        {
            HealthUI.Instance.SetHealth(currentHealth, maxHealth);
        }
    }
}