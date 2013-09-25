using UnityEngine;
using System.Collections;

public class CyanosisVibrator : MonoBehaviour {

	public Vector3 ShakeLimit = new Vector3(2, 1, 1);
	public float ShakeSpeed = 15;
	public float minDistance = 1;
	public float maxDistance = 1;

	private Vector3 originalrot = new Vector3(0, 0, 0);
	private Vector3 shakeOffset = new Vector3(0, 0, 0);
	private float multiplyer = 0.0f;

	// Use this for initialization
	void Start () {
                if(maxDistance == 0) { maxDistance = 1; Debug.LogError("ERROR: maxDistance is ZERO !!");}
		originalrot = transform.eulerAngles ;
	}

	// Update is called once per frame
	void Update () {
		if(ShakeLimit.x >0) shakeOffset.x = Mathf.PingPong(Time.time * ShakeSpeed, ShakeLimit.x*2) - ShakeLimit.x;
		if(ShakeLimit.y >0) shakeOffset.y = Mathf.PingPong(Time.time * ShakeSpeed, ShakeLimit.y*2) - ShakeLimit.y;
		if(ShakeLimit.z >0) shakeOffset.z = Mathf.PingPong(Time.time * ShakeSpeed, ShakeLimit.z*2) - ShakeLimit.z;

		//update angles
		float d = Vector3.Distance(transform.position, Camera.main.transform.position);
		multiplyer = (d / maxDistance) - (minDistance / maxDistance);
		multiplyer =Mathf.Clamp01(multiplyer);
		transform.eulerAngles  = originalrot + (shakeOffset * multiplyer);
	}
}