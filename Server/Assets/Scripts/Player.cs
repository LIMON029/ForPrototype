using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Dictionary<int, Player> players = new Dictionary<int, Player>();

    private Vector3 spawnPoint = new Vector3(-6f, -2.41f, -1.117188f);

    public ushort id { get; private set; }
    public string username { get; private set; }
    public ushort health { get; private set; }
    public PlayerMovement Movement => movement;

    [SerializeField] private PlayerMovement movement;

    private void OnDestroy()
    {
        players.Remove(id);
    }

    public static void Spawn(ushort _id, string _username)
    {
        foreach (Player otherPlayer in players.Values)
            otherPlayer.SendSpawned(_id);

        Player _player = Instantiate(GameManager.Singleton.PlayerPrefab, new Vector3(0f, 1f, 0f), Quaternion.identity).GetComponent<Player>();
        _player.name = $"Player {_id} ({(string.IsNullOrEmpty(_username) ? "Guest" : _username)})";
        _player.id = _id;
        _player.username = string.IsNullOrEmpty(_username) ? $"Guest {_id}" : _username;

        _player.SendSpawned();
        players.Add(_id, _player);
    }

    public void HealthDown()
    {
        health--;
    }

    #region Messages
    private Message AddSpawnData(Message message)
    {
        float xPos = Random.Range(-20f, 0);
        spawnPoint.x = xPos;
        message.AddUShort(id);
        message.AddString(username);
        message.AddVector3(spawnPoint);
        return message;
    }
    private void SendSpawned()
    {
        NetworkManager.Singleton.Server.SendToAll(AddSpawnData(Message.Create(MessageSendMode.reliable, ServerToClientId.playerSpawned)));
    }

    private void SendSpawned(ushort toClientId)
    {
        NetworkManager.Singleton.Server.Send(AddSpawnData(Message.Create(MessageSendMode.reliable, ServerToClientId.playerSpawned)), toClientId);
    }

    private void SendDied()
    {
        // 추가 작업 필요

        NetworkManager.Singleton.Server.SendToAll(Message.Create(MessageSendMode.reliable, ServerToClientId.playerDied));
    }

    [MessageHandler((ushort)ClientToServerId.username)]
    private static void Username(ushort _fromClientId, Message _message)
    {
        Spawn(_fromClientId, _message.GetString());
    }

    [MessageHandler((ushort)ClientToServerId.inputs)]
    private static void Input(ushort _fromClientId, Message _message)
    {
        if (players.TryGetValue(_fromClientId, out Player player))
            player.Movement.SetInput(_message.GetBools(3));
    }

    [MessageHandler((ushort)ClientToServerId.healthDown)]
    private static void HealthDown(ushort _fromClientId, Message _message)
    {
        if (players.TryGetValue(_fromClientId, out Player player))
        {
            if (_message.GetBool())
            {
                player.HealthDown();
            }
        }
    }
    #endregion
}
