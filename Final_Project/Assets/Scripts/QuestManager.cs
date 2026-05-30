using TMPro;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    public TextMeshProUGUI questText;

    public QuestData currentQuest;

    bool isCompleted;

    private void Awake()
    {
        Instance = this;
    }

    public void StartQuest(QuestData quest)
    {
        currentQuest = quest;
        isCompleted = false;

        UpdateUI();
    }

    public void CompleteQuest()
    {
        isCompleted = true;

        UpdateUI();
    }

    void UpdateUI()
    {
        if (currentQuest == null)
            return;

        string status =
            isCompleted ?
            "[완료]" :
            "[진행중]";

        questText.text =
            $"{status}\n{currentQuest.title}";
    }
}