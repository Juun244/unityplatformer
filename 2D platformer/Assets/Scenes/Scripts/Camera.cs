using System.Collections;

using System.Collections.Generic;

using UnityEngine;



public class Camera : MonoBehaviour
{



	public float smoothTimeX, smoothTimeY;

	public Vector2 velocity;

	public GameObject player;

	public Vector2 minPos, maxPos;

	public bool bound;





	// ĳ���� �ʱ�ȭ

	void Start()
	{

		player = GameObject.FindGameObjectWithTag("Player");

	}



	// ĳ������ ���� ���� ī�޶� �̵��ϵ��� �ϴ� �޼���

	void FixedUpdate()
	{

		float posX = Mathf.SmoothDamp(transform.position.x, player.transform.position.x, ref velocity.x, smoothTimeX);

		// Mathf.SmoothDamp�� õõ�� ���� ������Ű�� �޼����̴�.

		float posY = Mathf.SmoothDamp(transform.position.y, player.transform.position.y, ref velocity.y, smoothTimeY);

		// ī�޷� �̵�

		transform.position = new Vector3(posX, posY, transform.position.z);



		if (bound)
		{

			//Mathf.Clamp(���簪, �ִ밪, �ּҰ�);  ���簪�� �ִ밪������ ��ȯ���ְ� �ּҰ����� ������ �� �ּҰ������� ��ȯ�մϴ�.

			transform.position = new Vector3(Mathf.Clamp(transform.position.x, minPos.x, maxPos.x),

				Mathf.Clamp(transform.position.y, minPos.y, maxPos.y),

				Mathf.Clamp(transform.position.z, transform.position.z, transform.position.z)

			);

		}

	}

}
