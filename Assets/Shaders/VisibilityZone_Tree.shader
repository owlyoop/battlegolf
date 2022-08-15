// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "VisibilityZone_Tree"
{
	Properties
	{
		_Albedo("Albedo", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Mask("Mask", 2D) = "white" {}
		_Noise("Noise", 2D) = "white" {}
		_Scale("Scale", Float) = 1
		_Opacity("Opacity", Float) = 0
		[Toggle(UNITY_PASS_SHADOWCASTER)] _Keyword0("Keyword 0", Float) = 0
		_clip_value_043("clip_value_0.43", Float) = 0.43
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "TransparentCutout"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" }
		Cull Back
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha addshadow fullforwardshadows nolightmap  nodynlightmap nodirlightmap 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
		};

		uniform float4 _Color;
		uniform sampler2D _Albedo;
		uniform float4 _Albedo_ST;
		uniform float _Opacity;
		uniform sampler2D _Mask;
		uniform float _Scale;
		uniform float3 SW_PlayerPos;
		uniform sampler2D _Noise;
		uniform float _clip_value_043;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Albedo = i.uv_texcoord * _Albedo_ST.xy + _Albedo_ST.zw;
			float4 tex2DNode1 = tex2D( _Albedo, uv_Albedo );
			o.Albedo = ( _Color * tex2DNode1 ).rgb;
			o.Alpha = 1;
			float4 appendResult69 = (float4(_Scale , _Scale , 0.0 , 0.0));
			float3 ase_worldPos = i.worldPos;
			float2 appendResult47 = (float2(ase_worldPos.x , ase_worldPos.z));
			float2 appendResult43 = (float2(SW_PlayerPos.x , SW_PlayerPos.z));
			float4 tex2DNode2 = tex2D( _Mask, ( ( appendResult69 + float4( ( appendResult47 - (( ( float2( 0.5,0.5 ) * _Scale ) + appendResult43 )).xy ), 0.0 , 0.0 ) ) / _Scale ).xy );
			float2 appendResult35 = (float2(( (ase_worldPos).x * 0.3 ) , ( (ase_worldPos).y * 0.1 )));
			float4 temp_cast_3 = (0.2).xxxx;
			float4 temp_cast_4 = (1.0).xxxx;
			float4 clampResult24 = clamp( ( tex2DNode2 + ( tex2DNode2 * tex2D( _Noise, appendResult35 ) ) ) , temp_cast_3 , temp_cast_4 );
			float lerpResult82 = lerp( 0.0 , ( _Opacity * tex2DNode1.a ) , ( 0.59 * clampResult24 ).r);
			#ifdef UNITY_PASS_SHADOWCASTER
				float staticSwitch83 = 1.0;
			#else
				float staticSwitch83 = lerpResult82;
			#endif
			clip( staticSwitch83 - _clip_value_043 );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16201
299;92;886;701;-1808.783;160.4908;1;True;False
Node;AmplifyShaderEditor.Vector3Node;37;-869.414,187.861;Float;False;Global;SW_PlayerPos;SW_PlayerPos;5;0;Create;True;0;0;False;0;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;57;-839.1454,-46.09956;Float;False;Property;_Scale;Scale;4;0;Create;True;0;0;False;0;1;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;58;-872.0197,49.07431;Float;False;Constant;_Offset;Offset;6;0;Create;True;0;0;False;0;0.5,0.5;0.5,0.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;63;-649.2047,88.62061;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;43;-647.5494,224.4204;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;60;-507.65,201.9708;Float;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WorldPosInputsNode;53;-408.5682,47.50339;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.WorldPosInputsNode;30;-631.8339,432.9221;Float;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ComponentMaskNode;40;-227.2186,209.1605;Float;False;True;True;False;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.DynamicAppendNode;47;-201.6778,87.89513;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;31;-296.3883,319.544;Float;True;True;False;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;65;-15.41724,122.2642;Float;False;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;21;-63.07742,427.4848;Float;False;Constant;_Float2;Float 2;4;0;Create;True;0;0;False;0;0.3;0.3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;69;-72.70541,-28.80493;Float;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;34;-59.41113,565.0868;Float;False;Constant;_Float0;Float 0;5;0;Create;True;0;0;False;0;0.1;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ComponentMaskNode;32;-299.6126,501.7903;Float;True;False;True;False;False;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;33;109.4309,471.802;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;29;112.5189,372.5501;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;67;143.278,34.1413;Float;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT2;0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleDivideOpNode;61;318.28,110.0243;Float;False;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;35;276.5806,379.6603;Float;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;2;467.1208,82.49662;Float;True;Property;_Mask;Mask;2;0;Create;True;0;0;False;0;None;c3195eb6c063e0240b1b7655faf87f7e;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;17;465.41,273.9356;Float;True;Property;_Noise;Noise;3;0;Create;True;0;0;False;0;None;df24e0daa52391d44a1df2141b971c55;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;786.9426,253.3694;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;23;1021.193,91.72372;Float;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;26;1077.387,471.5432;Float;False;Constant;_Float4;Float 4;2;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;25;1064.52,305.2378;Float;False;Constant;_Float3;Float 3;2;0;Create;True;0;0;False;0;0.2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;73;1495.574,-65.61234;Float;False;Property;_Opacity;Opacity;5;0;Create;True;0;0;False;0;0;3.19;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;1169.3,-136.9349;Float;True;Property;_Albedo;Albedo;0;0;Create;True;0;0;False;0;None;f331ca9aa9069a442a22ded6c79b8f8f;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;27;1387.889,100.9096;Float;False;Constant;_float1;float1;4;0;Create;True;0;0;False;0;0.59;1.84;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;24;1307.969,219.4839;Float;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;1,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;72;1760.681,-49.62753;Float;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;1592.999,129.1591;Float;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;81;1734.654,342.0841;Float;False;Constant;_Float1;Float 1;2;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;82;2130.111,72.85379;Float;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;84;2252.783,230.5092;Float;False;Constant;_Float5;Float 5;7;0;Create;True;0;0;False;0;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;78;1257.733,-308.3033;Float;False;Property;_Color;Color;1;0;Create;True;0;0;False;0;1,1,1,1;1,1,1,1;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;83;2396.783,112.5092;Float;False;Property;_Keyword0;Keyword 0;6;0;Fetch;True;0;0;False;0;0;0;0;True;UNITY_PASS_SHADOWCASTER;Toggle;2;Key0;Key1;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;80;2385.441,-252.7566;Float;False;Constant;_clip_value_043;clip_value_0.43;9;0;Create;True;0;0;False;0;0.43;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;79;1686.521,-157.3803;Float;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;2758.679,-131.3456;Float;False;True;2;Float;ASEMaterialInspector;0;0;Standard;VisibilityZone_Tree;False;False;False;False;False;False;True;True;True;False;False;False;False;False;True;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.43;True;True;0;True;TransparentCutout;;Geometry;All;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;True;80;0;0;0;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;63;0;58;0
WireConnection;63;1;57;0
WireConnection;43;0;37;1
WireConnection;43;1;37;3
WireConnection;60;0;63;0
WireConnection;60;1;43;0
WireConnection;40;0;60;0
WireConnection;47;0;53;1
WireConnection;47;1;53;3
WireConnection;31;0;30;0
WireConnection;65;0;47;0
WireConnection;65;1;40;0
WireConnection;69;0;57;0
WireConnection;69;1;57;0
WireConnection;32;0;30;0
WireConnection;33;0;32;0
WireConnection;33;1;34;0
WireConnection;29;0;31;0
WireConnection;29;1;21;0
WireConnection;67;0;69;0
WireConnection;67;1;65;0
WireConnection;61;0;67;0
WireConnection;61;1;57;0
WireConnection;35;0;29;0
WireConnection;35;1;33;0
WireConnection;2;1;61;0
WireConnection;17;1;35;0
WireConnection;22;0;2;0
WireConnection;22;1;17;0
WireConnection;23;0;2;0
WireConnection;23;1;22;0
WireConnection;24;0;23;0
WireConnection;24;1;25;0
WireConnection;24;2;26;0
WireConnection;72;0;73;0
WireConnection;72;1;1;4
WireConnection;28;0;27;0
WireConnection;28;1;24;0
WireConnection;82;0;81;0
WireConnection;82;1;72;0
WireConnection;82;2;28;0
WireConnection;83;1;82;0
WireConnection;83;0;84;0
WireConnection;79;0;78;0
WireConnection;79;1;1;0
WireConnection;0;0;79;0
WireConnection;0;10;83;0
ASEEND*/
//CHKSM=AA050A524E1F6DC3FFC8F10A92AAF4FBCD824381