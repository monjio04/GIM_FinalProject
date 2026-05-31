using UnityEngine;
using UnityEngine.SceneManagement;

public class Scene2Manager : MonoBehaviour
{
    public static Scene2Manager Instance;

    [Header("Player & Distance")]
    public Transform playerTransform;
    private Vector3 startPosition;
    public float currentDistance = 0f;
    public float targetDistance = 200f;

    [Header("Quest Data")]
    public QuestData scene2Quest; // 씬 2 전용 퀘스트 에셋 연결

    [Header("Dialogue Datas")]
    public DialogueData startDialogue;   // 시작 시 대사 (운전대원 등)
    public DialogueData mono50m;         // 50m 독백
    public DialogueData mono100m;        // 100m 독백
    public DialogueData mono150m;        // 150m 독백

    // 대사가 1번만 실행되도록 체크하는 플래그
    private bool playedStart = false;
    private bool played50m = false;
    private bool played100m = false;
    private bool played150m = false;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (playerTransform != null)
        {
            startPosition = playerTransform.position;
        }

        // 1. 씬 2 시작하자마자 퀘스트 UI를 띄웁니다.
        if (QuestManager.Instance != null && scene2Quest != null)
        {
            QuestManager.Instance.StartQuest(scene2Quest);
        }

        // 2. 시작 대사(운전대원 대사 및 독백)를 실행합니다.
        if (TalkManager.Instance != null && startDialogue != null)
        {
            TalkManager.Instance.StartDialogue(startDialogue);
            playedStart = true;
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        // 플레이어 이동 거리 계산
        Vector3 currentPos = playerTransform.position;
        currentPos.y = startPosition.y; 
        currentDistance = Vector3.Distance(startPosition, currentPos);

        // 거리별 독백 이벤트 체크
        CheckDistanceEvents();

        // 200m 완주 시 퀘스트 완료 및 씬 전환
        if (currentDistance >= targetDistance)
        {
            EndScene2();
        }
    }

    void CheckDistanceEvents()
    {
        if (TalkManager.Instance == null) return;

        // 50m 지점 독백
        if (currentDistance >= 50f && !played50m)
        {
            played50m = true;
            TalkManager.Instance.StartDialogue(mono50m);
        }

        // 100m 지점 독백
        if (currentDistance >= 100f && !played100m)
        {
            played100m = true;
            TalkManager.Instance.StartDialogue(mono100m);
        }

        // 150m 지점 독백
        if (currentDistance >= 150f && !played150m)
        {
            played150m = true;
            TalkManager.Instance.StartDialogue(mono150m);
        }
    }

    void EndScene2()
    {
        // 퀘스트 완료 표시
        if (QuestManager.Instance != null)
        {
            QuestManager.Instance.CompleteQuest();
        }

        Debug.Log("200m 현장 도착! 다음 씬으로 전환");
        // SceneManager.LoadScene("Scene3"); // Scene 3 빌드 세팅 후 주석 해제
    }
}