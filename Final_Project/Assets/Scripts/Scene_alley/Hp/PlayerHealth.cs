using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance;

    [Header("HP 설정")]
    public float maxHp = 100f;
    public float currentHp;

    [Header("초당 HP 소모량 / 회복량")]
    [Tooltip("이제 걸을 때 이 수치만큼 초당 체력이 회복됩니다.")]
    public float walkHpDrain = 3f; 
    public float runHpDrain = 8f;
    public float idleHpHeal = 5f;

    [Header("탈진(Exhausted) 설정")]
    public float restDuration = 3f;
    public DialogueData exhaustedDialogue; 
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
            if (AlleySceneController.Instance != null && !AlleySceneController.Instance.IsIntroActive())
            {
                // 질주 중이므로 체력 소모/회복 진행 가능
            }
            else return;
        }

        // 이동 키 입력 여부 확인
        bool isMoving = Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;

        if (controller != null && controller.isGrounded)
        {
            if (isMoving)
            {
                // 1. 달리는 중일 때 (LeftShift 누름) -> 체력 소모
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    float dynamicScale = 1f;

                    // 골목길 질주 중일 때 체력 소모 속도를 3배로 보정
                    if (AlleySceneController.Instance != null && !AlleySceneController.Instance.IsIntroActive())
                    {
                        dynamicScale = 3.0f;
                    }

                    TakeDamage(runHpDrain * dynamicScale * Time.deltaTime);
                }
                // 2. 그냥 걷는 중일 때 (LeftShift 안 누름) -> 체력 회복
                else
                {
                    Heal(walkHpDrain * Time.deltaTime);
                }
            }
            // 3. 가만히 서 있을 때 (Idle) -> 체력 회복
            else
            {
                Heal(idleHpHeal * Time.deltaTime);
            }
        }
    } // <-- 기존에 이 아래로 똑같은 로직이 중복 삽입되어 있던 에러 유발 지점을 삭제했습니다.

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
        
        if (exhaustedDialogue != null && TalkManager.Instance != null)
        {
            isWaitingForDialogue = true; 
            TalkManager.Instance.StartDialogue(exhaustedDialogue);
            
            StartCoroutine(WaitForExhaustedDialogueEnd());
        }
        else
        {
            isWaitingForDialogue = false;
        }
        
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
            HealthUI.Instance.SetHealth(currentHp, maxHp);
        }
    }

    private IEnumerator WaitForExhaustedDialogueEnd()
    {
        yield return new WaitUntil(() => TalkManager.Instance.IsTalking == true);
        yield return new WaitWhile(() => TalkManager.Instance.IsTalking == true);

        isWaitingForDialogue = false;
    }
}