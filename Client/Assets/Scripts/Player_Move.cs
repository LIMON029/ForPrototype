using RiptideNetworking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Move : MonoBehaviour
{
    public float maxSpeed;
    public float jumpPower;
    public int maxJump;
   
    int flip;
    bool[] inputs;

    public CapsuleCollider2D capsuleCollider;
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    Animator anim;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        flip = 1;
        capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    private void SetInputs(string motion)
    {
        switch (motion)
        {
            case "Left":
                inputs[0] = true;
                break;
            case "Right":
                inputs[1] = true;
                break;
            case "Jump":
                inputs[2] = true;
                break;
        }
    }
    
    private void Update()//단발적인 키 입력
    {
        //jump
        if (Input.GetButtonDown("Jump") && !anim.GetBool("isJumping"))
        {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
            anim.SetBool("isJumping", true);
            GameManager.Singleton.Playsound("JUMP");
            SetInputs("Jump");
        }
           
        //stop horizontal
        if (Input.GetButtonUp("Horizontal"))
        {
            //벡터의 단위(방향)을 구할 때 rigid.velocity.normalized
            rigid.velocity = new Vector2(rigid.velocity.normalized.x * 0.5f, rigid.velocity.y);
        }

        //flip player
        if (Input.GetButton("Horizontal"))
        {

            if(flip * Input.GetAxisRaw("Horizontal") == -1)
            {

                spriteRenderer.flipX = true;
                SetInputs("Left");
            }

            if (flip * Input.GetAxisRaw("Horizontal") == 1)
            {
                spriteRenderer.flipX = false;
                SetInputs("Right");
            }
        }

        //idle or walk animation activate
        if (Mathf.Abs(rigid.velocity.x) < 0.3)
        {
            anim.SetBool("isWalking", false);
        }
        else
            anim.SetBool("isWalking", true);
    }
    void FixedUpdate() //1초에 50번 정도,물리 기반
    {
        //Move by Key Control
        float h = Input.GetAxisRaw("Horizontal");

        rigid.AddForce(Vector2.right * h, ForceMode2D.Impulse);

        if (rigid.velocity.x > maxSpeed) //Right Max Speed
            rigid.velocity = new Vector2(maxSpeed, rigid.velocity.y);
        else if(rigid.velocity.x < (-1)*maxSpeed) //Left Max  Speed
            rigid.velocity = new Vector2(maxSpeed*(-1), rigid.velocity.y);
        
        //RayCast (Landing at platform)
        if (rigid.velocity.y < 0)
        {

            Debug.DrawRay(rigid.position, Vector3.down, new Color(1, 0, 0));
            RaycastHit2D rayHit = Physics2D.Raycast(rigid.position, Vector3.down, 2, LayerMask.GetMask("Platform"));
            if (rayHit.collider != null)
            {
                if (rayHit.distance <1f)
                {
                    //Debug.Log(rayHit.collider.name);
                    anim.SetBool("isJumping", false);
                }

            }

        }
        SendInputs();
        for (int i = 0; i < inputs.Length; i++)
            inputs[i] = false;
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Enemy" || collision.gameObject.tag == "Spike")
        {   
            if(collision.gameObject.tag == "Spike")
            {
                OnDameged(collision.transform.position);
                GameManager.Singleton.Playsound("DAMAGED");
            }

            //When player Attack
            else if (rigid.velocity.y < 0 && transform.position.y > collision.transform.position.y)
            {
                OnAttack(collision.transform);
                GameManager.Singleton.Playsound("ATTACK");

            }
            else
            {
                OnDameged(collision.transform.position);
                GameManager.Singleton.Playsound("DAMAGED");
            }
        }


    }

    void OnTriggerEnter2D(Collider2D collision)
    {
       
        if (collision.gameObject.tag == "Item")
        {
            //point
            //bool isBronze = collision.gameObject.name.Contains("Bronze");
            // GameManager.Singleton.stagePoint += 100;
            SendGetPoint(100);

            //Deactive Item
            GameManager.Singleton.Playsound("ITEM");
            collision.gameObject.SetActive(false);
            Destroy(collision.gameObject);
        }
        else if (collision.gameObject.tag == "Finish")
        {
            // Next stage
            GameManager.Singleton.NextStage();
            GameManager.Singleton.Playsound("FINISH");
        }
    }

    void OnAttack(Transform enemy)
    {
        //Point

        //Reaction Force
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);
        
        //Enemy Dead
        Enemy_Move enemy_Move = enemy.GetComponent<Enemy_Move>();
        enemy_Move.OnDameged();
    }

    void OnDameged(Vector2 targetPos)
    {
        GameManager.Singleton.Damaged();

        //Change Layer
        gameObject.layer = 11;

        //View Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.5f);

        //Reaction Force
        int dirc = transform.position.x - targetPos.x > 0 ? 1 : -1;
        rigid.AddForce(new Vector2(dirc, 1)*7, ForceMode2D.Impulse);


        //Animation
        anim.SetTrigger("isDamaged");

        Invoke("OffDameged",0.5f);
    }

    void OffDameged()
    {
        //Change Layer
        gameObject.layer = 10;

        //View Alpha
        spriteRenderer.color = new Color(1, 1, 1, 1);

    }

    public void VelocityZero()
    {
        rigid.velocity = Vector2.zero;
    }

    public void SendInputs()
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.inputs);
        message.AddBools(inputs, false);
        NetworkManager.Singleton.Client.Send(message);
    }

    public void SendGetPoint(int _point)
    {
        Message message = Message.Create(MessageSendMode.reliable, ClientToServerId.getPoint);
        message.AddInt(_point);
        NetworkManager.Singleton.Client.Send(message);
    }
}
