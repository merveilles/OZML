public var wTexture : Texture2D;
var heightData : float;
var timeData : float;
var topData : float;

function FixedUpdate() { 
   if (timeData < 11250) { timeData = timeData+1;   }
   // if (timeData < 11250) { GUI.DrawTexture(Rect(40, ((Screen.height/6)*5)-300, 50, 50), wTexture);	}   
}

function Update(){
	
		if (Input.GetKey (KeyCode.Escape)) {
			Application.Quit();	
			//pressing escape quits the game
	}
	
		if (Input.GetKey (KeyCode.R)) {
			Application.LoadLevel (Application.loadedLevel);
			//restart the game - loadedlevel is the last level loaded

	}
	
	}

function OnGUI () {
	heightData = (((Screen.height/6)*5)-Camera.main.transform.position.y);
    
// JUMP

    GUI.DrawTexture(Rect(40, heightData, 5, 5), wTexture); 

    GUI.DrawTexture(Rect(40, ((Screen.height/6)*5), 5, 5), wTexture);
    GUI.DrawTexture(Rect(40, ((Screen.height/6)*5)-300, 5, 5), wTexture);

// Crosshair

    GUI.DrawTexture(Rect((Screen.width/2)-3, Screen.height/2-3, 5, 5), wTexture);

// TIMER

	GUI.DrawTexture(Rect(45, ((Screen.height/6)*5)-(timeData/45), 5, 5), wTexture); 
	GUI.DrawTexture(Rect(45, ((Screen.height/6)*5)-300, 5, 5), wTexture); 
	GUI.DrawTexture(Rect(45, ((Screen.height/6)*5), 5, 5), wTexture); 

 

}




//* Screen.width, Screen.height