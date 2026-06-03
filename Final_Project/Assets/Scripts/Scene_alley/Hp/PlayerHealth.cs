using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("HP Drain & Heal (장비 장착 상태)")]
    public float walkHpDrain = 3f;    
    public float runHpDrain = 10f;    
    public float idleHpHeal = 5f;     

    [Header("Exhausted Settings (탈진)")]
    public float restDuration = 3f;   
    private float restTimer = 0f;
    private bool isExhausted = false;
    
    // 대사가 완벽하게 끝났는지 체크하는 플래그
    private bool isWaitingForDialogue = false; 

    public bool IsExhausted => isExhausted;

    [Header("Exhausted Dialogue")]
    public DialogueData exhaustedDialogue; 

    private CharacterController controller;
    private bool isUIInitialized = false;

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
        // 1. UI 첫 선언 동기화
        if (!isUIInitialized && HealthUI.Instance != null)
        {
            UpdateHealthUI();
            isUIInitialized = true;
        }

        // 2. 탈진 상태 처리
        if (isExhausted)
        {
            // 탈진 대사가 완전히 끝날 때까지는 3초 타이머를 굴리지 않고 대기합니다.
            if (isWaitingForDialogue) return; 

            restTimer -= Time.deltaTime;
            if (restTimer <= 0f)
            {
                EndRest();
            }
            return; 
        }

        // 3. 일반적인 시스템 대화창이 켜져 있을 때는 체력 소모/회복 정지
        if (TalkManager.Instance != null && TalkManager.Instance.talkUI.activeSelf) return;

        // 4. 이동 및 회복 루틴
        bool isMoving = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;

        if (controller != null && controller.isGrounded)
        {
            if (isMoving)
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
            else
            {
                Heal(idleHpHeal * Time.deltaTime);
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

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        UpdateHealthUI();
    }

    void StartRest()
    {
        isExhausted = true;
        currentHealth = 0f; 
        UpdateHealthUI();
        
        Debug.Log("탈진! 제자리에 멈춰 숨을 고릅니다...");

        if (TalkManager.Instance != null && exhaustedDialogue != null)
        {
            isWaitingForDialogue = true; // 타이머 일시정지 마크

            // ★ TalkManager의 기능을 100% 활용하여 대사가 완전히 끝났을 때 실행할 행동(Action)을 넘겨줍니다.
            TalkManager.Instance.StartDialogue(exhaustedDialogue, () => 
            {
                // 대사가 끝나면 실행되는 구간
                isWaitingForDialogue = false; 
                restTimer = restDuration; // 대사가 끝난 시점부터 강제 휴식 3초 카운트다운 시작!
                Debug.Log("탈진 대사 종료. 3초간 강제 휴식을 시작합니다.");
            });
        }
        else
        {
            // 만약 대사 데이터가 없다면 예외 처리용으로 바로 3초 휴식 시작
            restTimer = restDuration;
        }
    }

    void EndRest()
    {
        isExhausted = false;
        isWaitingForDialogue = false;
        currentHealth = 20f; 
        UpdateHealthUI();
        Debug.Log("다시 출발 가능!");
    }

    private void UpdateHealthUI()
    {
        if (HealthUI.Instance != null)
        {
            HealthUI.Instance.SetHealth(currentHealth, maxHealth);
        }
    }
}