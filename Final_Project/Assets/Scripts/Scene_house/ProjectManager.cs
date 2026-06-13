using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement; 
using UnityEngine.Playables; 

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
    public GameObject exitCollapseTrigger; 
    public DialogueData reenterDialogue;   
    public DialogueData collapseDialogue;  
    public GameObject phase4BFolder;       
    public QuestData questFindSon;         
    private int clueFoundCount = 0;   
    public CanvasGroup vignetteUI;   

    [Header("4-B 건물 진입 컷씬")]
    public Camera mainCamera;
    public PlayableDirector enterCutscene; // PlayableDirector 연결
    public Camera cutsceneCamera;          // 건물 진입 컷씬용 카메라
    public Transform playerEnterStartPoint; // 컷씬 시작 시 플레이어 위치
    public Transform playerEnterEndPoint;   // 컷씬 끝난 후 플레이어 위치  

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
        
        if (vignetteUI != null) vignetteUI.alpha = 0f;
        if (exitCollapseTrigger != null) exitCollapseTrigger.SetActive(false);
        if (cutsceneCamera != null) cutsceneCamera.enabled = false;

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

// 1. 선택지 UI에서 호출하는 함수
    public void SelectEnterBuilding()
    {
        // 1. 선택지 UI 비활성화
        if (selectionUI != null) selectionUI.SetActive(false);
        
        // 마우스 커서는 대화가 완전히 끝날 때까지 잠그지 않거나, 
        // 대화 시스템 정책에 따라 조정합니다 (여기선 일단 잠금)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // 2. 먼저 대사를 시작합니다.
        if (reenterDialogue != null) 
        {
            TalkManager.Instance.StartDialogue(reenterDialogue, () => 
            {
                // ★ 대화가 끝난 후 컷씬 코루틴 시작!
                StartCoroutine(PlayEnterBuildingCutscene());
            });
        }
        else
        {
            // 대사 데이터가 없으면 바로 컷씬 시작
            StartCoroutine(PlayEnterBuildingCutscene());
        }
    }

    IEnumerator PlayEnterBuildingCutscene()
        {
            if (InteractionUI.Instance != null) InteractionUI.Instance.Hide();
            if (fadeCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < 1.0f) { elapsed += Time.deltaTime; fadeCanvasGroup.alpha = elapsed; yield return null; }
            fadeCanvasGroup.alpha = 1f;
        }

        var cc = playerTransform.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        
        playerTransform.position = playerEnterStartPoint.position;
        playerTransform.rotation = playerEnterStartPoint.rotation;

        // 1. 카메라 전환
        if (mainCamera != null) mainCamera.enabled = false; 
        if (cutsceneCamera != null) cutsceneCamera.enabled = true;

        // ★ 암전 상태에서 화면을 서서히 밝힘 (Fade In)
        if (fadeCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < 1.0f) { elapsed += Time.deltaTime; fadeCanvasGroup.alpha = 1f - elapsed; yield return null; }
            fadeCanvasGroup.alpha = 0f;
        }

        // 2. 컷씬 재생
        if (enterCutscene != null)
        {
            enterCutscene.Play();
            yield return new WaitForSeconds((float)enterCutscene.duration);
        }

        // ★ 종료 전 다시 암전 (Fade Out)
        if (fadeCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < 1.0f) { elapsed += Time.deltaTime; fadeCanvasGroup.alpha = elapsed; yield return null; }
            fadeCanvasGroup.alpha = 1f;
        }

        // 3. 카메라 복구 및 위치 이동
        if (cutsceneCamera != null) cutsceneCamera.enabled = false;
        if (mainCamera != null) mainCamera.enabled = true;
        
        playerTransform.position = playerEnterEndPoint.position;
        playerTransform.rotation = playerEnterEndPoint.rotation;
        
        if (cc != null) cc.enabled = true;

        // ★ 종료 후 화면 밝힘 (Fade In)
        if (fadeCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < 1.0f) { elapsed += Time.deltaTime; fadeCanvasGroup.alpha = 1f - elapsed; yield return null; }
            fadeCanvasGroup.alpha = 0f;
        }

        // 4. 나머지 진행 로직 (퀘스트 등)
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

        // 퀘스트 시작은 컷씬 종료 후 실행
        if (questFindSon != null) QuestManager.Instance.StartQuest(questFindSon);
    }

    public void OnClueFound()
    {
        clueFoundCount++;
        MainCharacter player = FindObjectOfType<MainCharacter>();

        // 공통 로직: 산소 감소 및 속도 저하
        if (OxygenManager.Instance != null) OxygenManager.Instance.DecreaseOxygen(20); 
        if (player != null) { player.walkSpeed *= 0.8f; player.runSpeed *= 0.8f; }

        // ★ 수정: 단서 갯수에 따라 비네트 알파값 서서히 증가
        if (vignetteUI != null)
        {
            if (clueFoundCount == 1) vignetteUI.alpha = 0.3f;
            else if (clueFoundCount == 2) vignetteUI.alpha = 0.6f;
            else if (clueFoundCount >= 3)
            {
                vignetteUI.alpha = 0.9f;
                StartCoroutine(Clue3EventRoutine());
            }
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
        if (vignetteUI != null)
        {
            float elapsed = 0f;
            float startAlpha = vignetteUI.alpha;
            while (elapsed < 0.5f)
            {
                elapsed += Time.deltaTime;
                vignetteUI.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / 0.5f);
                yield return null;
            }
            vignetteUI.alpha = 1f;
        }

        // 5. 검은색 페이드 아웃 (이제 비네트가 1.0이니까 fadeCanvasGroup으로 전체 암전)
        if (fadeCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < 1.5f)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / 1.5f);
                yield return null;
            }
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
        SceneManager.LoadScene("outro"); 
    }
}