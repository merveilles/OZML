using UnityEngine;
using System.Collections;

public class WobbleValue : MonoBehaviour 
{
    public int MaterialIndex = -1;
    public Material Mat;
    public float Speed = 1.0f;
	public float Multiply = 64.0f;
	public float Offset = 32.0f;
    public string ValueName = "_Distort";

    void LateUpdate() 
    {
		float flux = 0.0f;
		//flux = Mathf.Sin( Time.time * Speed );
		flux = audio.GetOutputData( 1, 1 )[0];
			
    	Mat.SetFloat( ValueName, Offset + flux * Multiply );
    }
}