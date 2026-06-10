using System.Collections;
using UnityEngine;

public class InvestigationManager : MonoBehaviour
{
    public static InvestigationManager Instance;

    public int targetCount = 4;

    private int currentCount = 0;

    private bool endingStarted = false;

    public DialogueData endingNarration;

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterInspection()
    {
        currentCount++;

        Debug.Log($"{currentCount}/{targetCount}");

        if (currentCount >= targetCount && !endingStarted)
        {
            StartCoroutine(WaitForUIClosed());
        }
    }

    IEnumerator WaitForUIClosed()
    {
        endingStarted = true;

        // 조사창이 닫힐 때까지 대기
        while (InvestigationUI.Instance != null &&
               InvestigationUI.Instance.IsOpen)
        {
            yield return null;
        }

        StartEnding();
    }

    void StartEnding()
    {
        Debug.Log("엔딩 시작");

        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.FadeOut(() =>
            {
                if (TalkManager.Instance != null &&
                    endingNarration != null)
                {
                    TalkManager.Instance.StartDialogue(
                        endingNarration,
                        OnNarrationFinished
                    );
                }
            });
        }
    }

    void OnNarrationFinished()
    {
        Debug.Log("나레이션 종료");

        // 여기서 크레딧 시작
        // CreditManager.Instance.StartCredit();
    }
}