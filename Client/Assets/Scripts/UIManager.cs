using RiptideNetworking;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private static UIManager _singleton;

    public static UIManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(UIManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Singleton = this;
    }

    [Header("Connect")]
    [SerializeField] GameObject LoginUI;
    [SerializeField] InputField UsernameField;

    [Header("UI Prefab")]
    [SerializeField] GameObject LocalUI;
    [SerializeField] GameObject MultiUI;

    public GameObject RtButton;
    public Text UiStage;

    public void GameStartClicked()
    {
        UsernameField.interactable = false;
        LoginUI.SetActive(false);

        NetworkManager.Singleton.Connect();
    }

    public void BackToMain()
    {
        UsernameField.interactable = true;
        LoginUI.SetActive(true);
    }

    public void ActiveRtButton(string innerText)
    {
        Text btnText = RtButton.GetComponentInChildren<Text>();
        btnText.text = innerText;
        RtButton.SetActive(true);
    }

    public void SetHealthUI(bool isLocal, ushort health)
    {
        if (isLocal)
        {
            Image[] localHealths = LocalUI.GetComponentsInChildren<Image>();
            localHealths[health].color = new Color(1, 1, 1, 0.2f);
        } else
        {
            Image[] multiHealths = MultiUI.GetComponentsInChildren<Image>();
            multiHealths[2 - health].color = new Color(1, 1, 1, 0.2f);
        }
    }

    public void SetPoint(bool isLocal, int _point)
    {
        if (isLocal)
        {
            Text localPoint = LocalUI.GetComponentInChildren<Text>();
            localPoint.text = _point.ToString();
        }
        else
        {
            Text multiPoint = MultiUI.GetComponentInChildren<Text>();
            multiPoint.text = _point.ToString();
        }
    }

    public void SetStageText(int stageIndex)
    {
        UiStage.text = "Stage " + (stageIndex + 1);
    }

    public void SendName()
    {
        // unreliable은 lost가 생겨도 괜찮은 경우 사용
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.username);
        message.AddString(UsernameField.text);
        NetworkManager.Singleton.Client.Send(message);
    }
}
