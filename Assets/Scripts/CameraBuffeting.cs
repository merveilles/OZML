using UnityEngine;
using System.Collections;

public class CameraBuffeting : MonoBehaviour 
{
	public Vector3 BuffetFrequency;
	public Vector3 BuffetScale;
	public float BuffetCorrection = 0.95f;
	
	private Quaternion cameraRotation;
	
	private float pitch = 0.0f;
	private float yaw = 0.0f;
	private float roll = 0.0f;
	private float thrust = 0.0f;

	float CalculateBuffeting( float frequency, float scale )
	{
		float firstPersonScale = 2.0f;
		//if( PlayerScript.LocalIsFirstPerson ) 
		//	firstPersonScale *= 2.0f;
		
		float dampedAccel = 0.5f + thrust * 0.5f;
		return ( 0.5f - Mathf.PerlinNoise( thrust, Time.time * frequency * 0.5f + thrust )) * scale * thrust * firstPersonScale;
	}
	
	public void Buffet( float inThrust )
	{
		Quaternion offsetRotation = Quaternion.identity;
		
		thrust = inThrust;
		
		pitch += CalculateBuffeting( BuffetFrequency.x, BuffetScale.x );
		yaw += CalculateBuffeting( BuffetFrequency.y, BuffetScale.y );
		roll += CalculateBuffeting( BuffetFrequency.z, BuffetScale.z );
		
		pitch *= BuffetCorrection; yaw *= BuffetCorrection; roll *= BuffetCorrection;
		
		Quaternion xq = Quaternion.AngleAxis( yaw, Vector3.up );
		Quaternion yq = Quaternion.AngleAxis( 0, Vector3.left );
		Quaternion zq = Quaternion.AngleAxis( 0, Vector3.forward );
		offsetRotation = xq * yq * zq;

		// Pitch and yaw the camera
		yq = Quaternion.AngleAxis( pitch, Vector3.left );
		offsetRotation = xq * yq * zq;
		
		zq = Quaternion.AngleAxis( roll , Vector3.forward );
		offsetRotation = xq * yq * zq;

		//if( PlayerScript.LocalIsFirstPerson )
		//{
			cameraRotation = transform.localRotation;
			cameraRotation.eulerAngles = offsetRotation.eulerAngles;
			transform.localRotation = cameraRotation;
		//}
		/*else
		{
			cameraRotation = transform.rotation;
			cameraRotation.eulerAngles += offsetRotation.eulerAngles;
			transform.rotation = cameraRotation;
		}*/
	}
	
	void LateUpdate()
	{
				Buffet( 1.5f );
	}
}
