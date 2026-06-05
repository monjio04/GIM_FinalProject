using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class AlleySceneController : MonoBehaviour
{
    public static AlleySceneController Instance;

    [Header("연동할 객체")]
    public Transform playerTransform;
    public GameObject introBlackScreen; 
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
        if (playerTransform != null)
        {
            lastPlayerPosition = playerTransform.position;
        }

        if (remainingDistanceText != null) remainingDistanceText.gameObject.SetActive(false);
        if (subtitleText != null) subtitleText.text = ""; 
        if (speakerText != null) speakerText.text = "";
        if (subtitleParentUI != null) subtitleParentUI.SetActive(false);

        StartCoroutine(PlayIntroSequence());
    }

    IEnumerator PlayIntroSequence()
    {
        isIntroActive = true;
        introBlackScreen.SetActive(true); 

        yield return null; 
        yield return new WaitForSeconds(1.0f); 

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

        // 2. 주인공 최초 독백 재생
        if (myIntroMonologue != null && TalkManager.Instance != null)
        {
            if (TalkManager.Instance.talkUI != null)
            {
                TalkManager.Instance.talkUI.transform.SetAsLastSibling();
            }

            TalkManager.Instance.StartDialogue(myIntroMonologue);
            yield return new WaitUntil(() => TalkManager.Instance.IsTalking == true);
            yield return new WaitWhile(() => TalkManager.Instance.IsTalking == true);
        }

        // 3. 차에서 내림 (검은 화면 해제)
        introBlackScreen.SetActive(false); 

        // 4. 장비 분실 독백 재생
        if (itemLostMonologue != null && TalkManager.Instance != null)
        {
            TalkManager.Instance.StartDialogue(itemLostMonologue);
            yield return new WaitUntil(() => TalkManager.Instance.IsTalking == true);
            yield return new WaitWhile(() => TalkManager.Instance.IsTalking == true);
        }

        // 5. 첫 번째 퀘스트 시작
        if (QuestManager.Instance != null && equipmentCheckQuest != null)
        {
            QuestManager.Instance.StartQuest(equipmentCheckQuest);
        }

        // 6. 소방도끼 획득 대기
        yield return new WaitUntil(() => HasPickedUpTestItem());

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
            remainingDistanceText.text = $"남은 거리: {Mathf.RoundToInt(remaining)}M";
        }

        if (distanceTraveled >= totalTargetDistance && !isTimelineEnded)
        {
            isTimelineEnded = true;
            ReachedDestination();
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
}