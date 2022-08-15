// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Ocean Water"
{
	Properties
	{
		_WaterColour("Water Colour", Color) = (0.2046547,0.4522334,0.6886792,0)
		_TopColour("Top Colour", Color) = (0.3406907,0.659827,0.8301887,1)
		_WaveDirection("Wave Direction", Vector) = (-1,0,0,0)
		_WaveSpeed("Wave Speed", Float) = 1
		_WaveStretch("WaveStretch", Vector) = (0.22,0.02,0,0)
		_WaveTile("Wave Tile", Float) = 1
		_WaveHeight("Wave Height", Float) = 1
		_EdgeDistance("Edge Distance", Float) = 1
		_EdgeStrength("Edge Strength", Range( 0 , 1)) = 0.5
		_NormalMap("Normal Map", 2D) = "white" {}
		_NormalStrength("Normal Strength", Range( 0 , 1)) = 1
		_NormalSpeed("Normal Speed", Float) = 1
		_NormalTile("Normal Tile", Float) = 1
		_SeaFoam("Sea Foam", 2D) = "white" {}
		_EdgeFoamTile("Edge Foam Tile", Float) = 1
		_SeaFoamTile("Sea Foam Tile", Float) = 1
		_RefractAmount("Refract Amount", Float) = 0.1
		_Depth("Depth", Float) = -4
		_TessellationAmount("Tessellation Amount", Float) = 4
		_MinTessellationDistance("Min Tessellation Distance", Float) = 0
		_MaxTessellationDistance("Max Tessellation Distance", Float) = 80
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		GrabPass{ }
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityStandardUtils.cginc"
		#include "UnityCG.cginc"
		#include "Tessellation.cginc"
		#pragma target 4.6
		#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex);
		#else
		#define ASE_DECLARE_SCREENSPACE_TEXTURE(tex) UNITY_DECLARE_SCREENSPACE_TEXTURE(tex)
		#endif
		#pragma surface surf Standard keepalpha noshadow vertex:vertexDataFunc tessellate:tessFunction 
		struct Input
		{
			float3 worldPos;
			float4 screenPos;
		};

		uniform float _WaveHeight;
		uniform float _WaveSpeed;
		uniform float2 _WaveDirection;
		uniform float2 _WaveStretch;
		uniform float _WaveTile;
		uniform sampler2D _NormalMap;
		uniform float _NormalSpeed;
		uniform float _NormalTile;
		uniform float _NormalStrength;
		uniform float4 _WaterColour;
		uniform float4 _TopColour;
		uniform sampler2D _SeaFoam;
		uniform float _SeaFoamTile;
		ASE_DECLARE_SCREENSPACE_TEXTURE( _GrabTexture )
		uniform float _RefractAmount;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _Depth;
		uniform float _EdgeStrength;
		uniform float _EdgeDistance;
		uniform float _EdgeFoamTile;
		uniform float _MinTessellationDistance;
		uniform float _MaxTessellationDistance;
		uniform float _TessellationAmount;


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		inline float4 ASE_ComputeGrabScreenPos( float4 pos )
		{
			#if UNITY_UV_STARTS_AT_TOP
			float scale = -1.0;
			#else
			float scale = 1.0;
			#endif
			float4 o = pos;
			o.y = pos.w * 0.5f;
			o.y = ( pos.y - o.y ) * _ProjectionParams.x * scale + o.y;
			return o;
		}


		float4 tessFunction( appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			return UnityDistanceBasedTess( v0.vertex, v1.vertex, v2.vertex, _MinTessellationDistance,_MaxTessellationDistance,_TessellationAmount);
		}

		void vertexDataFunc( inout appdata_full v )
		{
			float temp_output_7_0 = ( _Time.y * _WaveSpeed );
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float4 appendResult10 = (float4(ase_worldPos.x , ase_worldPos.z , 0.0 , 0.0));
			float4 WorldSpaceTile11 = appendResult10;
			float4 WaveTileUV21 = ( ( WorldSpaceTile11 * float4( _WaveStretch, 0.0 , 0.0 ) ) * _WaveTile );
			float2 panner3 = ( temp_output_7_0 * _WaveDirection + WaveTileUV21.xy);
			float simplePerlin2D1 = snoise( panner3 );
			float2 panner24 = ( temp_output_7_0 * _WaveDirection + ( WaveTileUV21 * float4( 0.1,0.1,0.1,0 ) ).xy);
			float simplePerlin2D25 = snoise( panner24 );
			float WavePattern30 = ( simplePerlin2D1 + simplePerlin2D25 );
			float3 WaveHeight34 = ( ( float3(0,1,0) * _WaveHeight ) * WavePattern30 );
			v.vertex.xyz += WaveHeight34;
			v.vertex.w = 1;
		}

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float4 appendResult10 = (float4(ase_worldPos.x , ase_worldPos.z , 0.0 , 0.0));
			float4 WorldSpaceTile11 = appendResult10;
			float4 temp_output_76_0 = ( WorldSpaceTile11 / 10.0 );
			float2 panner64 = ( 1.0 * _Time.y * ( float2( 1,0 ) * _NormalSpeed ) + ( temp_output_76_0 * _NormalTile ).xy);
			float2 panner65 = ( 1.0 * _Time.y * ( float2( -1,0 ) * ( _NormalSpeed * 3.0 ) ) + ( temp_output_76_0 * ( _NormalTile * 5.0 ) ).xy);
			float3 NormalMaps74 = BlendNormals( UnpackScaleNormal( tex2D( _NormalMap, panner64 ), _NormalStrength ) , UnpackScaleNormal( tex2D( _NormalMap, panner65 ), _NormalStrength ) );
			o.Normal = NormalMaps74;
			float2 panner98 = ( 1.0 * _Time.y * float2( 0.094,0.04 ) + ( WorldSpaceTile11 * 0.08 ).xy);
			float simplePerlin2D97 = snoise( panner98 );
			float clampResult104 = clamp( ( tex2D( _SeaFoam, ( ( WorldSpaceTile11 / 10.0 ) * _SeaFoamTile ).xy ).r * simplePerlin2D97 ) , 0.0 , 1.0 );
			float SeaFoam94 = clampResult104;
			float temp_output_7_0 = ( _Time.y * _WaveSpeed );
			float4 WaveTileUV21 = ( ( WorldSpaceTile11 * float4( _WaveStretch, 0.0 , 0.0 ) ) * _WaveTile );
			float2 panner3 = ( temp_output_7_0 * _WaveDirection + WaveTileUV21.xy);
			float simplePerlin2D1 = snoise( panner3 );
			float2 panner24 = ( temp_output_7_0 * _WaveDirection + ( WaveTileUV21 * float4( 0.1,0.1,0.1,0 ) ).xy);
			float simplePerlin2D25 = snoise( panner24 );
			float WavePattern30 = ( simplePerlin2D1 + simplePerlin2D25 );
			float clampResult44 = clamp( WavePattern30 , 0.0 , 1.0 );
			float4 lerpResult42 = lerp( _WaterColour , ( _TopColour + SeaFoam94 ) , clampResult44);
			float4 WaveAlbedo45 = lerpResult42;
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_grabScreenPos = ASE_ComputeGrabScreenPos( ase_screenPos );
			float4 ase_grabScreenPosNorm = ase_grabScreenPos / ase_grabScreenPos.w;
			float4 screenColor111 = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture,( float3( (ase_grabScreenPosNorm).xy ,  0.0 ) + ( _RefractAmount * NormalMaps74 ) ).xy);
			float4 clampResult112 = clamp( screenColor111 , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			float4 Refraction113 = clampResult112;
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth115 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth115 = abs( ( screenDepth115 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _Depth ) );
			float clampResult117 = clamp( ( 1.0 - distanceDepth115 ) , 0.0 , 1.0 );
			float Depth118 = clampResult117;
			float4 lerpResult119 = lerp( WaveAlbedo45 , Refraction113 , Depth118);
			o.Albedo = lerpResult119.rgb;
			float screenDepth48 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			float distanceDepth48 = abs( ( screenDepth48 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _EdgeDistance ) );
			float4 clampResult55 = clamp( ( _EdgeStrength * ( ( 1.0 - distanceDepth48 ) + tex2D( _SeaFoam, ( ( WorldSpaceTile11 / 10.0 ) * _EdgeFoamTile ).xy ) ) ) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
			float4 Edge53 = clampResult55;
			o.Emission = Edge53.rgb;
			o.Smoothness = 0.9;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18703
0;0;1920;1019;2205.271;565.0418;2.718105;True;True
Node;AmplifyShaderEditor.CommentaryNode;12;-2994.874,-1261.863;Inherit;False;806.3011;253.2853;;3;11;10;9;World Space UVs;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;9;-2962.002,-1208.344;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;10;-2662.739,-1208.19;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;23;-3057.734,-944.3761;Inherit;False;963.3706;340.4502;;6;15;17;13;14;16;21;Wave Tile UV;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;11;-2396.901,-1182.82;Float;False;WorldSpaceTile;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Vector2Node;15;-3007.734,-767.9265;Inherit;False;Property;_WaveStretch;WaveStretch;4;0;Create;True;0;0;False;0;False;0.22,0.02;0.064,0.02;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.GetLocalVarNode;13;-3001.273,-894.3762;Inherit;False;11;WorldSpaceTile;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;78;-4781.468,-2510.71;Inherit;False;2849.291;982.7661;;21;61;59;69;77;76;66;67;63;70;71;68;62;60;73;65;64;57;58;39;72;74;Normal Map;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;17;-2743.215,-755.5939;Inherit;False;Property;_WaveTile;Wave Tile;5;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;61;-4689.17,-2115.376;Inherit;False;Property;_NormalTile;Normal Tile;12;0;Create;True;0;0;False;0;False;1;0.4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;-2730.941,-887.4321;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;69;-3747.137,-2179.496;Inherit;False;Property;_NormalSpeed;Normal Speed;11;0;Create;True;0;0;False;0;False;1;0.01;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;59;-4731.468,-2460.71;Inherit;True;11;WorldSpaceTile;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;77;-4504.528,-2297.994;Inherit;False;Constant;_Float0;Float 0;13;0;Create;True;0;0;False;0;False;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;56;-4911.053,-292.9542;Inherit;False;2131.628;1439.803;;28;94;104;103;100;98;97;99;53;55;51;52;87;80;50;83;48;88;85;79;84;49;93;91;92;82;86;90;89;Edge Foam;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector2Node;66;-3919.521,-2255.04;Inherit;False;Constant;_PanDirection;Pan Direction;10;0;Create;True;0;0;False;0;False;1,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;-2515.396,-872.0419;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Vector2Node;67;-3807.523,-1691.945;Inherit;False;Constant;_PanD2;PanD2;10;0;Create;True;0;0;False;0;False;-1,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleDivideOpNode;76;-4309.097,-2428.206;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;63;-4122.99,-2051.217;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;70;-3509.589,-2087.868;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;100;-4813.064,928.5635;Inherit;False;Constant;_FoamMask;Foam Mask;16;0;Create;True;0;0;False;0;False;0.08;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;89;-4870.885,507.0474;Inherit;True;11;WorldSpaceTile;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;71;-3334.215,-1689.727;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;68;-3346.387,-2273.552;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;21;-2318.364,-846.0591;Inherit;False;WaveTileUV;-1;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;90;-4492.877,566.7385;Inherit;False;Constant;_Float2;Float 2;15;0;Create;True;0;0;False;0;False;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;62;-3868.125,-2038.657;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;-3949.007,-2434.123;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.CommentaryNode;36;-4853.434,-1334.717;Inherit;False;1710.682;832.8864;;13;29;6;8;28;22;5;7;24;3;1;25;27;30;Wave Pattern;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;29;-4685.85,-617.83;Inherit;False;21;WaveTileUV;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-4791.253,-753.2827;Inherit;False;Property;_WaveSpeed;Wave Speed;3;0;Create;True;0;0;False;0;False;1;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;99;-4539.457,883.5906;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleTimeNode;6;-4803.434,-856.7247;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;91;-4498.313,653.5956;Inherit;False;Property;_SeaFoamTile;Sea Foam Tile;15;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;92;-4275.724,497.4573;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TexturePropertyNode;57;-4194.041,-1838.73;Inherit;True;Property;_NormalMap;Normal Map;9;0;Create;True;0;0;False;0;False;None;ec7b798447bb0bc45badeaeb2edfacdf;True;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.RangedFloatNode;73;-3294.434,-1908.278;Inherit;False;Property;_NormalStrength;Normal Strength;10;0;Create;True;0;0;False;0;False;1;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;65;-3172.183,-1746.496;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;64;-3172.434,-2322.435;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;39;-2929.244,-2080.423;Inherit;True;Property;_TextureSample0;Texture Sample 0;5;0;Create;True;0;0;False;0;False;-1;ec7b798447bb0bc45badeaeb2edfacdf;ec7b798447bb0bc45badeaeb2edfacdf;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;7;-4552.174,-808.3468;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;93;-4109.229,500.4092;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.Vector2Node;5;-4699.106,-1036.176;Inherit;False;Property;_WaveDirection;Wave Direction;2;0;Create;True;0;0;False;0;False;-1,0;-1,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SamplerNode;58;-2945.102,-1818.065;Inherit;True;Property;_TextureSample1;Texture Sample 1;5;0;Create;True;0;0;False;0;False;-1;ec7b798447bb0bc45badeaeb2edfacdf;ec7b798447bb0bc45badeaeb2edfacdf;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;22;-4655.228,-1284.717;Inherit;False;21;WaveTileUV;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TexturePropertyNode;79;-4477.289,19.14324;Inherit;True;Property;_SeaFoam;Sea Foam;13;0;Create;True;0;0;False;0;False;6f3d5ccfd40199b439922068538e4e53;6f3d5ccfd40199b439922068538e4e53;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;-4355.172,-668.7257;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT4;0.1,0.1,0.1,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.PannerNode;98;-4254.39,864.9915;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0.094,0.04;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BlendNormalsNode;72;-2451.162,-2011.063;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;88;-3864.904,333.6873;Inherit;True;Property;_TextureSample3;Texture Sample 3;14;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;3;-4153.643,-1189.843;Inherit;True;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PannerNode;24;-4135.839,-835.6722;Inherit;True;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;97;-3862.399,852.0916;Inherit;True;Simplex2D;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;103;-3464.929,418.1296;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;86;-4656.745,296.4211;Inherit;False;Constant;_Float1;Float 1;15;0;Create;True;0;0;False;0;False;10;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;74;-2156.174,-2014.375;Inherit;False;NormalMaps;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;1;-3859.51,-1202.859;Inherit;True;Simplex2D;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;82;-4876.372,196.7852;Inherit;True;11;WorldSpaceTile;1;0;OBJECT;;False;1;FLOAT4;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;25;-3841.708,-848.6884;Inherit;True;Simplex2D;False;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;27;-3538.033,-1003.514;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GrabScreenPosition;105;-1172.192,-3492.816;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;49;-4862.777,-204.9057;Inherit;False;Property;_EdgeDistance;Edge Distance;7;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;104;-3211.953,438.8303;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;85;-4439.592,227.1402;Inherit;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;84;-4662.181,383.2793;Inherit;False;Property;_EdgeFoamTile;Edge Foam Tile;14;0;Create;True;0;0;False;0;False;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;107;-1137.187,-3232.674;Inherit;False;Property;_RefractAmount;Refract Amount;16;0;Create;True;0;0;False;0;False;0.1;0.05;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;109;-1123.559,-3114.906;Inherit;True;74;NormalMaps;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DepthFade;48;-4621.279,-224.921;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;30;-3366.752,-1061.642;Inherit;True;WavePattern;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;83;-4273.097,230.0933;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;94;-3033.143,406.823;Inherit;True;SeaFoam;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;106;-801.1619,-3486.816;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;108;-803.7493,-3220.154;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;116;-918.7859,-3937.906;Inherit;False;Property;_Depth;Depth;17;0;Create;True;0;0;False;0;False;-4;-6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;47;-690.4073,-1616.743;Inherit;False;1168.623;1000.452;;8;43;96;95;44;45;42;41;40;Wave Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;96;-624.0546,-1212.406;Inherit;True;94;SeaFoam;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;41;-636.5378,-1389.955;Inherit;False;Property;_TopColour;Top Colour;1;0;Create;True;0;0;False;0;False;0.3406907,0.659827,0.8301887,1;0.2832413,0.5339658,0.6320754,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;80;-4097.823,95.62517;Inherit;True;Property;_TextureSample2;Texture Sample 2;14;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DepthFade;115;-713.7742,-3944.854;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;37;-1816.791,-1226.069;Inherit;False;924.6331;553.9023;;6;19;31;20;32;33;34;Wave Height;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;110;-502.929,-3352.929;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;50;-4339.651,-218.3276;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;43;-437.6843,-958.5363;Inherit;True;30;WavePattern;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;122;-406.505,-3914.839;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-1759.906,-966.6807;Inherit;False;Property;_WaveHeight;Wave Height;6;0;Create;True;0;0;False;0;False;1;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;40;-629.607,-1571.303;Inherit;False;Property;_WaterColour;Water Colour;0;0;Create;True;0;0;False;0;False;0.2046547,0.4522334,0.6886792,0;0.2244126,0.3226424,0.3867925,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScreenColorNode;111;-206.5062,-3363.152;Inherit;False;Global;_GrabScreen0;Grab Screen 0;17;0;Create;True;0;0;False;0;False;Object;-1;False;False;1;0;FLOAT2;0,0;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;52;-3861.761,-154.0278;Inherit;False;Property;_EdgeStrength;Edge Strength;8;0;Create;True;0;0;False;0;False;0.5;0.24;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;95;-337.2849,-1281.603;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.Vector3Node;19;-1766.791,-1176.069;Inherit;False;Constant;_WaveUpDir;Wave Up Dir;4;0;Create;True;0;0;False;0;False;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleAddOpNode;87;-3719.17,65.05606;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ClampOpNode;44;-54.80397,-1238.935;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;112;73.19861,-3354.48;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;51;-3469.219,-215.3501;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;42;66.03997,-1492.818;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;32;-1531.369,-902.1667;Inherit;True;30;WavePattern;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;117;-154.1946,-3940.538;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;-1470.721,-1055.649;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;55;-3290.146,-177.8999;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,1,1,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;45;234.3586,-1505.127;Inherit;True;WaveAlbedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;113;338.2128,-3372.374;Inherit;True;Refraction;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;-1262.89,-942.9207;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;118;134.2914,-3944.091;Inherit;True;Depth;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;120;-287.7167,-316.8869;Inherit;False;113;Refraction;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;34;-1116.157,-949.0251;Inherit;True;WaveHeight;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;121;-299.0742,-225.6948;Inherit;False;118;Depth;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;125;-391.5705,763.3312;Inherit;False;Property;_MaxTessellationDistance;Max Tessellation Distance;20;0;Create;True;0;0;False;0;False;80;80;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;53;-3135.248,-236.0591;Inherit;False;Edge;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;46;-313.1747,-511.2998;Inherit;True;45;WaveAlbedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;124;-384.5705,674.3312;Inherit;False;Property;_MinTessellationDistance;Min Tessellation Distance;19;0;Create;True;0;0;False;0;False;0;8;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;18;-300.7708,533.1166;Inherit;False;Property;_TessellationAmount;Tessellation Amount;18;0;Create;True;0;0;False;0;False;4;6;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;38;1.190704,127.4256;Inherit;False;Constant;_Smoothness;Smoothness;5;0;Create;True;0;0;False;0;False;0.9;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;54;-6.777634,50.68086;Inherit;False;53;Edge;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;119;3.215021,-505.6484;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;35;125.7231,292.299;Inherit;True;34;WaveHeight;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;75;-40.90871,-156.8879;Inherit;True;74;NormalMaps;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DistanceBasedTessNode;123;-24.35388,529.0343;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;527.6505,-29.31392;Float;False;True;-1;6;ASEMaterialInspector;0;0;Standard;Ocean Water;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;False;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;True;2;15;10;25;False;0.5;False;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;10;0;9;1
WireConnection;10;1;9;3
WireConnection;11;0;10;0
WireConnection;14;0;13;0
WireConnection;14;1;15;0
WireConnection;16;0;14;0
WireConnection;16;1;17;0
WireConnection;76;0;59;0
WireConnection;76;1;77;0
WireConnection;63;0;61;0
WireConnection;70;0;69;0
WireConnection;71;0;67;0
WireConnection;71;1;70;0
WireConnection;68;0;66;0
WireConnection;68;1;69;0
WireConnection;21;0;16;0
WireConnection;62;0;76;0
WireConnection;62;1;63;0
WireConnection;60;0;76;0
WireConnection;60;1;61;0
WireConnection;99;0;89;0
WireConnection;99;1;100;0
WireConnection;92;0;89;0
WireConnection;92;1;90;0
WireConnection;65;0;62;0
WireConnection;65;2;71;0
WireConnection;64;0;60;0
WireConnection;64;2;68;0
WireConnection;39;0;57;0
WireConnection;39;1;64;0
WireConnection;39;5;73;0
WireConnection;7;0;6;0
WireConnection;7;1;8;0
WireConnection;93;0;92;0
WireConnection;93;1;91;0
WireConnection;58;0;57;0
WireConnection;58;1;65;0
WireConnection;58;5;73;0
WireConnection;28;0;29;0
WireConnection;98;0;99;0
WireConnection;72;0;39;0
WireConnection;72;1;58;0
WireConnection;88;0;79;0
WireConnection;88;1;93;0
WireConnection;3;0;22;0
WireConnection;3;2;5;0
WireConnection;3;1;7;0
WireConnection;24;0;28;0
WireConnection;24;2;5;0
WireConnection;24;1;7;0
WireConnection;97;0;98;0
WireConnection;103;0;88;1
WireConnection;103;1;97;0
WireConnection;74;0;72;0
WireConnection;1;0;3;0
WireConnection;25;0;24;0
WireConnection;27;0;1;0
WireConnection;27;1;25;0
WireConnection;104;0;103;0
WireConnection;85;0;82;0
WireConnection;85;1;86;0
WireConnection;48;0;49;0
WireConnection;30;0;27;0
WireConnection;83;0;85;0
WireConnection;83;1;84;0
WireConnection;94;0;104;0
WireConnection;106;0;105;0
WireConnection;108;0;107;0
WireConnection;108;1;109;0
WireConnection;80;0;79;0
WireConnection;80;1;83;0
WireConnection;115;0;116;0
WireConnection;110;0;106;0
WireConnection;110;1;108;0
WireConnection;50;0;48;0
WireConnection;122;0;115;0
WireConnection;111;0;110;0
WireConnection;95;0;41;0
WireConnection;95;1;96;0
WireConnection;87;0;50;0
WireConnection;87;1;80;0
WireConnection;44;0;43;0
WireConnection;112;0;111;0
WireConnection;51;0;52;0
WireConnection;51;1;87;0
WireConnection;42;0;40;0
WireConnection;42;1;95;0
WireConnection;42;2;44;0
WireConnection;117;0;122;0
WireConnection;20;0;19;0
WireConnection;20;1;31;0
WireConnection;55;0;51;0
WireConnection;45;0;42;0
WireConnection;113;0;112;0
WireConnection;33;0;20;0
WireConnection;33;1;32;0
WireConnection;118;0;117;0
WireConnection;34;0;33;0
WireConnection;53;0;55;0
WireConnection;119;0;46;0
WireConnection;119;1;120;0
WireConnection;119;2;121;0
WireConnection;123;0;18;0
WireConnection;123;1;124;0
WireConnection;123;2;125;0
WireConnection;0;0;119;0
WireConnection;0;1;75;0
WireConnection;0;2;54;0
WireConnection;0;4;38;0
WireConnection;0;11;35;0
WireConnection;0;14;123;0
ASEEND*/
//CHKSM=538BE282049A26E3419542C1C8E43FAB6CC07CB0