using System.Collections;
using UnityEngine;

public class InvestigationManager : MonoBehaviour
{
    public static InvestigationManager Instance;

    public int targetCount = 4;

    private int currentCount = 0;

    private bool endingStarted = false;

    public DialogueData endingNarration;

    [Header("엔딩 연출")]
    public CanvasGroup logoCanvasGroup;

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
                // 나레이션 시작 전 대사창 활성화 보장
                if (TalkManager.Instance != null && endingNarration != null)
                {
                    // 대사창 UI를 확실히 켭니다
                    if (TalkManager.Instance.talkUI != null)
                    {
                        TalkManager.Instance.talkUI.SetActive(true);
                    }

                    // 나레이션 시작
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
            Debug.Log("나레이션 종료 - 로고 페이드인 시작");
            
            // 1. 대사창은 끕니다.
            if (TalkManager.Instance != null && TalkManager.Instance.talkUI != null)
            {
                TalkManager.Instance.talkUI.SetActive(false);
            }

            // 2. 로고 페이드인 코루틴 시작
            if (logoCanvasGroup != null)
            {
                StartCoroutine(FadeInLogo(logoCanvasGroup, 2.0f)); // 2초간 페이드인
            }
        }

        IEnumerator FadeInLogo(CanvasGroup canvasGroup, float duration)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.gameObject.SetActive(true);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }
}