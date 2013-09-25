using UnityEngine;
using System.Collections;

public class Corrupt : MonoBehaviour 
{
	public Material CorruptMaterial;
	public AudioSource[] CorruptSounds;
	
	public float CorruptionThreshold = 0.5f;
	

	void Update() 
	{
		if( DiracGlobals.Corruption > CorruptionThreshold ) 
		{ 
			float CorruptionAdjusted = Mathf.Min( ( DiracGlobals.Corruption - CorruptionThreshold ) / ( 1.0f - CorruptionThreshold ), 1.0f );
			
			float f = CorruptMaterial.GetFloat( "_Alpha" );
			f = CorruptionAdjusted * 0.5f;
			CorruptMaterial.SetFloat( "_Alpha", f );
			
			for( int i = 0; i < CorruptSounds.Length; i++ )
			{
				CorruptSounds[i].volume = CorruptionAdjusted * 0.125f;
			}
		}
	}
	
	void OnApplicationQuit()
	{
		float f = CorruptMaterial.GetFloat( "_Alpha" );
		f = 0.0f;
		CorruptMaterial.SetFloat( "_Alpha", f );
	}
}
