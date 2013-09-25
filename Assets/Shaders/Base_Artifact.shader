Shader "Base_Artifact" 
{
	Properties  
	{
		_DataTex ( "Data Artifact", 2D ) = "black" {}
		_Rate ( "Artifact Rate", float ) = 2.0
		_Screeny_rate ( "Screen Rate", float ) = 6.0
		_WarpScale ( "Warp Scale", range( 0, 1 ) ) = 0.5
		_WarpOffset ( "Warp Offset", range( 0, 0.5 ) ) = 0.5
		
        _FresnelEmitColor ( "Fresnel Emit Color", Color ) = ( 0.89, 0.945, 1.0, 0.0 )
        _FresnelEmitPrimarySecondary ( "Primary/Secondary Emit Fresnel Degree", Range( 0.0, 1.0 ) ) = 1.0
        _FresnelEmitPower ( "Fresnel Emit Power", Range( 0.0, 16.0 ) ) = 8.0
	
		_DiffuseAmount ( "Diffuse Amount", Range( 0.0, 1.0 ) ) = 0.25
		_TexAmount ( "Texture Amount", Range( 0.0, 1.0 ) ) = 0.25
		_SpecAmount ( "Specular Amount", Range( 0.0, 16.0 ) ) = 1.0
		_SpecTexAmount ( "Spec Texture Amount", Range( 0.0, 1.0 ) ) = 0.25
		
		_Normalmap ( "Normalmap", 2D ) = "normal" {}
		_Specmap ( "Specmap", 2D ) = "spec" {}
		
		_Gloss ( "Gloss", Range( 0.001, 1.0 ) ) = 1.0
		_SRNI ( "Specular Reﬂectance at Normal Incidence", Range( 0.0, 1.0 ) ) = 0.075
	}
	    
	SubShader 
	{
        Tags
        {
          "Queue"="Geometry+0" 
          "IgnoreProjector"="False"
          "RenderType"="Opaque"
        }

        Cull Back
        ZWrite On
        ZTest LEqual

		CGPROGRAM
		#pragma target 3.0 
		#pragma surface surf SimpleSpecular vertex:vert novertexlights
		#pragma glsl
		
		sampler2D _DataTex;
		float4 _DataTex_ST;
		
		float _WarpScale;
		float _WarpOffset;
		float _Rate;
		float _Screeny_rate;
		
		struct Input 
		{
			float2 uv_Normalmap;
			float2 uv_Specmap;
			float2 uv_DataTex;
            
			float4 pos : POSITION;
			float4 dataUV : TEXCOORD1;
			float3 viewDir;
		};

		void vert ( inout appdata_full v, out Input o )
		{
		    float4 pos = mul( UNITY_MATRIX_MVP, v.vertex );
		    float2 screenuv = pos.xy / pos.w;
		    screenuv.y += _Time.x * _Screeny_rate;
			
			o.dataUV = float4( screenuv.x, screenuv.y, 0, 0 );
			o.dataUV.xy = TRANSFORM_TEX( o.dataUV.xy, _DataTex );
			float4 tex = ( tex2Dlod( _DataTex, o.dataUV ) * 0.5 ) + 0.0;
			
			float3 warp = normalize(float3(
				sin( v.normal.x*tex.r*v.vertex.x ),
				atan( v.normal.y*tex.g*v.vertex.y ),
				cos( v.normal.z*tex.b*v.vertex.z )
			));
			//warp *= 1.0 - ( v.normal * rand3d_3d( v.normal ) * _WarpOffset );// * _WarpOffset * sin( _Time.x * _Rate );
			
			float dist = distance( 0.0f, warp );
			v.vertex.xyz = lerp( v.vertex.xyz, v.vertex.xyz + warp, _WarpScale * dist );
		}
		
		float _Gloss;
		fixed _SpecAmount;
		float _SRNI;
 		
		float CalculateGGX( float alpha, float cosThetaM )
		{
		    float roughness = alpha * alpha;
		    roughness *= roughness;
			
		    float CosSquared = cosThetaM * cosThetaM;
		    float TanSquared = ( 1.0 - CosSquared ) + 1.0;
		    float GGX = ( CosSquared * ( roughness - 1.0 ) ) + 1.0;
		    GGX = 0.31831 * GGX * GGX;
		    return roughness / GGX;
		}
		
		float CalculateGeometric( float n_dot_v, float roughness )
		{
			return n_dot_v / ( n_dot_v * ( 1.0 - roughness ) + roughness );
		}
		
		float CalculateGeometricAtten( float n_dot_l, float n_dot_v, float roughness )
		{
			float roughnessRemap = roughness + 1.0;
			roughnessRemap *= roughnessRemap;
			roughnessRemap *= 0.125;
			
			float atten = CalculateGeometric( n_dot_v, roughnessRemap ) * CalculateGeometric( n_dot_l, roughnessRemap );
			return atten;
		}

		float CalculateSpecular( fixed3 lDir, fixed3 vDir, fixed3 norm, float lightSize, float gloss, float n_dot_l )
		{	 
			float3 halfVector = normalize( lDir + vDir );
		
			float n_dot_v = saturate( dot( norm, vDir ) );
			float h_dot_n = saturate( dot( halfVector, norm ) );
			float h_dot_v = saturate( dot( halfVector, vDir ) );
			
			float fresnel = pow( 1.0 - h_dot_v, 5.0 );
			fresnel = _SRNI + ( 1.0 - _SRNI ) * fresnel;

			float geoAtten = CalculateGeometricAtten( n_dot_l, h_dot_v, gloss );
			float distribution = CalculateGGX( saturate( gloss + lightSize * _WorldSpaceLightPos0.w ), h_dot_n );
			
			float BRDF = distribution * fresnel * geoAtten * saturate( h_dot_v / ( h_dot_n * n_dot_v ) ); 
			return BRDF;
		}
		
        float3 AreaLight( float3 light, float3 reflection, float size )
        {
            float3 centerToRay = light - dot( light, reflection ) * reflection;
            return light + centerToRay * saturate( size / centerToRay );
        }
        
		struct SurfaceOutputMod 
		{
		    half3 Albedo;
		    half3 Normal;
		    half3 Emission; 
		    half Specular;
		    half Gloss;
		    half Alpha;
		    float3 position;
		};
		
		fixed4 LightingSimpleSpecular( SurfaceOutputMod s, fixed3 lightDir, fixed3 viewDir, fixed atten ) 
		{
			//float3 areaLightDirection = normalize( AreaLight( lightDistance * lightDir, reflect( viewDir, s.Normal ), _LightColor0.w ) ); //lightDistance * reflect( viewDir, s.Normal )
			fixed lightSize = _LightColor0.w * 4.0;
			
			float n_dot_l = saturate( dot( s.Normal, lightDir ) );
			fixed spec = CalculateSpecular( lightDir, viewDir, s.Normal, lightSize, s.Gloss, n_dot_l ) * 0.005 * _SpecAmount;
			fixed3 diff = n_dot_l * s.Albedo * 0.31831;
			
			diff = lerp( diff, 0.0, spec );
			
			fixed4 c;
			//c.rgb = areaLightDirection;// * _LightColor0.rgb;
			c.rgb = diff * _LightColor0.rgb;
			c.rgb += _LightColor0.rgb * spec;
			c.rgb *= atten * 2.0; 
			c.a = s.Alpha; 
			
			return c;
		}
	
	 	sampler2D _Normalmap;
	 	sampler2D _Specmap; 

		fixed _SpecTexAmount; 
		fixed _TexAmount;
		fixed _DiffuseAmount;
		
        float4 _FresnelEmitColor;
        float _FresnelEmitPower;
        float _FresnelEmitPrimarySecondary;

		void surf( Input IN, inout SurfaceOutputMod o ) 
		{
			float spec = tex2D( _Specmap, IN.uv_Specmap ).r;
			
			o.Normal = UnpackNormal( tex2D( _Normalmap, IN.uv_Normalmap ) );
			
            float fresnelDot = saturate( 1.0 - dot( normalize( IN.viewDir ), o.Normal ) );
            float emitPrimaryBlob = pow( fresnelDot, _FresnelEmitPower * 2.0 );
            float emitSecondaryBlob = pow( fresnelDot, _FresnelEmitPower );
            float emitFresnel = lerp( emitPrimaryBlob, emitSecondaryBlob, _FresnelEmitPrimarySecondary );
            o.Emission = emitFresnel * _FresnelEmitColor.rgb * ( _FresnelEmitColor.a * 16.0f ) * spec;

			o.Gloss = lerp( spec * 2.0, 0.5, _SpecTexAmount ) * _Gloss;
			o.Albedo = _DiffuseAmount * lerp( spec, 1.0, _TexAmount );
			o.position = IN.pos;
		}
	
		ENDCG
	} 
	    
	Fallback "Diffuse"
}