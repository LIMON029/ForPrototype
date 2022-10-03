using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Enemy_Move : MonoBehaviour
{
    // Start is called before the first frame update
    public float maxSpeed;
    public int nextMove;
    Rigidbody2D rigid;
    Animator anim;
    SpriteRenderer spriteRenderer;
    CapsuleCollider2D capsuleCollider;
    bool live;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        live = true;
        Move_direct();

        Invoke("Move_direct", 5);
    }

    // Update is called once per frame
    void FixedUpdate()
    {   //only if enemy alive
        if (live) {
            rigid.velocity = new Vector2(nextMove, rigid.velocity.y);

            Vector2 frontVec = new Vector2(rigid.position.x + nextMove * 0.2f, rigid.position.y);

            Debug.DrawRay(frontVec, Vector3.down, new Color(1, 0, 0));
            RaycastHit2D rayHit = Physics2D.Raycast(frontVec, Vector3.down, 1, LayerMask.GetMask("Platform"));
            if (rayHit.collider == null)
            {
                Turn();
            }
        }
        
    }

    void Move_direct()
    {
        if (!live)
            return;
        nextMove = Random.Range(-1, 2);

        //float nextTime = Random.Range(1f, 5f);

        anim.SetInteger("WalkSpeed", nextMove);

        if (nextMove != 0)
            spriteRenderer.flipX = nextMove > 0;


        Invoke("Move_direct", 5/*nextTime*/);

    }
    void Turn()
    {
        nextMove *= -1;
        spriteRenderer.flipX = nextMove == 1;

        CancelInvoke();
        Invoke("Move_direct", 2);
    }

    public void OnDameged()
    {
        //exit move function
        live = false;

        //Sprite Alpha
        spriteRenderer.color = new Color(1, 1, 1, 0.5f);

        //Sprite flipY
        spriteRenderer.flipY = true;

        //Collider Disable
        capsuleCollider.enabled = false;

        //Die Effect Jump
        rigid.AddForce(Vector2.up * 5, ForceMode2D.Impulse);

        //Destroy
        DeActive();
    }

    void DeActive()
    {
        Destroy(gameObject,2);
    }
}
