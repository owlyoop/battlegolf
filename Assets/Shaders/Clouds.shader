// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "Clouds"
{
	Properties
	{
		_NoiseScale("Noise Scale", Float) = 1
		_Speed("Speed", Float) = 1
		_TessellationFactor("Tessellation Factor", Int) = 1
		_TessellationMinDistance("Tessellation Min Distance", Float) = 0
		_TessellationMaxDistance("Tessellation Max Distance", Float) = 0
		_VertexOffsetScale("Vertex Offset Scale", Float) = 1
		_Remap("Remap", Vector) = (0,1,-1,1)
		_CloudColor1("Cloud Color 1", Color) = (0,0,0,0)
		_CloudColor2("Cloud Color 2", Color) = (1,1,1,0)
		_SmoothstepMin("Smoothstep Min", Float) = 0
		_SmoothstepMax("Smoothstep Max", Float) = 0
		_NoiseExponent("Noise Exponent", Float) = 0
		_BaseNoiseScale("Base Noise Scale", Float) = 0
		_BaseNoiseSpeed("Base Noise Speed", Float) = 0
		_BasenoiseStrength("Base noise Strength", Float) = 1
		_ColorMultiply("Color Multiply", Float) = 1
		_CurvatureRadius("Curvature Radius", Float) = 180
		_FresnelPower("Fresnel Power", Float) = 1
		_FresnelOpacity("Fresnel Opacity", Float) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Off
		ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "Tessellation.cginc"
		#pragma target 5.0
		#pragma surface surf Unlit keepalpha noshadow vertex:vertexDataFunc tessellate:tessFunction 
		struct Input
		{
			float3 worldPos;
			float3 worldNormal;
		};

		uniform float _SmoothstepMin;
		uniform float _SmoothstepMax;
		uniform float _Speed;
		uniform float _NoiseScale;
		uniform float _NoiseExponent;
		uniform float4 _Remap;
		uniform float _BaseNoiseSpeed;
		uniform float _BaseNoiseScale;
		uniform float _BasenoiseStrength;
		uniform float _VertexOffsetScale;
		uniform float _CurvatureRadius;
		uniform float _FresnelPower;
		uniform float _FresnelOpacity;
		uniform float4 _CloudColor1;
		uniform float4 _CloudColor2;
		uniform float _ColorMultiply;
		uniform float _TessellationMinDistance;
		uniform float _TessellationMaxDistance;
		uniform int _TessellationFactor;


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


		float4 tessFunction( appdata_full v0, appdata_full v1, appdata_full v2 )
		{
			return UnityDistanceBasedTess( v0.vertex, v1.vertex, v2.vertex, _TessellationMinDistance,_TessellationMaxDistance,(float)_TessellationFactor);
		}

		void vertexDataFunc( inout appdata_full v )
		{
			float2 temp_cast_0 = (_Speed).xx;
			float3 ase_worldPos = mul( unity_ObjectToWorld, v.vertex );
			float2 appendResult4 = (float2(ase_worldPos.x , ase_worldPos.z));
			float2 panner7 = ( _Time.y * temp_cast_0 + appendResult4);
			float simplePerlin2D1 = snoise( panner7*_NoiseScale );
			simplePerlin2D1 = simplePerlin2D1*0.5 + 0.5;
			float simplePerlin2D25 = snoise( appendResult4*_NoiseScale );
			simplePerlin2D25 = simplePerlin2D25*0.5 + 0.5;
			float4 break36 = _Remap;
			float smoothstepResult49 = smoothstep( _SmoothstepMin , _SmoothstepMax , abs( (break36.z + (pow( saturate( ( ( simplePerlin2D1 + simplePerlin2D25 ) / 2.0 ) ) , _NoiseExponent ) - break36.x) * (break36.w - break36.z) / (break36.y - break36.x)) ));
			float2 temp_cast_1 = (_BaseNoiseSpeed).xx;
			float2 panner58 = ( _Time.y * temp_cast_1 + appendResult4);
			float simplePerlin2D56 = snoise( panner58*_BaseNoiseScale );
			simplePerlin2D56 = simplePerlin2D56*0.5 + 0.5;
			float cloudNoise28 = ( ( smoothstepResult49 + ( simplePerlin2D56 * _BasenoiseStrength ) ) / ( 1.0 + _BasenoiseStrength ) );
			float3 ase_vertexNormal = v.normal.xyz;
			float4 transform73 = mul(unity_WorldToObject,float4( 0,0,0,1 ));
			float3 vertexOffset86 = ( ( cloudNoise28 * ( float3(0,1,0) * _VertexOffsetScale ) ) + ( ase_vertexNormal * pow( ( distance( float4( ase_worldPos , 0.0 ) , transform73 ) / _CurvatureRadius ) , 3.0 ) ) );
			v.vertex.xyz += vertexOffset86;
			v.vertex.w = 1;
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 temp_cast_0 = (_Speed).xx;
			float3 ase_worldPos = i.worldPos;
			float2 appendResult4 = (float2(ase_worldPos.x , ase_worldPos.z));
			float2 panner7 = ( _Time.y * temp_cast_0 + appendResult4);
			float simplePerlin2D1 = snoise( panner7*_NoiseScale );
			simplePerlin2D1 = simplePerlin2D1*0.5 + 0.5;
			float simplePerlin2D25 = snoise( appendResult4*_NoiseScale );
			simplePerlin2D25 = simplePerlin2D25*0.5 + 0.5;
			float4 break36 = _Remap;
			float smoothstepResult49 = smoothstep( _SmoothstepMin , _SmoothstepMax , abs( (break36.z + (pow( saturate( ( ( simplePerlin2D1 + simplePerlin2D25 ) / 2.0 ) ) , _NoiseExponent ) - break36.x) * (break36.w - break36.z) / (break36.y - break36.x)) ));
			float2 temp_cast_1 = (_BaseNoiseSpeed).xx;
			float2 panner58 = ( _Time.y * temp_cast_1 + appendResult4);
			float simplePerlin2D56 = snoise( panner58*_BaseNoiseScale );
			simplePerlin2D56 = simplePerlin2D56*0.5 + 0.5;
			float cloudNoise28 = ( ( smoothstepResult49 + ( simplePerlin2D56 * _BasenoiseStrength ) ) / ( 1.0 + _BasenoiseStrength ) );
			float3 ase_worldViewDir = normalize( UnityWorldSpaceViewDir( ase_worldPos ) );
			float3 ase_worldNormal = i.worldNormal;
			float fresnelNdotV84 = dot( ase_worldNormal, ase_worldViewDir );
			float fresnelNode84 = ( 0.0 + 1.0 * pow( max( 1.0 - fresnelNdotV84 , 0.0001 ), _FresnelPower ) );
			float4 lerpResult39 = lerp( _CloudColor1 , _CloudColor2 , cloudNoise28);
			float4 cloudColor47 = ( lerpResult39 * _ColorMultiply );
			o.Emission = ( ( ( cloudNoise28 * fresnelNode84 ) * _FresnelOpacity ) + cloudColor47 ).rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18900
0;0;1920;1019;2465.561;-106.5493;1.90405;True;True
Node;AmplifyShaderEditor.CommentaryNode;40;-4293.879,-1036.223;Inherit;False;3261.683;1807.286;Comment;30;5;58;57;59;56;25;1;52;55;28;49;51;50;33;32;36;35;54;31;27;7;9;6;4;3;60;61;62;63;64;Noise Generator;1,1,1,1;0;0
Node;AmplifyShaderEditor.WorldPosInputsNode;3;-4245.936,-958.8409;Inherit;True;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;4;-3968.861,-957.424;Inherit;True;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleTimeNode;6;-3996.696,-583.1624;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-3959.833,-714.5666;Inherit;False;Property;_Speed;Speed;2;0;Create;True;0;0;0;False;0;False;1;100;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;7;-3665.833,-969.5667;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;5;-3655.611,-703.6758;Inherit;False;Property;_NoiseScale;Noise Scale;1;0;Create;True;0;0;0;False;0;False;1;0.0065;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;25;-3406.258,-650.4048;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;1;-3408.783,-920.9299;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;0.1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;27;-3070.848,-710.5239;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;31;-2926.775,-711.4606;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;54;-2948.345,-518.6546;Inherit;False;Property;_NoiseExponent;Noise Exponent;12;0;Create;True;0;0;0;False;0;False;0;1.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;35;-2908.96,-308.4688;Inherit;False;Property;_Remap;Remap;7;0;Create;True;0;0;0;False;0;False;0,1,-1,1;0,1,-1,1;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SaturateNode;55;-2800.648,-682.0335;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.BreakToComponentsNode;36;-2683.96,-354.4688;Inherit;False;FLOAT4;1;0;FLOAT4;0,0,0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.PowerNode;52;-2652.452,-616.3441;Inherit;False;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;59;-3971.917,-163.0013;Inherit;False;Property;_BaseNoiseSpeed;Base Noise Speed;14;0;Create;True;0;0;0;False;0;False;0;100;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;32;-2490.045,-669.0705;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-1;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;57;-3635.263,-30.396;Inherit;False;Property;_BaseNoiseScale;Base Noise Scale;13;0;Create;True;0;0;0;False;0;False;0;0.0045;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;58;-3635.917,-172.0013;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.AbsOpNode;33;-2242.868,-745.6605;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;62;-3173.234,113.1456;Inherit;False;Property;_BasenoiseStrength;Base noise Strength;15;0;Create;True;0;0;0;False;0;False;1;2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;51;-2300.196,-420.5692;Inherit;False;Property;_SmoothstepMax;Smoothstep Max;11;0;Create;True;0;0;0;False;0;False;0;1.5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.NoiseGeneratorNode;56;-3384.4,-159.8904;Inherit;True;Simplex2D;True;False;2;0;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;50;-2300.196,-492.5693;Inherit;False;Property;_SmoothstepMin;Smoothstep Min;10;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;49;-2030.015,-723.3208;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;61;-2905.229,-60.28137;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;85;-4288.726,2279.382;Inherit;False;1794.78;1004.571;Comment;15;73;70;83;74;22;82;21;81;80;29;23;24;75;78;86;Vertex Offset;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleAddOpNode;60;-1733.695,-276.7235;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;64;-1976.868,-47.71744;Inherit;False;2;2;0;FLOAT;1;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;70;-4238.726,2888.817;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldToObjectTransfNode;73;-4196.074,3076.953;Inherit;False;1;0;FLOAT4;0,0,0,1;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;83;-3860.928,3087.052;Inherit;False;Property;_CurvatureRadius;Curvature Radius;17;0;Create;True;0;0;0;False;0;False;180;180;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DistanceOpNode;74;-3853.075,2918.953;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;63;-1562.368,-285.8174;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;82;-3596.342,2961.644;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;28;-1270.59,-460.0281;Inherit;True;cloudNoise;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;22;-3524.921,2601.581;Inherit;False;Property;_VertexOffsetScale;Vertex Offset Scale;6;0;Create;True;0;0;0;False;0;False;1;48;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;46;-4376.515,1164.972;Inherit;False;1827.13;907.9384;Comment;15;68;99;47;91;93;100;92;90;88;84;89;41;39;44;42;Colorize;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector3Node;21;-3531.807,2392.191;Inherit;False;Constant;_WaveUpDir;Wave Up Dir;4;0;Create;True;0;0;0;False;0;False;0,1,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ColorNode;44;-4280.515,1404.972;Inherit;False;Property;_CloudColor2;Cloud Color 2;9;0;Create;True;0;0;0;False;0;False;1,1,1,0;0.7649519,0.8262783,0.8490566,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;89;-3855.034,1441.424;Inherit;False;Property;_FresnelPower;Fresnel Power;18;0;Create;True;0;0;0;False;0;False;1;8;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-3235.736,2512.612;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.PowerNode;81;-3438.798,2963.251;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;3;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;42;-4232.515,1212.972;Inherit;False;Property;_CloudColor1;Cloud Color 1;8;0;Create;True;0;0;0;False;0;False;0,0,0,0;0.2322891,0.3032995,0.5660378,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.NormalVertexDataNode;80;-3276.452,2687.959;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;41;-4274.515,1793.972;Inherit;False;28;cloudNoise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;29;-3264.329,2379.377;Inherit;False;28;cloudNoise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;75;-3050.853,2864.333;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;39;-3896.636,1577.627;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;68;-3670.778,1950.671;Inherit;False;Property;_ColorMultiply;Color Multiply;16;0;Create;True;0;0;0;False;0;False;1;1.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;100;-3479.637,1209.217;Inherit;False;28;cloudNoise;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.FresnelNode;84;-3562.094,1322.398;Inherit;True;Standard;WorldNormal;ViewDir;True;True;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;24;-2874.602,2581.845;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;78;-2831.928,2860.145;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;91;-3155.857,1536.152;Inherit;False;Property;_FresnelOpacity;Fresnel Opacity;19;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;99;-3336.129,1835.201;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;88;-3226.433,1215.739;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-258.2065,963.9758;Inherit;False;Property;_TessellationMaxDistance;Tessellation Max Distance;5;0;Create;True;0;0;0;False;0;False;0;2000;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;90;-2980.306,1258.588;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;86;-2789.417,3065.856;Inherit;False;vertexOffset;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.CommentaryNode;137;-1974.423,1517.152;Inherit;False;1216.543;372.209;Comment;7;129;128;134;135;130;131;132;Depth Fade;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-303.2065,827.9758;Inherit;False;Property;_TessellationMinDistance;Tessellation Min Distance;4;0;Create;True;0;0;0;False;0;False;0;200;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.IntNode;14;-246.2065,718.9759;Inherit;False;Property;_TessellationFactor;Tessellation Factor;3;0;Create;True;0;0;0;False;0;False;1;30;False;0;1;INT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;47;-3108.139,1652.062;Inherit;True;cloudColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;131;-1146.328,1574.407;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;100;False;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;93;-3634.434,1552.313;Inherit;False;baseColor;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;87;-244.8141,257.1413;Inherit;False;86;vertexOffset;1;0;OBJECT;;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleAddOpNode;92;-2823.485,1447.634;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.DistanceBasedTessNode;10;-21.94282,690.9376;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.ScreenPosInputsNode;129;-1924.423,1663.152;Float;False;1;False;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ScreenDepthNode;128;-1677.423,1567.152;Inherit;False;0;True;1;0;FLOAT4;0,0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;134;-1649.431,1754.361;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;135;-1244.758,1715.924;Inherit;False;Property;_DepthStrength;Depth Strength;20;0;Create;True;0;0;0;False;0;False;0;32;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;130;-1404.423,1603.152;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;132;-922.8796,1609.227;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;98;2.062411,1;Float;False;True;-1;7;ASEMaterialInspector;0;0;Unlit;Clouds;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;1;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;True;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;4;0;3;1
WireConnection;4;1;3;3
WireConnection;7;0;4;0
WireConnection;7;2;9;0
WireConnection;7;1;6;0
WireConnection;25;0;4;0
WireConnection;25;1;5;0
WireConnection;1;0;7;0
WireConnection;1;1;5;0
WireConnection;27;0;1;0
WireConnection;27;1;25;0
WireConnection;31;0;27;0
WireConnection;55;0;31;0
WireConnection;36;0;35;0
WireConnection;52;0;55;0
WireConnection;52;1;54;0
WireConnection;32;0;52;0
WireConnection;32;1;36;0
WireConnection;32;2;36;1
WireConnection;32;3;36;2
WireConnection;32;4;36;3
WireConnection;58;0;4;0
WireConnection;58;2;59;0
WireConnection;58;1;6;0
WireConnection;33;0;32;0
WireConnection;56;0;58;0
WireConnection;56;1;57;0
WireConnection;49;0;33;0
WireConnection;49;1;50;0
WireConnection;49;2;51;0
WireConnection;61;0;56;0
WireConnection;61;1;62;0
WireConnection;60;0;49;0
WireConnection;60;1;61;0
WireConnection;64;1;62;0
WireConnection;74;0;70;0
WireConnection;74;1;73;0
WireConnection;63;0;60;0
WireConnection;63;1;64;0
WireConnection;82;0;74;0
WireConnection;82;1;83;0
WireConnection;28;0;63;0
WireConnection;23;0;21;0
WireConnection;23;1;22;0
WireConnection;81;0;82;0
WireConnection;75;0;80;0
WireConnection;75;1;81;0
WireConnection;39;0;42;0
WireConnection;39;1;44;0
WireConnection;39;2;41;0
WireConnection;84;3;89;0
WireConnection;24;0;29;0
WireConnection;24;1;23;0
WireConnection;78;0;24;0
WireConnection;78;1;75;0
WireConnection;99;0;39;0
WireConnection;99;1;68;0
WireConnection;88;0;100;0
WireConnection;88;1;84;0
WireConnection;90;0;88;0
WireConnection;90;1;91;0
WireConnection;86;0;78;0
WireConnection;47;0;99;0
WireConnection;131;0;130;0
WireConnection;131;1;135;0
WireConnection;93;0;39;0
WireConnection;92;0;90;0
WireConnection;92;1;47;0
WireConnection;10;0;14;0
WireConnection;10;1;12;0
WireConnection;10;2;13;0
WireConnection;134;0;129;4
WireConnection;130;0;128;0
WireConnection;130;1;134;0
WireConnection;132;0;131;0
WireConnection;98;2;92;0
WireConnection;98;11;87;0
WireConnection;98;14;10;0
ASEEND*/
//CHKSM=02E0BB1E0888C7B9F4B92CF11A202ABD30A7CC1C