using UnityEngine;
using System.Collections;

public class Tank : MonoBehaviour
{
	public float speed = 5.0f;
	public float rotate = 10.0f;

	private void Update()
	{

		if (Input.GetKey(KeyCode.W))
			transform.position += transform.forward * Time.deltaTime * speed;

		if (Input.GetKey(KeyCode.S))
			transform.position -= transform.forward * Time.deltaTime * speed;

		if (Input.GetKey(KeyCode.D))
			transform.Rotate(Vector3.up * Time.deltaTime * rotate);

		if (Input.GetKey(KeyCode.A))
			transform.Rotate(Vector3.down * Time.deltaTime * rotate);

	}
}