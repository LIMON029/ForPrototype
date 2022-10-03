using RiptideNetworking;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//점수, 스테이지 관리
public class GameManager : MonoBehaviour
{
    AudioSource audioSource;
    private static GameManager _singleton;
    public static GameManager Singleton
    {
        get => _singleton;
        set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(GameManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Singleton = this;
        audioSource = GetComponent<AudioSource>();
    }

    public GameObject LocalPlayerPrefab => localPlayerPrefab;
    public GameObject PlayerPrefab => playerPrefab;

    [Header("Prefabs")]
    [SerializeField] private GameObject localPlayerPrefab;
    [SerializeField] private GameObject playerPrefab;

    public int totalPoint;
    public int stagePoint;
    public int stageIndex;
    public int health;
    public Player_Move player;
    public GameObject[] Stages;
    
    public AudioClip audioFalling;
    public AudioClip audioJump;
    public AudioClip audioAttack;
    public AudioClip audioDamaged;
    public AudioClip audioItem;
    public AudioClip audioDie;
    public AudioClip audioFinish;

    private void Update()
    {
        UIManager.Singleton.SetStageText(stageIndex);
        audioSource = GetComponent<AudioSource>();
    }

    public void Playsound(string action)
    {
        switch (action)
        {
            case "JUMP":
                audioSource.clip = audioJump;
                break;
            case "ATTACK":
                audioSource.clip = audioAttack;
                break;
            case "DAMAGED":
                audioSource.clip = audioDamaged;
                break;
            case "ITEM":
                audioSource.clip = audioItem;
                break;
            case "DIE":
                audioSource.clip = audioDie;
                Invoke("freezeTime", 1f);
                break;
            case "FINISH":
                audioSource.clip = audioFinish;
                break;
            case "Falling":
                audioSource.clip = audioFalling;
                break;
        }
        audioSource.Play();
    }

    // Start is called before the first frame update
    public void NextStage()
    {
        //respone
        if (stageIndex < Stages.Length-1)
        {
            Stages[stageIndex].SetActive(false);
            stageIndex++;
            Stages[stageIndex].SetActive(true);
            PlayerRespon();

            UIManager.Singleton.SetStageText(stageIndex);
        }
        else
        {
            //Game Clear

            //Player Control Lock
            freezeTime();
            //Result UI
            Debug.Log("Clear!");
            //Restart Button UI
            UIManager.Singleton.ActiveRtButton("Clear!");
        }

        //cal point
        totalPoint += stagePoint;
        stagePoint = 0;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            //Player Respone
            if (health > 1)
            {
                Playsound("Falling");
                PlayerRespon();
            }

            //HP Down
            Damaged();
        }
            
    }

    void PlayerRespon()
    {
        player.transform.position = new Vector3(0, 0, -1);
        player.VelocityZero();

    }

    public void Restart()
    {
        restTimeScale();
        SceneManager.LoadScene(0);
    }

    public void freezeTime()
    {
        Time.timeScale = 0;
    }

    public void restTimeScale()
    {
        Time.timeScale = 1;
    }

    public void Damaged()
    {
        health--;
        Playsound("DAMAGED");

        SendStatus((ushort)ClientToServerId.healthDown);
    }

    public void SendStatus(ushort status)
    {
        Message message = Message.Create(MessageSendMode.reliable, status);
        // 추후 데미지 세분화할 경우 ushort로 보낼 예정
        message.AddBool(true);
        NetworkManager.Singleton.Client.Send(message);
    }

    public void SendPoint(int _point)
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.getPoint);
        message.AddInt(_point);
        NetworkManager.Singleton.Client.Send(message);
    }
}
