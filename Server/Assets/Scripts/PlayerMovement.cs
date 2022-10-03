using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private float gravity;
    [SerializeField] private float movementSpeed;
    [SerializeField] private float jumpHeight;

    private float gravityAcceleration;
    private float moveSpeed;
    public float maxSpeed;
    private float jumpPower;
    private bool isJumping;

    private bool[] inputs;
    private float yVelocity;

    Rigidbody2D rigid;

    private void OnValidate()
    {
        if (player == null)
            player = GetComponent<Player>();
        if(rigid == null)
            rigid = GetComponent<Rigidbody2D>();
        Initialize();
    }

    private void Start()
    {
        Initialize();
        inputs = new bool[3];
    }

    private void FixedUpdate()
    {
        if (inputs[0])
            rigid.AddForce(Vector2.right * -1, ForceMode2D.Impulse);
        if (inputs[1])
            rigid.AddForce(Vector2.right, ForceMode2D.Impulse);
        if (inputs[2])
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);

        if (rigid.velocity.x > maxSpeed) //Right Max Speed
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        else if (rigid.velocity.x < (-1) * maxSpeed) //Left Max Speed
            rigid.velocity = new Vector2(maxSpeed * (-1), rigid.velocity.y);
        SendMovement();
    }

    private void Initialize()
    {
        gravityAcceleration = gravity * Time.fixedDeltaTime * Time.fixedDeltaTime;
        moveSpeed = movementSpeed * Time.fixedDeltaTime;
        jumpPower = Mathf.Sqrt(jumpHeight * -2f * gravityAcceleration);
    }

    public void SetInput(bool[] _inputs)
    {
        inputs = _inputs;
    }

    private void SendMovement()
    {
        Message message = Message.Create(MessageSendMode.unreliable, ServerToClientId.playerMovement);
        message.AddUShort(player.id);
        message.AddVector3(transform.position);
        NetworkManager.Singleton.Server.SendToAll(message);
    }
}
