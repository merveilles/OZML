using UnityEngine;
using System.Collections;
public class FlyCam : MonoBehaviour {

	public float Speed = 1.0f;
	
	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		float speed = Speed;
		if(Input.GetButton("Fire3"))
			speed = speed * 2.0f;

        transform.Translate(Input.GetAxis("Horizontal") * Time.deltaTime * speed,0 , Input.GetAxis("Vertical") * Time.deltaTime * speed);

	}
}