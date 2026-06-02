using System.Collections;
using TMPro;
using UnityEngine;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance;

    [Header("우상단 상시 표시 UI (컴포넌트 분리 완료)")]
    public TextMeshProUGUI statusText;       // [진행중] / [완료] 텍스트만 담당
    public TextMeshProUGUI titleText;        // 퀘스트 제목 텍스트만 담당
    public TextMeshProUGUI descriptionText;  // 퀘스트 설명 텍스트만 담당

    [Header("화면 중앙 대형 팝업 UI")]
    public GameObject questPopUpPanel;       
    public CanvasGroup popUpCanvasGroup;      
    public TextMeshProUGUI popUpIDText;       
    public TextMeshProUGUI popUpTitleText;    

    [Header("연출 시간 설정")]
    public float fadeDuration = 0.5f;         
    public float popUpDisplayTime = 2.0f;     

    // ★ 추가: 현재 팝업 연출이 돌고 있는지 외부에 알려줄 신호 변수
    private bool isPopUpActive = false;
    public bool IsPopUpActive => isPopUpActive;

    private QuestData currentQuest;
    private bool isCompleted;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (questPopUpPanel != null) 
            questPopUpPanel.SetActive(false);
    }

    public void StartQuest(QuestData quest)
    {
        currentQuest = quest;
        isCompleted = false;

        UpdateUI();
        StartCoroutine(ShowQuestPopUpRoutine(quest));
    }

    IEnumerator ShowQuestPopUpRoutine(QuestData quest)
    {
        if (questPopUpPanel == null || popUpCanvasGroup == null) yield break;

        // ★ 추가: 연출이 시작되었으므로 플레이어를 묶기 위해 true로 변경
        isPopUpActive = true; 

        if (popUpIDText != null) popUpIDText.text = $"{quest.questID}";
        if (popUpTitleText != null) popUpTitleText.text = quest.title;

        popUpCanvasGroup.alpha = 0f;
        questPopUpPanel.SetActive(true);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            popUpCanvasGroup.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        popUpCanvasGroup.alpha = 1f; 

        yield return new WaitForSeconds(popUpDisplayTime);

        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            popUpCanvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / fadeDuration));
            yield return null;
        }
        popUpCanvasGroup.alpha = 0f; 

        questPopUpPanel.SetActive(false);

        // ★ 추가: 연출이 완벽히 끝났으므로 플레이어를 풀어주기 위해 false로 변경
        isPopUpActive = false; 
    }

    public void CompleteQuest()
    {
        isCompleted = true;
        UpdateUI();
    }

    // 각각의 텍스트 컴포넌트에 알맞은 값만 순수하게 전달합니다.
    void UpdateUI()
    {
        if (currentQuest == null) return;

        // 1. 상태 텍스트 세팅
        if (statusText != null)
        {
            statusText.text = isCompleted ? "완료" : "진행중"; // 오타 수정: 잔행중 -> 진행중
            statusText.color = isCompleted ? Color.green : Color.yellow;
        }

        // 2. 제목 세팅
        if (titleText != null)
        {
            titleText.text = currentQuest.title;
        }

        // 3. 설명 세팅
        if (descriptionText != null)
        {
            descriptionText.text = currentQuest.description;
        }
    }
}