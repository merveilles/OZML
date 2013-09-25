Shader "Hidden/ContrastComposite" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "" {}
		_MainTexBlurred ("Base Blurred (RGB)", 2D) = "" {}
	}
	
	// Shader code pasted into all further CGPROGRAM blocks	
	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	struct v2f {
		float4 pos : POSITION;
		float2 uv[2] : TEXCOORD0;
	};
	
	sampler2D _MainTex;
	sampler2D _MainTexBlurred;
	
	float4 _MainTex_TexelSize;
	
	float intensity; 
	float threshhold;
	
	float Contrast;
	float Saturation;
	float Brightness;
		
	v2f vert( appdata_img v ) {
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		
		o.uv[0] = v.texcoord.xy;
		o.uv[1] = v.texcoord.xy;
		#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0)
			o.uv[0].y = 1-o.uv[0].y;
		#endif			
		return o;
	}
	
	float3 ContrastSaturationBrightness( float3 color, float brt, float sat, float con )
	{
		// Increase or decrease theese values to adjust r, g and b color channels seperately
		const float AvgLumR = 0.5;
		const float AvgLumG = 0.5;
		const float AvgLumB = 0.5;
		
		const float3 LumCoeff = float3( 0.2125, 0.7154, 0.0721 );
		
		float3 AvgLumin = float3( AvgLumR, AvgLumG, AvgLumB );
		float3 brtColor = color * brt;
		float3 intes = float3( dot( brtColor, LumCoeff ) );
		float3 satColor = lerp( intes, brtColor, sat );
		float3 conColor = lerp( AvgLumin, satColor, con );
		return conColor;
	}
	
	half4 frag(v2f i) : COLOR 
	{
		half4 color = tex2D (_MainTex, i.uv[1]);
		half4 blurred = tex2D (_MainTexBlurred, (i.uv[0]));
		
		half4 difff = color - blurred;
		half4 signs = sign (difff);
		
		difff = saturate ( (color-blurred) - threshhold) * signs * 1.0/(1.0-threshhold);
		color += difff * intensity;
		
		color.rgb = ContrastSaturationBrightness( color.rgb, Brightness, Saturation, Contrast );
		
		return color;
	}

	ENDCG
	
Subshader {
 Pass {
	  ZTest Always Cull Off ZWrite Off
	  Fog { Mode off }      

      CGPROGRAM
      #pragma fragmentoption ARB_precision_hint_fastest
      #pragma vertex vert
      #pragma fragment frag
      ENDCG
  }
}

Fallback off
	
} // shader