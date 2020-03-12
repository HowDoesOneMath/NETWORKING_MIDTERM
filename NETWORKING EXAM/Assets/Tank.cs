using UnityEngine;
using System.Collections;

public class Tank : MonoBehaviour
{
	public LayerMask collisionMask;
    public float timeDelay = 0f;
    float timestep = 0.02f;
    float currentDelay = 0;
	public float speed = 5.0f;
	public float rotate = 10.0f;
	public GameObject bullet;
	public GameObject firePos;
	private GameObject newBullet;
    public float defaultDelay = 0.033f;
    public bool isYou = true;

    private void Update()
    {
        if (isYou)
        {
            RaycastHit hit;

            if (!Physics.SphereCast(transform.position, 1f, transform.forward, out hit, Time.deltaTime * speed + .1f, collisionMask))
            {
                if (Input.GetKey(KeyCode.W))
                {
                    transform.position += transform.forward * Time.deltaTime * speed;
                }
            }
            if (!Physics.SphereCast(transform.position, 1f, -transform.forward, out hit, Time.deltaTime * speed + .1f, collisionMask))
            {
                if (Input.GetKey(KeyCode.S))
                {
                    transform.position -= transform.forward * Time.deltaTime * speed;
                }
            }

            if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
                transform.Rotate(Vector3.up * Time.deltaTime * rotate);

            if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
                transform.Rotate(Vector3.down * Time.deltaTime * rotate);
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                fire();
            }

            if (Input.GetKeyUp(KeyCode.LeftBracket))
            {
                timeDelay += timestep;
            }
            if (Input.GetKeyUp(KeyCode.RightBracket))
            {
                timeDelay = Mathf.Max(timeDelay - timestep, 0f);
            }
            if (Input.GetKeyUp(KeyCode.Backslash))
            {
                timeDelay = 0f;
            }

            currentDelay -= Time.deltaTime;

            if (currentDelay <= 0f)
            {
                NetworkingManager.SendPosition(transform.position, transform.rotation.eulerAngles.y);
                if (timeDelay + defaultDelay == 0)
                {
                    currentDelay = 0;
                }
                else
                {
                    while (currentDelay <= 0)
                    {
                        currentDelay += (timeDelay + defaultDelay);
                    }
                }
            }
        }
    }

	void fire()
	{
		if (newBullet == null)
		{
			newBullet = Instantiate(bullet, firePos.transform.position, Quaternion.LookRotation(this.transform.forward));
            newBullet.GetComponent<Bullet>().B_ID = NetworkingManager.MY_ID;
			NetworkingManager.SendBullet(firePos.transform.position, this.transform.forward);
		}
	}
	public void die()
	{
		Destroy(this.gameObject);
	}
}