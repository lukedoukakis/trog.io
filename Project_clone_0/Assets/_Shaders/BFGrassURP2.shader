﻿// MADE BY MATTHIEU HOULLIER
// Copyright 2021 BRUTE FORCE, all rights reserved.
// You are authorized to use this work if you have purchased the asset.
// Mail me at bruteforcegamesstudio@gmail.com if you have any questions or improvements you want.
Shader "BruteForceURP/InteractiveGrassURP2"
{
	Properties
	{
		[Header(Tint Colors)]
		[Space]
		[MainColor]_Color("ColorTint",Color) = (0.5 ,0.5 ,0.5,1.0)
		_GroundColor("GroundColorTint",Color) = (0.7 ,0.68 ,0.68,1.0)
		_SelfShadowColor("ShadowColor",Color) = (0.41 ,0.41 ,0.36,1.0)
		_ProjectedShadowColor("ProjectedShadowColor",Color) = (0.45 ,0.42 ,0.04,1.0)
		_GrassShading("GrassShading", Range(0.0, 1)) = 0.197
		_GrassSaturation("GrassSaturation", Float) = 2

		[Header(Textures)]
		[Space]
		[MainTexture]_MainTex("Color Grass", 2D) = "white" {}
		[NoScaleOffset]_GroundTex("Ground Texture", 2D) = "white" {}
		[NoScaleOffset]_NoGrassTex("NoGrassTexture", 2D) = "white" {}
		[NoScaleOffset]_GrassTex("Grass Pattern", 2D) = "white" {}
		[NoScaleOffset]_Noise("NoiseColor", 2D) = "white" {}
		[NoScaleOffset]_Distortion("DistortionWind", 2D) = "white" {}

		[Header(Geometry Values)]
		[Space]
		_NumberOfStacks("NumberOfStacks", Range(0, 17)) = 12
		_OffsetValue("OffsetValueNormal", Float) = 1
		_OffsetVector("OffsetVector", Vector) = (0,0,0)
		_FadeDistanceStart("FadeDistanceStart", Float) = 16
		_FadeDistanceEnd("FadeDistanceEnd", Float) = 26
		_MinimumNumberStacks("MinimumNumberOfStacks", Range(0, 17)) = 2

		[Header(Rim Lighting)]
		[Space]
		_RimColor("Rim Color", Color) = (0.14, 0.18, 0.09, 1)
		_RimPower("Rim Power", Range(0.0, 8.0)) = 3.14
		_RimMin("Rim Min", Range(0,1)) = 0.241
		_RimMax("Rim Max", Range(0,1)) = 0.62

		[Header(Grass Values)]
		[Space]
		_GrassThinness("GrassThinness", Range(0.01, 3)) = 0.4
		_GrassThinnessIntersection("GrassThinnessIntersection", Range(0.01, 2)) = 0.43
		_TilingN1("TilingOfGrass", Float) = 6.06
		_WindMovement("WindMovementSpeed", Float) = 0.55
		_WindForce("WindForce", Float) = 0.35
		_TilingN3("WindNoiseTiling", Float) = 1
		_GrassCut("GrassCut", Range(0, 1)) = 0
		_TilingN2("TilingOfNoiseColor", Float) = 0.05
		_NoisePower("NoisePower", Float) = 2
		[Toggle(USE_RT)] _UseRT("Use RenderTexture Effect", Float) = 1
		[Toggle(USE_S)] _UseShadow("Use Shadows", Float) = 1
		[Toggle(USE_SC)] _UseShadowCast("Use Shadow Casting", Float) = 1

		[Header(Procedural Tiling)]
		[Space]
		[Toggle(USE_PR)] _UsePR("Use Procedural Tiling (Reduce performance)", Float) = 0
		_ProceduralDistance("Tile start distance", Float) = 5.5
		_ProceduralStrength("Tile Smoothness", Float) = 1.5
		[Space]
		[Toggle(USE_WC)] _UseWC("Use World Coordinates", Float) = 0
		_WorldScale("World Scale", Float) = 10
		_WorldRotation("World Rotation", Range(0, 360)) = 0

		[Header(Lighting Parameters)]
		[Space]
		_LightIntensity("Additional Lights Intensity", Range(0.00, 2)) = 1

		[Header(Added Parameters)]
		[Space]
		_ColorNoise("Color Noise", 2D) = "white" {}
		_ColorGradient("Color Gradient", 2D) = "white" {}
		_ColorTiling("Color Tiling", Float) = .1
		_WaterHeight("Water Height", Float) = 0
		_GrassNormal("Grass Normal", Float) = .95
		_CampOrigin("Camp Origin", Vector) = (0,0,0,0)
		_CampRadius("Camp Radius", float) = 0
	}
		SubShader
		{
			Tags{"DisableBatching" = "true" "RenderType"="Transparent" "Queue"="Transparent"}
			//Blend SrcAlpha OneMinusSrcAlpha
			pass
			{
			Tags{"RenderPipeline" = "UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent"}
			//Blend SrcAlpha OneMinusSrcAlpha
			LOD 100
			HLSLPROGRAM

			// excluded shader from OpenGL ES 2.0 because it uses non-square matrices, if you need it to work on ES 2.0 comment the line below
			#pragma exclude_renderers gles
			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom
			#pragma multi_compile_fog
			#pragma multi_compile_instancing
			#pragma prefer_hlslcc gles
			#pragma shader_feature USE_RT
			#pragma shader_feature USE_PR
			#pragma shader_feature USE_S
			#pragma shader_feature USE_SC
			#pragma shader_feature USE_WC

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			#pragma multi_compile _ LIGHTMAP_ON

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 normal : NORMAL;
				UNITY_VERTEX_INPUT_INSTANCE_ID
#ifdef LIGHTMAP_ON
					half4 texcoord1 : TEXCOORD1;
#endif
			};

			struct v2g
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
				float4 objPos : TEXCOORD1;
				float3 normal : TEXCOORD2;
				float4 shadowCoord : TEXCOORD4;
				float fogCoord : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
#ifdef LIGHTMAP_ON
					float2 lmap : TEXCOORD6;
#endif
			};

			struct g2f
			{
				float2 uv : TEXCOORD0;
				float4 pos : SV_POSITION;
				float3 worldPos : TEXCOORD1;
				half1 color : TEXCOORD2;
				float3 normal : TEXCOORD3;
				float4 shadowCoord : TEXCOORD4;
				float fogCoord : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
#ifdef LIGHTMAP_ON
					float2 lmap : TEXCOORD6;
#endif
			};
			// Render Texture Effects //
			uniform Texture2D _GlobalEffectRT;
			uniform float3 _Position;
			uniform float _OrthographicCamSize;
			uniform float _HasRT;


			int _NumberOfStacks, _MinimumNumberStacks;
			Texture2D _MainTex;
			Texture2D _NoGrassTex;
			Texture2D _GroundTex;

			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;

			Texture2D _Distortion;
			sampler2D _GrassTex;
			Texture2D _Noise;
			float _TilingN1;
			float _TilingN2, _WindForce;
			float4 _Color, _SelfShadowColor, _GroundColor, _ProjectedShadowColor;
			float4 _OffsetVector;
			float _TilingN3;
			float _WindMovement, _OffsetValue;
			half _GrassThinness, _GrassShading, _GrassThinnessIntersection, _GrassCut;
			half4 _RimColor;
			half _RimPower, _NoisePower, _GrassSaturation, _FadeDistanceStart, _FadeDistanceEnd, _WorldScale, _WorldRotation;
			half _RimMin, _RimMax;
			float _ProceduralDistance, _ProceduralStrength;
			SamplerState my_linear_repeat_sampler;
			SamplerState my_linear_clamp_sampler;
			half _LightIntensity;

			Texture2D _ColorGradient;
			Texture2D _ColorNoise;
			half _WaterHeight;
			half _ColorTiling;
			half _GrassNormal;
			float4 _CampOrigin;
			half _CampRadius;

			float2 hash2D2D(float2 s)
			{
				//magic numbers
				return frac(sin(s)*4.5453);
			}

			//stochastic sampling
			float4 tex2DStochastic(sampler2D tex, float2 UV)
			{
				float4x3 BW_vx;
				float2 skewUV = mul(float2x2 (1.0, 0.0, -0.57735027, 1.15470054), UV * 3.464);

				//vertex IDs and barycentric coords
				float2 vxID = float2 (floor(skewUV));
				float3 barry = float3 (frac(skewUV), 0);
				barry.z = 1.0 - barry.x - barry.y;

				BW_vx = ((barry.z > 0) ?
					float4x3(float3(vxID, 0), float3(vxID + float2(0, 1), 0), float3(vxID + float2(1, 0), 0), barry.zyx) :
					float4x3(float3(vxID + float2 (1, 1), 0), float3(vxID + float2 (1, 0), 0), float3(vxID + float2 (0, 1), 0), float3(-barry.z, 1.0 - barry.y, 1.0 - barry.x)));

				//calculate derivatives to avoid triangular grid artifacts
				float2 dx = ddx(UV);
				float2 dy = ddy(UV);

				//blend samples with calculated weights
				return mul(tex2D(tex, UV + hash2D2D(BW_vx[0].xy), dx, dy), BW_vx[3].x) +
					mul(tex2D(tex, UV + hash2D2D(BW_vx[1].xy), dx, dy), BW_vx[3].y) +
					mul(tex2D(tex, UV + hash2D2D(BW_vx[2].xy), dx, dy), BW_vx[3].z);
			}

			v2g vert(appdata v)
			{
				v2g o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
				o.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);

				o.objPos = v.vertex;
				o.pos = GetVertexPositionInputs(v.vertex).positionCS;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.shadowCoord = GetShadowCoord(vertexInput);
				o.normal = v.normal;
#ifdef LIGHTMAP_ON
				o.lmap = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
#endif
				return o;
			}

			#define UnityObjectToWorld(o) mul(unity_ObjectToWorld, float4(o.xyz,1.0))
			[maxvertexcount(51)]
			void geom(triangle v2g input[3], inout TriangleStream<g2f> tristream)
			{
				g2f o;
				_OffsetValue *= 0.01;
				// Loop 3 times for the base ground geometry
				for (int i = 0; i < 3; i++)
				{
					UNITY_SETUP_INSTANCE_ID(input[i]);
					UNITY_TRANSFER_INSTANCE_ID(input[i], o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					o.uv = input[i].uv;
					o.pos = input[i].pos;
					o.color = 0.0 + _GrassCut;
					o.normal = GetVertexNormalInputs(input[i].normal).normalWS;
					o.worldPos = UnityObjectToWorld(input[i].objPos);
					o.shadowCoord = input[i].shadowCoord;
					o.fogCoord = ComputeFogFactor(input[i].pos.z);
#ifdef LIGHTMAP_ON
					o.lmap = input[i].lmap.xy;
#endif
					// only show vert if the normal is greater than _GrassNormal
					if(input[i].normal.y >= _GrassNormal){
						tristream.Append(o);
					}
				}
				tristream.RestartStrip();

				float dist = distance(_WorldSpaceCameraPos, UnityObjectToWorld((input[0].objPos / 3 + input[1].objPos / 3 + input[2].objPos / 3)))*2;
				if (dist > 0)
				{
					int NumStacks = lerp(_NumberOfStacks + 1, 0, (dist - _FadeDistanceStart)*(1 / max(_FadeDistanceEnd - _FadeDistanceStart, 0.0001)));//Clamp because people will start dividing by 0
					_NumberOfStacks = min(clamp(NumStacks, clamp(_MinimumNumberStacks, 0, _NumberOfStacks), 17), _NumberOfStacks);
				}

				float4 P; // P is shadow coords new position
				float4 objSpace; // objSpace is the vertex new position
				// Loop 3 times * numbersOfStacks for the grass
					for (float i = 1; i <= _NumberOfStacks; i++)
					{
						float4 offsetNormal = _OffsetVector * i*0.01;
						for (int ii = 0; ii < 3; ii++)
						{
							UNITY_SETUP_INSTANCE_ID(input[ii]);
							UNITY_TRANSFER_INSTANCE_ID(input[ii], o);
							UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
							P = input[ii].shadowCoord + _OffsetVector * _NumberOfStacks*0.01;
							float4 NewNormal = float4(input[ii].normal,0); // problem is here

							// change height
							objSpace = float4(input[ii].objPos + NewNormal * _OffsetValue*i + offsetNormal);
							if(UnityObjectToWorld(input[ii].objPos).y <= _WaterHeight){
								objSpace = input[ii].objPos;
							}

							o.color = (i / (_NumberOfStacks - _GrassCut));
							o.uv = input[ii].uv;
							o.pos = GetVertexPositionInputs(objSpace).positionCS;
							o.shadowCoord = P;
							o.worldPos = UnityObjectToWorld(objSpace);
							o.normal = GetVertexNormalInputs(input[ii].normal).normalWS;
							o.fogCoord = ComputeFogFactor(input[ii].pos.z);
#ifdef LIGHTMAP_ON
							o.lmap = input[ii].lmap.xy;
#endif						
							// only show vert if the normal is greater than _GrassNormal and outside radius from _CampOrigin
							if(input[ii].normal.y >= _GrassNormal && distance(o.worldPos, _CampOrigin) > _CampRadius){
								tristream.Append(o);
							}
							
						}
						tristream.RestartStrip();
					}
			}

			half4 frag(g2f i) : SV_Target
			{


				if(true){
					float2 mainUV;
			//Setup Coordinate Space
#ifdef USE_WC
			mainUV = i.worldPos.xz / max(_WorldScale, 0.001);

			float rotationAngle = _WorldRotation * 3.14159265359f / 180.0;
			float sina, cosa;
			sincos(rotationAngle, sina, cosa);
			float2x2 m = float2x2(cosa, -sina, sina, cosa);
			mainUV = mul(m, mainUV.xy);
#else
			mainUV = i.uv;
#endif
				float dist = 1;
#ifdef USE_PR
					dist = clamp(lerp(0, 1, (distance(_WorldSpaceCameraPos, i.worldPos) - _ProceduralDistance) / max(_ProceduralStrength,0.05)), 0, 1);
#endif
				float2 uv = i.worldPos.xz - _Position.xz;
				uv = uv / (_OrthographicCamSize * 2);
				uv += 0.5;

				float bRipple = 1;

#ifdef USE_RT
				if (_HasRT)
				{
					bRipple = 1 - clamp(_GlobalEffectRT.Sample(my_linear_clamp_sampler, uv).b * 5, 0, 2);
				}
#endif
				float2 dis = _Distortion.Sample(my_linear_repeat_sampler, mainUV * _TilingN3 + _Time.xx * 3 * _WindMovement);
				float displacementStrengh = 0.6* (((sin(_Time.y + dis * 5) + sin(_Time.y*0.5 + 1.051)) / 5.0) + 0.15*dis)*bRipple; //hmm math
				dis = dis * displacementStrengh*(i.color.r*1.3)*_WindForce*bRipple;

				float ripples = 0.25;
				float ripples2 = 0;
				float ripples3 = 0;
				float ripplesG = 0;
#ifdef USE_RT
				if (_HasRT)
				{		// .b(lue) = Grass height / .r(ed) = Grass shadow / .g(reen) is unassigned you can put anything you want if you need a new effect
					float3 rippleMain = _GlobalEffectRT.Sample(my_linear_clamp_sampler, uv + dis.xy * 0.04);
					ripples = (0.25 - rippleMain.z);
					ripples2 = (rippleMain.x);
					ripplesG = (0 - rippleMain.y);
					ripples3 = (0 - ripples2) * ripples2;
				}
#endif

				float3 normalDir = i.normal;
				float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
				float rim = 1 - saturate(dot(viewDir, normalDir));
				float3 rimLight = pow(abs(rim), _RimPower);
				rimLight = smoothstep(_RimMin, _RimMax, rimLight);

#ifdef USE_PR
				float3 grassPattern = lerp(tex2D(_GrassTex, mainUV * _TilingN1 + dis.xy), tex2DStochastic(_GrassTex, mainUV * _TilingN1 + dis.xy), dist);
#else
				float3 grassPattern = tex2D(_GrassTex, mainUV * _TilingN1 + dis.xy);
#endif

				float2 loc = float2(.5, .5);
				loc.x = noise(3);
				loc.y = .5;
				half4 col = _MainTex.Sample(my_linear_repeat_sampler, mainUV + dis.xy * 0.09);

				// color noise color
				float4 noiseCol = _ColorNoise.Sample(my_linear_repeat_sampler, mainUV * _ColorTiling);
				float2 coord = float2(noiseCol.r, 0);
				col *= _ColorGradient.Sample(my_linear_repeat_sampler, coord) * .5;


				half4 colGround = _GroundTex.Sample(my_linear_repeat_sampler, mainUV + dis.xy * 0.05);
				float3 noise = _Noise.Sample(my_linear_repeat_sampler, mainUV * _TilingN2 + dis.xy) * _NoisePower;
				half3 NoGrass = _NoGrassTex.Sample(my_linear_repeat_sampler, mainUV + dis.xy * 0.05);
				NoGrass.r = saturate(NoGrass.r + ripplesG);

				half alpha = step(1 - ((col.x + col.y + col.z + grassPattern.x) * _GrassThinness)*((2 - i.color.r)*NoGrass.r*grassPattern.x)*saturate(ripples + 1)*saturate(ripples + 1), ((1 - i.color.r)*(ripples + 1))*(NoGrass.r*grassPattern.x)*_GrassThinness - dis.x * 5);
				alpha = lerp(alpha, alpha + (grassPattern.x*NoGrass.r*(1 - i.color.r))*_GrassThinnessIntersection, 1 - (NoGrass.r)*(ripples*NoGrass.r + 0.75));

				if (i.color.r >= 0.01)
				{
					if (alpha*(ripples3 + 1) - (i.color.r) < -0.02)discard;
				}
				_Color *= 2;
				if(i.worldPos.y <= _WaterHeight - .02){
					//_Color *= .75;
				}

				col.xyz = (pow(abs(col), _GrassSaturation) * _GrassSaturation)*float3(_Color.x, _Color.y, _Color.z);
				col.xyz *= saturate(lerp(_SelfShadowColor, 1, pow(abs(i.color.x), 1.1)) + (_GrassShading  * (ripples * 1 + 1) - noise.x*dis.x * 2) + (1 - NoGrass.r) - noise.x*dis.x * 2);
				col.xyz *= _Color * (ripples*-0.1 + 1);
				col.xyz *= 1 - (ripples2*(1 - saturate(i.color.r - 0.7)));

				if (i.color.r <= 0.01)
				{
					colGround.xyz *= ((1 - NoGrass.r)*_GroundColor*_GroundColor * 2);
					col.xyz = lerp(col.xyz, colGround.xyz, 1 - NoGrass.r);
				}
				col.xyz += _RimColor.rgb * pow(abs(rimLight), _RimPower);

				float4 shadowCoord;	
				half3 lm = 1;
#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				shadowCoord = i.shadowCoord;
#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
				shadowCoord = TransformWorldToShadowCoord(i.worldPos);
#else
				shadowCoord = float4(0, 0, 0, 0);
#endif
				Light mainLight = GetMainLight(shadowCoord);
#ifdef USE_S		
				half3 shadowmapColor = lerp(_ProjectedShadowColor, 1, mainLight.shadowAttenuation);
#ifdef LIGHTMAP_ON
				lm = SampleLightmap(i.lmap, normalDir);

				col.rgb *= saturate(lm + 0.1);
#else
				col.xyz = col.xyz * saturate(shadowmapColor);
#endif
#endif
#ifdef LIGHTMAP_ON
#else
				if (mainLight.color.r + mainLight.color.g + mainLight.color.b > 0)
				{
					col.xyz *= mainLight.color;
				}
#endif
				// Additional light pass in URP, thank you Unity for this //
				int additionalLightsCount = GetAdditionalLightsCount();
				for (int ii = 0; ii < additionalLightsCount; ++ii)
				{
					Light light = GetAdditionalLight(ii, i.worldPos);
					col.xyz += (light.color * light.distanceAttenuation* light.distanceAttenuation)* (_LightIntensity * 0.5);
				}
				col.xyz = MixFog(col.xyz, i.fogCoord);

				return col;
				}

			
			}
				ENDHLSL
		}

		Pass
		{
			Name "ShadowCaster"
				Tags {"LightMode" = "ShadowCaster" "RenderPipeline" = "UniversalPipeline" "RenderType"="Transparent" "Queue"="Transparent"}
				//Blend SrcAlpha OneMinusSrcAlpha

			HLSLPROGRAM
			#pragma target 4.5
			// GPU Instancing
			#pragma multi_compile_instancing
			#pragma multi_compile _ DOTS_INSTANCING_ON

			#pragma vertex vert
			#pragma fragment frag
			#pragma geometry geom
			#pragma shader_feature USE_RT
			#pragma shader_feature USE_SC
			#pragma shader_feature USE_S
			#pragma shader_feature USE_WC

			#include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
			// I did not include shadowcasting because this pass includes a geometry pass and will do a custom shadow casting

			float3 _LightDirection;
			float3 _LightPosition;

			struct appdata
			{
				float4 vertex   : POSITION;
				float3 normal     : NORMAL;
				float2 uv     : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2g
			{
				float2 uv           : TEXCOORD0;
				float4 pos : SV_POSITION;
				float4 objPos : TEXCOORD1;
				float3 normal : TEXCOORD3;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct g2f
			{
				float4 pos : POSITION; // ????
				float2 uv : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				float3 normal : TEXCOORD3;
				float1 color : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
			};

			Texture2D _MainTex;
			Texture2D _NoGrassTex;
			Texture2D _Noise;

			uniform Texture2D _GlobalEffectRT;
			uniform float3 _Position;
			uniform float _OrthographicCamSize;
			uniform float _HasRT;

			int _NumberOfStacks, _MinimumNumberStacks;
			float4 _MainTex_ST;
			Texture2D _Distortion;
			Texture2D _GrassTex;
			float _TilingN1;
			float _WindForce, _TilingN2;
			float4 _OffsetVector;
			float _TilingN3;
			float _WindMovement, _OffsetValue, _FadeDistanceStart, _FadeDistanceEnd, _WorldScale, _WorldRotation;
			half _GrassThinness, _GrassThinnessIntersection, _GrassCut, _NoisePower;
			SamplerState my_linear_repeat_sampler;
			SamplerState my_linear_clamp_sampler;

			Texture2D _ColorGradient;
			Texture2D _ColorNoise;
			half _WaterHeight;
			half _ColorTiling;
			half _GrassNormal;

			v2g vert(appdata v)
			{
				v2g o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.objPos = v.vertex;
				//o.pos = GetVertexPositionInputs(v.vertex).positionCS;
				o.pos = TransformWorldToHClip(ApplyShadowBias(GetVertexPositionInputs(v.vertex).positionWS, GetVertexNormalInputs(v.normal).normalWS, _LightDirection));

#if defined(USE_SC) && defined(USE_S)
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
#endif
				o.normal = v.normal;
				return o;
			}

#define UnityObjectToWorld(o) mul(unity_ObjectToWorld, float4(o.xyz,1.0))
			[maxvertexcount(51)]
			void geom(triangle v2g input[3], inout TriangleStream<g2f> tristream) {

				g2f o;
				UNITY_SETUP_INSTANCE_ID(input);
				_OffsetValue *= 0.01;

				for (int i = 0; i < 3; i++)
				{
					UNITY_SETUP_INSTANCE_ID(input[i]);
					UNITY_TRANSFER_INSTANCE_ID(input[i], o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					o.uv = input[i].uv;
					o.pos = input[i].pos;
					o.color = float3(0 + _GrassCut, 0 + _GrassCut, 0 + _GrassCut);
					o.normal = GetVertexNormalInputs(input[i].normal).normalWS;
					o.worldPos = UnityObjectToWorld(input[i].objPos);

					tristream.Append(o);
				}
#if defined(USE_SC) && defined(USE_S)
				float4 P;
				float4 objSpace;
				tristream.RestartStrip();

				float dist = distance(_WorldSpaceCameraPos, UnityObjectToWorld((input[0].objPos / 3 + input[1].objPos / 3 + input[2].objPos / 3)));
				if (dist > 0)
				{
					int NumStacks = lerp(_NumberOfStacks + 1, 0, (dist - _FadeDistanceStart)*(1 / max(_FadeDistanceEnd - _FadeDistanceStart, 0.0001)));//Clamp because people will start dividing by 0
					_NumberOfStacks = min(clamp(NumStacks, clamp(_MinimumNumberStacks, 0, _NumberOfStacks), 17), _NumberOfStacks);
				}

				for (float i = 1; i <= _NumberOfStacks; i++)
				{
					float4 offsetNormal = _OffsetVector * i*0.01;
					for (int ii = 0; ii < 3; ii++)
					{
						UNITY_SETUP_INSTANCE_ID(input[ii]);
						UNITY_TRANSFER_INSTANCE_ID(input[ii], o);
						UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
						float4 NewNormal = float4(input[ii].normal, 0);

						// change height
						objSpace = float4(input[ii].objPos + NewNormal * _OffsetValue*i + offsetNormal);
						if(UnityObjectToWorld(input[ii].objPos).y <= _WaterHeight){
							objSpace = input[ii].objPos;	
						}

						o.color = (i / (_NumberOfStacks - _GrassCut));
						o.uv = input[ii].uv;
						o.pos = TransformWorldToHClip(ApplyShadowBias(GetVertexPositionInputs(objSpace).positionWS, GetVertexNormalInputs(input[ii].normal).normalWS, _LightDirection));
						o.worldPos = UnityObjectToWorld(objSpace);
						o.normal = GetVertexNormalInputs(input[ii].normal).normalWS;

						tristream.Append(o);
					}
					tristream.RestartStrip();
				}
#endif
			}

				float4 frag(g2f i) : SV_Target
			{
				float2 mainUV;
				//Setup Coordinate Space
	#ifdef USE_WC
				mainUV = i.worldPos.xz / max(_WorldScale, 0.001);

				float rotationAngle = _WorldRotation * 3.14159265359f / 180.0;
				float sina, cosa;
				sincos(rotationAngle, sina, cosa);
				float2x2 m = float2x2(cosa, -sina, sina, cosa);
				mainUV = mul(m, mainUV.xy);
	#else
				mainUV = i.uv;
	#endif
#if defined(USE_SC) && defined(USE_S)
				float2 uv = i.worldPos.xz - _Position.xz;
				uv = uv / (_OrthographicCamSize * 2);
				uv += 0.5;

				float bRipple = 1;
#ifdef USE_RT
				if (_HasRT)
				{
					bRipple = 1 - clamp(_GlobalEffectRT.Sample(my_linear_clamp_sampler, uv).b * 5, 0, 2);
				}
#endif

				float2 dis = _Distortion.Sample(my_linear_repeat_sampler, mainUV * _TilingN3 + _Time.xx * 3 * _WindMovement);
				float displacementStrengh = 0.6* (((sin(_Time.y + dis * 5) + sin(_Time.y*0.5 + 1.051)) / 5.0) + 0.15*dis)*bRipple; //hmm math
				dis = dis * displacementStrengh*(i.color.r*1.3)*_WindForce*bRipple;

				float ripples = 0.25;
				float ripples2 = 0;
				float ripplesG = 0;
				float ripples3 = 0;
#ifdef USE_RT
				if (_HasRT)
				{
					// .b(lue) = Grass height / .r(ed) = Grass shadow / .g(reen) is unassigned you can put anything you want if you need a new effect
					float3 rippleMain = _GlobalEffectRT.Sample(my_linear_clamp_sampler, uv + dis.xy * 0.04);
					ripples = (0.25 - rippleMain.z);
					ripples2 = (rippleMain.x);
					ripplesG = (0 - rippleMain.y);
					ripples3 = (0 - ripples2) * ripples2;
				}
#endif
				half4 col = _MainTex.Sample(my_linear_repeat_sampler, mainUV + dis.xy * 0.09);

				// color noise color
				float4 noiseCol = _ColorNoise.Sample(my_linear_repeat_sampler, mainUV * _ColorTiling);
				float2 coord = float2(noiseCol.r, 0);
				col *= _ColorGradient.Sample(my_linear_repeat_sampler, coord);

				float3 noise = _Noise.Sample(my_linear_repeat_sampler, mainUV * _TilingN2 + dis.xy) * _NoisePower;
				float3 grassPattern = _GrassTex.Sample(my_linear_repeat_sampler, mainUV * _TilingN1 + dis.xy);
				half3 NoGrass = _NoGrassTex.Sample(my_linear_repeat_sampler, mainUV + dis.xy * 0.05);
				NoGrass.r = saturate(NoGrass.r + ripplesG);

				half alpha = step(1 - ((col.x + col.y + col.z + grassPattern.x) * _GrassThinness)*((2 - i.color.r)*NoGrass.r*grassPattern.x)*saturate(ripples + 1)*saturate(ripples + 1), ((1 - i.color.r)*(ripples + 1))*(NoGrass.r*grassPattern.x)*_GrassThinness - dis.x * 5);
				alpha = lerp(alpha, alpha + (grassPattern.x*NoGrass.r*(1 - i.color.r))*_GrassThinnessIntersection, 1 - (NoGrass.r)*(ripples*NoGrass.r + 0.75));


				if (i.color.r >= 0.01)
				{
					if (alpha*(ripples3 + 1) - (i.color.r) < -0.02)discard;
				}

				return 0; //Same as SHADOW_CASTER_FRAGMENT(i)
#else
				return 0;
#endif
			}
			ENDHLSL
		}
		}// Fallback "VertexLit"
}

