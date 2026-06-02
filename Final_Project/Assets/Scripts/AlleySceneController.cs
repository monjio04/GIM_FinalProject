using System.Collections;
using UnityEngine;

public class AlleySceneController : MonoBehaviour
{
    [Header("연동할 객체")]
    public Transform playerTransform;
    public GameObject introBlackScreen; 

    [Header("[기존] 퀘스트 및 대사 데이터")]
    public QuestData alleyDashQuest;
    public DialogueData driverDialogue; 
    public DialogueData myIntroMonologue; 

    [Header("[신규] 테스트용 장비 수습 데이터")]
    public QuestData equipmentCheckQuest;       // 장비 수습 퀘스트 데이터
    public DialogueData itemLostMonologue;      // 도끼 떨어뜨렸을 때의 독백
    public ItemData testItemData;               // 인벤토리 등록 여부를 감시할 도끼 아이템 데이터

    [Header("거리별 독백 데이터 (FreezePlayer = False)")]
    public DialogueData monologue50m;
    public DialogueData monologue100m;
    public DialogueData monologue150m;

    private Vector3 startPosition;
    private float distanceTraveled;
    
    private bool trigger50m = false;
    private bool trigger100m = false;
    private bool trigger150m = false;
    private bool isTimelineEnded = false;
    private bool isIntroActive = true; 

    void Start()
    {
        if (playerTransform != null)
        {
            startPosition = playerTransform.position;
        }

        StartCoroutine(PlayIntroSequence());
    }

    IEnumerator PlayIntroSequence()
    {
        isIntroActive = true;
        introBlackScreen.SetActive(true); 

        yield return new WaitForSeconds(1.0f); 

        // 1. 운전 대원 및 차량 진입 불가 대사 재생
        if (driverDialogue != null)
        {
            TalkManager.Instance.StartDialogue(driverDialogue);
            yield return new WaitWhile(() => TalkManager.Instance.IsTalking); 
        }

        // 2. 뛰어간다는 주인공 최초 독백 재생
        if (myIntroMonologue != null)
        {
            TalkManager.Instance.StartDialogue(myIntroMonologue);
            yield return new WaitWhile(() => TalkManager.Instance.IsTalking);
        }

        // 3. 차에서 내림 (검은 화면 해제)
        introBlackScreen.SetActive(false); 

        // ================= [테스트용 신규 시퀀스 시작] =================
        
        // 4. 장비 분실 독백 자막 재생
        if (itemLostMonologue != null)
        {
            TalkManager.Instance.StartDialogue(itemLostMonologue);
            yield return new WaitWhile(() => TalkManager.Instance.IsTalking);
        }

        // 5. 첫 번째 퀘스트 [기본 장비 수습] 시작
        if (QuestManager.Instance != null && equipmentCheckQuest != null)
        {
            QuestManager.Instance.StartQuest(equipmentCheckQuest);
        }

        // 6. 플레이어가 E키를 눌러 인벤토리에 '소방도끼'를 집어넣을 때까지 무한 대기합니다.
        // 플레이어는 멈추지 않고 맵을 돌아다니며 도끼를 찾을 수 있습니다.
        yield return new WaitUntil(() => HasPickedUpTestItem());

        // 7. 도끼를 획득했다면 장비 수습 퀘스트 완료 처리!
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteQuest();
            yield return new WaitForSeconds(1.0f); // 잠시 여운을 위해 1초 대기
        }

        // ================= [테스트용 신규 시퀀스 종료] =================

        // 8. 장비를 다 챙겼으니 비로소 메인 [골목길 질주] 퀘스트 시작
        if (QuestManager.Instance != null && alleyDashQuest != null)
        {
            QuestManager.Instance.StartQuest(alleyDashQuest);
        }

        // 질주 시작 지점을 도끼를 주운 현재 위치로 재정의하고 거리 계산 활성화
        startPosition = playerTransform.position;
        isIntroActive = false; 
    }

    // 인벤토리 매니저의 아이템 리스트를 뒤져서 소방도끼가 들어왔는지 검사하는 헬퍼 함수
    bool HasPickedUpTestItem()
    {
        if (InventoryManager.Instance == null || testItemData == null) return false;
        return InventoryManager.Instance.items.Contains(testItemData);
    }

    void Update()
    {
        if (playerTransform == null || isIntroActive || isTimelineEnded) return;

        // 거리 계산 및 거리별 독백 트리거 (기존 유지)
        distanceTraveled = Vector3.Distance(new Vector3(startPosition.x, 0, startPosition.z), 
                                            new Vector3(playerTransform.position.x, 0, playerTransform.position.z));

        if (distanceTraveled >= 50f && !trigger50m)
        {
            trigger50m = true;
            TalkManager.Instance.StartDialogue(monologue50m);
        }
        else if (distanceTraveled >= 100f && !trigger100m)
        {
            trigger100m = true;
            TalkManager.Instance.StartDialogue(monologue100m);
        }
        else if (distanceTraveled >= 150f && !trigger150m)
        {
            trigger150m = true;
            TalkManager.Instance.StartDialogue(monologue150m);
        }
        else if (distanceTraveled >= 200f && !isTimelineEnded)
        {
            isTimelineEnded = true;
            ReachedDestination();
        }
    }

    void ReachedDestination()
    {
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteQuest();
        }
        Debug.Log("200m 완주! 화재 현장 도착");
    }
}