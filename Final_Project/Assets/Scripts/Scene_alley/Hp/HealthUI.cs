using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public static HealthUI Instance;

    [Header("UI Elements")]
    public Slider healthSlider;         // 유니티 UI Slider 컴포넌트 연결칸
    public TextMeshProUGUI healthText;  // HP 숫자 텍스트 연결칸

    private void Awake()
    {
        Instance = this;
    }

    public void SetHealth(float currentHP, float maxHP)
    {
        if (healthSlider != null)
        {
            // ★ 중요: 0 ~ 1 사이의 소수점 비율로 계산해서 슬라이더에 입력
            healthSlider.value = currentHP / maxHP; 
        }

        if (healthText != null)
        {
            // 정수로 깔끔하게 출력
            healthText.text = $"{Mathf.RoundToInt(currentHP)}";
        }
    }
}