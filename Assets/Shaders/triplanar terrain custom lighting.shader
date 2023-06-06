// Made with Amplify Shader Editor v1.9.1.5
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "terrain_custom_lighting"
{
	Properties
	{
		_TriplanarAlbedo("Triplanar Albedo", 2D) = "white" {}
		_Cutoff( "Mask Clip Value", Float ) = 0.1
		_Normal("Normal", 2D) = "bump" {}
		_Metallic("Metallic", 2D) = "white" {}
		_TopAlbedo("Top Albedo", 2D) = "white" {}
		_TopNormal("Top Normal", 2D) = "bump" {}
		_TopMetallic("Top Metallic", 2D) = "white" {}
		_TextureScale("TextureScale", Float) = 1
		[IntRange]_WorldtoObjectSwitch("World to Object Switch", Range( 0 , 1)) = 0
		_CoverageAmount("Coverage Amount", Range( -1 , 1)) = 0
		_CoverageFalloff("Coverage Falloff", Range( 0.01 , 2)) = 0.5
		_Smoothness("Smoothness", Range( 0 , 1)) = 0
		_DepthFadeLength("Depth Fade Length", Float) = 0
		_DepthFadeOffset("Depth Fade Offset", Float) = 0
		_Maskcenter("Mask center", Range( -1 , 1)) = 0
		_MaskWidth("Mask Width", Range( 0 , 1)) = 0
		[Toggle(_MASKENABLED_ON)] _MaskEnabled("Mask Enabled", Float) = 0
		_Heightmap("Heightmap", 2D) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
		[Header(Forward Rendering Options)]
		[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[ToggleOff] _GlossyReflections("Reflections", Float) = 1.0
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" }
		Cull Back
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "UnityStandardUtils.cginc"
		#include "Lighting.cginc"
		#pragma target 5.0
		#pragma shader_feature _SPECULARHIGHLIGHTS_OFF
		#pragma shader_feature _GLOSSYREFLECTIONS_OFF
		#pragma shader_feature_local _MASKENABLED_ON
		#ifdef UNITY_PASS_SHADOWCASTER
			#undef INTERNAL_DATA
			#undef WorldReflectionVector
			#undef WorldNormalVector
			#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
			#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
			#define WorldNormalVector(data,normal) half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))
		#endif
		struct Input
		{
			float eyeDepth;
			float4 screenPos;
			float3 worldNormal;
			INTERNAL_DATA
			float3 worldPos;
		};

		struct SurfaceOutputCustomLightingCustom
		{
			half3 Albedo;
			half3 Normal;
			half3 Emission;
			half Metallic;
			half Smoothness;
			half Occlusion;
			half Alpha;
			Input SurfInput;
			UnityGIInput GIData;
		};

		uniform sampler2D _Heightmap;
		uniform float _DepthFadeLength;
		uniform float _DepthFadeOffset;
		uniform float _Maskcenter;
		uniform float _MaskWidth;
		uniform sampler2D _Normal;
		uniform float _TextureScale;
		uniform sampler2D _TopNormal;
		uniform float _WorldtoObjectSwitch;
		uniform float _CoverageAmount;
		uniform float _CoverageFalloff;
		uniform sampler2D _TriplanarAlbedo;
		uniform sampler2D _TopAlbedo;
		uniform float _Smoothness;
		uniform sampler2D _Metallic;
		uniform sampler2D _TopMetallic;
		uniform float _Cutoff = 0.1;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			o.eyeDepth = -UnityObjectToViewPos( v.vertex.xyz ).z;
		}

		inline half4 LightingStandardCustomLighting( inout SurfaceOutputCustomLightingCustom s, half3 viewDir, UnityGI gi )
		{
			UnityGIInput data = s.GIData;
			Input i = s.SurfInput;
			half4 c = 0;
			#ifdef UNITY_PASS_FORWARDBASE
			float ase_lightAtten = data.atten;
			if( _LightColor0.a == 0)
			ase_lightAtten = 0;
			#else
			float3 ase_lightAttenRGB = gi.light.color / ( ( _LightColor0.rgb ) + 0.000001 );
			float ase_lightAtten = max( max( ase_lightAttenRGB.r, ase_lightAttenRGB.g ), ase_lightAttenRGB.b );
			#endif
			#if defined(HANDLE_SHADOWS_BLENDING_IN_GI)
			half bakedAtten = UnitySampleBakedOcclusion(data.lightmapUV.xy, data.worldPos);
			float zDist = dot(_WorldSpaceCameraPos - data.worldPos, UNITY_MATRIX_V[2].xyz);
			float fadeDist = UnityComputeShadowFadeDistance(data.worldPos, zDist);
			ase_lightAtten = UnityMixRealtimeAndBakedShadows(data.atten, bakedAtten, UnityComputeShadowFade(fadeDist));
			#endif
			float4 color794 = IsGammaSpace() ? float4(1,1,1,1) : float4(1,1,1,1);
			float cameraDepthFade599 = (( i.eyeDepth -_ProjectionParams.y - _DepthFadeOffset ) / _DepthFadeLength);
			float cameraDepthFade782 = (( i.eyeDepth -_ProjectionParams.y - 0.0 ) / 0.0);
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			float4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float temp_output_708_0 = ( _ScreenParams.x / _ScreenParams.y );
			float2 appendResult711 = (float2(temp_output_708_0 , 1.0));
			float2 appendResult715 = (float2(-( temp_output_708_0 * 0.5 ) , -0.5));
			float temp_output_623_0 = saturate( length( ( ( (ase_screenPosNorm).xy * appendResult711 ) + appendResult715 ) ) );
			float4 temp_cast_0 = (( ( saturate( cameraDepthFade599 ) + ( 1.0 - saturate( cameraDepthFade782 ) ) ) + saturate( ( ( temp_output_623_0 - _Maskcenter ) / _MaskWidth ) ) )).xxxx;
			#ifdef _MASKENABLED_ON
				float4 staticSwitch795 = temp_cast_0;
			#else
				float4 staticSwitch795 = color794;
			#endif
			float3 ase_worldPos = i.worldPos;
			float2 appendResult385 = (float2(ase_worldPos.y , ase_worldPos.z));
			float TextureScale379 = _TextureScale;
			float3 ase_worldNormal = WorldNormalVector( i, float3( 0, 0, 1 ) );
			float3 temp_output_374_0 = abs( mul( unity_WorldToObject, float4( ase_worldNormal , 0.0 ) ).xyz );
			float dotResult375 = dot( temp_output_374_0 , float3(1,1,1) );
			float3 BlendComponents377 = ( temp_output_374_0 / dotResult375 );
			float2 appendResult391 = (float2(ase_worldPos.x , ase_worldPos.z));
			float2 appendResult388 = (float2(ase_worldPos.x , ase_worldPos.y));
			float3 temp_output_408_0 = ( ( ( UnpackNormal( tex2D( _Normal, ( appendResult385 * TextureScale379 ) ) ) * BlendComponents377.x ) + ( UnpackNormal( tex2D( _Normal, ( appendResult391 * TextureScale379 ) ) ) * BlendComponents377.y ) ) + ( UnpackNormal( tex2D( _Normal, ( appendResult388 * TextureScale379 ) ) ) * BlendComponents377.z ) );
			float3 ase_vertex3Pos = mul( unity_WorldToObject, float4( i.worldPos , 1 ) );
			float WorldObjectSwitch411 = _WorldtoObjectSwitch;
			float3 lerpResult435 = lerp( ase_worldPos , ase_vertex3Pos , WorldObjectSwitch411);
			float3 break460 = lerpResult435;
			float2 appendResult482 = (float2(break460.x , break460.z));
			float2 temp_output_494_0 = ( appendResult482 * TextureScale379 );
			float temp_output_507_0 = pow( saturate( ( ase_worldNormal.y + _CoverageAmount ) ) , _CoverageFalloff );
			float3 lerpResult511 = lerp( temp_output_408_0 , UnpackNormal( tex2D( _TopNormal, temp_output_494_0 ) ) , temp_output_507_0);
			float3 CalculatedNormal517 = lerpResult511;
			#if defined(LIGHTMAP_ON) && UNITY_VERSION < 560 //aseld
			float3 ase_worldlightDir = 0;
			#else //aseld
			float3 ase_worldlightDir = normalize( UnityWorldSpaceLightDir( ase_worldPos ) );
			#endif //aseld
			float dotResult566 = dot( (WorldNormalVector( i , CalculatedNormal517 )) , ase_worldlightDir );
			#if defined(LIGHTMAP_ON) && ( UNITY_VERSION < 560 || ( defined(LIGHTMAP_SHADOW_MIXING) && !defined(SHADOWS_SHADOWMASK) && defined(SHADOWS_SCREEN) ) )//aselc
			float4 ase_lightColor = 0;
			#else //aselc
			float4 ase_lightColor = _LightColor0;
			#endif //aselc
			UnityGI gi574 = gi;
			float3 diffNorm574 = normalize( WorldNormalVector( i , CalculatedNormal517 ) );
			gi574 = UnityGI_Base( data, 1, diffNorm574 );
			float3 indirectDiffuse574 = gi574.indirect.diffuse + diffNorm574 * 0.0001;
			float2 appendResult448 = (float2(ase_worldPos.y , ase_worldPos.z));
			float2 appendResult444 = (float2(ase_worldPos.x , ase_worldPos.z));
			float2 appendResult452 = (float2(ase_worldPos.x , ase_worldPos.y));
			float3 PixelNormal416 = (WorldNormalVector( i , temp_output_408_0 ));
			float3 lerpResult473 = lerp( PixelNormal416 , mul( unity_WorldToObject, float4( PixelNormal416 , 0.0 ) ).xyz , WorldObjectSwitch411);
			float3 temp_cast_7 = (_CoverageFalloff).xxx;
			float4 lerpResult520 = lerp( ( ( ( tex2D( _TriplanarAlbedo, ( appendResult448 * TextureScale379 ) ) * BlendComponents377.x ) + ( tex2D( _TriplanarAlbedo, ( appendResult444 * TextureScale379 ) ) * BlendComponents377.y ) ) + ( tex2D( _TriplanarAlbedo, ( appendResult452 * TextureScale379 ) ) * BlendComponents377.z ) ) , tex2D( _TopAlbedo, temp_output_494_0 ) , pow( saturate( ( lerpResult473 + _CoverageAmount ) ) , temp_cast_7 ).y);
			float3 indirectNormal579 = normalize( WorldNormalVector( i , CalculatedNormal517 ) );
			Unity_GlossyEnvironmentData g579 = UnityGlossyEnvironmentSetup( _Smoothness, data.worldViewDir, indirectNormal579, float3(0,0,0));
			float3 indirectSpecular579 = UnityGI_IndirectSpecular( data, 1.0, indirectNormal579, g579 );
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldTangent = WorldNormalVector( i, float3( 1, 0, 0 ) );
			float3 ase_worldBitangent = WorldNormalVector( i, float3( 0, 1, 0 ) );
			float3x3 ase_tangentToWorldFast = float3x3(ase_worldTangent.x,ase_worldBitangent.x,ase_worldNormal.x,ase_worldTangent.y,ase_worldBitangent.y,ase_worldNormal.y,ase_worldTangent.z,ase_worldBitangent.z,ase_worldNormal.z);
			float2 appendResult439 = (float2(ase_worldPos.y , ase_worldPos.z));
			float2 appendResult440 = (float2(ase_worldPos.x , ase_worldPos.z));
			float2 appendResult453 = (float2(ase_worldPos.x , ase_worldPos.y));
			float4 lerpResult519 = lerp( ( ( ( tex2D( _Metallic, ( appendResult439 * TextureScale379 ) ) * BlendComponents377.x ) + ( tex2D( _Metallic, ( appendResult440 * TextureScale379 ) ) * BlendComponents377.y ) ) + ( tex2D( _Metallic, ( appendResult453 * TextureScale379 ) ) * BlendComponents377.z ) ) , tex2D( _TopMetallic, temp_output_494_0 ) , temp_output_507_0);
			half3 specColor584 = (0).xxx;
			half oneMinusReflectivity584 = 0;
			half3 diffuseAndSpecularFromMetallic584 = DiffuseAndSpecularFromMetallic(float3( 0,0,0 ),lerpResult519.r,specColor584,oneMinusReflectivity584);
			float fresnelNdotV578 = dot( mul(ase_tangentToWorldFast,CalculatedNormal517), ase_worldViewDir );
			float fresnelNode578 = ( diffuseAndSpecularFromMetallic584.x + _Smoothness * pow( max( 1.0 - fresnelNdotV578 , 0.0001 ), 2.5 ) );
			float4 lerpResult580 = lerp( ( float4( ( ( max( dotResult566 , 0.0 ) * ( ase_lightAtten * ase_lightColor.rgb ) ) + indirectDiffuse574 ) , 0.0 ) * lerpResult520 ) , float4( indirectSpecular579 , 0.0 ) , fresnelNode578);
			c.rgb = lerpResult580.rgb;
			c.a = 1;
			clip( staticSwitch795.r - _Cutoff );
			return c;
		}

		inline void LightingStandardCustomLighting_GI( inout SurfaceOutputCustomLightingCustom s, UnityGIInput data, inout UnityGI gi )
		{
			s.GIData = data;
		}

		void surf( Input i , inout SurfaceOutputCustomLightingCustom o )
		{
			o.SurfInput = i;
			o.Normal = float3(0,0,1);
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf StandardCustomLighting keepalpha fullforwardshadows vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 5.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float1 customPack1 : TEXCOORD1;
				float4 screenPos : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.x = customInputData.eyeDepth;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.screenPos = ComputeScreenPos( o.pos );
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.eyeDepth = IN.customPack1.x;
				float3 worldPos = float3( IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w );
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.worldPos = worldPos;
				surfIN.worldNormal = float3( IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z );
				surfIN.internalSurfaceTtoW0 = IN.tSpace0.xyz;
				surfIN.internalSurfaceTtoW1 = IN.tSpace1.xyz;
				surfIN.internalSurfaceTtoW2 = IN.tSpace2.xyz;
				surfIN.screenPos = IN.screenPos;
				SurfaceOutputCustomLightingCustom o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputCustomLightingCustom, o )
				surf( surfIN, o );
				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT( UnityGI, gi );
				o.Alpha = LightingStandardCustomLighting( o, worldViewDir, gi ).a;
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19105
Node;AmplifyShaderEditor.WorldToObjectMatrix;371;-4113.633,-1419.018;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.WorldNormalVector;370;-4113.633,-1323.018;Inherit;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;372;-3841.633,-1355.018;Inherit;False;2;2;0;FLOAT4x4;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.AbsOpNode;374;-3681.633,-1355.018;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.Vector3Node;373;-3714.751,-1174.568;Float;False;Constant;_Vector0;Vector 0;-1;0;Create;True;0;0;0;False;0;False;1,1,1;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DotProductOpNode;375;-3507.733,-1288.62;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;376;-3345.633,-1355.018;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;377;-3185.633,-1355.018;Float;True;BlendComponents;1;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;378;-3372.008,-1741.797;Inherit;False;Property;_TextureScale;TextureScale;7;0;Create;True;0;0;0;False;0;False;1;0.16;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;409;-2497.633,-587.0175;Float;False;Property;_WorldtoObjectSwitch;World to Object Switch;8;1;[IntRange];Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;379;-3090.284,-1739.068;Float;False;TextureScale;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;381;-2870.57,623.6203;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;382;-2852.436,937.2699;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RegisterLocalVarNode;411;-2161.941,-589.9225;Float;False;WorldObjectSwitch;4;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;413;-1777.633,-1083.018;Inherit;False;224;239;Coverage in World mode;1;418;;1,1,1,1;0;0
Node;AmplifyShaderEditor.BreakToComponentsNode;389;-3151.479,624.5276;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;385;-2598.045,368.9582;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;388;-2663.179,963.4889;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;412;-1777.633,-811.0175;Inherit;False;235.9301;237.3099;Coverage in Object mode;1;419;;1,1,1,1;0;0
Node;AmplifyShaderEditor.DynamicAppendNode;391;-2645.045,646.9586;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WireNode;417;-1871.613,-790.7575;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;390;-2616.205,475.9234;Inherit;False;379;TextureScale;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;384;-2665.998,759.3646;Inherit;False;379;TextureScale;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;387;-3151.479,912.527;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;395;-2459.602,653.2816;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WireNode;396;-2879.479,576.5276;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;392;-2879.479,1088.527;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;393;-2447.809,372.8405;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PosVertexDataNode;419;-1745.633,-747.0175;Inherit;False;0;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldPosInputsNode;418;-1745.633,-1019.018;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WireNode;426;-1823.842,-830.1223;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;421;-2193.085,40.19036;Float;False;Property;_CoverageFalloff;Coverage Falloff;10;0;Create;True;0;0;0;False;0;False;0.5;0.2;0.01;2;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;394;-2458.268,964.0719;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;420;-2204.041,-82.01948;Float;False;Property;_CoverageAmount;Coverage Amount;9;0;Create;True;0;0;0;False;0;False;0;-0.5;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;435;-1505.633,-923.0175;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode;443;-1999.481,352.5272;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;402;-3151.479,768.5276;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.WireNode;445;-1809.633,180.9825;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;397;-2847.479,544.5276;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;400;-2847.479,1120.527;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;460;-1345.633,-923.0175;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.WireNode;457;-1663.48,464.5274;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;458;-1724.48,275.5268;Inherit;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;404;-1983.481,496.5276;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;405;-1983.481,768.5276;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode;459;-1471.48,608.5276;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;735;-220.552,-2727.921;Inherit;False;2272.764;768.0081;Uses screen position instead of a centered texture;19;707;708;615;726;733;711;616;715;706;617;619;620;623;622;626;745;770;769;746;Screen Space Center Circle;1,1,1,1;0;0
Node;AmplifyShaderEditor.WireNode;406;-1759.481,912.527;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;477;-1503.48,448.5274;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;407;-1743.48,608.5276;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode;478;-1455.48,704.5279;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;476;-1266.801,-728.2653;Inherit;False;379;TextureScale;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;482;-1120.999,-910.1867;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ScreenParams;707;-170.5518,-2422.157;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;493;-1423.48,736.5278;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;494;-973.4049,-868.3483;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SaturateNode;495;-1357.48,614.5276;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;507;-1140.48,602.5276;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldNormalVector;410;-302.3934,984.6539;Inherit;False;False;1;0;FLOAT3;0,0,0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleDivideOpNode;708;51.37399,-2386.389;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;726;196.8129,-2342.436;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;615;-157.4468,-2677.921;Float;True;0;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldPosInputsNode;434;855.7709,309.2608;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.BreakToComponentsNode;415;-2881.633,-1227.018;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.WorldPosInputsNode;425;875.3988,-55.31489;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CommentaryNode;414;-2366.342,-485.8906;Inherit;False;436.2993;336.8007;Coverage in Object mode;3;456;432;430;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;416;-16.53117,949.3169;Float;True;PixelNormal;3;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WorldPosInputsNode;424;868.5409,566.3531;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;422;278.0788,427.9201;Inherit;False;377;BlendComponents;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;511;-806.1334,774.415;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;711;249.74,-2486.389;Inherit;False;FLOAT2;4;0;FLOAT;1;False;1;FLOAT;1;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.NegateNode;733;379.2128,-2334.355;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;616;130.196,-2642.341;Inherit;False;True;True;False;False;1;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;439;1073.769,15.85662;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;437;999.9159,713.586;Inherit;False;379;TextureScale;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;440;1081.513,298.3509;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BreakToComponentsNode;442;614.0788,571.9194;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;453;1091.379,580.8813;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BreakToComponentsNode;431;-2881.633,-1515.018;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.GetLocalVarNode;441;1049.01,412.8255;Inherit;False;379;TextureScale;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldToObjectMatrix;432;-2331.643,-387.9796;Inherit;False;0;1;FLOAT4x4;0
Node;AmplifyShaderEditor.GetLocalVarNode;430;-2332.613,-285.1566;Inherit;False;416;PixelNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;423;-2264.54,-848.5224;Inherit;False;317.8;243.84;Coverage in World mode;1;438;;1,1,1,1;0;0
Node;AmplifyShaderEditor.GetLocalVarNode;436;1039.571,133.3981;Inherit;False;379;TextureScale;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;454;614.0788,283.9201;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.DynamicAppendNode;715;550.0748,-2342.222;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;-0.5;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;456;-2089.043,-365.7895;Inherit;False;2;2;0;FLOAT4x4;0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;470;1226.312,606.5031;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;706;445.9885,-2643.724;Inherit;True;2;2;0;FLOAT2;0,0;False;1;FLOAT2;1,1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BreakToComponentsNode;450;-2881.633,-1371.018;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.WireNode;465;886.079,747.9194;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;468;886.079,235.9201;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;591;617.5495,-417.3223;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;469;1233.967,30.31512;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WireNode;449;-2694.633,-1599.018;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;471;1217.406,309.7425;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;438;-2195.519,-741.4548;Inherit;False;416;PixelNormal;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;486;1382.077,299.9201;Inherit;True;Property;_TextureSample4;Texture Sample 4;3;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;False;Instance;488;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;592;890.5839,-493.959;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;474;614.0788,427.9201;Inherit;False;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.LerpOp;473;-1871.634,-552.0175;Inherit;True;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.WireNode;467;-1841.634,-1323.018;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;489;918.079,203.9201;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;472;-1759.541,-117.2225;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;463;-1841.634,-1595.018;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;490;918.079,779.919;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;475;1382.077,603.9194;Inherit;True;Property;_TextureSample2;Texture Sample 2;3;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;False;Instance;488;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;464;-1841.634,-1051.018;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;617;702.1938,-2629.167;Inherit;True;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;488;1381.648,27.91936;Inherit;True;Property;_Metallic;Metallic;3;0;Create;True;0;0;0;False;0;False;-1;32b32591980b5bd4aa89d5026a213a96;32b32591980b5bd4aa89d5026a213a96;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LengthOpNode;619;1013.932,-2403.771;Inherit;True;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;496;1782.077,731.9194;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;484;-2096.631,-1250.519;Inherit;True;Property;_TextureSample3;Texture Sample 3;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;481;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;485;-1490.641,62.17852;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;480;-2099.416,-1528.456;Inherit;True;Property;_TextureSample1;Texture Sample 1;0;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Instance;481;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldSpaceLightDirHlpNode;565;2833.594,-816.1432;Inherit;False;False;1;0;FLOAT;0;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SamplerNode;481;-2094.333,-1783.918;Inherit;True;Property;_TriplanarAlbedo;Triplanar Albedo;0;0;Create;True;0;0;0;False;0;False;-1;d0c46703cf1657e489200a11a5d5c760;d0c46703cf1657e489200a11a5d5c760;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WireNode;491;-1809.633,-1355.018;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;479;-1809.633,-1083.018;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;483;-1809.633,-1627.018;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;487;-1617.633,-540.0175;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;502;1782.077,427.9201;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;497;1782.077,155.9201;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.WorldNormalVector;564;2834.251,-1051.569;Inherit;True;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.CameraDepthFade;782;2200.242,-2148.813;Inherit;False;3;2;FLOAT3;0,0,0;False;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;620;1085,-2124.759;Inherit;False;Property;_Maskcenter;Mask center;16;0;Create;True;0;0;0;False;0;False;0;0.5;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;623;1365.547,-2352.469;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;600;2219.784,-2229.1;Inherit;False;Property;_DepthFadeOffset;Depth Fade Offset;15;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LightAttenuation;568;3089.168,-803.4509;Inherit;True;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;601;2235.375,-2303.021;Inherit;False;Property;_DepthFadeLength;Depth Fade Length;14;0;Create;True;0;0;0;False;0;False;0;16;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LightColorNode;585;3128.55,-619.1851;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.DotProductOpNode;566;3157.898,-1032.36;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;503;2006.077,571.9194;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;498;-1729.633,-1803.018;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;506;2022.078,267.9201;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;501;-1473.633,-532.0175;Inherit;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;499;-1729.633,-1531.018;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;500;-1729.633,-1291.018;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;492;-1399.841,-33.02155;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;509;-1489.633,-1691.018;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;516;2185.198,683.9194;Inherit;True;Property;_TopMetallic;Top Metallic;6;0;Create;True;0;0;0;True;0;False;-1;8d63b7caa4e75dd4aa5e84f4b97befa6;8d63b7caa4e75dd4aa5e84f4b97befa6;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;586;3433.396,-750.524;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMaxOpNode;567;3426.599,-887.3232;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;508;-1473.633,-1339.018;Inherit;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;783;2442.697,-2138.952;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;622;1085.159,-2036.163;Inherit;False;Property;_MaskWidth;Mask Width;17;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;745;1601.019,-2207.153;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CameraDepthFade;599;2468.864,-2267.859;Inherit;False;3;2;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PowerNode;504;-1297.633,-539.0175;Inherit;True;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;514;2262.078,459.9205;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;781;2703.448,-2269.609;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;746;1763.019,-2203.153;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;569;3718.68,-800.2327;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BreakToComponentsNode;515;-922.7334,-540.3176;Inherit;True;FLOAT3;1;0;FLOAT3;0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.SimpleAddOpNode;513;-1233.633,-1435.018;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.LerpOp;519;2790.078,459.9205;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.OneMinusNode;792;2663.192,-2102.288;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.IndirectDiffuseLighting;574;3320.969,-395.959;Inherit;True;Tangent;1;0;FLOAT3;0,0,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;791;2970.838,-2150.443;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;587;3072.759,188.0259;Inherit;False;Property;_Smoothness;Smoothness;13;0;Create;True;0;0;0;False;0;False;0;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;626;1890.317,-2183.58;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;520;1192.072,-918.3181;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.DiffuseAndSpecularFromMetallicNode;584;3061.943,288.0414;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;3;FLOAT3;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleAddOpNode;576;3987.497,-611.3776;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;773;3269.767,-2104.781;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;794;3405.661,-2220.731;Inherit;False;Constant;_Color0;Color 0;19;0;Create;True;0;0;0;False;0;False;1,1,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FresnelNode;578;3516.923,176.6965;Inherit;True;Standard;TangentNormal;ViewDir;False;True;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0.04;False;2;FLOAT;1;False;3;FLOAT;2.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.IndirectSpecularLight;579;3583.572,-119.9788;Inherit;True;Tangent;3;0;FLOAT3;0,0,1;False;1;FLOAT;0.2;False;2;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;577;4151.164,-361.1165;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.CommentaryNode;734;-143.4636,-1816.668;Inherit;False;1687.703;564.0649;Alternative method if I want something more than a centered circle;10;544;633;693;679;692;637;695;665;639;689;Screen Space Center Texture;1,1,1,1;0;0
Node;AmplifyShaderEditor.ScreenParams;633;6.618426,-1523.526;Inherit;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;693;686.8008,-1659.003;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;665;439.6938,-1679.581;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;692;599.8008,-1429.003;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;1080;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;695;400.8005,-1473.003;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;637;234.1506,-1547.27;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;546;3085.569,445.6241;Float;True;PixelMetallic;3;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;543;-3381.328,-1629.113;Inherit;False;Property;_NormalBlendStrength;Normal Blend Strength;11;0;Create;True;0;0;0;False;0;False;0;0.69;0;10;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;689;212.8005,-1429.003;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;777;2068.695,-1793.275;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;539;-3054.091,-1621.865;Float;False;normalBlendStrength;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;639;940.8407,-1568.552;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;769;1597.606,-2076.015;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.24;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;771;1913.95,-1933.525;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;580;4469.979,-175.7229;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;770;1746.95,-2060.525;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;795;3757.299,-1982.386;Inherit;False;Property;_MaskEnabled;Mask Enabled;18;0;Create;True;0;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;True;True;All;9;1;COLOR;0,0,0,0;False;0;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;5;COLOR;0,0,0,0;False;6;COLOR;0,0,0,0;False;7;COLOR;0,0,0,0;False;8;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GrabScreenPosition;679;-1.841597,-1708.777;Inherit;False;0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;544;1140.927,-1579.2;Inherit;True;Property;_Circle;Circle;12;0;Create;True;0;0;0;False;0;False;544;df6320c088d01954aa72efe451df3185;39b0b77200e99f149b32fff7debeb0bf;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;4960.032,-554.9289;Float;False;True;-1;7;ASEMaterialInspector;0;0;CustomLighting;terrain_custom_lighting;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;True;True;False;Back;0;False;;0;False;;False;0;False;;0;False;;False;0;Custom;0.1;True;True;0;True;TransparentCutout;;Geometry;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;0;5;False;;10;False;;0;5;False;;10;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;1;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.GetLocalVarNode;383;-3487.479,768.5276;Inherit;False;377;BlendComponents;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;505;-1580.36,1024.527;Inherit;True;Property;_TopNormal;Top Normal;5;0;Create;True;0;0;0;False;0;False;-1;219386670151bc542a3b2fd69819e13e;219386670151bc542a3b2fd69819e13e;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;510;-827.6334,-940.0177;Inherit;True;Property;_TopAlbedo;Top Albedo;4;0;Create;True;0;0;0;False;0;False;-1;216e2729631e003468211108545a08f1;216e2729631e003468211108545a08f1;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;517;-595.9623,746.077;Float;True;CalculatedNormal;2;False;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;408;-1503.48,800.528;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;403;-1983.481,1072.527;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;398;-2299.481,936.527;Inherit;True;Property;_TextureSample5;Texture Sample 5;2;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Instance;399;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;386;-2675.664,1072.155;Inherit;False;379;TextureScale;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;401;-2321.481,647.5276;Inherit;True;Property;_TextureSample0;Texture Sample 0;2;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Instance;399;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;399;-2318.481,362.5272;Inherit;True;Property;_Normal;Normal;2;0;Create;True;0;0;0;False;0;False;-1;f1f5f9dada5ebb1438b0bc6b1fdc924f;f1f5f9dada5ebb1438b0bc6b1fdc924f;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.WorldPosInputsNode;380;-2823.767,343.5408;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.TextureCoordinatesNode;802;-4011.178,-859.1061;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;803;-4150.354,-436.3683;Inherit;False;Property;_HeightmapScale;Heightmap Scale;20;0;Create;True;0;0;0;False;0;False;0;1;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;804;-4076.353,-319.3683;Inherit;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ParallaxOcclusionMappingNode;797;-3453.122,-670.3751;Inherit;False;0;8;False;;16;False;;2;0.02;0;False;1,1;False;0,0;8;0;FLOAT2;0,0;False;1;SAMPLER2D;;False;7;SAMPLERSTATE;;False;2;FLOAT;0.02;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT2;0,0;False;6;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;807;-4205.172,0.01284266;Inherit;False;Property;_CurvatureV;Curvature V;22;0;Create;True;0;0;0;False;0;False;0;0;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;805;-4254.252,-95.16824;Inherit;False;Property;_CurvatureU;Curvature U;21;0;Create;True;0;0;0;False;0;False;0;0;0;100;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;808;-3947.171,-105.9871;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;809;-3144.527,-665.5774;Inherit;False;parallaxUVs;-1;True;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DdxOpNode;810;-3441.693,-390.9695;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DdyOpNode;811;-3448.745,-284.8651;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WorldPosInputsNode;428;-2994.32,-1920.566;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;447;-2786.124,-1752.124;Inherit;False;379;TextureScale;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;448;-2752.618,-1873.826;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;466;-2576.728,-1897.207;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TexturePropertyNode;801;-4062.292,-663.2515;Inherit;True;Property;_Heightmap;Heightmap;19;0;Create;True;0;0;0;True;0;False;None;47a4eeb183efe2f4fa6a6a799d6b527d;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.WorldPosInputsNode;427;-2698.254,-1560.15;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;451;-2534.376,-1408.699;Inherit;False;379;TextureScale;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;444;-2490.173,-1532.492;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WireNode;429;-2778.633,-1021.018;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;446;-2746.633,-989.0179;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;433;-2754.639,-1260.667;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.GetLocalVarNode;455;-2612.379,-1086.285;Inherit;False;379;TextureScale;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;452;-2486.698,-1242.386;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;462;-2351.983,-1161.368;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;461;-2337.405,-1548.869;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
WireConnection;372;0;371;0
WireConnection;372;1;370;0
WireConnection;374;0;372;0
WireConnection;375;0;374;0
WireConnection;375;1;373;0
WireConnection;376;0;374;0
WireConnection;376;1;375;0
WireConnection;377;0;376;0
WireConnection;379;0;378;0
WireConnection;411;0;409;0
WireConnection;389;0;383;0
WireConnection;385;0;380;2
WireConnection;385;1;380;3
WireConnection;388;0;382;1
WireConnection;388;1;382;2
WireConnection;391;0;381;1
WireConnection;391;1;381;3
WireConnection;417;0;411;0
WireConnection;387;0;383;0
WireConnection;395;0;391;0
WireConnection;395;1;384;0
WireConnection;396;0;389;0
WireConnection;392;0;387;2
WireConnection;393;0;385;0
WireConnection;393;1;390;0
WireConnection;426;0;417;0
WireConnection;394;0;388;0
WireConnection;394;1;386;0
WireConnection;435;0;418;0
WireConnection;435;1;419;0
WireConnection;435;2;426;0
WireConnection;443;0;421;0
WireConnection;402;0;383;0
WireConnection;445;0;420;0
WireConnection;397;0;396;0
WireConnection;400;0;392;0
WireConnection;460;0;435;0
WireConnection;457;0;445;0
WireConnection;404;0;399;0
WireConnection;404;1;397;0
WireConnection;405;0;401;0
WireConnection;405;1;402;1
WireConnection;459;0;443;0
WireConnection;406;0;403;0
WireConnection;477;0;458;2
WireConnection;477;1;457;0
WireConnection;407;0;404;0
WireConnection;407;1;405;0
WireConnection;478;0;459;0
WireConnection;482;0;460;0
WireConnection;482;1;460;2
WireConnection;493;0;478;0
WireConnection;494;0;482;0
WireConnection;494;1;476;0
WireConnection;495;0;477;0
WireConnection;507;0;495;0
WireConnection;507;1;493;0
WireConnection;410;0;408;0
WireConnection;708;0;707;1
WireConnection;708;1;707;2
WireConnection;726;0;708;0
WireConnection;415;0;377;0
WireConnection;416;0;410;0
WireConnection;511;0;408;0
WireConnection;511;1;505;0
WireConnection;511;2;507;0
WireConnection;711;0;708;0
WireConnection;733;0;726;0
WireConnection;616;0;615;0
WireConnection;439;0;425;2
WireConnection;439;1;425;3
WireConnection;440;0;434;1
WireConnection;440;1;434;3
WireConnection;442;0;422;0
WireConnection;453;0;424;1
WireConnection;453;1;424;2
WireConnection;431;0;377;0
WireConnection;454;0;422;0
WireConnection;715;0;733;0
WireConnection;456;0;432;0
WireConnection;456;1;430;0
WireConnection;470;0;453;0
WireConnection;470;1;437;0
WireConnection;706;0;616;0
WireConnection;706;1;711;0
WireConnection;450;0;377;0
WireConnection;465;0;442;2
WireConnection;468;0;454;0
WireConnection;591;0;517;0
WireConnection;469;0;439;0
WireConnection;469;1;436;0
WireConnection;449;0;431;0
WireConnection;471;0;440;0
WireConnection;471;1;441;0
WireConnection;486;1;471;0
WireConnection;592;0;591;0
WireConnection;474;0;422;0
WireConnection;473;0;438;0
WireConnection;473;1;456;0
WireConnection;473;2;411;0
WireConnection;467;0;450;1
WireConnection;489;0;468;0
WireConnection;472;0;420;0
WireConnection;463;0;449;0
WireConnection;490;0;465;0
WireConnection;475;1;470;0
WireConnection;464;0;446;0
WireConnection;617;0;706;0
WireConnection;617;1;715;0
WireConnection;488;1;469;0
WireConnection;619;0;617;0
WireConnection;496;0;475;0
WireConnection;496;1;490;0
WireConnection;484;1;462;0
WireConnection;485;0;421;0
WireConnection;480;1;461;0
WireConnection;481;1;466;0
WireConnection;491;0;467;0
WireConnection;479;0;464;0
WireConnection;483;0;463;0
WireConnection;487;0;473;0
WireConnection;487;1;472;0
WireConnection;502;0;486;0
WireConnection;502;1;474;1
WireConnection;497;0;488;0
WireConnection;497;1;489;0
WireConnection;564;0;592;0
WireConnection;623;0;619;0
WireConnection;566;0;564;0
WireConnection;566;1;565;0
WireConnection;503;0;496;0
WireConnection;498;0;481;0
WireConnection;498;1;483;0
WireConnection;506;0;497;0
WireConnection;506;1;502;0
WireConnection;501;0;487;0
WireConnection;499;0;480;0
WireConnection;499;1;491;0
WireConnection;500;0;484;0
WireConnection;500;1;479;0
WireConnection;492;0;485;0
WireConnection;509;0;498;0
WireConnection;509;1;499;0
WireConnection;516;1;494;0
WireConnection;586;0;568;0
WireConnection;586;1;585;1
WireConnection;567;0;566;0
WireConnection;508;0;500;0
WireConnection;783;0;782;0
WireConnection;745;0;623;0
WireConnection;745;1;620;0
WireConnection;599;0;601;0
WireConnection;599;1;600;0
WireConnection;504;0;501;0
WireConnection;504;1;492;0
WireConnection;514;0;506;0
WireConnection;514;1;503;0
WireConnection;781;0;599;0
WireConnection;746;0;745;0
WireConnection;746;1;622;0
WireConnection;569;0;567;0
WireConnection;569;1;586;0
WireConnection;515;0;504;0
WireConnection;513;0;509;0
WireConnection;513;1;508;0
WireConnection;519;0;514;0
WireConnection;519;1;516;0
WireConnection;519;2;507;0
WireConnection;792;0;783;0
WireConnection;574;0;592;0
WireConnection;791;0;781;0
WireConnection;791;1;792;0
WireConnection;626;0;746;0
WireConnection;520;0;513;0
WireConnection;520;1;510;0
WireConnection;520;2;515;1
WireConnection;584;1;519;0
WireConnection;576;0;569;0
WireConnection;576;1;574;0
WireConnection;773;0;791;0
WireConnection;773;1;626;0
WireConnection;578;0;592;0
WireConnection;578;1;584;0
WireConnection;578;2;587;0
WireConnection;579;0;592;0
WireConnection;579;1;587;0
WireConnection;577;0;576;0
WireConnection;577;1;520;0
WireConnection;693;0;665;0
WireConnection;693;1;692;0
WireConnection;665;0;679;1
WireConnection;665;1;637;0
WireConnection;692;0;689;0
WireConnection;692;1;695;0
WireConnection;695;0;633;1
WireConnection;637;0;633;1
WireConnection;637;1;633;2
WireConnection;546;0;519;0
WireConnection;689;0;633;3
WireConnection;689;1;633;4
WireConnection;777;0;771;0
WireConnection;539;0;543;0
WireConnection;639;0;693;0
WireConnection;639;1;679;2
WireConnection;639;2;679;3
WireConnection;639;3;679;4
WireConnection;769;0;623;0
WireConnection;771;0;770;0
WireConnection;580;0;577;0
WireConnection;580;1;579;0
WireConnection;580;2;578;0
WireConnection;770;0;769;0
WireConnection;795;1;794;0
WireConnection;795;0;773;0
WireConnection;544;1;639;0
WireConnection;0;10;795;0
WireConnection;0;13;580;0
WireConnection;505;1;494;0
WireConnection;510;1;494;0
WireConnection;517;0;511;0
WireConnection;408;0;407;0
WireConnection;408;1;406;0
WireConnection;403;0;398;0
WireConnection;403;1;400;0
WireConnection;398;1;394;0
WireConnection;401;1;395;0
WireConnection;399;1;393;0
WireConnection;797;0;802;0
WireConnection;797;1;801;0
WireConnection;797;2;803;0
WireConnection;797;3;804;0
WireConnection;808;0;805;0
WireConnection;808;1;807;0
WireConnection;809;0;797;0
WireConnection;810;0;802;0
WireConnection;811;0;802;0
WireConnection;448;0;428;2
WireConnection;448;1;428;3
WireConnection;466;0;448;0
WireConnection;466;1;447;0
WireConnection;444;0;427;1
WireConnection;444;1;427;3
WireConnection;429;0;415;2
WireConnection;446;0;429;0
WireConnection;452;0;433;1
WireConnection;452;1;433;2
WireConnection;462;0;452;0
WireConnection;462;1;455;0
WireConnection;461;0;444;0
WireConnection;461;1;451;0
ASEEND*/
//CHKSM=A873C63F51232EFE3033D0519402217B21061C57