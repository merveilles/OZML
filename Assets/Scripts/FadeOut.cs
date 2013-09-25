using UnityEngine;
using System.Collections;

public class FadeOut : MonoBehaviour 
{
	public float FadeDuration = 1.0f;
	public bool FadeOnStart = false;
	public bool KillOnEnd = false;
	public bool Fading = false;
	
	private float fadeTime = 1.0f;
	
	void Start()
	{
		if( FadeOnStart ) Activate();
	}
	
	public void Activate()
	{
		Fading = true;
		renderer.enabled = true;
		fadeTime = FadeDuration;
	}

	void Update() 
	{
		if( Fading )
			fadeTime -= Time.deltaTime;
		
		if( fadeTime < 0.0f )
		{
			if( KillOnEnd ) Destroy( gameObject );
			else renderer.enabled = false;
			
			Fading = false;
		}
	}
}
