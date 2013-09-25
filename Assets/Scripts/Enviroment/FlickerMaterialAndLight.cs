using UnityEngine;
using System.Collections;

public class FlickerMaterialAndLight : MonoBehaviour
{
	public Material MaterialOne;
	public Material MaterialTwo;
	public float BrightnessOne = 2.0f;
	public float BrightnessTwo = 0.0f;
	public GameObject TargetLight;
	public float SoundLevelOne = 1.0f;
	public float SoundLevelTwo = 0.0f;
	public GameObject TargetSound;
	public float CoolDown = 0.1f;
	public float Offset = 0.0f;
	public bool Randomized = true;
	
	private float CurrentCoolDown = 0.0f;
	private bool CurrentMaterialOne = false;
	
	void Start()
	{
		CurrentCoolDown = Offset;
	}

	void Update()
	{
		CurrentCoolDown -= Time.deltaTime;
		
		if( CurrentCoolDown < 0.0f )
		{
			CurrentMaterialOne = !CurrentMaterialOne;
			if( CurrentMaterialOne ) 
			{
				TargetLight.light.intensity = BrightnessOne;
				if( TargetSound ) TargetSound.audio.volume = SoundLevelOne;
				gameObject.renderer.material = MaterialOne;
			}
			else 
			{
				TargetLight.light.intensity = BrightnessTwo;
				if( TargetSound ) TargetSound.audio.volume = SoundLevelTwo;
				gameObject.renderer.material = MaterialTwo;
			}
			
			Random.seed++;
			CurrentCoolDown = CoolDown * ( Randomized ? Random.value : 1.0f );
		}
	}
}
