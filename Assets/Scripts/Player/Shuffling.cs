using UnityEngine;
using System.Collections;

public class Shuffling : MonoBehaviour 
{
	public float StepDelay = 1.0f;
	public float StepOffset = 0.25f;
	public float StepVolume = 0.25f;
	public float StepPitch = 0.125f;
	public float Panning = 0.25f;
	public AudioClip[] Sounds;
	public CharacterController Controller;
	
	private bool left = true;
	
	void Start() 
	{
		StartCoroutine("Step");
	}
		IEnumerator Step()
	{
		while( true )
		{
			if( ( Input.GetAxis( "Mouse X" ) != 0.0f || Input.GetAxis( "Mouse Y" ) != 0.0f ) )
			{
				audio.clip = Sounds[ (int)(Random.value * Sounds.Length) ];
				audio.volume = 0.275f - Random.value * StepVolume;
				audio.pitch = 1.0f - Random.value * StepPitch;
				if( left ) transform.localPosition = new Vector3( Panning, 0.0f, 0.0f );
				else transform.localPosition = new Vector3( 0.0f - Panning, 0.0f, 0.0f );
				left = !left;
				audio.Play();
			}
			
			yield return new WaitForSeconds( StepOffset + Random.value * StepDelay );
		}
	}
}
