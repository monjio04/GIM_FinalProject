using TMPro;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public TextMeshProUGUI questText;
    private QuestData currentQuest;
    private bool isCompleted;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 1. 퀘스트 시작 시 QuestData 데이터만 깔끔하게 출력
    public void StartQuest(QuestData quest)
    {
        currentQuest = quest;
        isCompleted = false;
        UpdateUI();
    }

    // 2. 퀘스트 완료 처리
    public void CompleteQuest()
    {
        isCompleted = true;
        UpdateUI();
    }

    // UI에 오직 [상태] 제목, 설명만 그리도록 고정
    void UpdateUI()
    {
        if (currentQuest == null) return;

        string status = isCompleted ? "[완료]" : "[진행중]";
        
        // 데이터에 적힌 원본 텍스트만 보여줍니다.
        questText.text = $"{status} {currentQuest.title}\n<size=16>{currentQuest.description}</size>";
    }
}