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

        //������ �ִϸ��̼�
        if (Input.GetAxisRaw("Horizontal") == 0)
        {
            animator.SetBool("isMoving", false);
        }
        else if (Input.GetAxisRaw("Horizontal") != 0)
        {
            animator.SetBool("isMoving", true);
        }

        //����
        if (Input.GetButtonDown("Jump") && !animator.GetBool("isJumping") && !animator.GetBool("isFalling"))
        {
            isJumping = true;
            animator.SetTrigger("triggerJumping"); //���� �ִϸ��̼�
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

           transform.localScale = new Vector3(-1, 1, 1); //�������� ȸ��
        }
        else if (Input.GetAxisRaw("Horizontal") > 0)
        {
            moveVelocity = Vector3.right;

           transform.localScale = new Vector3(1, 1, 1); //���������� ȸ��
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
            if (other.gameObject.layer == 6 || other.gameObject.layer == 7) //�׶��� �Ǵ� ���� ����� ��
            {
                if (rib.velocity.y < 0) // �÷��̾ �ϰ� ���϶�
                    animator.SetBool("isJumping", false);
            }




        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("Detach : " + other.gameObject.layer);
    }
}
