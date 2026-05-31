using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public static HealthUI Instance;

    [Header("UI Elements")]
    public Slider healthSlider;         // 유니티 UI Slider 컴포넌트
    public TextMeshProUGUI healthText;  // "100 / 100" 형태로 보여줄 텍스트

    private void Awake()
    {
        Instance = this;
    }

    // 플레이어 체력이 바뀔 때마다 실행되어 UI 게이지와 텍스트를 갱신하는 함수
    public void SetHealth(float currentHP, float maxHP)
    {
        if (healthSlider != null)
        {
            // 🚨 [수정] currentHP를 그대로 넣지 않고, maxHP로 나누어 0 ~ 1 사이의 비율로 변환합니다.
            // 예: 체력이 100/100 이면 1이 들어가고, 50/100 이면 0.5가 들어가서 슬라이더가 완벽하게 인지합니다.
            healthSlider.value = currentHP / maxHP; 
        }

        if (healthText != null)
        {
            // 숫자는 플레이어가 읽기 편하게 정수 형태로 그대로 출력합니다.
            healthText.text = $"{Mathf.RoundToInt(currentHP)}";
        }
    }
}