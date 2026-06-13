using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InvestigationUI : MonoBehaviour
{
    public static InvestigationUI Instance;

    [Header("전체 UI")]
    public GameObject root;

    [Header("이미지 조사 패널")]
    public GameObject imageRoot;
    public TextMeshProUGUI imageTitleText;
    public TextMeshProUGUI imageContentText;
    public Image investigationImage;

    [Header("텍스트 조사 패널")]
    public GameObject textRoot;
    public TextMeshProUGUI textTitleText;
    public TextMeshProUGUI textContentText;

    [Header("위령비 전용 패널")]
    public GameObject memorialRoot;

    public TextMeshProUGUI memorialTitleText;
    public TextMeshProUGUI memorialNamesText;

    public bool IsOpen => root.activeSelf;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        root.SetActive(false);

        imageRoot.SetActive(false);
        textRoot.SetActive(false);

        if (memorialRoot != null)
            memorialRoot.SetActive(false);
    }

    // ==========================
    // 이미지 있는 조사물
    // ==========================
    public void ShowImage(InvestigationData data)
    {
        root.SetActive(true);

        imageRoot.SetActive(true);
        textRoot.SetActive(false);

        imageTitleText.text = data.title;
        imageContentText.text = data.content;

        investigationImage.sprite = data.image;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // ==========================
    // 텍스트만 있는 조사물
    // ==========================
    public void ShowText(InvestigationTextData data)
    {
        root.SetActive(true);

        imageRoot.SetActive(false);
        textRoot.SetActive(true);

        textTitleText.text = data.title;
        textContentText.text = data.content;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ShowMemorial()
    {
        root.SetActive(true);

        imageRoot.SetActive(false);
        textRoot.SetActive(false);

        memorialRoot.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Hide()
    {
        root.SetActive(false);

        imageRoot.SetActive(false);
        textRoot.SetActive(false);
        memorialRoot.SetActive(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}