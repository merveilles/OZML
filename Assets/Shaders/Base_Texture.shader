Shader "Base_Texture" 
{
	Properties  
	{
		_DiffuseAmount ( "Diffuse Amount", Range( 0.0, 1.0 ) ) = 0.25
		_TexAmount ( "Texture Amount", Range( 0.0, 1.0 ) ) = 0.25
		_SpecAmount ( "Specular Amount", Range( 0.0, 16.0 ) ) = 1.0
		_SpecTexAmount ( "Spec Texture Amount", Range( 0.0, 1.0 ) ) = 0.25
		
		_Diffusemap ( "Diffusemap", 2D ) = "diffuse" {}
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
		#pragma debug
		#pragma target 3.0 
		#pragma surface surf SimpleSpecular novertexlights noambient nolightmap nodirlightmap 
		
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

		float CalculateSpecular( fixed3 lDir, fixed3 vDir, fixed3 norm, float lightSize, float lightDistance, float gloss, float n_dot_l )
		{	 
			float3 halfVector = normalize( lDir + vDir );
		
			float n_dot_v = saturate( dot( norm, vDir ) );
			float h_dot_n = saturate( dot( halfVector, norm ) );
			float h_dot_v = saturate( dot( halfVector, vDir ) );
			
			float fresnel = pow( 1.0 - h_dot_v, 5.0 );
			fresnel = _SRNI + ( 1.0 - _SRNI ) * fresnel;

			float geoAtten = CalculateGeometricAtten( n_dot_l, h_dot_v, gloss );
			float distribution = CalculateGGX( saturate( gloss + ( lightSize / ( 3.0 * lightDistance ) ) * _WorldSpaceLightPos0.w ), h_dot_n );
			
			float BRDF = fresnel * distribution * geoAtten * saturate( h_dot_v / ( h_dot_n * n_dot_v ) ); 
			return BRDF;
		}
		
       /* float3 AreaLight( float3 light, float3 reflection, float size )
        {
            float3 centerToRay = light - dot( light, reflection ) * reflection;
            return light + centerToRay * saturate( size / centerToRay );
        }*/
        
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
			float lightDistance = length( _WorldSpaceLightPos0.xyz - s.position );
			fixed lightSize = _LightColor0.w * 4.0;
			
			float n_dot_l = saturate( dot( s.Normal, lightDir ) );
			fixed spec = CalculateSpecular( lightDir, viewDir, s.Normal, lightSize, lightDistance, s.Gloss * s.Gloss, n_dot_l ) * 0.005 * _SpecAmount;
			fixed3 diff = n_dot_l * s.Albedo * 0.31831;
			
			diff = lerp( diff, 0.0, spec );
			
			fixed4 c;
			c.rgb = diff * _LightColor0.rgb;
			c.rgb += _LightColor0.rgb * spec;
			c.rgb *= atten * 2.0; 
			c.a = s.Alpha; 
			
			return c;
		}
		
		fixed4 LightingSimpleSpecular_DirLightmap( SurfaceOutputMod s, fixed4 color, fixed4 scale, fixed3 viewDir, bool surfFuncWritesNormal, out fixed3 specColor ) 
		{
			UNITY_DIRBASIS
			half3 scalePerBasisVector; 
			
			half3 lm = DirLightmapDiffuse( unity_DirBasis, color, scale, s.Normal, surfFuncWritesNormal, scalePerBasisVector );
			half3 lightDir = normalize( scalePerBasisVector.x * unity_DirBasis[0] + scalePerBasisVector.y * unity_DirBasis[1] + scalePerBasisVector.z * unity_DirBasis[2 ]);
			
			specColor = lm * CalculateSpecular( lightDir, viewDir, s.Normal, 0.0, 0.0, s.Gloss, saturate( dot( s.Normal, lightDir ) ) ) * 0.005 * _SpecAmount;
			
			return half4( lm * 0.5, 1.0 ); 
		} 
		
		struct Input  
		{
			float2 uv_Diffusemap;
			float2 uv_Normalmap;
			float2 uv_Specmap;
            float3 worldPos;
            float3 viewDir;
		};
		
	 	sampler2D _Diffusemap; 
	 	sampler2D _Normalmap; 
	 	sampler2D _Specmap; 

		fixed _SpecTexAmount; 
		fixed _TexAmount;
		fixed _DiffuseAmount; 

		void surf( Input IN, inout SurfaceOutputMod o ) 
		{
			o.Normal = UnpackNormal( tex2D( _Normalmap, IN.uv_Normalmap ) );
			
			float spec = tex2D( _Specmap, IN.uv_Specmap ).r;
			o.Gloss = lerp( spec * 0.5, 0.5, _SpecTexAmount ) * _Gloss * 2.0;
			o.Albedo = _DiffuseAmount * lerp( tex2D( _Diffusemap, IN.uv_Diffusemap ), 1.0, _TexAmount );
			o.position = IN.worldPos;
		}
	
		ENDCG
	} 
	    
	Fallback "Diffuse"
}