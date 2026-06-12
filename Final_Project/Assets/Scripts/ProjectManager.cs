using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class ProjectManager : MonoBehaviour
{
    public static ProjectManager Instance;

    [Header("1. 대본(데이터) 연결")]
    public DialogueData firstDialogue;       
    public QuestData questSCBA;              
    public QuestData questPerson;            
    public QuestData questEscape;  
    public DialogueData escapeMonologue;     
    public DialogueData radioDialogue;       
    public DialogueData mother0Trigger;      
    public DialogueData silenceDialogue;     
    public DialogueData monologueDialogue;   

    [Header("2. 씬 오브젝트 연결")]
    public GameObject scbaObject;            
    public GameObject insideExitTrigger;     
    public GameObject grandmaTrigger;        
    public GameObject selectionUI;           
    public Transform grandmaTransform;       

    [Header("3. 순간이동 & 페이드 연출 연결")]
    public Transform playerTransform;        
    public Transform outsideTeleportPoint;   
    public CanvasGroup fadeCanvasGroup; 

    [Header("할머니 이벤트 대사")]
    public DialogueData grandma1Dialogue;
    public DialogueData player2Dialogue;
    public DialogueData grandma2Dialogue;     

    [Header("5. 건물 재진입(4-B) 연출 연결")]
    public CanvasGroup bloodScreenUI;      
    public GameObject exitCollapseTrigger; 
    public DialogueData reenterDialogue;   
    public DialogueData collapseDialogue;  
    public GameObject phase4BFolder;       
    public QuestData questFindSon;         
    private int clueFoundCount = 0;        

    [Header("6. 단서 3 이후 탈출 연출")]
    public Transform cameraTransform;            
    public DialogueData escapeAfterClueDialogue; 
    public QuestData questFinalEscape;           

    [Header("7. 최종 붕괴 및 엔딩 연출")]
    // 산소 파손 대본 변수는 지우고 무전기 대본만 남겼습니다.
    public DialogueData finalRadioDialogue;   
    
    private int currentPhase = 0; 

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (insideExitTrigger != null) insideExitTrigger.SetActive(false);
        if (grandmaTrigger != null) grandmaTrigger.SetActive(false);
        if (selectionUI != null) selectionUI.SetActive(false);
        if (grandmaTransform != null) grandmaTransform.gameObject.SetActive(false); 
        if (fadeCanvasGroup != null) fadeCanvasGroup.alpha = 0f;
        
        if (bloodScreenUI != null) bloodScreenUI.alpha = 0f;
        if (exitCollapseTrigger != null) exitCollapseTrigger.SetActive(false);

        if (phase4BFolder != null) phase4BFolder.SetActive(false);

        TalkManager.Instance.StartDialogue(firstDialogue, () => 
        {
            QuestManager.Instance.StartQuest(questSCBA);
            currentPhase = 1; 
        });
    }

    private void Update()
    {
        if (currentPhase == 1 && scbaObject == null)
        {
            currentPhase = 2; 
            StartCoroutine(ScbaObtainedRoutine());
        }
    }

    IEnumerator ScbaObtainedRoutine()
    {
        if (OxygenManager.Instance != null && !OxygenManager.Instance.isO2Active)
        {
            OxygenManager.Instance.ActivateOxygen();
        }

        yield return new WaitUntil(() => !TalkManager.Instance.IsTalking);

        QuestManager.Instance.CompleteQuest(); 
        yield return new WaitForSeconds(1.0f); 
        QuestManager.Instance.StartQuest(questPerson); 
    }

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
        yield return new WaitUntil(() => !TalkManager.Instance.IsTalking);

        if (OxygenManager.Instance != null) OxygenManager.Instance.DecreaseOxygen(30);

        QuestManager.Instance.CompleteQuest(); 
        yield return new WaitForSeconds(1.0f); 
        if (questEscape != null) QuestManager.Instance.StartQuest(questEscape);

        if (insideExitTrigger != null) insideExitTrigger.SetActive(true);
    }

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
        bool isMonologueDone = false;
        if (escapeMonologue != null)
        {
            TalkManager.Instance.StartDialogue(escapeMonologue, () => { isMonologueDone = true; });
            yield return new WaitUntil(() => isMonologueDone);
        }

        bool isRadioDone = false;
        if (radioDialogue != null)
        {
            TalkManager.Instance.StartDialogue(radioDialogue, () => { isRadioDone = true; });
            yield return new WaitUntil(() => isRadioDone);
        }

        if (fadeCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < 1f) { elapsed += Time.deltaTime; fadeCanvasGroup.alpha = elapsed; yield return null; }
            fadeCanvasGroup.alpha = 1f;
        }

        if (playerTransform != null && outsideTeleportPoint != null)
        {
            CharacterController cc = playerTransform.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false; 

            playerTransform.position = outsideTeleportPoint.position;
            playerTransform.rotation = outsideTeleportPoint.rotation;

            if (cc != null) cc.enabled = true; 
        }

        if (grandmaTransform != null) grandmaTransform.gameObject.SetActive(true);
        if (grandmaTrigger != null) grandmaTrigger.SetActive(true);

        yield return new WaitForSeconds(1.0f); 

        if (fadeCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < 1f) { elapsed += Time.deltaTime; fadeCanvasGroup.alpha = 1f - elapsed; yield return null; }
            fadeCanvasGroup.alpha = 0f;
        }
    }

    public void OnPlayerExitBuilding()
    {
        if (currentPhase != 4) return; 
        currentPhase = 5;

        if (playerTransform != null && grandmaTransform != null)
            StartCoroutine(ForceLookAtRoutine(grandmaTransform));

        DialogueData[] sequence = { 
            mother0Trigger, grandma1Dialogue, player2Dialogue, 
            grandma2Dialogue, silenceDialogue, monologueDialogue 
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

    public void OnReachGrandmaTrigger()
    {
        if (currentPhase != 4) return;
        currentPhase = 5; 

        if (playerTransform != null && grandmaTransform != null)
            StartCoroutine(ForceLookAtRoutine(grandmaTransform));

        TalkManager.Instance.StartDialogue(mother0Trigger, () => { });
    }

    public void OnTalkToGrandma()
    {
        if (currentPhase < 5) return; 
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

    public void OnGrandmaTalkEnd() { }

    public void SelectDoNotEnter()
    {
        SceneManager.LoadScene("Scene 4-A"); 
    }

    public void SelectEnterBuilding()
    {
        if (selectionUI != null) selectionUI.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        currentPhase = 6; 
        
        if (insideExitTrigger != null) insideExitTrigger.SetActive(false);
        if (grandmaTrigger != null) grandmaTrigger.SetActive(false);

        if (phase4BFolder != null) phase4BFolder.SetActive(true);

        if (OxygenManager.Instance != null)
        {
            OxygenManager.Instance.currentO2 = 70;
            OxygenManager.Instance.isO2Active = true;
            if (OxygenManager.Instance.o2UIPanel != null) OxygenManager.Instance.o2UIPanel.SetActive(true);
        }

        if (reenterDialogue != null) 
        {
            TalkManager.Instance.StartDialogue(reenterDialogue, () => 
            {
                if (questFindSon != null) QuestManager.Instance.StartQuest(questFindSon);
            });
        }
    }

    public void OnClueFound()
    {
        clueFoundCount++;
        MainCharacter player = FindObjectOfType<MainCharacter>();

        if (clueFoundCount == 1)
        {
            if (OxygenManager.Instance != null) OxygenManager.Instance.DecreaseOxygen(20); 
            if (player != null) { player.walkSpeed *= 0.8f; player.runSpeed *= 0.8f; }
        }
        else if (clueFoundCount == 2)
        {
            if (OxygenManager.Instance != null) OxygenManager.Instance.DecreaseOxygen(20); 
            if (player != null) { player.walkSpeed *= 0.8f; player.runSpeed *= 0.8f; }
            if (bloodScreenUI != null) bloodScreenUI.alpha = 0.3f; 
        }
        else if (clueFoundCount >= 3)
        {
            if (OxygenManager.Instance != null) OxygenManager.Instance.DecreaseOxygen(20); 
            if (player != null) { player.walkSpeed *= 0.8f; player.runSpeed *= 0.8f; }
            if (bloodScreenUI != null) bloodScreenUI.alpha = 0.6f; 

            StartCoroutine(Clue3EventRoutine());
        }
    }

    IEnumerator Clue3EventRoutine()
    {
        yield return new WaitUntil(() => !TalkManager.Instance.IsTalking);

        if (cameraTransform != null)
        {
            Vector3 originalPos = cameraTransform.localPosition;
            float elapsed = 0f;
            float duration = 1.0f; 

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float yOffset = Mathf.Sin(Time.time * 60f) * 0.15f; 
                cameraTransform.localPosition = originalPos + new Vector3(0, yOffset, 0);
                yield return null;
            }
            cameraTransform.localPosition = originalPos; 
        }

        bool isDialogueDone = false;
        if (escapeAfterClueDialogue != null)
        {
            TalkManager.Instance.StartDialogue(escapeAfterClueDialogue, () => { isDialogueDone = true; });
            yield return new WaitUntil(() => isDialogueDone); 
        }

        QuestManager.Instance.CompleteQuest();
        yield return new WaitForSeconds(0.5f);
        if (questFinalEscape != null)
        {
            QuestManager.Instance.StartQuest(questFinalEscape);
        }

        if (exitCollapseTrigger != null) exitCollapseTrigger.SetActive(true);
    }

    public void OnReachCollapsePoint()
    {
        StartCoroutine(CollapseRoutine());
    }

    // ★★★ 핵심 수정: O2 게이지 하락 연출로 변경! ★★★
    IEnumerator CollapseRoutine()
    {
        // 1. "다 왔..." 대사 출력 후 대기
        bool isCollapseDone = false;
        if (collapseDialogue != null)
        {
            TalkManager.Instance.StartDialogue(collapseDialogue, () => { isCollapseDone = true; });
            yield return new WaitUntil(() => isCollapseDone);
        }

        // 2. 다시 한 번 카메라 강진 (무너지는 연출)
        if (cameraTransform != null)
        {
            Vector3 originalPos = cameraTransform.localPosition;
            float elapsed = 0f;
            float duration = 1.5f; 

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float yOffset = Mathf.Sin(Time.time * 80f) * 0.2f; 
                cameraTransform.localPosition = originalPos + new Vector3(0, yOffset, 0);
                yield return null;
            }
            cameraTransform.localPosition = originalPos; 
        }

        // 3. 잔해에 깔려 산소가 0으로 깎임 (UI 게이지 업데이트)
        if (OxygenManager.Instance != null) 
        {
            OxygenManager.Instance.DecreaseOxygen(10); 
        }
        
        // 4. 화면 빨갛게 페이드 아웃 (0.5초 만에 확 덮음)
        if (bloodScreenUI != null)
        {
            float elapsed = 0f;
            float startAlpha = bloodScreenUI.alpha;
            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                bloodScreenUI.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / 0.5f);
                yield return null;
            }
            bloodScreenUI.alpha = 1f;
        }

        // 5. 그 위를 서서히 검은색으로 덮음 (1.5초간)
        if (fadeCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < 1.5f)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / 1.5f);
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;
        }

        // 6. 완전한 암전 속에서 3초 유지
        yield return new WaitForSeconds(3.0f); 

        // 7. 지지직... 마지막 무전기 대사 출력
        bool isRadioDone = false;
        if (finalRadioDialogue != null)
        {
            TalkManager.Instance.StartDialogue(finalRadioDialogue, () => { isRadioDone = true; });
            yield return new WaitUntil(() => isRadioDone);
        }

        // 8. 무전이 끝나면 진엔딩(혹은 씬 5)로 넘어감!
        SceneManager.LoadScene("Scene 5"); 
    }
}