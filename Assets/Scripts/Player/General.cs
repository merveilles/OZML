using UnityEngine;
using System.Collections;

public class General : MonoBehaviour 
{
	void Start() 
	{
		//Screen.fullScreen = true;
		//Screen.SetResolution( 1920, 1080, true );
		//Screen.lockCursor = true;
		Application.targetFrameRate = 60;
	}
	
    void Update() 
	{
        Screen.lockCursor = true;
    }
}
