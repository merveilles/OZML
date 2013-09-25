using UnityEngine;
using System.Collections;

public class HitscanSpawn : MonoBehaviour 
{
	public GameObject Prefab;
	public float ThinkRate = 0.05f;
	
	void Start() 
	{
		StartCoroutine( Think() );
	}
	
	IEnumerator Think()
	{
		while( true ) 
		{ 
			yield return new WaitForSeconds( ThinkRate );
			
			RaycastHit hit;
		    if( Physics.Raycast( transform.position, transform.TransformDirection( Vector3.forward ), out hit, 1000.0f ) ) 
				Instantiate( Prefab, hit.point, Quaternion.identity );
		}
	}
}
