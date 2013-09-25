/////////////////////////////////////////////////////////////////////////////////
//
//	vp_Layer.cs
//	© 2012 VisionPunk, Minea Softworks. All Rights Reserved.
//
//	description:	this class defines the layers used on objects. you may want
//					to modify the integers assigned here to suit your needs.
//					for example, you may want to keep 'Player' in another layer
//					or you may want to address rendering or physics issues
//					related to incompatibility with other systems.
//
/////////////////////////////////////////////////////////////////////////////////

using UnityEngine;

public sealed class vp_Layer
{

	public static readonly vp_Layer instance = new vp_Layer();

	// layers
	public const int Weapon = 31;
	public const int Player = 30;
	public const int Debris = 29;
	public const int None = 0;


	///////////////////////////////////////////////////////////
	// constructor
	///////////////////////////////////////////////////////////
	static vp_Layer()
	{
		Physics.IgnoreLayerCollision(Player, Debris);		// player should never collide with small debris
		Physics.IgnoreLayerCollision(Debris, Debris);		// gun shells should not collide against each other
	}
	private vp_Layer(){}


	///////////////////////////////////////////////////////////
	// sets the layer of a gameobject and optionally its descendants
	///////////////////////////////////////////////////////////
	public static void Set(GameObject obj, int layer, bool recursive = false)
	{

		if (layer < 0 || layer > 31)
		{
			Debug.LogError("vp_Layer: Attempted to set layer id out of range [0-31].");
			return;
		}

		obj.layer = layer;

		if (recursive)
		{
			foreach (Transform t in obj.transform)
			{
				Set(t.gameObject, layer, true);
			}
		}

	}
	

}

