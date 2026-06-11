using System.Collections;
using UnityEngine;

public class ProjectManager : MonoBehaviour
{
    public static ProjectManager Instance;

    [Header("1. 대본(데이터) 연결")]
    public DialogueData firstDialogue;       
    public QuestData questSCBA;              
    public QuestData questPerson;            
    public QuestData questEscape;  
    public DialogueData escapeMonologue;     // ★ 신규: "대피 완료. 철수한다." 독백
    public DialogueData radioDialogue;       
    public DialogueData mother0Trigger;      
    public DialogueData silenceDialogue;     
    public DialogueData monologueDialogue;   

    [Header("2. 씬 오브젝트 연결")]
    public GameObject scbaObject;            // ★ 신규: 바닥에 놓인 SCBA 장비 본체
    public GameObject insideExitTrigger;     // ★ 신규: 건물 안쪽 문앞 투명 덫
    public GameObject grandmaTrigger;        // 건물 밖 할머니 이벤트용 투명 덫
    public GameObject selectionUI;           
    public Transform grandmaTransform;       // 할머니 캡슐 본체

    [Header("3. 순간이동 & 페이드 연출 연결")]
    public Transform playerTransform;        
    public Transform outsideTeleportPoint;   
    public CanvasGroup fadeCanvasGroup; 

    [Header("할머니 이벤트 대사")]
    public DialogueData grandma1Dialogue;
    public DialogueData player2Dialogue;
    public DialogueData grandma2Dialogue;     

    private int currentPhase = 0; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        // 0번 문제 해결: 게임 시작 시 할머니 캡슐과 덫들을 모두 안 보이게 숨깁니다!
        if (insideExitTrigger != null) insideExitTrigger.SetActive(false);
        if (grandmaTrigger != null) grandmaTrigger.SetActive(false);
        if (selectionUI != null) selectionUI.SetActive(false);
        if (grandmaTransform != null) grandmaTransform.gameObject.SetActive(false); 
        if (fadeCanvasGroup != null) fadeCanvasGroup.alpha = 0f;

        TalkManager.Instance.StartDialogue(firstDialogue, () => 
        {
            QuestManager.Instance.StartQuest(questSCBA);
            currentPhase = 1; 
        });
    }

    private void Update()
    {
        // 1, 2번 문제 해결: Item.cs를 안 건드리고, 맵에 있던 SCBA가 사라지면 획득한 것으로 간주!
        if (currentPhase == 1 && scbaObject == null)
        {
            currentPhase = 2; 
            StartCoroutine(ScbaObtainedRoutine());
        }
    }

    IEnumerator ScbaObtainedRoutine()
    {
        // SCBA 획득 즉시 산소 UI를 켭니다.
        if (OxygenManager.Instance != null && !OxygenManager.Instance.isO2Active)
        {
            OxygenManager.Instance.ActivateOxygen();
        }

        // "공기잔량 100..." 대사가 끝날 때까지 얌전히 대기
        yield return new WaitUntil(() => !TalkManager.Instance.IsTalking);

        // 대사가 끝나면 즉시 퀘스트 3 완료 후 퀘스트 4(사람 구하기) 시작
        QuestManager.Instance.CompleteQuest(); 
        yield return new WaitForSeconds(1.0f); 
        QuestManager.Instance.StartQuest(questPerson); 
    }

    // 노인을 구출했을 때 호출됨
    public void OnNpcRescued()
    {
        if (currentPhase == 2)
        {
            currentPhase = 3;
            StartCoroutine(NpcRescuedRoutine()); 
        }
    }

    IEnumerator NpcRescuedRoutine()
    {
        // "찾았다! 나갑시다!" 대사 대기
        yield return new WaitUntil(() => !TalkManager.Instance.IsTalking);

        if (OxygenManager.Instance != null) OxygenManager.Instance.DecreaseOxygen(30);

        QuestManager.Instance.CompleteQuest(); 
        yield return new WaitForSeconds(1.0f); 
        if (questEscape != null) QuestManager.Instance.StartQuest(questEscape);

        // 3번 문제: 구출 직후가 아니라, 플레이어가 걸어나갈 '안쪽 문 덫'을 켜줍니다!
        if (insideExitTrigger != null) insideExitTrigger.SetActive(true);
    }

    // 안쪽 문 앞 덫을 밟았을 때 호출됨 (텔레포트 시작)
    public void OnReachInsideExit()
    {
        if (currentPhase == 3)
        {
            currentPhase = 4;
            StartCoroutine(EscapeAndTeleportRoutine());
        }
    }

    IEnumerator EscapeAndTeleportRoutine()
    {
        // "대피 완료. 철수한다." 독백 출력
        bool isMonologueDone = false;
        if (escapeMonologue != null)
        {
            TalkManager.Instance.StartDialogue(escapeMonologue, () => { isMonologueDone = true; });
            yield return new WaitUntil(() => isMonologueDone);
        }

        // 무전 출력
        bool isRadioDone = false;
        if (radioDialogue != null)
        {
            TalkManager.Instance.StartDialogue(radioDialogue, () => { isRadioDone = true; });
            yield return new WaitUntil(() => isRadioDone);
        }

        // 화면 까매짐
        if (fadeCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < 1f) { elapsed += Time.deltaTime; fadeCanvasGroup.alpha = elapsed; yield return null; }
            fadeCanvasGroup.alpha = 1f;
        }

        // 텔레포트 실행
        if (playerTransform != null && outsideTeleportPoint != null)
        {
            CharacterController cc = playerTransform.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false; 

            playerTransform.position = outsideTeleportPoint.position;
            playerTransform.rotation = outsideTeleportPoint.rotation;

            if (cc != null) cc.enabled = true; 
        }

        // 텔레포트 완료 후, 할머니 캡슐과 밖의 덫을 뿅 하고 나타나게 합니다.
        if (grandmaTransform != null) grandmaTransform.gameObject.SetActive(true);
        if (grandmaTrigger != null) grandmaTrigger.SetActive(true);

        yield return new WaitForSeconds(1.0f); 

        // 화면 밝아짐
        if (fadeCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < 1f) { elapsed += Time.deltaTime; fadeCanvasGroup.alpha = 1f - elapsed; yield return null; }
            fadeCanvasGroup.alpha = 0f;
        }
    }

    public void OnPlayerExitBuilding()
    {
        // 페이즈 4(밖으로 나옴)일 때만 대화 시작 가능
        if (currentPhase != 4) return; 

        currentPhase = 5;

        if (playerTransform != null && grandmaTransform != null)
            StartCoroutine(ForceLookAtRoutine(grandmaTransform));

        // 순차적으로 출력할 대사들 (silenceDialogue, monologueDialogue 포함)
        DialogueData[] sequence = { 
            mother0Trigger, grandma1Dialogue, player2Dialogue, 
            grandma2Dialogue, silenceDialogue, monologueDialogue 
        };

        TalkManager.Instance.StartDialogueSequence(sequence, () =>
        {
            // 모든 대사가 끝난 후 선택지 UI 활성화
            if (selectionUI != null)
            {
                selectionUI.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        });
    }

    public void OnReachGrandmaTrigger()
    {
        // 페이즈 4에서만 작동
        if (currentPhase != 4) return;
        currentPhase = 5; // 자동 대사 시작

        if (playerTransform != null && grandmaTransform != null)
            StartCoroutine(ForceLookAtRoutine(grandmaTransform));

        TalkManager.Instance.StartDialogue(mother0Trigger, () => {
            // 자동 대사 종료 후, 이제 대화 준비 완료 상태 (페이즈는 그대로 5 유지)
        });
    }

    // 2. E키를 눌렀을 때 (나머지 시퀀스 출력)
    public void OnTalkToGrandma()
    {
        // 자동 대사(mother0)가 끝난 이후 상태(페이즈 5)라면 실행
        // 혹은 혹시 모를 상황을 위해 페이즈 5 이상이면 실행되게 조건 완화
        if (currentPhase < 5) return; 
        
        // 이미 대화 중이라면 다시 시작하지 않음 (이중 실행 방지)
        if (TalkManager.Instance.IsTalking) return;

        DialogueData[] sequence = { 
            grandma1Dialogue, player2Dialogue, grandma2Dialogue, 
            silenceDialogue, monologueDialogue 
        };

        TalkManager.Instance.StartDialogueSequence(sequence, () =>
        {
            if (selectionUI != null)
            {
                selectionUI.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        });
    }
    IEnumerator ForceLookAtRoutine(Transform target)
    {
        Vector3 direction = (target.position - playerTransform.position).normalized;
        direction.y = 0; 
        Quaternion startRotation = playerTransform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        float elapsed = 0f, duration = 0.3f; 
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            playerTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
            yield return null;
        }
        playerTransform.rotation = targetRotation;
    }

    public void OnGrandmaTalkEnd()
    {
        TalkManager.Instance.StartDialogue(silenceDialogue, () => 
        {
            TalkManager.Instance.StartDialogue(monologueDialogue, () => 
            {
                if (selectionUI != null)
                {
                    selectionUI.SetActive(true);
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
            });
        });
    }
}