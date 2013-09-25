using UnityEngine;
using System.Collections;

public class Platform : MonoBehaviour 
{
	public bool Active = false;
	public float Speed = 0.4f;
	public bool Up = true;
	public float StopMove = 10.0f;
	public float StartDelay = 1.0f;
	public Vector3 Direction = Vector3.up;
	
	private Vector3 position;
	private bool StartDelayOn = false;
	private float StartCountdown;
	
	void Start() 
	{
		position = Vector3.zero;
		StartCountdown = StartDelay;
	}
	
	public void Activate()
	{
		StartDelayOn = true;
	}
	
	void Update() 
	{
		if( StartDelayOn ) 
		{
			StartCountdown -= Time.deltaTime;
			if( StartCountdown < 0.0f )
			{
				StartDelayOn = false;
				Active = true;
			}
		}
		
		if( Active ) 
		{
			float frameMove = Time.deltaTime * Speed;
			position.y += frameMove;
			
			if( position.y > StopMove && Up )
			{
				Active = false;
				position.y = StopMove;
			}
			
			if( position.y < StopMove && !Up )
			{
				Active = false;
				position.y = StopMove;
			}
			
			transform.position += new Vector3( 0, frameMove, 0 );
		}
	}
	
	void OnTriggerEnter()
	{
		StartDelayOn = true;
	}
}
