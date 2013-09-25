using UnityEngine;
using System.Collections;

public class LookBob : MonoBehaviour 
{
	public float ViewHeight = 0.7f;
	public float BobSpeed = 4.0f;
	public float BobScale = 0.05f;
	public float YawScale = 0.5f;
	
	private float KeyDownTime = 0.0f;
	private float CurrentOffset = 0.0f;
	
	void UpdateBob()
	{
		Vector3 newloc = new Vector3( 0.0f, ViewHeight + CurrentOffset * BobScale, 0.0f );
		transform.localPosition = newloc;
		
		Vector3 newrot = transform.localEulerAngles;
		newrot.z = CurrentOffset * YawScale;
		
		transform.localEulerAngles = newrot;
	}
	
	void Update() 
	{
		if ( Input.GetAxis( "Horizontal" ) != 0.0f || Input.GetAxis( "Vertical" ) != 0.0f ) 
		{ 
			KeyDownTime += Time.deltaTime; CurrentOffset = Mathf.Sin( KeyDownTime * BobSpeed ); 
		}
		else
		{
			KeyDownTime = 0.0f; if( CurrentOffset > 0.0f ) CurrentOffset -= Time.deltaTime * BobSpeed * 0.5f;
		}
		
		UpdateBob();
	}
}
