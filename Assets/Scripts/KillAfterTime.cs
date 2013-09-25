using UnityEngine;
using System.Collections;

public class KillAfterTime : MonoBehaviour 
{
	public float Lifetime = 2.0f;
	
	void Start() 
	{
		StartCoroutine( Kill() );
	}
	
	IEnumerator Kill() 
	{
		yield return new WaitForSeconds( Lifetime );
		Destroy( gameObject );
	}
}
