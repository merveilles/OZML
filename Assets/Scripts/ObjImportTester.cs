using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class ObjImportTester : MonoBehaviour 
{
	public Material Mat;
	
	IEnumerator Start() 
	{
		string objText;
        using( var www = new WWW( "https://dl.dropboxusercontent.com/u/61703399/Heightmap_04-L3.obj" ) )
        {
            while( !www.isDone )
                yield return new WaitForEndOfFrame();
			
            objText = www.text;
			print ("found");
        }
		
		List<Mesh> meshes = ObjImporter.CreateMesh( objText );
		foreach( Mesh m in meshes )
		{
			GameObject submesh = new GameObject( "Submesh" );
			submesh.transform.parent = transform;
			MeshRenderer submeshrenderer = submesh.AddComponent<MeshRenderer>();
			submeshrenderer.material = Mat;
			
			MeshFilter submeshfilter = submesh.AddComponent<MeshFilter>();
			submeshfilter.mesh = m;
			
			print(submeshfilter.mesh.vertexCount);
		}
	}
}
