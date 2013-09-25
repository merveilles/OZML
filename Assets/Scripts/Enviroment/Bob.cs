using UnityEngine;
using System.Collections;

public class Bob : MonoBehaviour 
{
	public float Speed = 2.0f;
	public float Amount = 1.0f;
	
	void Start()
	{
		Speed = Random.value * Speed;
		Random.seed++;
	}
	
	void Update() 
	{
		transform.Translate( Vector3.up * Mathf.Sin( Time.time * Speed ) * Amount );
	}
}
