using UnityEngine;

public class ProjectManager : MonoBehaviour
{
    public QuestData firstQuest;

    void Start()
    {
        QuestManager.Instance.StartQuest(firstQuest);
    }
}