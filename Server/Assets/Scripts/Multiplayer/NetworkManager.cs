using RiptideNetworking;
using RiptideNetworking.Utils;
using UnityEngine;

public enum ClientToServerId : ushort
{
    username = 1,
    inputs,
    healthDown,
    getPoint
}

public enum ServerToClientId : ushort
{
    playerSpawned = 1,
    playerMovement,
    playerHealth,
    playerDied,
    playerPoint
}

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager _singleton;
    public static NetworkManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Singleton = this;
    }

    public Server Server { get; private set; }

    [SerializeField] private ushort port;
    [SerializeField] private ushort maxClientCount;

    private void Start()
    {
        Application.targetFrameRate = 60;

        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Server = new Server();
        Server.Start(port, maxClientCount);
        Server.ClientDisconnected += PlayerLeft;
    }

    private void FixedUpdate()
    {
        Server.Tick();
    }


    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        /*
        if (Player.players.TryGetValue(e.Id, out Player player))
            Destroy(player.gameObject);
        */
    }
}
