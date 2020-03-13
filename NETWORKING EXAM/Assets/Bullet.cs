using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int B_ID = -1;
	public LayerMask collisionMask;
	private int bounceCount;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

		transform.Translate(Vector3.forward * Time.deltaTime * 10);


		Ray projectileRay = new Ray(transform.position, transform.forward);
		RaycastHit hit;
		Debug.DrawRay(transform.position, transform.forward);
        if (Physics.Raycast(projectileRay, out hit, Time.deltaTime * 10 + .1f, collisionMask))
        {
            Debug.Log("HIT A THING: " + hit.transform.gameObject.name);

            if (bounceCount <= 1)
            {
                Vector3 reflection = Vector3.Reflect(projectileRay.direction, hit.normal);
                float rot = 90 - (Mathf.Atan2(reflection.z, reflection.x) * Mathf.Rad2Deg);
                transform.eulerAngles = new Vector3(0, rot, 0);
                bounceCount++;
            }
            else
            {
                Destroy(this.gameObject);
            }

            Tank tnk = hit.transform.gameObject.GetComponentInParent<Tank>();

            if (tnk != null)
            {
                //Debug.Log("HIT!");
                if (B_ID == NetworkingManager.MY_ID)
                {
                    if (tnk.isYou)
                    {
                        NetworkingManager.SendScore(1 - NetworkingManager.MY_ID);
                    }
                    else
                    {
                        NetworkingManager.SendScore(NetworkingManager.MY_ID);
                    }
                }
                //hit.transform.gameObject.GetComponentInParent<Tank>().die();
                Destroy(this.gameObject);
            }

        }
		
    }
}
