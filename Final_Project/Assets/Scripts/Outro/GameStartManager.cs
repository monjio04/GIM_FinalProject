using System.Collections;
using UnityEngine;

public class GameStartManager : MonoBehaviour
{
    public QuestData firstQuest;

    private IEnumerator Start()
    {
        // 1. 대사 UI 강제 비활성화
        if (TalkManager.Instance != null && TalkManager.Instance.talkUI != null)
        {
            TalkManager.Instance.talkUI.SetActive(false);
        }

        yield return new WaitForSeconds(2.2f);

        QuestManager.Instance.StartQuest(firstQuest);
    }
}