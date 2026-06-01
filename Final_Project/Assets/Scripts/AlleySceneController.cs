using System.Collections;
using UnityEngine;

public class AlleySceneController : MonoBehaviour
{
    [Header("연동할 객체")]
    public Transform playerTransform;
    public QuestData alleyDashQuest;
    public GameObject introBlackScreen; 

    [Header("씬 시작 인트로 대사")]
    public DialogueData driverDialogue; 
    public DialogueData myIntroMonologue; 

    [Header("거리별 독백 데이터 (FreezePlayer = False 필수)")]
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

        if (driverDialogue != null)
        {
            TalkManager.Instance.StartDialogue(driverDialogue);
            yield return new WaitWhile(() => TalkManager.Instance.IsTalking); 
        }

        if (myIntroMonologue != null)
        {
            TalkManager.Instance.StartDialogue(myIntroMonologue);
            yield return new WaitWhile(() => TalkManager.Instance.IsTalking);
        }

        introBlackScreen.SetActive(false); 

        // 퀘스트 시작 (이제 QuestData 내용만 UI에 고정됩니다)
        if (QuestManager.Instance != null && alleyDashQuest != null)
        {
            QuestManager.Instance.StartQuest(alleyDashQuest);
        }

        startPosition = playerTransform.position;
        isIntroActive = false; 
    }

    void Update()
    {
        if (playerTransform == null || isIntroActive || isTimelineEnded) return;

        // 1. 달린 거리 계산
        distanceTraveled = Vector3.Distance(new Vector3(startPosition.x, 0, startPosition.z), 
                                            new Vector3(playerTransform.position.x, 0, playerTransform.position.z));

        // 2. [수정] 퀘스트 텍스트를 강제로 바꾸던 코드를 완전히 삭제했습니다.

        // 3. 거리별 자동 독백 트리거 (기존 유지)
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
        // 4. 200m 완주 시 완료 처리
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