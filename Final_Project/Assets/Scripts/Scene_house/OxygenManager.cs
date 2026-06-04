using UnityEngine;
using UnityEngine.UI;

public class OxygenManager : MonoBehaviour
{
    // 어디서든 이 매니저를 쉽게 부를 수 있게 싱글톤 패턴 적용
    public static OxygenManager Instance;

    [Header("산소 설정")]
    public int currentO2 = 100;        // 현재 산소량 (100에서 시작)
    public bool isO2Active = false;    // 산소통 획득 전에는 감소하지 않음

    [Header("UI 연결")]
    public Text o2Text;                // O2 수치를 텍스트로 보여줄 UI
    public Image o2GaugeImage;         // O2 수치를 시각적으로 보여줄 게이지 바 UI

    void Awake()
    {
        // 씬에 매니저가 하나만 존재하도록 설정
        if (Instance == null) Instance = this;
    }

    // 1. 산소통 획득 시 호출할 함수 (Scene 3 현장 진입 시 사용)
    public void ActivateOxygen()
    {
        isO2Active = true;             // 산소 소모 시작
        currentO2 = 100;               // 초기값 100 세팅
        UpdateUI();
        Debug.Log("산소통 장착 완료. 산소 측정 시작.");
    }

    // 2. 이벤트 발생 시 산소를 감소시킬 함수 (예: 화재진압, 수색 등)
    public void DecreaseOxygen(int amount)
    {
        // 산소통을 안 먹었으면 깎이지 않음
        if (!isO2Active) return;

        currentO2 -= amount;
        
        // 산소가 0 밑으로 떨어지지 않게 고정
        if (currentO2 < 0) currentO2 = 0;

        Debug.Log($"산소 감소! 현재 산소량: {currentO2}");
        
        UpdateUI();
        CheckOxygenEffects();
    }

    // 3. UI에 현재 산소량 반영
    void UpdateUI()
    {
        if (o2Text != null)
            o2Text.text = currentO2.ToString();
            
        if (o2GaugeImage != null)
            o2GaugeImage.fillAmount = currentO2 / 100f; 
    }

    // 4. 산소 수치에 따른 패널티 및 연출 체크 (APPENDIX B 기준)
    void CheckOxygenEffects()
    {
        if (currentO2 == 50)
        {
            // 이동속도 10% 감소 시작
            Debug.Log("[시스템] 산소 50 - 이동속도 10% 감소");
        }
        else if (currentO2 == 30)
        {
            // 게이지 주황색 변경 / 이동속도 추가 감소
            if (o2GaugeImage != null) o2GaugeImage.color = new Color(1f, 0.5f, 0f); // 주황색
            Debug.Log("[시스템] 산소 30 - 게이지 주황색, 화면 붉은 맥박 연출, 이동속도 추가 감소");
        }
        else if (currentO2 == 10)
        {
            // 게이지 빨간색 / 경보음 / 이동속도 추가 감소
            if (o2GaugeImage != null) o2GaugeImage.color = Color.red; // 빨간색
            Debug.Log("[시스템] 산소 10 - 게이지 빨간색, 잔량 경보음(삐삐삐) 시작, 비네팅 켜짐");
        }
        else if (currentO2 == 0)
        {
            // 즉사 / 블랙아웃 연출
            Debug.Log("[시스템] 산소 0 - 사망 / 건물 붕괴 이벤트 발동");
        }
    }
}