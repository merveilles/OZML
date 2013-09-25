using UnityEngine;
using System.Collections;

public class GrowBall : MonoBehaviour 
{
	public float ScaleScale = 5.0f;
	public float ScaleMin = 0.5f;
	public float GrowSpeed = 0.01f;
	public float ThinkRate = 0.5f;
	
	float destinationScale;
	float scale = 0.0f;
	
	void Start() 
	{
		destinationScale = Random.value * ScaleScale + ScaleMin;
		StartCoroutine( Think() );
	}
	
	IEnumerator Think()
	{
		while( true ) 
		{ 
			yield return new WaitForSeconds( ThinkRate );
			
			scale = Mathf.Min( scale + GrowSpeed, destinationScale );
			transform.localScale = new Vector3( scale , scale, scale );
		}
	}
}
