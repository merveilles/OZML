using UnityEngine;
using System.Collections;

public class TimeScale : MonoBehaviour 
{
	public float Size = 1.0f;
	
	void LateUpdate() 
	{
		float distance = Vector3.Distance( GameObject.FindGameObjectsWithTag("Player")[0].transform.position, transform.position );
		float scale = Mathf.Min( distance / Size, 1.0f );
		Time.timeScale *= scale; Time.fixedDeltaTime = 0.016666f * Time.timeScale;
	}
	
	void Update() 
	{
		Time.timeScale = 1.0f;
	}
}
