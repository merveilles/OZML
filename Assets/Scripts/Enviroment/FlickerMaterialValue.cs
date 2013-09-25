using UnityEngine;
using System.Collections;

public class FlickerMaterialValue : MonoBehaviour
{
	public Material MaterialOne;
	public float CoolDown = 0.1f;
	public string Name = "_TintColor";
	
	private float CurrentCoolDown = 0.0f;
	private bool CurrentMaterialOne = false;

	void Update()
	{
		CurrentCoolDown -= Time.deltaTime;
		
		if( CurrentCoolDown < 0.0f )
		{
			CurrentMaterialOne = !CurrentMaterialOne;
			
			Color color = MaterialOne.GetColor( Name );
			if( CurrentMaterialOne )
				color.a = 1;
			else
				color.a = 0;
			MaterialOne.SetColor( Name, color );
			
			Random.seed++;
			
			CurrentCoolDown = Random.value * CoolDown;
		}
	}
}
