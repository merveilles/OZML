using UnityEngine;
using System.Collections.Generic;

// <ozml>
public class Ozml
{
    public OzmlHead Head { get; set; } //<head>
    public Dictionary<string, OzmlMaterial> Materials { get; set; } //<materials>
	public Dictionary<string, OzmlObject> Objects; //All objects
}

// <head>
public class OzmlHead
{
    public string Title { get; set; }			//<title>
    public OzmlScene Scene { get; set; }	//<scene>
    public OzmlCamera Camera { get; set; }	//<origin>
    public OzmlAudio Audio { get; set; }	//<origin>
}

// <scene>
public class OzmlScene
{
    public Color Background = Color.white;
    public FogParameters Fog = new FogParameters { Color = Color.white, Density = 0 };
    //public float Fov = 70;
}

// fog:
public struct FogParameters
{
    public Color Color;
    public float Density;
}

// <origin>
public class OzmlCamera
{
    public Vector3 Position;
    public Vector3 Rotation;
	public float Speed = 10;
}

// <audio>
public class OzmlAudio
{
    public string Url;
}

// .mat
public class OzmlMaterial
{
	public string Name;
	public Color AmbientDiffuse = Color.white;
	public Color Specular = Color.white;
	public Color Emissive = Color.black;
	public float Power = 16;
	public float Opacity = 1;
	public string Texture = "";
}

public class OzmlObject
{
	public string Id;
	public Vector3 Position;
    public Vector3 Rotation; 
	public Vector3 Size;
	public string Mat;
	public string Type;
	public string Href;
}
