using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public float movePower = 1f;
    public float jumpPower = 1f;
    Animator animator;
    Rigidbody2D rib;
    Vector3 playermove;
    bool isJumping = false;
    bool isFalling = false;

    // Start is called before the first frame update
    void Start()
    {
        rib = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(isFalling);

        //움직임 애니메이션
        if (Input.GetAxisRaw("Horizontal") == 0)
        {
            animator.SetBool("isMoving", false);
        }
        else if (Input.GetAxisRaw("Horizontal") != 0)
        {
            animator.SetBool("isMoving", true);
        }

        //점프
        if (Input.GetButtonDown("Jump") && !animator.GetBool("isJumping") && !animator.GetBool("isFalling"))
        {
            isJumping = true;
            animator.SetTrigger("triggerJumping"); //점프 애니메이션
            animator.SetBool("isJumping", true);
        }
    }

    void Awake()
    {

    }

    void FixedUpdate()
    {
        Move();
        Jump();
    }

    void Move()
    {
        Vector3 moveVelocity = Vector3.zero;

        if (Input.GetAxisRaw("Horizontal") < 0)
        {
            moveVelocity = Vector3.left;

           transform.localScale = new Vector3(-1, 1, 1); //왼쪽으로 회전
        }
        else if (Input.GetAxisRaw("Horizontal") > 0)
        {
            moveVelocity = Vector3.right;

           transform.localScale = new Vector3(1, 1, 1); //오른쪽으로 회전
        }

        transform.position += moveVelocity * movePower * Time.deltaTime;
    }


    void Jump()
    {
        if (!isJumping) return;

        rib.velocity = Vector2.zero;

        Vector2 jumpVelocity = new Vector2(0, jumpPower);
        rib.AddForce(jumpVelocity, ForceMode2D.Impulse);

        isJumping = false;
    }


    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Attach : " + other.gameObject.layer);
        {
            if (other.gameObject.layer == 6 || other.gameObject.layer == 7) //그라운드 또는 몹에 닿았을 때
            {
                if (rib.velocity.y < 0) // 플레이어가 하강 중일때
                    animator.SetBool("isJumping", false);
            }




        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("Detach : " + other.gameObject.layer);
    }
}
