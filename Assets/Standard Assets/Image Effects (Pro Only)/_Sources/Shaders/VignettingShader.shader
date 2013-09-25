Shader "Hidden/Vignetting" {
	Properties {
		_MainTex ("Base", 2D) = "white" {}
		_VignetteTex ("Vignette", 2D) = "white" {}
		_GlitchTex ("Glitch", 2D) = "white" {}
	}
	
	CGINCLUDE
	
	#include "UnityCG.cginc"
	
	struct v2f {
		float4 pos : POSITION;
		float2 uv : TEXCOORD0;
		float2 uv2 : TEXCOORD1;
	};
	
	sampler2D _MainTex;
	sampler2D _VignetteTex;
	sampler2D _GlitchTex;
	
	half _Intensity;
	half _Blur;
	half _GlitchIntensity;

	float4 _MainTex_TexelSize;
		
	v2f vert( appdata_img v ) {
		v2f o;
		o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
		o.uv = v.texcoord.xy;
		o.uv2 = v.texcoord.xy;

		#if UNITY_UV_STARTS_AT_TOP
		if (_MainTex_TexelSize.y < 0)
			 o.uv2.y = 1.0 - o.uv2.y;
		#endif

		return o;
	} 
	
	half4 frag(v2f i) : COLOR {
		fixed4 glitch = 1.0 - tex2D( _GlitchTex, i.uv2 * 1.0 ) - 0.5; 
		fixed n = glitch.x + step( 0.95, _GlitchIntensity ); 
		fixed2 GlitchedUV = n * floor( _Time.z + glitch / n );
	
		half2 coords = i.uv2;
		half2 uv = i.uv;
		
		coords = (coords - 0.5) * 2.0;		
		half coordDot = dot (coords,coords);
		half4 color = tex2D (_MainTex, i.uv);	 

		float mask = 1.0 - coordDot * _Intensity * 0.1; 
		
		half4 colorBlur = tex2D (_VignetteTex, i.uv2);
		color = lerp (color, colorBlur, saturate (_Blur * coordDot));
		
		color *= 0.95 + 0.05 * sin( 10.0 * _Time.y + uv.x * _ScreenParams.x * 2.0 + uv.y * _ScreenParams.y * 2.0 );// * sin( 10.0 * _Time.y + uv.y * _ScreenParams.y * 2.0 );
		color *= 0.975 + 0.025 * sin( 110.0 * _Time.y );
		
		return color * mask;// - tex2D (_MainTex, uv);
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
} 