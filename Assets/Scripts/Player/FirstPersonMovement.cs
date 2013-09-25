using UnityEngine;
using System.Collections;

public class FirstPersonMovement : MonoBehaviour 
{
	public float Speed = 2.0f;
	public float Gravity = 9.0f;
	
	private CharacterController Controller;
	
	void Start() 
	{
		Controller = GetComponent<CharacterController>();
	}

	void Update() 
	{
		Vector3 desiredMovement = Vector3.zero;
		Vector3 movement = Vector3.zero;
		
		desiredMovement.x = Input.GetAxis( "Horizontal" );
		desiredMovement.z = Input.GetAxis( "Vertical" );
		desiredMovement.Normalize();
		
		desiredMovement *= Speed;
		
		desiredMovement.y = -Gravity;
		movement = transform.TransformDirection( desiredMovement );

		Controller.Move( movement * Time.deltaTime );
	}
}
