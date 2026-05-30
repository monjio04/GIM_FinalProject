using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public Quest currentQuest;

    public void StartQuest(Quest quest)
    {
        currentQuest = quest;
        currentQuest.state = QuestState.InProgress;
    }

    public void CompleteQuest()
    {
        currentQuest.state = QuestState.Completed;
    }
}