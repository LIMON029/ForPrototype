using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> players = new Dictionary<ushort, Player>();

    private static ushort maxHealth = 3;
    public CapsuleCollider2D capsuleCollider;
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;

    public ushort id { get; private set; }
    public string username { get; private set; }
    public bool isLocal { get; private set; }

    public int stagePoint { get; private set; }
    public int totalPoint { get; private set; }

    public ushort health { get; private set; }

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    private void OnDestroy()
    {
        players.Remove(id);
    }

    public static void Spawn(ushort _id, string _username, Vector2 _position)
    {
        Player _player;
        if(_id == NetworkManager.Singleton.Client.Id)
        {
            _player = Instantiate(GameManager.Singleton.LocalPlayerPrefab, _position, Quaternion.identity).GetComponent<Player>();
            _player.isLocal = true;
        }
        else
        {
            _player = Instantiate(GameManager.Singleton.PlayerPrefab, _position, Quaternion.identity).GetComponent<Player>();
            _player.isLocal = false;
        }

        _player.name = $"Player {_id} ({(string.IsNullOrEmpty(_username) ? "Guest" : _username)})";
        _player.id = _id;
        _player.username = string.IsNullOrEmpty(_username) ? $"Guest {_id}" : _username;
        _player.health = maxHealth;
        players.Add(_id, _player);
    }

    public void Move(Vector2 newPosition)
    {
        transform.position = newPosition;
    }

    public void OnDie()
    {
        //sound
        GameManager.Singleton.Playsound("DIE");
        //Sprite Alpha
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0.5f);

        //Sprite flipY
        spriteRenderer.flipY = true;

        //Collider Disable
        capsuleCollider.enabled = false;

        //Die Effect Jump
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);

        //result ui
        Debug.Log("You Died");

        UIManager.Singleton.ActiveRtButton("Retry?");
    }


    #region Messages
    [MessageHandler((ushort)ServerToClientId.playerSpawned)]
    private static void SpawnPlayer(Message message)
    {
        Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
    }

    // 서버에서 움직임을 관리하는 경우
    [MessageHandler((ushort)ServerToClientId.playerMovement)]
    private static void PlayerMovement(Message message)
    {
        if (players.TryGetValue(message.GetUShort(), out Player player))
            player.Move(message.GetVector3());
    }

    [MessageHandler((ushort)ServerToClientId.playerHealth)]
    private static void SetHealth(Message message)
    {
        if (players.TryGetValue(message.GetUShort(), out Player localPlayer))
        {
            localPlayer.health = message.GetUShort();
            UIManager.Singleton.SetHealthUI(true, localPlayer.health);
        }

        if (players.TryGetValue(message.GetUShort(), out Player player))
        {
            player.health = message.GetUShort();
            UIManager.Singleton.SetHealthUI(false, player.health);
        }
    }

    [MessageHandler((ushort)ServerToClientId.playerDied)]
    private static void PlayerDied(Message message)
    {
        if (players.TryGetValue(message.GetUShort(), out Player player))
            player.OnDie();
    }

    [MessageHandler((ushort)ServerToClientId.playerPoint)]
    private static void SetPoint(Message message)
    {
        if (players.TryGetValue(message.GetUShort(), out Player localPlayer))
        {
            localPlayer.stagePoint = message.GetInt();
            localPlayer.totalPoint = message.GetInt();
            UIManager.Singleton.SetPoint(true, localPlayer.totalPoint);
        }

        if (players.TryGetValue(message.GetUShort(), out Player player))
        {
            player.stagePoint = message.GetInt();
            player.totalPoint = message.GetInt();
            UIManager.Singleton.SetPoint(false, player.totalPoint);
        }
    }
    #endregion
}
