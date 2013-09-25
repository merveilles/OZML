using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CustomEditor( typeof( Matedit ) )]
public class MateditEditor : Editor
{
	private List<Material> backup;
	
	void OnEnable()
	{
		backup = new List<Material>();	
	}
	
	public void ReplaceMaterials()
	{
		backup.Clear();
		
		Renderer[] renderers = GameObject.FindObjectsOfType( typeof( Renderer ) ) as Renderer[];
		int i = 0;
		foreach( Renderer r in renderers ) 
		{
			Matedit me = (Matedit)target;
			
			if( r.castShadows != true ) continue;
			backup.Add( r.sharedMaterial );
			r.material = me.NewMaterial;
			i++;
		}
	}
	
	public void RestoreMaterials()
	{
		Renderer[] renderers = GameObject.FindObjectsOfType( typeof( Renderer ) ) as Renderer[];
		int i = 0;
		foreach( Renderer r in renderers ) 
		{
			if( r.castShadows != true ) continue;
			r.sharedMaterial = backup[i];
			i++;
		}
		
		backup.Clear();
	}
	
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		if( GUILayout.Button( "Replace" ) )
			ReplaceMaterials();
		
		if( GUILayout.Button( "Restore" ) )
			RestoreMaterials();
	}
}