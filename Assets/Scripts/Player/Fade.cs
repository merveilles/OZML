using UnityEngine;
using System.Collections;

public class Fade : MonoBehaviour 
{
	public static float Amount = 0.0f;
	public Material FadeMaterial;

	void Update () 
	{
		float f = FadeMaterial.GetFloat( "_Fade" );
		f = 1.0f - Amount;
		FadeMaterial.SetFloat( "_Fade", f );
	}
}
