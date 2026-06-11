using TMPro;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance;

    [Header("화면 안내 텍스트 TMP")]
    public TextMeshProUGUI promptText;

    [Header("오브젝트 머리 위로 띄울 높이 오프셋")]
    public Vector3 worldOffset = new Vector3(0, 0.6f, 0); // Y축으로 0.6미터 위에 띄웁니다.

    private Transform currentTarget; // 현재 바라보고 있는 오브젝트의 위치 정보

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Hide();
    }

    private void Update()
    {
        if (TalkManager.Instance != null && TalkManager.Instance.IsTalking)
        {
            if (promptText.gameObject.activeSelf) 
            {
                Hide();
            }
            return; // 대화 중일 때는 아래 위치 갱신 로직을 타지 않음
        }

        // 바라보는 타겟이 있고, UI가 켜져 있을 때만 매 프레임 위치를 갱신합니다.
        if (currentTarget != null && promptText.gameObject.activeSelf)
        {
            // 1. 오브젝트의 실제 3D 위치에 오프셋(높이)을 더합니다.
            Vector3 worldPos = currentTarget.position + worldOffset;

            // 2. [핵심] 3D 좌표를 2D 모니터 화면 좌표로 변환합니다.
            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

            // 3. 카메라 뒤편에 있는 물체의 UI가 화면에 뒤집혀 그려지는 버그 방지 (Z축이 0보다 커야 카메라 앞임)
            if (screenPos.z > 0)
            {
                promptText.transform.position = screenPos;
            }
        }

        
    }

    // ★ 수정: 이제 텍스트 내용뿐만 아니라, 타겟 오브젝트의 위치(Transform)도 함께 받습니다.
    public void Show(string text, Transform target)
    {
        if (promptText == null) return;
        
        promptText.text = text;
        currentTarget = target; // 타겟 등록
        promptText.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (promptText == null) return;
        
        currentTarget = null; // 타겟 해제
        promptText.gameObject.SetActive(false);
    }
}