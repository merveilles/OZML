using UnityEngine;
using System.Collections;

public class RandomDelayedPlayback : MonoBehaviour 
{
	public float DelayMultiplier = 4.0f;
	public float DelayOffset = 4.0f;
	
	public float VolumeMultiplier = 4.0f;
	public float PitchMultiplier = 0.1f;
	
	void Start() 
	{
		StartCoroutine("DelayLoop");
	}
	
	IEnumerator DelayLoop()
	{
		while( true )
		{
			yield return new WaitForSeconds( DelayOffset + Random.value * DelayMultiplier );
			
			audio.volume = 1.0f - Random.value * VolumeMultiplier;
			audio.pitch = 1.0f - Random.value * PitchMultiplier;
			audio.Play();
		}
	}
}
