using UnityEngine;
using System.Collections;
using System.Xml;

public class OZMLSave : MonoBehaviour 
{
	public string LevelName = "dori0";
	public string MusicName = "mu_dori1";
	public string MeshSrcUrl = "https://dl.dropboxusercontent.com/u/61703399/";
	public Material[] SceneMaterials;
	
	void Start() 
	{
		XmlWriter writer = XmlWriter.Create( LevelName + ".xml" );
		writer.WriteStartDocument();
		writer.WriteStartElement("ozml");
		
		// Scene
		writer.WriteStartElement("head");
		writer.WriteStartElement("scene");
		
		//string background = GetComponentInChildren<Camera>().backgroundColor.ToString();
		//background = background.Substring( 5, background.Length - 13 );
		//writer.WriteAttributeString( "background", background );
		
		//string fog = RenderSettings.fogColor.ToString();
		//fog = fog.Substring( 5, fog.Length - 6 );
		//writer.WriteAttributeString( "fog", fog );
			
		writer.WriteEndElement();
		
		// Camera
		writer.WriteStartElement("camera");
		
		string camPositionString = transform.position.ToString();
		writer.WriteAttributeString( "position", camPositionString.Substring( 1, camPositionString.Length - 2 ) );

		writer.WriteEndElement();
		writer.Flush();
		
		// Music
		//writer.WriteStartElement("audio");
		//writer.WriteAttributeString( "url", "https://dl.dropbox.com/u/17070747/" + MusicName );
		//writer.WriteEndElement();
		//writer.Flush();
		
		// Materials
		/*writer.WriteStartElement("materials");
		
		foreach( Material mat in SceneMaterials )
		{
			writer.WriteRaw( "." + mat.name + "{texture:http://dev.xxiivv.com/ozml/img/" + mat.name + ".jpg}");
		}
		
		writer.WriteEndElement();
		writer.Flush();*/
		
		// Geometry
		writer.WriteStartElement("geometry");
		
		Renderer[] objects = gameObject.GetComponentsInChildren<Renderer>();
		for( int i = 0; i < objects.Length; i++ )
	    {
			Renderer obj = objects[i];
			
			writer.WriteStartElement( "mesh" );
	
			writer.WriteAttributeString( "name", obj.name + ":" + i );
			
			writer.WriteAttributeString( "src", MeshSrcUrl + obj.name + ".obj" );
			
			writer.WriteAttributeString( "material", obj.renderer.sharedMaterial.name );
			
			string posString = obj.transform.position.ToString();
			writer.WriteAttributeString( "position", posString.Substring( 1, posString.Length - 2 ) );
			
			string scaleString = obj.transform.lossyScale.ToString();
			writer.WriteAttributeString( "size", scaleString.Substring( 1, scaleString.Length - 2 ) );
			
			string rotString = obj.transform.eulerAngles.ToString();
			writer.WriteAttributeString( "rotation", rotString.Substring( 1, rotString.Length - 2 ) );
			
			string layerString = LayerMask.LayerToName( obj.gameObject.layer );
			writer.WriteAttributeString( "layer", layerString );
	
			writer.WriteEndElement();
			
			writer.Flush();
	    }
		
		writer.WriteEndElement();
		
		writer.WriteEndElement();
		writer.WriteEndDocument();
		
		writer.Flush();
	}
}
