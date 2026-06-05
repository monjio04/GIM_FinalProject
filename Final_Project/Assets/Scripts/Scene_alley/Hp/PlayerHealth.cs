using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance;

    [Header("HP 설정")]
    public float maxHp = 100f;
    public float currentHp;

    [Header("초당 HP 소모량 / 회복량")]
    public float walkHpDrain = 3f;
    public float runHpDrain = 8f;
    public float idleHpHeal = 5f;

    [Header("탈진(Exhausted) 설정")]
    public float restDuration = 3f;
    private float restTimer = 0f;
    private bool isExhausted = false;
    private bool isWaitingForDialogue = false;

    private CharacterController controller;
    private bool isUIInitialized = false;

    public bool IsExhausted => isExhausted;

    private void Awake()
    {
        Instance = this;
        currentHp = maxHp;
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!isUIInitialized && HealthUI.Instance != null)
        {
            UpdateHealthUI();
            isUIInitialized = true;
        }

        if (isExhausted)
        {
            if (isWaitingForDialogue) return;

            restTimer -= Time.deltaTime;
            if (restTimer <= 0f) EndRest();
            return;
        }

        // 질주 퀘스트 도중일 때는 대사창 유무와 상관없이 무조건 통과하도록 예외 처리
        if (TalkManager.Instance != null && TalkManager.Instance.talkUI.activeSelf)
        {
            // [★수정]: 에러 방지를 위해 IsIntroActive 프로퍼티 대신 내장 함수(IsIntroActive)를 직접 호출하도록 우회
            if (AlleySceneController.Instance != null && !AlleySceneController.Instance.IsIntroActive())
            {
                // 질주 중이므로 체력 소모 진행
            }
            else return;
        }

        bool isMoving = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;

        if (controller != null && controller.isGrounded)
        {
            if (isMoving)
            {
                float dynamicScale = 1f;
                
                // 골목길 질주 중일 때 체력 소모 속도를 3배로 보정
                if (AlleySceneController.Instance != null && !AlleySceneController.Instance.IsIntroActive())
                {
                    dynamicScale = 3.0f;
                }

                if (Input.GetKey(KeyCode.LeftShift))
                {
                    TakeDamage(runHpDrain * dynamicScale * Time.deltaTime);
                }
                else
                {
                    TakeDamage(walkHpDrain * dynamicScale * Time.deltaTime);
                }
            }
            else
            {
                Heal(idleHpHeal * Time.deltaTime);
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (isExhausted) return;

        currentHp -= amount;
        currentHp = Mathf.Clamp(currentHp, 0f, maxHp);
        UpdateHealthUI();

        if (currentHp <= 0f)
        {
            StartRest();
        }
    }

    public void Heal(float amount)
    {
        if (isExhausted) return;

        currentHp += amount;
        currentHp = Mathf.Clamp(currentHp, 0f, maxHp);
        UpdateHealthUI();
    }

    private void StartRest()
    {
        isExhausted = true;
        restTimer = restDuration;
        isWaitingForDialogue = false;
        
        Debug.Log("플레이어 탈진 상태 진입");
    }

    private void EndRest()
    {
        isExhausted = false;
        currentHp = maxHp * 0.3f; 
        UpdateHealthUI();
        
        Debug.Log("플레이어 탈진 회복");
    }

    private void UpdateHealthUI()
    {
        if (HealthUI.Instance != null)
        {
            // 🎯 [★수정]: 에러나던 UpdateHealthBar 대신 원래 작성하셨던 SetHealth 함수명으로 복구!
            HealthUI.Instance.SetHealth(currentHp, maxHp);
        }
    }
}