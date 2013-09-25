using UnityEngine;
using System.Collections;

public class Follow : MonoBehaviour 
{
	public Transform Target;
	public float Speed = 1.0f;
	
	void LateUpdate() 
	{
		transform.position = Target.position;//Vector3.Lerp( transform.position, Target.position, Speed * Time.deltaTime );
	}
}
