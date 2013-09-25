using UnityEngine;
using System.Collections;

public class ImageSequenceAnimation : MonoBehaviour 
{
	public Texture[] Sequence;
	public string Name = "_MainTex";
	
	private int Frame = 0;
	
	void FixedUpdate() 
	{
		renderer.materials[0].SetTexture( Name, Sequence[Frame] );
		Frame++;
		if( Frame > Sequence.Length-1 ) Frame = 0;
	}
}
