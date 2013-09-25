using UnityEngine;
using System.Collections;

public class Noclip : MonoBehaviour 
{
	public GameObject Fab1;
	public GameObject Fab2;
	
	static private bool swit = true;
	static private float cooldown = 0.0f;
	
	void Update() 
	{
		cooldown -= Time.deltaTime;
		if( Input.GetKey( KeyCode.V ) && cooldown < 0.0f ) 
		{
			GameObject fab;
			if( swit == true ) 
				fab = Fab1;
			else
				fab = Fab2;
			
			swit = !swit;
			cooldown = 1.0f;
			
			GameObject p = (GameObject)Instantiate( fab, transform.position, transform.rotation );
			Destroy( gameObject );
		}
	}
}
