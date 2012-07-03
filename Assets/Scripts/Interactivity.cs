using UnityEngine;
using System.Collections;

public class Interactivity : FrobbableObject 
{
	public Bootstrap bootstrap;
	public string LinkURL = "";
	
	public override void Frob( GameObject Frobber )
	{
		//print ( "You clicked on " + gameObject.name + "!" );
		
		if( LinkURL != "" ) 
		{
			if( LinkURL.Contains( "http://" ) ) bootstrap.Url = LinkURL;
			else
			{
				string[] spliturl = bootstrap.Url.Split( "/"[0] );
				string stemurl = "";
				for( int i = 0; i < spliturl.Length - 1; i++ )
					stemurl += spliturl[i] + "/";
				bootstrap.Url = stemurl + LinkURL;
			}
			
			bootstrap.FullRefresh();
			print( "Linking to: " + bootstrap.Url );
		}
	}
}
