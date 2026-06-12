using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class AlleySceneController : MonoBehaviour
{
    public static AlleySceneController Instance;

    [Header("연동할 객체")]
    public Transform playerTransform;
    public CanvasGroup introBlackScreen; 
    public TextMeshProUGUI remainingDistanceText; 

    [Header("인게임 자체 자막 UI 연동")]
    public GameObject subtitleParentUI; 
    public TextMeshProUGUI subtitleText;    
    public TextMeshProUGUI speakerText; 

    [Header("퀘스트 및 대사 데이터")]
    public QuestData alleyDashQuest;
    public DialogueData driverDialogue; 
    public DialogueData myIntroMonologue; 

    [Header("테스트용 장비 수습 데이터")]
    public QuestData equipmentCheckQuest;       
    public DialogueData itemLostMonologue;      
    public ItemData testItemData;               

    [Header("거리별 독백 데이터")]
    public DialogueData monologue50m;
    public DialogueData monologue100m;
    public DialogueData monologue150m;

    [Header("거리 시스템 설정")]
    public float totalTargetDistance = 200f;    
    public Transform destinationTransform;   

    [Header("컷씬")]
    public Camera mainCamera;
    public Camera cutsceneCamera;
    public PlayableDirector blockedRoadCutscene;

    [Header("현장 도착")]
    public DialogueData arrivedDialogue;

    [Header("현장 도착 컷씬")]
    public Camera arrivalCutsceneCamera;
    public PlayableDirector arrivalCutscene;

    [Header("오디오 설정")]
    public AudioSource audioSource;      // 급정거용
    public AudioSource sirenSource;      // 사이렌용

    public AudioClip brakeScreechClip;
    public AudioClip sirenClip;

    private float distanceTraveled;             
    private Vector3 lastPlayerPosition;         
    private float distanceScaleFactor = 1f;     
    
    private bool trigger50m = false;
    private bool trigger100m = false;
    private bool trigger150m = false;
    private bool isTimelineEnded = false;
    private bool isIntroActive = true; 

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 시작은 인트로 컷씬 카메라가 담당
        if (cutsceneCamera != null)
        {
            cutsceneCamera.gameObject.SetActive(true);
            cutsceneCamera.enabled = true;
        }

        if (mainCamera != null)
        {
            mainCamera.enabled = true;
        }

        // 도착 컷씬 카메라만 처음에 꺼둠
        if (arrivalCutsceneCamera != null)
        {
            arrivalCutsceneCamera.gameObject.SetActive(false);
            arrivalCutsceneCamera.enabled = false;
        }


        if (playerTransform != null)
        {
            lastPlayerPosition = playerTransform.position;
        }

        if (remainingDistanceText != null)
            remainingDistanceText.gameObject.SetActive(false);

        if (subtitleText != null)
            subtitleText.text = "";

        if (speakerText != null)
            speakerText.text = "";

        if (subtitleParentUI != null)
            subtitleParentUI.SetActive(false);

        StartCoroutine(PlayIntroSequence());

        introBlackScreen.alpha = 1f;
    }
    IEnumerator PlayIntroSequence()
    {
        isIntroActive = true;
        introBlackScreen.alpha=1f;

        yield return null; 
        yield return new WaitForSeconds(1.0f);

        if (sirenSource != null && sirenClip != null)
        {
            sirenSource.clip = sirenClip;
            sirenSource.loop = true;
            sirenSource.volume = 0.2f;
            sirenSource.Play();
        } 

    // 1. 운전 대원 대사 재생
    if (driverDialogue != null && TalkManager.Instance != null)
    {
        if (TalkManager.Instance.talkUI != null)
        {
            TalkManager.Instance.talkUI.SetActive(true);
            TalkManager.Instance.talkUI.transform.SetAsLastSibling();
        }

        TalkManager.Instance.StartDialogue(driverDialogue);

        yield return new WaitUntil(() => TalkManager.Instance.IsTalking == true);
        yield return new WaitWhile(() => TalkManager.Instance.IsTalking == true);
    }

    // ===== 컷씬 재생 =====
    yield return StartCoroutine(PlayBlockedRoadCutscene());

    if (itemLostMonologue != null && TalkManager.Instance != null)
    {
        bool isDone = false;
        // 대화가 끝나면 isDone을 true로 바꿔주는 콜백을 넘김
        TalkManager.Instance.StartDialogue(itemLostMonologue, () => { isDone = true; });
        
        // 이 방식은 WaitWhile보다 훨씬 안전합니다.
        yield return new WaitUntil(() => isDone);
    }

        // 5. 첫 번째 퀘스트 시작
        if (QuestManager.Instance != null && equipmentCheckQuest != null)
        {
            QuestManager.Instance.StartQuest(equipmentCheckQuest);
        }

        // 6. 소방도끼 획득 대기
        yield return new WaitUntil(() => HasPickedUpTestItem());

        // [안전장치] 획득 즉시 [E] 조사하기 UI 제거
        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.Hide();
        }

        // [안전장치] 아이템 스크립트에서 터진 획득 대사가 끝날 때까지 확실히 대기
        if (TalkManager.Instance != null)
        {
            yield return new WaitUntil(() => TalkManager.Instance.IsTalking == true);
            yield return new WaitWhile(() => TalkManager.Instance.IsTalking == true);
        }

        // 7. 장비 수습 퀘스트 완료
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteQuest();
            yield return new WaitForSeconds(1.0f); 
        }

        // 8. 메인 골목길 질주 퀘스트 시작
        if (QuestManager.Instance != null && alleyDashQuest != null)
        {
            QuestManager.Instance.StartQuest(alleyDashQuest);
        }

        distanceTraveled = 0f;
        lastPlayerPosition = playerTransform.position;

        if (destinationTransform != null)
        {
            float realMapLength = Vector3.Distance(
                new Vector3(lastPlayerPosition.x, 0, lastPlayerPosition.z),
                new Vector3(destinationTransform.position.x, 0, destinationTransform.position.z)
            );

            if (realMapLength > 0)
            {
                distanceScaleFactor = totalTargetDistance / realMapLength;
            }
        }

        if (remainingDistanceText != null) remainingDistanceText.gameObject.SetActive(true);

        isIntroActive = false; 
    }

    bool HasPickedUpTestItem()
    {
        if (InventoryManager.Instance == null || testItemData == null) return false;
        return InventoryManager.Instance.items.Contains(testItemData);
    }

    void Update()
    {
        if (playerTransform == null || isIntroActive || isTimelineEnded) return;

        Vector3 currentPos = new Vector3(playerTransform.position.x, 0, playerTransform.position.z);
        Vector3 lastPos = new Vector3(lastPlayerPosition.x, 0, lastPlayerPosition.z);
        
        float frameMovement = Vector3.Distance(currentPos, lastPos);

        if (TalkManager.Instance != null && !TalkManager.Instance.talkUI.activeSelf)
        {
            distanceTraveled += (frameMovement * distanceScaleFactor);
        }

        lastPlayerPosition = playerTransform.position;

        if (remainingDistanceText != null)
        {
            float remaining = Mathf.Max(0f, totalTargetDistance - distanceTraveled);
            remainingDistanceText.text = $"{Mathf.RoundToInt(remaining)}M";
        }

        if (distanceTraveled >= totalTargetDistance && !isTimelineEnded)
        {
            isTimelineEnded = true;
            StartCoroutine(ReachedDestinationSequence());
        }
    }

    public void Trigger50mMonologue()
    {
        if (!trigger50m)
        {
            trigger50m = true;
            StartCoroutine(ShowInGameSubtitle(monologue50m));
        }
    }

    public void Trigger100mMonologue()
    {
        if (!trigger100m)
        {
            trigger100m = true;
            StartCoroutine(ShowInGameSubtitle(monologue100m));
        }
    }

    public void Trigger150mMonologue()
    {
        if (!trigger150m)
        {
            trigger150m = true;
            StartCoroutine(ShowInGameSubtitle(monologue150m));
        }
    }

    IEnumerator ShowInGameSubtitle(DialogueData dialogue)
    {
        if (dialogue == null || subtitleText == null) yield break;

        if (subtitleParentUI != null) subtitleParentUI.SetActive(true);

        if (speakerText != null)
        {
            if (dialogue.dialogueType == DialogueType.Monologue || dialogue.dialogueType == DialogueType.Narration)
            {
                speakerText.text = ""; 
            }
            else
            {
                speakerText.text = $"[{dialogue.speaker}]"; 
            }
        }

        foreach (string line in dialogue.lines)
        {
            subtitleText.text = line;
            subtitleText.maxVisibleCharacters = 0;

            for (int i = 0; i <= line.Length; i++)
            {
                subtitleText.maxVisibleCharacters = i;
                yield return new WaitForSeconds(0.05f); 
            }

            yield return new WaitForSeconds(dialogue.autoDisplayTime);
        }

        subtitleText.text = ""; 
        if (speakerText != null) speakerText.text = "";
        if (subtitleParentUI != null) subtitleParentUI.SetActive(false);
    }

    public bool IsIntroActive()
    {
        return isIntroActive;
    }

    void ReachedDestination()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteQuest();
        }
        if (remainingDistanceText != null) remainingDistanceText.text = "현장 도착!";
        Debug.Log("200m 완주! 화재 현장 도착");
    }

    IEnumerator FadeOut(float duration)
        {
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;

                introBlackScreen.alpha =
                    Mathf.Lerp(0f, 1f, t / duration);

                yield return null;
            }

            introBlackScreen.alpha = 1f;
        }

    IEnumerator FadeIn(float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            introBlackScreen.alpha =
                Mathf.Lerp(1f, 0f, t / duration);

            yield return null;
        }

        introBlackScreen.alpha = 0f;
    }

    IEnumerator PlayBlockedRoadCutscene()
    {
        
        // 2. 화면이 완전히 까만 상태에서 카메라 교체 및 타임라인 준비
        mainCamera.enabled = false;
        cutsceneCamera.enabled = true;

        if (blockedRoadCutscene != null)
        {
            blockedRoadCutscene.Evaluate(); // 0초 시점 카메라 위치 강제 고정
        }
        yield return new WaitForEndOfFrame();

        // 3. 암전 상태에서 급정거 효과음 재생
        if (audioSource != null && brakeScreechClip != null)
        {
            audioSource.PlayOneShot(brakeScreechClip);
            yield return new WaitForSeconds(brakeScreechClip.length);

            if (sirenSource != null)
            {
                sirenSource.volume = 0.05f;
            } 
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        // 4. 소리가 완전히 끝난 "직후" 타임라인을 재생하면서 화면을 밝힙니다!
        if (blockedRoadCutscene != null)
        {
            blockedRoadCutscene.Play();
        }
        
        yield return StartCoroutine(FadeIn(1f));

        // 5. 1초 뒤 독백 대사 출력
        yield return new WaitForSeconds(1f);
        StartCoroutine(ShowInGameSubtitle(myIntroMonologue));

        // 6. 컷씬 끝날 때까지 대기
        float remainingCutsceneTime = (float)blockedRoadCutscene.duration - 1f;
        if (remainingCutsceneTime > 0)
        {
            yield return new WaitForSeconds(remainingCutsceneTime);
        }

        // 7. 메인 게임으로 돌아오기 위한 암전 및 카메라 복귀
        yield return StartCoroutine(FadeOut(1f));
        if (blockedRoadCutscene != null)
        {
            blockedRoadCutscene.Stop();
        }

        cutsceneCamera.enabled = false;
        cutsceneCamera.gameObject.SetActive(false);


        mainCamera.gameObject.SetActive(true);
        mainCamera.enabled = true;


        yield return StartCoroutine(FadeIn(1f));

        if (sirenSource != null)
        {
            sirenSource.volume = 0.005f;
        }
    }

    IEnumerator ReachedDestinationSequence()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteQuest();
        }

        if (remainingDistanceText != null)
        {
            remainingDistanceText.text = "현장 도착!";
        }

        // 헐떡임 종료
        if (PlayerHealth.Instance != null &&
            PlayerHealth.Instance.breathingSource != null)
        {
            PlayerHealth.Instance.breathingSource.Stop();
        }

        yield return new WaitForSeconds(1f);

        // 도착 대사
        if (arrivedDialogue != null)
        {
            TalkManager.Instance.StartDialogue(arrivedDialogue);

            yield return new WaitUntil(() => TalkManager.Instance.IsTalking);
            yield return new WaitWhile(() => TalkManager.Instance.IsTalking);
        }

        // 현장 도착 컷씬
        yield return StartCoroutine(PlayArrivalCutscene());

        Debug.Log("현장 도착 컷씬 종료");
    }

    IEnumerator FadeOutWithAudio(float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            float progress = t / duration;

            introBlackScreen.alpha =
                Mathf.Lerp(0f, 1f, progress);

            AudioListener.volume =
                Mathf.Lerp(1f, 0f, progress);

            yield return null;
        }

        introBlackScreen.alpha = 1f;
        AudioListener.volume = 0f;
    }

   IEnumerator PlayArrivalCutscene()
    {
        yield return StartCoroutine(FadeOutWithAudio(1f));


        // 기존 카메라 OFF
        mainCamera.enabled = false;


        // 도착 컷씬 카메라 ON
        arrivalCutsceneCamera.gameObject.SetActive(true);
        arrivalCutsceneCamera.enabled = true;


        if (arrivalCutscene != null)
        {
            arrivalCutscene.time = 0;
            arrivalCutscene.Evaluate();
        }


        yield return new WaitForEndOfFrame();


        AudioListener.volume = 1f;

        yield return StartCoroutine(FadeIn(1f));


        if (arrivalCutscene != null)
        {
            arrivalCutscene.Play();

            yield return new WaitWhile(() =>
                arrivalCutscene.state == PlayState.Playing);
        }


        yield return StartCoroutine(FadeOutWithAudio(1f));


        if (arrivalCutscene != null)
        {
            arrivalCutscene.Stop();
        }


        arrivalCutsceneCamera.enabled = false;
        arrivalCutsceneCamera.gameObject.SetActive(false);


        mainCamera.enabled = true;


        AudioListener.volume = 1f;


        yield return StartCoroutine(FadeIn(1f));
    }
}