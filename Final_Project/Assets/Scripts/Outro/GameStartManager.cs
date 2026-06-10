using System.Collections;
using UnityEngine;

public class GameStartManager : MonoBehaviour
{
    public QuestData firstQuest;

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(2.2f);

        QuestManager.Instance.StartQuest(firstQuest);
    }
}