using UnityEngine;
using System.Collections;

public class BreakableLight : Impact
{
	public Material MaterialTwo;
	public GameObject TargetLight;
	public GameObject TargetSound;
	public int DamageLevel = 2;
	
	private bool Smashed = false;

 	public override void OnImpact( GameObject Impactor ) 
	{
		DamageLevel--;
		if( DamageLevel == 1 )
		{
			GetComponent<FlickerMaterialAndLight>().enabled = true;
			if( GetComponent<Spin>() ) GetComponent<Spin>().enabled = false;
		}
		if( DamageLevel == 0 )
		{
			GetComponent<FlickerMaterialAndLight>().enabled = false;
			renderer.material = MaterialTwo;
			if( TargetSound ) TargetSound.audio.volume = 0.0f;
			TargetLight.active = false;
		}
	}
}
