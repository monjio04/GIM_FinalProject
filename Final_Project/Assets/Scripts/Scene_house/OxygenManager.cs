using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OxygenManager : MonoBehaviour
{
    public static OxygenManager Instance;

    [Header("산소 설정")]
    public int currentO2 = 100;        
    public bool isO2Active = false;    

    [Header("UI 연결")]
    public GameObject o2UIPanel;       // ★ 신규 추가: O2 UI 전체를 끄고 켤 부모 오브젝트
    public TextMeshProUGUI o2Text;     
    public Slider o2GaugeSlider;       

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // ★ 신규 추가: 게임 시작 시 일단 UI를 숨겨둡니다.
    void Start()
    {
        if (o2UIPanel != null)
        {
            o2UIPanel.SetActive(false);
        }
    }

    public void ActivateOxygen()
    {
        isO2Active = true;             
        currentO2 = 100;               
        
        // ★ 장비를 획득하면 UI를 화면에 나타나게 합니다.
        if (o2UIPanel != null)
        {
            o2UIPanel.SetActive(true);
        }

        UpdateUI();
        Debug.Log("산소통 장착 완료. 산소 측정 시작.");
    }

    public void DecreaseOxygen(int amount)
    {
        if (!isO2Active) return;

        currentO2 -= amount;
        
        if (currentO2 < 0) currentO2 = 0;

        Debug.Log($"산소 감소! 현재 산소량: {currentO2}");
        
        UpdateUI();
        CheckOxygenEffects();
    }

    void UpdateUI()
    {
        if (o2Text != null)
            o2Text.text = currentO2.ToString();
            
        if (o2GaugeSlider != null)
            o2GaugeSlider.value = currentO2 / 100f; 
    }

    void CheckOxygenEffects()
    {
        if (currentO2 == 50)
        {
            Debug.Log("[시스템] 산소 50 - 이동속도 10% 감소");
        }
        else if (currentO2 == 30)
        {
            Debug.Log("[시스템] 산소 30 - 게이지 주황색, 화면 붉은 맥박 연출, 이동속도 추가 감소");
        }
        else if (currentO2 == 10)
        {
            Debug.Log("[시스템] 산소 10 - 게이지 빨간색, 잔량 경보음(삐삐삐) 시작, 비네팅 켜짐");
        }
        else if (currentO2 == 0)
        {
            Debug.Log("[시스템] 산소 0 - 사망 / 건물 붕괴 이벤트 발동");
        }
    }
}