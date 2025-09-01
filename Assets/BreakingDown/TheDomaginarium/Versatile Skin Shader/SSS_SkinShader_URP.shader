// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "The Domaginarium/PBR Skin URP"
{
	Properties
	{
		[HideInInspector] _EmissionColor("Emission Color", Color) = (1,1,1,1)
		[HideInInspector] _AlphaCutoff("Alpha Cutoff ", Range(0, 1)) = 0.5
		[ASEBegin]_SkinTint("Skin Tint", Color) = (1,1,1,1)
		_AlbedoTransparencySkin("Albedo Transparency Skin", 2D) = "white" {}
		_AlbedoSkinSaturation("Albedo Skin Saturation", Range( 0 , 1)) = 0
		_PeachSkinValue("Peach Skin Value", Range( 0 , 1)) = 1
		_PeachSkinSpread("Peach Skin Spread", Range( 0 , 1)) = 1
		_MetallicSkinAlpha("Metallic Skin (Alpha)", 2D) = "white" {}
		_MetallicValue("Metallic Value", Range( 0 , 1)) = 0
		_MetallicSmoothnessSkin("Metallic Smoothness Skin", Range( 0 , 1)) = 1
		_NormalMapSkin("Normal Map Skin", 2D) = "bump" {}
		_NormalMapSkinValue("Normal Map Skin Value", Range( 0 , 1)) = 1
		_DetailNormal("Detail Normal", 2D) = "bump" {}
		_DetailNormalValue("Detail Normal Value", Range( 0 , 1)) = 1
		_Tattoo1("Tattoo1", 2D) = "black" {}
		_Tattoo1Value("Tattoo 1 Value", Range( 0 , 1)) = 1
		_Tattoo2("Tattoo2", 2D) = "black" {}
		_Tattoo2Value("Tattoo 2 Value", Range( 0 , 1)) = 1
		_EmissiveTexture("Emissive Texture", 2D) = "black" {}
		[HDR]_EmissiveColor("Emissive Color", Color) = (0,0,0,0)
		_SubSurfaceScattering("SubSurface Scattering", Range( 0 , 1)) = 0.1
		_SubSurfaceScatteringColor("SubSurface Scattering Color", Color) = (1,1,1,1)
		_BloodRimColor("BloodRim Color", Color) = (1,0,0,0)
		_BloodRimScatterValue("BloodRim Scatter Value", Range( 0 , 1)) = 1
		_WrinkleNormal1("Wrinkle Normal 1", 2D) = "bump" {}
		_WrinkleNormalValue1("WrinkleNormal Value 1", Range( 0 , 1)) = 0
		_WrinkleNormal2("Wrinkle Normal 2", 2D) = "bump" {}
		_WrinkleNormalValue2("WrinkleNormal Value 2", Range( 0 , 1)) = 0
		_WrinkleNormal3("Wrinkle Normal 3", 2D) = "bump" {}
		_WrinkleNormalValue3("WrinkleNormal Value 3", Range( 0 , 1)) = 0
		_WrinkleNormal4("Wrinkle Normal 4", 2D) = "bump" {}
		_WrinkleNormalValue4("WrinkleNormal Value 4", Range( 0 , 1)) = 0
		_WrinkleNormal5("Wrinkle Normal 5", 2D) = "bump" {}
		_WrinkleNormalValue5("WrinkleNormal Value 5", Range( 0 , 1)) = 0
		_WrinkleNormal6("Wrinkle Normal 6", 2D) = "bump" {}
		[ASEEnd]_WrinkleNormalValue6("WrinkleNormal Value 6", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

		//_TransmissionShadow( "Transmission Shadow", Range( 0, 1 ) ) = 0.5
		//_TransStrength( "Trans Strength", Range( 0, 50 ) ) = 1
		//_TransNormal( "Trans Normal Distortion", Range( 0, 1 ) ) = 0.5
		//_TransScattering( "Trans Scattering", Range( 1, 50 ) ) = 2
		//_TransDirect( "Trans Direct", Range( 0, 1 ) ) = 0.9
		//_TransAmbient( "Trans Ambient", Range( 0, 1 ) ) = 0.1
		//_TransShadow( "Trans Shadow", Range( 0, 1 ) ) = 0.5
		//_TessPhongStrength( "Tess Phong Strength", Range( 0, 1 ) ) = 0.5
		//_TessValue( "Tess Max Tessellation", Range( 1, 32 ) ) = 16
		//_TessMin( "Tess Min Distance", Float ) = 10
		//_TessMax( "Tess Max Distance", Float ) = 25
		//_TessEdgeLength ( "Tess Edge length", Range( 2, 50 ) ) = 16
		//_TessMaxDisp( "Tess Max Displacement", Float ) = 25
	}

	SubShader
	{
		LOD 0

		

		Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
		Cull Back
		AlphaToMask Off
		
		HLSLINCLUDE
		#pragma target 2.0

		#pragma prefer_hlslcc gles
		#pragma exclude_renderers d3d11_9x 

		#ifndef ASE_TESS_FUNCS
		#define ASE_TESS_FUNCS
		float4 FixedTess( float tessValue )
		{
			return tessValue;
		}
		
		float CalcDistanceTessFactor (float4 vertex, float minDist, float maxDist, float tess, float4x4 o2w, float3 cameraPos )
		{
			float3 wpos = mul(o2w,vertex).xyz;
			float dist = distance (wpos, cameraPos);
			float f = clamp(1.0 - (dist - minDist) / (maxDist - minDist), 0.01, 1.0) * tess;
			return f;
		}

		float4 CalcTriEdgeTessFactors (float3 triVertexFactors)
		{
			float4 tess;
			tess.x = 0.5 * (triVertexFactors.y + triVertexFactors.z);
			tess.y = 0.5 * (triVertexFactors.x + triVertexFactors.z);
			tess.z = 0.5 * (triVertexFactors.x + triVertexFactors.y);
			tess.w = (triVertexFactors.x + triVertexFactors.y + triVertexFactors.z) / 3.0f;
			return tess;
		}

		float CalcEdgeTessFactor (float3 wpos0, float3 wpos1, float edgeLen, float3 cameraPos, float4 scParams )
		{
			float dist = distance (0.5 * (wpos0+wpos1), cameraPos);
			float len = distance(wpos0, wpos1);
			float f = max(len * scParams.y / (edgeLen * dist), 1.0);
			return f;
		}

		float DistanceFromPlane (float3 pos, float4 plane)
		{
			float d = dot (float4(pos,1.0f), plane);
			return d;
		}

		bool WorldViewFrustumCull (float3 wpos0, float3 wpos1, float3 wpos2, float cullEps, float4 planes[6] )
		{
			float4 planeTest;
			planeTest.x = (( DistanceFromPlane(wpos0, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[0]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[0]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.y = (( DistanceFromPlane(wpos0, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[1]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[1]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.z = (( DistanceFromPlane(wpos0, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[2]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[2]) > -cullEps) ? 1.0f : 0.0f );
			planeTest.w = (( DistanceFromPlane(wpos0, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos1, planes[3]) > -cullEps) ? 1.0f : 0.0f ) +
						  (( DistanceFromPlane(wpos2, planes[3]) > -cullEps) ? 1.0f : 0.0f );
			return !all (planeTest);
		}

		float4 DistanceBasedTess( float4 v0, float4 v1, float4 v2, float tess, float minDist, float maxDist, float4x4 o2w, float3 cameraPos )
		{
			float3 f;
			f.x = CalcDistanceTessFactor (v0,minDist,maxDist,tess,o2w,cameraPos);
			f.y = CalcDistanceTessFactor (v1,minDist,maxDist,tess,o2w,cameraPos);
			f.z = CalcDistanceTessFactor (v2,minDist,maxDist,tess,o2w,cameraPos);

			return CalcTriEdgeTessFactors (f);
		}

		float4 EdgeLengthBasedTess( float4 v0, float4 v1, float4 v2, float edgeLength, float4x4 o2w, float3 cameraPos, float4 scParams )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;
			tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
			tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
			tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
			tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			return tess;
		}

		float4 EdgeLengthBasedTessCull( float4 v0, float4 v1, float4 v2, float edgeLength, float maxDisplacement, float4x4 o2w, float3 cameraPos, float4 scParams, float4 planes[6] )
		{
			float3 pos0 = mul(o2w,v0).xyz;
			float3 pos1 = mul(o2w,v1).xyz;
			float3 pos2 = mul(o2w,v2).xyz;
			float4 tess;

			if (WorldViewFrustumCull(pos0, pos1, pos2, maxDisplacement, planes))
			{
				tess = 0.0f;
			}
			else
			{
				tess.x = CalcEdgeTessFactor (pos1, pos2, edgeLength, cameraPos, scParams);
				tess.y = CalcEdgeTessFactor (pos2, pos0, edgeLength, cameraPos, scParams);
				tess.z = CalcEdgeTessFactor (pos0, pos1, edgeLength, cameraPos, scParams);
				tess.w = (tess.x + tess.y + tess.z) / 3.0f;
			}
			return tess;
		}
		#endif //ASE_TESS_FUNCS

		ENDHLSL

		
		Pass
		{
			
			Name "Forward"
			Tags { "LightMode"="UniversalForward" }
			
			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 110000

			
			#pragma multi_compile _ _SCREEN_SPACE_OCCLUSION
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS _ADDITIONAL_OFF
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			
			#pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
			#pragma multi_compile _ SHADOWS_SHADOWMASK

			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_FORWARD

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			
			#if ASE_SRP_VERSION <= 70108
			#define REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
			#endif

			#if defined(UNITY_INSTANCING_ENABLED) && defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL)
			    #define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			#define ASE_NEEDS_FRAG_WORLD_TANGENT
			#define ASE_NEEDS_FRAG_WORLD_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_BITANGENT
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_FRAG_SHADOWCOORDS
			#define ASE_NEEDS_FRAG_WORLD_VIEW_DIR


			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord : TEXCOORD0;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 lightmapUVOrVertexSH : TEXCOORD0;
				half4 fogFactorAndVertexLight : TEXCOORD1;
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				float4 shadowCoord : TEXCOORD2;
				#endif
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 screenPos : TEXCOORD6;
				#endif
				float4 ase_texcoord7 : TEXCOORD7;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _NormalMapSkin_ST;
			float4 _EmissiveColor;
			float4 _EmissiveTexture_ST;
			float4 _BloodRimColor;
			float4 _SubSurfaceScatteringColor;
			float4 _AlbedoTransparencySkin_ST;
			float4 _SkinTint;
			float4 _Tattoo1_ST;
			float4 _Tattoo2_ST;
			float4 _WrinkleNormal6_ST;
			float4 _WrinkleNormal5_ST;
			float4 _MetallicSkinAlpha_ST;
			float4 _WrinkleNormal4_ST;
			float4 _DetailNormal_ST;
			float4 _WrinkleNormal1_ST;
			float4 _WrinkleNormal3_ST;
			float4 _WrinkleNormal2_ST;
			float _NormalMapSkinValue;
			float _BloodRimScatterValue;
			float _DetailNormalValue;
			float _SubSurfaceScattering;
			float _PeachSkinSpread;
			float _PeachSkinValue;
			float _WrinkleNormalValue1;
			float _WrinkleNormalValue4;
			float _WrinkleNormalValue2;
			float _Tattoo1Value;
			float _Tattoo2Value;
			float _MetallicValue;
			float _WrinkleNormalValue6;
			float _WrinkleNormalValue3;
			float _WrinkleNormalValue5;
			float _AlbedoSkinSaturation;
			float _MetallicSmoothnessSkin;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _NormalMapSkin;
			sampler2D _DetailNormal;
			sampler2D _WrinkleNormal1;
			sampler2D _WrinkleNormal2;
			sampler2D _WrinkleNormal3;
			sampler2D _WrinkleNormal4;
			sampler2D _WrinkleNormal5;
			sampler2D _WrinkleNormal6;
			sampler2D _MetallicSkinAlpha;
			sampler2D _Tattoo2;
			sampler2D _Tattoo1;
			sampler2D _AlbedoTransparencySkin;
			sampler2D _EmissiveTexture;


			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord7.xy = v.texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord7.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 positionVS = TransformWorldToView( positionWS );
				float4 positionCS = TransformWorldToHClip( positionWS );

				VertexNormalInputs normalInput = GetVertexNormalInputs( v.ase_normal, v.ase_tangent );

				o.tSpace0 = float4( normalInput.normalWS, positionWS.x);
				o.tSpace1 = float4( normalInput.tangentWS, positionWS.y);
				o.tSpace2 = float4( normalInput.bitangentWS, positionWS.z);

				OUTPUT_LIGHTMAP_UV( v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy );
				OUTPUT_SH( normalInput.normalWS.xyz, o.lightmapUVOrVertexSH.xyz );

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					o.lightmapUVOrVertexSH.zw = v.texcoord;
					o.lightmapUVOrVertexSH.xy = v.texcoord * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

				half3 vertexLight = VertexLighting( positionWS, normalInput.normalWS );
				#ifdef ASE_FOG
					half fogFactor = ComputeFogFactor( positionCS.z );
				#else
					half fogFactor = 0;
				#endif
				o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
				
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				
				o.clipPos = positionCS;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				o.screenPos = ComputeScreenPos(positionCS);
				#endif
				return o;
			}
			
			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_tangent = v.ase_tangent;
				o.texcoord = v.texcoord;
				o.texcoord1 = v.texcoord1;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif

			half4 frag ( VertexOutput IN 
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float2 sampleCoords = (IN.lightmapUVOrVertexSH.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					float3 WorldNormal = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					float3 WorldTangent = -cross(GetObjectToWorldMatrix()._13_23_33, WorldNormal);
					float3 WorldBiTangent = cross(WorldNormal, -WorldTangent);
				#else
					float3 WorldNormal = normalize( IN.tSpace0.xyz );
					float3 WorldTangent = IN.tSpace1.xyz;
					float3 WorldBiTangent = IN.tSpace2.xyz;
				#endif
				float3 WorldPosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldViewDirection = _WorldSpaceCameraPos.xyz  - WorldPosition;
				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 ScreenPos = IN.screenPos;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					ShadowCoords = IN.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
				#endif
	
				WorldViewDirection = SafeNormalize( WorldViewDirection );

				float3 LightWrapVector47_g15 = (( 0.45 * 0.5 )).xxx;
				float2 uv_NormalMapSkin = IN.ase_texcoord7.xy * _NormalMapSkin_ST.xy + _NormalMapSkin_ST.zw;
				float3 unpack23 = UnpackNormalScale( tex2D( _NormalMapSkin, uv_NormalMapSkin ), _NormalMapSkinValue );
				unpack23.z = lerp( 1, unpack23.z, saturate(_NormalMapSkinValue) );
				float2 uv_DetailNormal = IN.ase_texcoord7.xy * _DetailNormal_ST.xy + _DetailNormal_ST.zw;
				float3 unpack28 = UnpackNormalScale( tex2D( _DetailNormal, uv_DetailNormal ), _DetailNormalValue );
				unpack28.z = lerp( 1, unpack28.z, saturate(_DetailNormalValue) );
				float2 uv_WrinkleNormal1 = IN.ase_texcoord7.xy * _WrinkleNormal1_ST.xy + _WrinkleNormal1_ST.zw;
				float3 unpack265 = UnpackNormalScale( tex2D( _WrinkleNormal1, uv_WrinkleNormal1 ), _WrinkleNormalValue1 );
				unpack265.z = lerp( 1, unpack265.z, saturate(_WrinkleNormalValue1) );
				float2 uv_WrinkleNormal2 = IN.ase_texcoord7.xy * _WrinkleNormal2_ST.xy + _WrinkleNormal2_ST.zw;
				float3 unpack268 = UnpackNormalScale( tex2D( _WrinkleNormal2, uv_WrinkleNormal2 ), _WrinkleNormalValue2 );
				unpack268.z = lerp( 1, unpack268.z, saturate(_WrinkleNormalValue2) );
				float2 uv_WrinkleNormal3 = IN.ase_texcoord7.xy * _WrinkleNormal3_ST.xy + _WrinkleNormal3_ST.zw;
				float3 unpack271 = UnpackNormalScale( tex2D( _WrinkleNormal3, uv_WrinkleNormal3 ), _WrinkleNormalValue3 );
				unpack271.z = lerp( 1, unpack271.z, saturate(_WrinkleNormalValue3) );
				float2 uv_WrinkleNormal4 = IN.ase_texcoord7.xy * _WrinkleNormal4_ST.xy + _WrinkleNormal4_ST.zw;
				float3 unpack274 = UnpackNormalScale( tex2D( _WrinkleNormal4, uv_WrinkleNormal4 ), _WrinkleNormalValue4 );
				unpack274.z = lerp( 1, unpack274.z, saturate(_WrinkleNormalValue4) );
				float2 uv_WrinkleNormal5 = IN.ase_texcoord7.xy * _WrinkleNormal5_ST.xy + _WrinkleNormal5_ST.zw;
				float3 unpack272 = UnpackNormalScale( tex2D( _WrinkleNormal5, uv_WrinkleNormal5 ), _WrinkleNormalValue5 );
				unpack272.z = lerp( 1, unpack272.z, saturate(_WrinkleNormalValue5) );
				float2 uv_WrinkleNormal6 = IN.ase_texcoord7.xy * _WrinkleNormal6_ST.xy + _WrinkleNormal6_ST.zw;
				float3 unpack277 = UnpackNormalScale( tex2D( _WrinkleNormal6, uv_WrinkleNormal6 ), _WrinkleNormalValue6 );
				unpack277.z = lerp( 1, unpack277.z, saturate(_WrinkleNormalValue6) );
				float3 temp_output_281_0 = BlendNormal( BlendNormal( BlendNormal( BlendNormal( BlendNormal( unpack265 , unpack268 ) , unpack271 ) , unpack274 ) , unpack272 ) , unpack277 );
				float3 Normal222 = BlendNormal( BlendNormal( unpack23 , unpack28 ) , temp_output_281_0 );
				float3 tanToWorld0 = float3( WorldTangent.x, WorldBiTangent.x, WorldNormal.x );
				float3 tanToWorld1 = float3( WorldTangent.y, WorldBiTangent.y, WorldNormal.y );
				float3 tanToWorld2 = float3( WorldTangent.z, WorldBiTangent.z, WorldNormal.z );
				float3 tanNormal19_g15 = Normal222;
				float3 worldNormal19_g15 = normalize( float3(dot(tanToWorld0,tanNormal19_g15), dot(tanToWorld1,tanNormal19_g15), dot(tanToWorld2,tanNormal19_g15)) );
				float3 CurrentNormal23_g15 = worldNormal19_g15;
				float dotResult20_g15 = dot( CurrentNormal23_g15 , _MainLightPosition.xyz );
				float NDotL21_g15 = dotResult20_g15;
				float ase_lightAtten = 0;
				Light ase_mainLight = GetMainLight( ShadowCoords );
				ase_lightAtten = ase_mainLight.distanceAttenuation * ase_mainLight.shadowAttenuation;
				float3 AttenuationColor8_g15 = ( _MainLightColor.rgb * ase_lightAtten );
				float2 uv_MetallicSkinAlpha = IN.ase_texcoord7.xy * _MetallicSkinAlpha_ST.xy + _MetallicSkinAlpha_ST.zw;
				float4 tex2DNode409 = tex2D( _MetallicSkinAlpha, uv_MetallicSkinAlpha );
				float4 SubSurfaceScatter410 = tex2DNode409;
				float2 uv_Tattoo2 = IN.ase_texcoord7.xy * _Tattoo2_ST.xy + _Tattoo2_ST.zw;
				float4 tex2DNode501 = tex2D( _Tattoo2, uv_Tattoo2 );
				float2 uv_Tattoo1 = IN.ase_texcoord7.xy * _Tattoo1_ST.xy + _Tattoo1_ST.zw;
				float4 tex2DNode499 = tex2D( _Tattoo1, uv_Tattoo1 );
				float2 uv_AlbedoTransparencySkin = IN.ase_texcoord7.xy * _AlbedoTransparencySkin_ST.xy + _AlbedoTransparencySkin_ST.zw;
				float4 tex2DNode162 = tex2D( _AlbedoTransparencySkin, uv_AlbedoTransparencySkin );
				float3 desaturateInitialColor16 = tex2DNode162.rgb;
				float desaturateDot16 = dot( desaturateInitialColor16, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar16 = lerp( desaturateInitialColor16, desaturateDot16.xxx, (1.0 + (_AlbedoSkinSaturation - 0.0) * (0.0 - 1.0) / (1.0 - 0.0)) );
				float3 tanNormal52 = Normal222;
				float temp_output_372_0 = (0.0 + (_PeachSkinValue - 0.0) * (5.0 - 0.0) / (1.0 - 0.0));
				float temp_output_144_0 = (10.0 + (_PeachSkinSpread - 0.0) * (2.0 - 10.0) / (1.0 - 0.0));
				float fresnelNdotV52 = dot( float3(dot(tanToWorld0,tanNormal52), dot(tanToWorld1,tanNormal52), dot(tanToWorld2,tanNormal52)), WorldViewDirection );
				float fresnelNode52 = ( 0.0 + temp_output_372_0 * pow( max( 1.0 - fresnelNdotV52 , 0.0001 ), temp_output_144_0 ) );
				float layeredBlendVar505 = ( _Tattoo1Value * tex2DNode499.a );
				float4 layeredBlend505 = ( lerp( ( ( _SkinTint * float4( desaturateVar16 , 0.0 ) ) + float4( ( desaturateVar16 * fresnelNode52 ) , 0.0 ) ),tex2DNode499 , layeredBlendVar505 ) );
				float layeredBlendVar506 = ( _Tattoo2Value * tex2DNode501.a );
				float4 layeredBlend506 = ( lerp( layeredBlend505,tex2DNode501 , layeredBlendVar506 ) );
				half4 AlbedoSkinTx217 = layeredBlend506;
				float4 Translucency236 = ( _SubSurfaceScattering * _SubSurfaceScatteringColor );
				float3 DiffuseColor70_g15 = ( ( ( max( ( LightWrapVector47_g15 + ( ( 1.0 - LightWrapVector47_g15 ) * NDotL21_g15 ) ) , float3(0,0,0) ) * AttenuationColor8_g15 ) + (UNITY_LIGHTMODEL_AMBIENT).rgb ) * ( SubSurfaceScatter410 * AlbedoSkinTx217 * Translucency236 ).rgb );
				float3 normalizeResult77_g15 = normalize( _MainLightPosition.xyz );
				float3 normalizeResult28_g15 = normalize( ( normalizeResult77_g15 + WorldViewDirection ) );
				float3 HalfDirection29_g15 = normalizeResult28_g15;
				float dotResult32_g15 = dot( HalfDirection29_g15 , CurrentNormal23_g15 );
				float SpecularPower14_g15 = exp2( ( ( 0.0 * 10.0 ) + 1.0 ) );
				float3 specularFinalColor42_g15 = ( AttenuationColor8_g15 * pow( max( dotResult32_g15 , 0.0 ) , SpecularPower14_g15 ) * 0.0 );
				float3 LightWrapVector47_g14 = (( 0.45 * 0.5 )).xxx;
				float3 tanNormal19_g14 = Normal222;
				float3 worldNormal19_g14 = normalize( float3(dot(tanToWorld0,tanNormal19_g14), dot(tanToWorld1,tanNormal19_g14), dot(tanToWorld2,tanNormal19_g14)) );
				float3 CurrentNormal23_g14 = worldNormal19_g14;
				float dotResult20_g14 = dot( CurrentNormal23_g14 , _MainLightPosition.xyz );
				float NDotL21_g14 = dotResult20_g14;
				float3 AttenuationColor8_g14 = ( _MainLightColor.rgb * ase_lightAtten );
				float3 DiffuseColor70_g14 = ( ( ( max( ( LightWrapVector47_g14 + ( ( 1.0 - LightWrapVector47_g14 ) * NDotL21_g14 ) ) , float3(0,0,0) ) * AttenuationColor8_g14 ) + (UNITY_LIGHTMODEL_AMBIENT).rgb ) * float3( 1,1,1 ) );
				float3 normalizeResult77_g14 = normalize( _MainLightPosition.xyz );
				float3 normalizeResult28_g14 = normalize( ( normalizeResult77_g14 + WorldViewDirection ) );
				float3 HalfDirection29_g14 = normalizeResult28_g14;
				float dotResult32_g14 = dot( HalfDirection29_g14 , CurrentNormal23_g14 );
				float SpecularPower14_g14 = exp2( ( ( 0.0 * 10.0 ) + 1.0 ) );
				float3 specularFinalColor42_g14 = ( AttenuationColor8_g14 * pow( max( dotResult32_g14 , 0.0 ) , SpecularPower14_g14 ) * 0.0 );
				float3 desaturateInitialColor459 = ( DiffuseColor70_g15 + specularFinalColor42_g15 );
				float desaturateDot459 = dot( desaturateInitialColor459, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar459 = lerp( desaturateInitialColor459, desaturateDot459.xxx, ( DiffuseColor70_g14 + specularFinalColor42_g14 ).x );
				float3 LightWrapVector47_g16 = (( 0.55 * 0.5 )).xxx;
				float3 tanNormal19_g16 = Normal222;
				float3 worldNormal19_g16 = normalize( float3(dot(tanToWorld0,tanNormal19_g16), dot(tanToWorld1,tanNormal19_g16), dot(tanToWorld2,tanNormal19_g16)) );
				float3 CurrentNormal23_g16 = worldNormal19_g16;
				float dotResult20_g16 = dot( CurrentNormal23_g16 , _MainLightPosition.xyz );
				float NDotL21_g16 = dotResult20_g16;
				float3 AttenuationColor8_g16 = ( _MainLightColor.rgb * ase_lightAtten );
				float3 DiffuseColor70_g16 = ( ( ( max( ( LightWrapVector47_g16 + ( ( 1.0 - LightWrapVector47_g16 ) * NDotL21_g16 ) ) , float3(0,0,0) ) * AttenuationColor8_g16 ) + (UNITY_LIGHTMODEL_AMBIENT).rgb ) * AlbedoSkinTx217.rgb );
				float3 normalizeResult77_g16 = normalize( _MainLightPosition.xyz );
				float3 normalizeResult28_g16 = normalize( ( normalizeResult77_g16 + WorldViewDirection ) );
				float3 HalfDirection29_g16 = normalizeResult28_g16;
				float dotResult32_g16 = dot( HalfDirection29_g16 , CurrentNormal23_g16 );
				float SpecularPower14_g16 = exp2( ( ( 0.0 * 10.0 ) + 1.0 ) );
				float3 specularFinalColor42_g16 = ( AttenuationColor8_g16 * pow( max( dotResult32_g16 , 0.0 ) , SpecularPower14_g16 ) * 0.0 );
				float3 temp_output_439_0 = ( DiffuseColor70_g16 + specularFinalColor42_g16 );
				float3 LightWrapVector47_g8 = (( 0.54 * 0.5 )).xxx;
				float3 tanNormal19_g8 = Normal222;
				float3 worldNormal19_g8 = normalize( float3(dot(tanToWorld0,tanNormal19_g8), dot(tanToWorld1,tanNormal19_g8), dot(tanToWorld2,tanNormal19_g8)) );
				float3 CurrentNormal23_g8 = worldNormal19_g8;
				float dotResult20_g8 = dot( CurrentNormal23_g8 , _MainLightPosition.xyz );
				float NDotL21_g8 = dotResult20_g8;
				float3 AttenuationColor8_g8 = ( _MainLightColor.rgb * ase_lightAtten );
				float4 color398 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
				float3 DiffuseColor70_g8 = ( ( ( max( ( LightWrapVector47_g8 + ( ( 1.0 - LightWrapVector47_g8 ) * NDotL21_g8 ) ) , float3(0,0,0) ) * AttenuationColor8_g8 ) + (UNITY_LIGHTMODEL_AMBIENT).rgb ) * color398.rgb );
				float3 normalizeResult77_g8 = normalize( _MainLightPosition.xyz );
				float3 normalizeResult28_g8 = normalize( ( normalizeResult77_g8 + WorldViewDirection ) );
				float3 HalfDirection29_g8 = normalizeResult28_g8;
				float dotResult32_g8 = dot( HalfDirection29_g8 , CurrentNormal23_g8 );
				float SpecularPower14_g8 = exp2( ( ( 0.0 * 10.0 ) + 1.0 ) );
				float3 specularFinalColor42_g8 = ( AttenuationColor8_g8 * pow( max( dotResult32_g8 , 0.0 ) , SpecularPower14_g8 ) * 0.0 );
				float3 temp_output_400_0 = ( DiffuseColor70_g8 + specularFinalColor42_g8 );
				float3 clampResult403 = clamp( ( temp_output_400_0 + temp_output_400_0 + temp_output_400_0 + temp_output_400_0 ) , float3( 0,0,0 ) , float3( 1,1,1 ) );
				float3 LightWrapVector47_g7 = (( 0.39 * 0.5 )).xxx;
				float3 tanNormal19_g7 = Normal222;
				float3 worldNormal19_g7 = normalize( float3(dot(tanToWorld0,tanNormal19_g7), dot(tanToWorld1,tanNormal19_g7), dot(tanToWorld2,tanNormal19_g7)) );
				float3 CurrentNormal23_g7 = worldNormal19_g7;
				float dotResult20_g7 = dot( CurrentNormal23_g7 , _MainLightPosition.xyz );
				float NDotL21_g7 = dotResult20_g7;
				float3 AttenuationColor8_g7 = ( _MainLightColor.rgb * ase_lightAtten );
				float3 DiffuseColor70_g7 = ( ( ( max( ( LightWrapVector47_g7 + ( ( 1.0 - LightWrapVector47_g7 ) * NDotL21_g7 ) ) , float3(0,0,0) ) * AttenuationColor8_g7 ) + (UNITY_LIGHTMODEL_AMBIENT).rgb ) * color398.rgb );
				float3 normalizeResult77_g7 = normalize( _MainLightPosition.xyz );
				float3 normalizeResult28_g7 = normalize( ( normalizeResult77_g7 + WorldViewDirection ) );
				float3 HalfDirection29_g7 = normalizeResult28_g7;
				float dotResult32_g7 = dot( HalfDirection29_g7 , CurrentNormal23_g7 );
				float SpecularPower14_g7 = exp2( ( ( 0.0 * 10.0 ) + 1.0 ) );
				float3 specularFinalColor42_g7 = ( AttenuationColor8_g7 * pow( max( dotResult32_g7 , 0.0 ) , SpecularPower14_g7 ) * 0.0 );
				float3 temp_output_399_0 = ( DiffuseColor70_g7 + specularFinalColor42_g7 );
				float3 clampResult404 = clamp( ( temp_output_399_0 + temp_output_399_0 + temp_output_399_0 + temp_output_399_0 ) , float3( 0,0,0 ) , float3( 1,1,1 ) );
				float3 clampResult395 = clamp( ( clampResult403 - clampResult404 ) , float3( 0,0,0 ) , float3( 1,1,1 ) );
				float MetallicAlpha412 = tex2DNode409.a;
				float4 BloodRim406 = ( float4( clampResult395 , 0.0 ) * _BloodRimColor * _BloodRimScatterValue * MetallicAlpha412 );
				float4 Albedo515 = ( float4( desaturateVar459 , 0.0 ) + ( float4( temp_output_439_0 , 0.0 ) + BloodRim406 ) + AlbedoSkinTx217 );
				
				float2 uv_EmissiveTexture = IN.ase_texcoord7.xy * _EmissiveTexture_ST.xy + _EmissiveTexture_ST.zw;
				float4 temp_output_389_0 = ( tex2D( _EmissiveTexture, uv_EmissiveTexture ) * _EmissiveColor );
				float4 Emissive407 = temp_output_389_0;
				
				float Metallic417 = ( tex2DNode409.a * _MetallicValue );
				
				float4 Glossiness418 = ( tex2DNode409 * (0.35 + (_MetallicSmoothnessSkin - 0.0) * (1.0 - 0.35) / (1.0 - 0.0)) );
				
				half AlbedoAlpha219 = tex2DNode162.a;
				
				float3 Albedo = Albedo515.rgb;
				float3 Normal = Normal222;
				float3 Emission = Emissive407.rgb;
				float3 Specular = 0.5;
				float Metallic = Metallic417;
				float Smoothness = Glossiness418.r;
				float Occlusion = 1;
				float Alpha = AlbedoAlpha219;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				InputData inputData;
				inputData.positionWS = WorldPosition;
				inputData.viewDirectionWS = WorldViewDirection;
				inputData.shadowCoord = ShadowCoords;

				#ifdef _NORMALMAP
					#if _NORMAL_DROPOFF_TS
					inputData.normalWS = TransformTangentToWorld(Normal, half3x3( WorldTangent, WorldBiTangent, WorldNormal ));
					#elif _NORMAL_DROPOFF_OS
					inputData.normalWS = TransformObjectToWorldNormal(Normal);
					#elif _NORMAL_DROPOFF_WS
					inputData.normalWS = Normal;
					#endif
					inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				#else
					inputData.normalWS = WorldNormal;
				#endif

				#ifdef ASE_FOG
					inputData.fogCoord = IN.fogFactorAndVertexLight.x;
				#endif

				inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = IN.lightmapUVOrVertexSH.xyz;
				#endif

				inputData.bakedGI = SAMPLE_GI( IN.lightmapUVOrVertexSH.xy, SH, inputData.normalWS );
				#ifdef _ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#endif
				
				inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(IN.clipPos);
				inputData.shadowMask = SAMPLE_SHADOWMASK(IN.lightmapUVOrVertexSH.xy);

				half4 color = UniversalFragmentPBR(
					inputData, 
					Albedo, 
					Metallic, 
					Specular, 
					Smoothness, 
					Occlusion, 
					Emission, 
					Alpha);

				#ifdef _TRANSMISSION_ASE
				{
					float shadow = _TransmissionShadow;

					Light mainLight = GetMainLight( inputData.shadowCoord );
					float3 mainAtten = mainLight.color * mainLight.distanceAttenuation;
					mainAtten = lerp( mainAtten, mainAtten * mainLight.shadowAttenuation, shadow );
					half3 mainTransmission = max(0 , -dot(inputData.normalWS, mainLight.direction)) * mainAtten * Transmission;
					color.rgb += Albedo * mainTransmission;

					#ifdef _ADDITIONAL_LIGHTS
						int transPixelLightCount = GetAdditionalLightsCount();
						for (int i = 0; i < transPixelLightCount; ++i)
						{
							Light light = GetAdditionalLight(i, inputData.positionWS);
							float3 atten = light.color * light.distanceAttenuation;
							atten = lerp( atten, atten * light.shadowAttenuation, shadow );

							half3 transmission = max(0 , -dot(inputData.normalWS, light.direction)) * atten * Transmission;
							color.rgb += Albedo * transmission;
						}
					#endif
				}
				#endif

				#ifdef _TRANSLUCENCY_ASE
				{
					float shadow = _TransShadow;
					float normal = _TransNormal;
					float scattering = _TransScattering;
					float direct = _TransDirect;
					float ambient = _TransAmbient;
					float strength = _TransStrength;

					Light mainLight = GetMainLight( inputData.shadowCoord );
					float3 mainAtten = mainLight.color * mainLight.distanceAttenuation;
					mainAtten = lerp( mainAtten, mainAtten * mainLight.shadowAttenuation, shadow );

					half3 mainLightDir = mainLight.direction + inputData.normalWS * normal;
					half mainVdotL = pow( saturate( dot( inputData.viewDirectionWS, -mainLightDir ) ), scattering );
					half3 mainTranslucency = mainAtten * ( mainVdotL * direct + inputData.bakedGI * ambient ) * Translucency;
					color.rgb += Albedo * mainTranslucency * strength;

					#ifdef _ADDITIONAL_LIGHTS
						int transPixelLightCount = GetAdditionalLightsCount();
						for (int i = 0; i < transPixelLightCount; ++i)
						{
							Light light = GetAdditionalLight(i, inputData.positionWS);
							float3 atten = light.color * light.distanceAttenuation;
							atten = lerp( atten, atten * light.shadowAttenuation, shadow );

							half3 lightDir = light.direction + inputData.normalWS * normal;
							half VdotL = pow( saturate( dot( inputData.viewDirectionWS, -lightDir ) ), scattering );
							half3 translucency = atten * ( VdotL * direct + inputData.bakedGI * ambient ) * Translucency;
							color.rgb += Albedo * translucency * strength;
						}
					#endif
				}
				#endif

				#ifdef _REFRACTION_ASE
					float4 projScreenPos = ScreenPos / ScreenPos.w;
					float3 refractionOffset = ( RefractionIndex - 1.0 ) * mul( UNITY_MATRIX_V, float4( WorldNormal,0 ) ).xyz * ( 1.0 - dot( WorldNormal, WorldViewDirection ) );
					projScreenPos.xy += refractionOffset.xy;
					float3 refraction = SHADERGRAPH_SAMPLE_SCENE_COLOR( projScreenPos.xy ) * RefractionColor;
					color.rgb = lerp( refraction, color.rgb, color.a );
					color.a = 1;
				#endif

				#ifdef ASE_FINAL_COLOR_ALPHA_MULTIPLY
					color.rgb *= color.a;
				#endif

				#ifdef ASE_FOG
					#ifdef TERRAIN_SPLAT_ADDPASS
						color.rgb = MixFogColor(color.rgb, half3( 0, 0, 0 ), IN.fogFactorAndVertexLight.x );
					#else
						color.rgb = MixFog(color.rgb, IN.fogFactorAndVertexLight.x);
					#endif
				#endif

				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif

				return color;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "ShadowCaster"
			Tags { "LightMode"="ShadowCaster" }

			ZWrite On
			ZTest LEqual
			AlphaToMask Off
			ColorMask 0

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 110000

			
			#pragma vertex vert
			#pragma fragment frag
#if ASE_SRP_VERSION >= 110000
			#pragma multi_compile _ _CASTING_PUNCTUAL_LIGHT_SHADOW
#endif
			#define SHADERPASS_SHADOWCASTER

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _NormalMapSkin_ST;
			float4 _EmissiveColor;
			float4 _EmissiveTexture_ST;
			float4 _BloodRimColor;
			float4 _SubSurfaceScatteringColor;
			float4 _AlbedoTransparencySkin_ST;
			float4 _SkinTint;
			float4 _Tattoo1_ST;
			float4 _Tattoo2_ST;
			float4 _WrinkleNormal6_ST;
			float4 _WrinkleNormal5_ST;
			float4 _MetallicSkinAlpha_ST;
			float4 _WrinkleNormal4_ST;
			float4 _DetailNormal_ST;
			float4 _WrinkleNormal1_ST;
			float4 _WrinkleNormal3_ST;
			float4 _WrinkleNormal2_ST;
			float _NormalMapSkinValue;
			float _BloodRimScatterValue;
			float _DetailNormalValue;
			float _SubSurfaceScattering;
			float _PeachSkinSpread;
			float _PeachSkinValue;
			float _WrinkleNormalValue1;
			float _WrinkleNormalValue4;
			float _WrinkleNormalValue2;
			float _Tattoo1Value;
			float _Tattoo2Value;
			float _MetallicValue;
			float _WrinkleNormalValue6;
			float _WrinkleNormalValue3;
			float _WrinkleNormalValue5;
			float _AlbedoSkinSaturation;
			float _MetallicSmoothnessSkin;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _AlbedoTransparencySkin;


			
			float3 _LightDirection;
#if ASE_SRP_VERSION >= 110000 
			float3 _LightPosition;
#endif
			VertexOutput VertexFunction( VertexInput v )
			{
				VertexOutput o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif
				float3 normalWS = TransformObjectToWorldDir(v.ase_normal);

		#if ASE_SRP_VERSION >= 110000 
			#if _CASTING_PUNCTUAL_LIGHT_SHADOW
				float3 lightDirectionWS = normalize(_LightPosition - positionWS);
			#else
				float3 lightDirectionWS = _LightDirection;
			#endif
				float4 clipPos = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, lightDirectionWS));
			#if UNITY_REVERSED_Z
				clipPos.z = min(clipPos.z, UNITY_NEAR_CLIP_VALUE);
			#else
				clipPos.z = max(clipPos.z, UNITY_NEAR_CLIP_VALUE);
			#endif
		#else
				float4 clipPos = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
			#if UNITY_REVERSED_Z
				clipPos.z = min(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
			#else
				clipPos.z = max(clipPos.z, clipPos.w * UNITY_NEAR_CLIP_VALUE);
			#endif
		#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = clipPos;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				o.clipPos = clipPos;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif

			half4 frag(	VertexOutput IN 
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );
				
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float2 uv_AlbedoTransparencySkin = IN.ase_texcoord2.xy * _AlbedoTransparencySkin_ST.xy + _AlbedoTransparencySkin_ST.zw;
				float4 tex2DNode162 = tex2D( _AlbedoTransparencySkin, uv_AlbedoTransparencySkin );
				half AlbedoAlpha219 = tex2DNode162.a;
				
				float Alpha = AlbedoAlpha219;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					#ifdef _ALPHATEST_SHADOW_ON
						clip(Alpha - AlphaClipThresholdShadow);
					#else
						clip(Alpha - AlphaClipThreshold);
					#endif
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif
				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif
				return 0;
			}

			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthOnly"
			Tags { "LightMode"="DepthOnly" }

			ZWrite On
			ColorMask 0
			AlphaToMask Off

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 110000

			
			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_DEPTHONLY

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _NormalMapSkin_ST;
			float4 _EmissiveColor;
			float4 _EmissiveTexture_ST;
			float4 _BloodRimColor;
			float4 _SubSurfaceScatteringColor;
			float4 _AlbedoTransparencySkin_ST;
			float4 _SkinTint;
			float4 _Tattoo1_ST;
			float4 _Tattoo2_ST;
			float4 _WrinkleNormal6_ST;
			float4 _WrinkleNormal5_ST;
			float4 _MetallicSkinAlpha_ST;
			float4 _WrinkleNormal4_ST;
			float4 _DetailNormal_ST;
			float4 _WrinkleNormal1_ST;
			float4 _WrinkleNormal3_ST;
			float4 _WrinkleNormal2_ST;
			float _NormalMapSkinValue;
			float _BloodRimScatterValue;
			float _DetailNormalValue;
			float _SubSurfaceScattering;
			float _PeachSkinSpread;
			float _PeachSkinValue;
			float _WrinkleNormalValue1;
			float _WrinkleNormalValue4;
			float _WrinkleNormalValue2;
			float _Tattoo1Value;
			float _Tattoo2Value;
			float _MetallicValue;
			float _WrinkleNormalValue6;
			float _WrinkleNormalValue3;
			float _WrinkleNormalValue5;
			float _AlbedoSkinSaturation;
			float _MetallicSmoothnessSkin;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _AlbedoTransparencySkin;


			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;
				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				o.clipPos = positionCS;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord = v.ase_texcoord;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif
			half4 frag(	VertexOutput IN 
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float2 uv_AlbedoTransparencySkin = IN.ase_texcoord2.xy * _AlbedoTransparencySkin_ST.xy + _AlbedoTransparencySkin_ST.zw;
				float4 tex2DNode162 = tex2D( _AlbedoTransparencySkin, uv_AlbedoTransparencySkin );
				half AlbedoAlpha219 = tex2DNode162.a;
				
				float Alpha = AlbedoAlpha219;
				float AlphaClipThreshold = 0.5;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif
				#ifdef ASE_DEPTH_WRITE_ON
				outputDepth = DepthValue;
				#endif

				return 0;
			}
			ENDHLSL
		}
		
		
		Pass
		{
			
			Name "Meta"
			Tags { "LightMode"="Meta" }

			Cull Off

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 110000

			
			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_META

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_FRAG_SHADOWCOORDS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _SHADOWS_SOFT


			#pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_tangent : TANGENT;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _NormalMapSkin_ST;
			float4 _EmissiveColor;
			float4 _EmissiveTexture_ST;
			float4 _BloodRimColor;
			float4 _SubSurfaceScatteringColor;
			float4 _AlbedoTransparencySkin_ST;
			float4 _SkinTint;
			float4 _Tattoo1_ST;
			float4 _Tattoo2_ST;
			float4 _WrinkleNormal6_ST;
			float4 _WrinkleNormal5_ST;
			float4 _MetallicSkinAlpha_ST;
			float4 _WrinkleNormal4_ST;
			float4 _DetailNormal_ST;
			float4 _WrinkleNormal1_ST;
			float4 _WrinkleNormal3_ST;
			float4 _WrinkleNormal2_ST;
			float _NormalMapSkinValue;
			float _BloodRimScatterValue;
			float _DetailNormalValue;
			float _SubSurfaceScattering;
			float _PeachSkinSpread;
			float _PeachSkinValue;
			float _WrinkleNormalValue1;
			float _WrinkleNormalValue4;
			float _WrinkleNormalValue2;
			float _Tattoo1Value;
			float _Tattoo2Value;
			float _MetallicValue;
			float _WrinkleNormalValue6;
			float _WrinkleNormalValue3;
			float _WrinkleNormalValue5;
			float _AlbedoSkinSaturation;
			float _MetallicSmoothnessSkin;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _NormalMapSkin;
			sampler2D _DetailNormal;
			sampler2D _WrinkleNormal1;
			sampler2D _WrinkleNormal2;
			sampler2D _WrinkleNormal3;
			sampler2D _WrinkleNormal4;
			sampler2D _WrinkleNormal5;
			sampler2D _WrinkleNormal6;
			sampler2D _MetallicSkinAlpha;
			sampler2D _Tattoo2;
			sampler2D _Tattoo1;
			sampler2D _AlbedoTransparencySkin;
			sampler2D _EmissiveTexture;


			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				float3 ase_worldTangent = TransformObjectToWorldDir(v.ase_tangent.xyz);
				o.ase_texcoord3.xyz = ase_worldTangent;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord4.xyz = ase_worldNormal;
				float ase_vertexTangentSign = v.ase_tangent.w * unity_WorldTransformParams.w;
				float3 ase_worldBitangent = cross( ase_worldNormal, ase_worldTangent ) * ase_vertexTangentSign;
				o.ase_texcoord5.xyz = ase_worldBitangent;
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
				o.ase_texcoord3.w = 0;
				o.ase_texcoord4.w = 0;
				o.ase_texcoord5.w = 0;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				o.clipPos = MetaVertexPosition( v.vertex, v.texcoord1.xy, v.texcoord1.xy, unity_LightmapST, unity_DynamicLightmapST );
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = o.clipPos;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord2 : TEXCOORD2;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_tangent : TANGENT;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.texcoord1 = v.texcoord1;
				o.texcoord2 = v.texcoord2;
				o.ase_texcoord = v.ase_texcoord;
				o.ase_tangent = v.ase_tangent;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				o.texcoord2 = patch[0].texcoord2 * bary.x + patch[1].texcoord2 * bary.y + patch[2].texcoord2 * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float3 LightWrapVector47_g15 = (( 0.45 * 0.5 )).xxx;
				float2 uv_NormalMapSkin = IN.ase_texcoord2.xy * _NormalMapSkin_ST.xy + _NormalMapSkin_ST.zw;
				float3 unpack23 = UnpackNormalScale( tex2D( _NormalMapSkin, uv_NormalMapSkin ), _NormalMapSkinValue );
				unpack23.z = lerp( 1, unpack23.z, saturate(_NormalMapSkinValue) );
				float2 uv_DetailNormal = IN.ase_texcoord2.xy * _DetailNormal_ST.xy + _DetailNormal_ST.zw;
				float3 unpack28 = UnpackNormalScale( tex2D( _DetailNormal, uv_DetailNormal ), _DetailNormalValue );
				unpack28.z = lerp( 1, unpack28.z, saturate(_DetailNormalValue) );
				float2 uv_WrinkleNormal1 = IN.ase_texcoord2.xy * _WrinkleNormal1_ST.xy + _WrinkleNormal1_ST.zw;
				float3 unpack265 = UnpackNormalScale( tex2D( _WrinkleNormal1, uv_WrinkleNormal1 ), _WrinkleNormalValue1 );
				unpack265.z = lerp( 1, unpack265.z, saturate(_WrinkleNormalValue1) );
				float2 uv_WrinkleNormal2 = IN.ase_texcoord2.xy * _WrinkleNormal2_ST.xy + _WrinkleNormal2_ST.zw;
				float3 unpack268 = UnpackNormalScale( tex2D( _WrinkleNormal2, uv_WrinkleNormal2 ), _WrinkleNormalValue2 );
				unpack268.z = lerp( 1, unpack268.z, saturate(_WrinkleNormalValue2) );
				float2 uv_WrinkleNormal3 = IN.ase_texcoord2.xy * _WrinkleNormal3_ST.xy + _WrinkleNormal3_ST.zw;
				float3 unpack271 = UnpackNormalScale( tex2D( _WrinkleNormal3, uv_WrinkleNormal3 ), _WrinkleNormalValue3 );
				unpack271.z = lerp( 1, unpack271.z, saturate(_WrinkleNormalValue3) );
				float2 uv_WrinkleNormal4 = IN.ase_texcoord2.xy * _WrinkleNormal4_ST.xy + _WrinkleNormal4_ST.zw;
				float3 unpack274 = UnpackNormalScale( tex2D( _WrinkleNormal4, uv_WrinkleNormal4 ), _WrinkleNormalValue4 );
				unpack274.z = lerp( 1, unpack274.z, saturate(_WrinkleNormalValue4) );
				float2 uv_WrinkleNormal5 = IN.ase_texcoord2.xy * _WrinkleNormal5_ST.xy + _WrinkleNormal5_ST.zw;
				float3 unpack272 = UnpackNormalScale( tex2D( _WrinkleNormal5, uv_WrinkleNormal5 ), _WrinkleNormalValue5 );
				unpack272.z = lerp( 1, unpack272.z, saturate(_WrinkleNormalValue5) );
				float2 uv_WrinkleNormal6 = IN.ase_texcoord2.xy * _WrinkleNormal6_ST.xy + _WrinkleNormal6_ST.zw;
				float3 unpack277 = UnpackNormalScale( tex2D( _WrinkleNormal6, uv_WrinkleNormal6 ), _WrinkleNormalValue6 );
				unpack277.z = lerp( 1, unpack277.z, saturate(_WrinkleNormalValue6) );
				float3 temp_output_281_0 = BlendNormal( BlendNormal( BlendNormal( BlendNormal( BlendNormal( unpack265 , unpack268 ) , unpack271 ) , unpack274 ) , unpack272 ) , unpack277 );
				float3 Normal222 = BlendNormal( BlendNormal( unpack23 , unpack28 ) , temp_output_281_0 );
				float3 ase_worldTangent = IN.ase_texcoord3.xyz;
				float3 ase_worldNormal = IN.ase_texcoord4.xyz;
				float3 ase_worldBitangent = IN.ase_texcoord5.xyz;
				float3 tanToWorld0 = float3( ase_worldTangent.x, ase_worldBitangent.x, ase_worldNormal.x );
				float3 tanToWorld1 = float3( ase_worldTangent.y, ase_worldBitangent.y, ase_worldNormal.y );
				float3 tanToWorld2 = float3( ase_worldTangent.z, ase_worldBitangent.z, ase_worldNormal.z );
				float3 tanNormal19_g15 = Normal222;
				float3 worldNormal19_g15 = normalize( float3(dot(tanToWorld0,tanNormal19_g15), dot(tanToWorld1,tanNormal19_g15), dot(tanToWorld2,tanNormal19_g15)) );
				float3 CurrentNormal23_g15 = worldNormal19_g15;
				float dotResult20_g15 = dot( CurrentNormal23_g15 , _MainLightPosition.xyz );
				float NDotL21_g15 = dotResult20_g15;
				float ase_lightAtten = 0;
				Light ase_mainLight = GetMainLight( ShadowCoords );
				ase_lightAtten = ase_mainLight.distanceAttenuation * ase_mainLight.shadowAttenuation;
				float3 AttenuationColor8_g15 = ( _MainLightColor.rgb * ase_lightAtten );
				float2 uv_MetallicSkinAlpha = IN.ase_texcoord2.xy * _MetallicSkinAlpha_ST.xy + _MetallicSkinAlpha_ST.zw;
				float4 tex2DNode409 = tex2D( _MetallicSkinAlpha, uv_MetallicSkinAlpha );
				float4 SubSurfaceScatter410 = tex2DNode409;
				float2 uv_Tattoo2 = IN.ase_texcoord2.xy * _Tattoo2_ST.xy + _Tattoo2_ST.zw;
				float4 tex2DNode501 = tex2D( _Tattoo2, uv_Tattoo2 );
				float2 uv_Tattoo1 = IN.ase_texcoord2.xy * _Tattoo1_ST.xy + _Tattoo1_ST.zw;
				float4 tex2DNode499 = tex2D( _Tattoo1, uv_Tattoo1 );
				float2 uv_AlbedoTransparencySkin = IN.ase_texcoord2.xy * _AlbedoTransparencySkin_ST.xy + _AlbedoTransparencySkin_ST.zw;
				float4 tex2DNode162 = tex2D( _AlbedoTransparencySkin, uv_AlbedoTransparencySkin );
				float3 desaturateInitialColor16 = tex2DNode162.rgb;
				float desaturateDot16 = dot( desaturateInitialColor16, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar16 = lerp( desaturateInitialColor16, desaturateDot16.xxx, (1.0 + (_AlbedoSkinSaturation - 0.0) * (0.0 - 1.0) / (1.0 - 0.0)) );
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - WorldPosition );
				ase_worldViewDir = normalize(ase_worldViewDir);
				float3 tanNormal52 = Normal222;
				float temp_output_372_0 = (0.0 + (_PeachSkinValue - 0.0) * (5.0 - 0.0) / (1.0 - 0.0));
				float temp_output_144_0 = (10.0 + (_PeachSkinSpread - 0.0) * (2.0 - 10.0) / (1.0 - 0.0));
				float fresnelNdotV52 = dot( float3(dot(tanToWorld0,tanNormal52), dot(tanToWorld1,tanNormal52), dot(tanToWorld2,tanNormal52)), ase_worldViewDir );
				float fresnelNode52 = ( 0.0 + temp_output_372_0 * pow( max( 1.0 - fresnelNdotV52 , 0.0001 ), temp_output_144_0 ) );
				float layeredBlendVar505 = ( _Tattoo1Value * tex2DNode499.a );
				float4 layeredBlend505 = ( lerp( ( ( _SkinTint * float4( desaturateVar16 , 0.0 ) ) + float4( ( desaturateVar16 * fresnelNode52 ) , 0.0 ) ),tex2DNode499 , layeredBlendVar505 ) );
				float layeredBlendVar506 = ( _Tattoo2Value * tex2DNode501.a );
				float4 layeredBlend506 = ( lerp( layeredBlend505,tex2DNode501 , layeredBlendVar506 ) );
				half4 AlbedoSkinTx217 = layeredBlend506;
				float4 Translucency236 = ( _SubSurfaceScattering * _SubSurfaceScatteringColor );
				float3 DiffuseColor70_g15 = ( ( ( max( ( LightWrapVector47_g15 + ( ( 1.0 - LightWrapVector47_g15 ) * NDotL21_g15 ) ) , float3(0,0,0) ) * AttenuationColor8_g15 ) + (UNITY_LIGHTMODEL_AMBIENT).rgb ) * ( SubSurfaceScatter410 * AlbedoSkinTx217 * Translucency236 ).rgb );
				float3 normalizeResult77_g15 = normalize( _MainLightPosition.xyz );
				float3 normalizeResult28_g15 = normalize( ( normalizeResult77_g15 + ase_worldViewDir ) );
				float3 HalfDirection29_g15 = normalizeResult28_g15;
				float dotResult32_g15 = dot( HalfDirection29_g15 , CurrentNormal23_g15 );
				float SpecularPower14_g15 = exp2( ( ( 0.0 * 10.0 ) + 1.0 ) );
				float3 specularFinalColor42_g15 = ( AttenuationColor8_g15 * pow( max( dotResult32_g15 , 0.0 ) , SpecularPower14_g15 ) * 0.0 );
				float3 LightWrapVector47_g14 = (( 0.45 * 0.5 )).xxx;
				float3 tanNormal19_g14 = Normal222;
				float3 worldNormal19_g14 = normalize( float3(dot(tanToWorld0,tanNormal19_g14), dot(tanToWorld1,tanNormal19_g14), dot(tanToWorld2,tanNormal19_g14)) );
				float3 CurrentNormal23_g14 = worldNormal19_g14;
				float dotResult20_g14 = dot( CurrentNormal23_g14 , _MainLightPosition.xyz );
				float NDotL21_g14 = dotResult20_g14;
				float3 AttenuationColor8_g14 = ( _MainLightColor.rgb * ase_lightAtten );
				float3 DiffuseColor70_g14 = ( ( ( max( ( LightWrapVector47_g14 + ( ( 1.0 - LightWrapVector47_g14 ) * NDotL21_g14 ) ) , float3(0,0,0) ) * AttenuationColor8_g14 ) + (UNITY_LIGHTMODEL_AMBIENT).rgb ) * float3( 1,1,1 ) );
				float3 normalizeResult77_g14 = normalize( _MainLightPosition.xyz );
				float3 normalizeResult28_g14 = normalize( ( normalizeResult77_g14 + ase_worldViewDir ) );
				float3 HalfDirection29_g14 = normalizeResult28_g14;
				float dotResult32_g14 = dot( HalfDirection29_g14 , CurrentNormal23_g14 );
				float SpecularPower14_g14 = exp2( ( ( 0.0 * 10.0 ) + 1.0 ) );
				float3 specularFinalColor42_g14 = ( AttenuationColor8_g14 * pow( max( dotResult32_g14 , 0.0 ) , SpecularPower14_g14 ) * 0.0 );
				float3 desaturateInitialColor459 = ( DiffuseColor70_g15 + specularFinalColor42_g15 );
				float desaturateDot459 = dot( desaturateInitialColor459, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar459 = lerp( desaturateInitialColor459, desaturateDot459.xxx, ( DiffuseColor70_g14 + specularFinalColor42_g14 ).x );
				float3 LightWrapVector47_g16 = (( 0.55 * 0.5 )).xxx;
				float3 tanNormal19_g16 = Normal222;
				float3 worldNormal19_g16 = normalize( float3(dot(tanToWorld0,tanNormal19_g16), dot(tanToWorld1,tanNormal19_g16), dot(tanToWorld2,tanNormal19_g16)) );
				float3 CurrentNormal23_g16 = worldNormal19_g16;
				float dotResult20_g16 = dot( CurrentNormal23_g16 , _MainLightPosition.xyz );
				float NDotL21_g16 = dotResult20_g16;
				float3 AttenuationColor8_g16 = ( _MainLightColor.rgb * ase_lightAtten );
				float3 DiffuseColor70_g16 = ( ( ( max( ( LightWrapVector47_g16 + ( ( 1.0 - LightWrapVector47_g16 ) * NDotL21_g16 ) ) , float3(0,0,0) ) * AttenuationColor8_g16 ) + (UNITY_LIGHTMODEL_AMBIENT).rgb ) * AlbedoSkinTx217.rgb );
				float3 normalizeResult77_g16 = normalize( _MainLightPosition.xyz );
				float3 normalizeResult28_g16 = normalize( ( normalizeResult77_g16 + ase_worldViewDir ) );
				float3 HalfDirection29_g16 = normalizeResult28_g16;
				float dotResult32_g16 = dot( HalfDirection29_g16 , CurrentNormal23_g16 );
				float SpecularPower14_g16 = exp2( ( ( 0.0 * 10.0 ) + 1.0 ) );
				float3 specularFinalColor42_g16 = ( AttenuationColor8_g16 * pow( max( dotResult32_g16 , 0.0 ) , SpecularPower14_g16 ) * 0.0 );
				float3 temp_output_439_0 = ( DiffuseColor70_g16 + specularFinalColor42_g16 );
				float3 LightWrapVector47_g8 = (( 0.54 * 0.5 )).xxx;
				float3 tanNormal19_g8 = Normal222;
				float3 worldNormal19_g8 = normalize( float3(dot(tanToWorld0,tanNormal19_g8), dot(tanToWorld1,tanNormal19_g8), dot(tanToWorld2,tanNormal19_g8)) );
				float3 CurrentNormal23_g8 = worldNormal19_g8;
				float dotResult20_g8 = dot( CurrentNormal23_g8 , _MainLightPosition.xyz );
				float NDotL21_g8 = dotResult20_g8;
				float3 AttenuationColor8_g8 = ( _MainLightColor.rgb * ase_lightAtten );
				float4 color398 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
				float3 DiffuseColor70_g8 = ( ( ( max( ( LightWrapVector47_g8 + ( ( 1.0 - LightWrapVector47_g8 ) * NDotL21_g8 ) ) , float3(0,0,0) ) * AttenuationColor8_g8 ) + (UNITY_LIGHTMODEL_AMBIENT).rgb ) * color398.rgb );
				float3 normalizeResult77_g8 = normalize( _MainLightPosition.xyz );
				float3 normalizeResult28_g8 = normalize( ( normalizeResult77_g8 + ase_worldViewDir ) );
				float3 HalfDirection29_g8 = normalizeResult28_g8;
				float dotResult32_g8 = dot( HalfDirection29_g8 , CurrentNormal23_g8 );
				float SpecularPower14_g8 = exp2( ( ( 0.0 * 10.0 ) + 1.0 ) );
				float3 specularFinalColor42_g8 = ( AttenuationColor8_g8 * pow( max( dotResult32_g8 , 0.0 ) , SpecularPower14_g8 ) * 0.0 );
				float3 temp_output_400_0 = ( DiffuseColor70_g8 + specularFinalColor42_g8 );
				float3 clampResult403 = clamp( ( temp_output_400_0 + temp_output_400_0 + temp_output_400_0 + temp_output_400_0 ) , float3( 0,0,0 ) , float3( 1,1,1 ) );
				float3 LightWrapVector47_g7 = (( 0.39 * 0.5 )).xxx;
				float3 tanNormal19_g7 = Normal222;
				float3 worldNormal19_g7 = normalize( float3(dot(tanToWorld0,tanNormal19_g7), dot(tanToWorld1,tanNormal19_g7), dot(tanToWorld2,tanNormal19_g7)) );
				float3 CurrentNormal23_g7 = worldNormal19_g7;
				float dotResult20_g7 = dot( CurrentNormal23_g7 , _MainLightPosition.xyz );
				float NDotL21_g7 = dotResult20_g7;
				float3 AttenuationColor8_g7 = ( _MainLightColor.rgb * ase_lightAtten );
				float3 DiffuseColor70_g7 = ( ( ( max( ( LightWrapVector47_g7 + ( ( 1.0 - LightWrapVector47_g7 ) * NDotL21_g7 ) ) , float3(0,0,0) ) * AttenuationColor8_g7 ) + (UNITY_LIGHTMODEL_AMBIENT).rgb ) * color398.rgb );
				float3 normalizeResult77_g7 = normalize( _MainLightPosition.xyz );
				float3 normalizeResult28_g7 = normalize( ( normalizeResult77_g7 + ase_worldViewDir ) );
				float3 HalfDirection29_g7 = normalizeResult28_g7;
				float dotResult32_g7 = dot( HalfDirection29_g7 , CurrentNormal23_g7 );
				float SpecularPower14_g7 = exp2( ( ( 0.0 * 10.0 ) + 1.0 ) );
				float3 specularFinalColor42_g7 = ( AttenuationColor8_g7 * pow( max( dotResult32_g7 , 0.0 ) , SpecularPower14_g7 ) * 0.0 );
				float3 temp_output_399_0 = ( DiffuseColor70_g7 + specularFinalColor42_g7 );
				float3 clampResult404 = clamp( ( temp_output_399_0 + temp_output_399_0 + temp_output_399_0 + temp_output_399_0 ) , float3( 0,0,0 ) , float3( 1,1,1 ) );
				float3 clampResult395 = clamp( ( clampResult403 - clampResult404 ) , float3( 0,0,0 ) , float3( 1,1,1 ) );
				float MetallicAlpha412 = tex2DNode409.a;
				float4 BloodRim406 = ( float4( clampResult395 , 0.0 ) * _BloodRimColor * _BloodRimScatterValue * MetallicAlpha412 );
				float4 Albedo515 = ( float4( desaturateVar459 , 0.0 ) + ( float4( temp_output_439_0 , 0.0 ) + BloodRim406 ) + AlbedoSkinTx217 );
				
				float2 uv_EmissiveTexture = IN.ase_texcoord2.xy * _EmissiveTexture_ST.xy + _EmissiveTexture_ST.zw;
				float4 temp_output_389_0 = ( tex2D( _EmissiveTexture, uv_EmissiveTexture ) * _EmissiveColor );
				float4 Emissive407 = temp_output_389_0;
				
				half AlbedoAlpha219 = tex2DNode162.a;
				
				
				float3 Albedo = Albedo515.rgb;
				float3 Emission = Emissive407.rgb;
				float Alpha = AlbedoAlpha219;
				float AlphaClipThreshold = 0.5;

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				MetaInput metaInput = (MetaInput)0;
				metaInput.Albedo = Albedo;
				metaInput.Emission = Emission;
				
				return MetaFragment(metaInput);
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "Universal2D"
			Tags { "LightMode"="Universal2D" }

			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 110000

			
			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_2D

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			
			#define ASE_NEEDS_VERT_NORMAL
			#define ASE_NEEDS_FRAG_WORLD_POSITION
			#define ASE_NEEDS_FRAG_SHADOWCOORDS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _SHADOWS_SOFT


			#pragma shader_feature _ _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_tangent : TANGENT;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float4 ase_texcoord2 : TEXCOORD2;
				float4 ase_texcoord3 : TEXCOORD3;
				float4 ase_texcoord4 : TEXCOORD4;
				float4 ase_texcoord5 : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _NormalMapSkin_ST;
			float4 _EmissiveColor;
			float4 _EmissiveTexture_ST;
			float4 _BloodRimColor;
			float4 _SubSurfaceScatteringColor;
			float4 _AlbedoTransparencySkin_ST;
			float4 _SkinTint;
			float4 _Tattoo1_ST;
			float4 _Tattoo2_ST;
			float4 _WrinkleNormal6_ST;
			float4 _WrinkleNormal5_ST;
			float4 _MetallicSkinAlpha_ST;
			float4 _WrinkleNormal4_ST;
			float4 _DetailNormal_ST;
			float4 _WrinkleNormal1_ST;
			float4 _WrinkleNormal3_ST;
			float4 _WrinkleNormal2_ST;
			float _NormalMapSkinValue;
			float _BloodRimScatterValue;
			float _DetailNormalValue;
			float _SubSurfaceScattering;
			float _PeachSkinSpread;
			float _PeachSkinValue;
			float _WrinkleNormalValue1;
			float _WrinkleNormalValue4;
			float _WrinkleNormalValue2;
			float _Tattoo1Value;
			float _Tattoo2Value;
			float _MetallicValue;
			float _WrinkleNormalValue6;
			float _WrinkleNormalValue3;
			float _WrinkleNormalValue5;
			float _AlbedoSkinSaturation;
			float _MetallicSmoothnessSkin;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			sampler2D _NormalMapSkin;
			sampler2D _DetailNormal;
			sampler2D _WrinkleNormal1;
			sampler2D _WrinkleNormal2;
			sampler2D _WrinkleNormal3;
			sampler2D _WrinkleNormal4;
			sampler2D _WrinkleNormal5;
			sampler2D _WrinkleNormal6;
			sampler2D _MetallicSkinAlpha;
			sampler2D _Tattoo2;
			sampler2D _Tattoo1;
			sampler2D _AlbedoTransparencySkin;


			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );

				float3 ase_worldTangent = TransformObjectToWorldDir(v.ase_tangent.xyz);
				o.ase_texcoord3.xyz = ase_worldTangent;
				float3 ase_worldNormal = TransformObjectToWorldNormal(v.ase_normal);
				o.ase_texcoord4.xyz = ase_worldNormal;
				float ase_vertexTangentSign = v.ase_tangent.w * unity_WorldTransformParams.w;
				float3 ase_worldBitangent = cross( ase_worldNormal, ase_worldTangent ) * ase_vertexTangentSign;
				o.ase_texcoord5.xyz = ase_worldBitangent;
				
				o.ase_texcoord2.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord2.zw = 0;
				o.ase_texcoord3.w = 0;
				o.ase_texcoord4.w = 0;
				o.ase_texcoord5.w = 0;
				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif

				o.clipPos = positionCS;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_texcoord : TEXCOORD0;
				float4 ase_tangent : TANGENT;

				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_texcoord = v.ase_texcoord;
				o.ase_tangent = v.ase_tangent;
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_texcoord = patch[0].ase_texcoord * bary.x + patch[1].ase_texcoord * bary.y + patch[2].ase_texcoord * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			half4 frag(VertexOutput IN  ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				float3 LightWrapVector47_g15 = (( 0.45 * 0.5 )).xxx;
				float2 uv_NormalMapSkin = IN.ase_texcoord2.xy * _NormalMapSkin_ST.xy + _NormalMapSkin_ST.zw;
				float3 unpack23 = UnpackNormalScale( tex2D( _NormalMapSkin, uv_NormalMapSkin ), _NormalMapSkinValue );
				unpack23.z = lerp( 1, unpack23.z, saturate(_NormalMapSkinValue) );
				float2 uv_DetailNormal = IN.ase_texcoord2.xy * _DetailNormal_ST.xy + _DetailNormal_ST.zw;
				float3 unpack28 = UnpackNormalScale( tex2D( _DetailNormal, uv_DetailNormal ), _DetailNormalValue );
				unpack28.z = lerp( 1, unpack28.z, saturate(_DetailNormalValue) );
				float2 uv_WrinkleNormal1 = IN.ase_texcoord2.xy * _WrinkleNormal1_ST.xy + _WrinkleNormal1_ST.zw;
				float3 unpack265 = UnpackNormalScale( tex2D( _WrinkleNormal1, uv_WrinkleNormal1 ), _WrinkleNormalValue1 );
				unpack265.z = lerp( 1, unpack265.z, saturate(_WrinkleNormalValue1) );
				float2 uv_WrinkleNormal2 = IN.ase_texcoord2.xy * _WrinkleNormal2_ST.xy + _WrinkleNormal2_ST.zw;
				float3 unpack268 = UnpackNormalScale( tex2D( _WrinkleNormal2, uv_WrinkleNormal2 ), _WrinkleNormalValue2 );
				unpack268.z = lerp( 1, unpack268.z, saturate(_WrinkleNormalValue2) );
				float2 uv_WrinkleNormal3 = IN.ase_texcoord2.xy * _WrinkleNormal3_ST.xy + _WrinkleNormal3_ST.zw;
				float3 unpack271 = UnpackNormalScale( tex2D( _WrinkleNormal3, uv_WrinkleNormal3 ), _WrinkleNormalValue3 );
				unpack271.z = lerp( 1, unpack271.z, saturate(_WrinkleNormalValue3) );
				float2 uv_WrinkleNormal4 = IN.ase_texcoord2.xy * _WrinkleNormal4_ST.xy + _WrinkleNormal4_ST.zw;
				float3 unpack274 = UnpackNormalScale( tex2D( _WrinkleNormal4, uv_WrinkleNormal4 ), _WrinkleNormalValue4 );
				unpack274.z = lerp( 1, unpack274.z, saturate(_WrinkleNormalValue4) );
				float2 uv_WrinkleNormal5 = IN.ase_texcoord2.xy * _WrinkleNormal5_ST.xy + _WrinkleNormal5_ST.zw;
				float3 unpack272 = UnpackNormalScale( tex2D( _WrinkleNormal5, uv_WrinkleNormal5 ), _WrinkleNormalValue5 );
				unpack272.z = lerp( 1, unpack272.z, saturate(_WrinkleNormalValue5) );
				float2 uv_WrinkleNormal6 = IN.ase_texcoord2.xy * _WrinkleNormal6_ST.xy + _WrinkleNormal6_ST.zw;
				float3 unpack277 = UnpackNormalScale( tex2D( _WrinkleNormal6, uv_WrinkleNormal6 ), _WrinkleNormalValue6 );
				unpack277.z = lerp( 1, unpack277.z, saturate(_WrinkleNormalValue6) );
				float3 temp_output_281_0 = BlendNormal( BlendNormal( BlendNormal( BlendNormal( BlendNormal( unpack265 , unpack268 ) , unpack271 ) , unpack274 ) , unpack272 ) , unpack277 );
				float3 Normal222 = BlendNormal( BlendNormal( unpack23 , unpack28 ) , temp_output_281_0 );
				float3 ase_worldTangent = IN.ase_texcoord3.xyz;
				float3 ase_worldNormal = IN.ase_texcoord4.xyz;
				float3 ase_worldBitangent = IN.ase_texcoord5.xyz;
				float3 tanToWorld0 = float3( ase_worldTangent.x, ase_worldBitangent.x, ase_worldNormal.x );
				float3 tanToWorld1 = float3( ase_worldTangent.y, ase_worldBitangent.y, ase_worldNormal.y );
				float3 tanToWorld2 = float3( ase_worldTangent.z, ase_worldBitangent.z, ase_worldNormal.z );
				float3 tanNormal19_g15 = Normal222;
				float3 worldNormal19_g15 = normalize( float3(dot(tanToWorld0,tanNormal19_g15), dot(tanToWorld1,tanNormal19_g15), dot(tanToWorld2,tanNormal19_g15)) );
				float3 CurrentNormal23_g15 = worldNormal19_g15;
				float dotResult20_g15 = dot( CurrentNormal23_g15 , _MainLightPosition.xyz );
				float NDotL21_g15 = dotResult20_g15;
				float ase_lightAtten = 0;
				Light ase_mainLight = GetMainLight( ShadowCoords );
				ase_lightAtten = ase_mainLight.distanceAttenuation * ase_mainLight.shadowAttenuation;
				float3 AttenuationColor8_g15 = ( _MainLightColor.rgb * ase_lightAtten );
				float2 uv_MetallicSkinAlpha = IN.ase_texcoord2.xy * _MetallicSkinAlpha_ST.xy + _MetallicSkinAlpha_ST.zw;
				float4 tex2DNode409 = tex2D( _MetallicSkinAlpha, uv_MetallicSkinAlpha );
				float4 SubSurfaceScatter410 = tex2DNode409;
				float2 uv_Tattoo2 = IN.ase_texcoord2.xy * _Tattoo2_ST.xy + _Tattoo2_ST.zw;
				float4 tex2DNode501 = tex2D( _Tattoo2, uv_Tattoo2 );
				float2 uv_Tattoo1 = IN.ase_texcoord2.xy * _Tattoo1_ST.xy + _Tattoo1_ST.zw;
				float4 tex2DNode499 = tex2D( _Tattoo1, uv_Tattoo1 );
				float2 uv_AlbedoTransparencySkin = IN.ase_texcoord2.xy * _AlbedoTransparencySkin_ST.xy + _AlbedoTransparencySkin_ST.zw;
				float4 tex2DNode162 = tex2D( _AlbedoTransparencySkin, uv_AlbedoTransparencySkin );
				float3 desaturateInitialColor16 = tex2DNode162.rgb;
				float desaturateDot16 = dot( desaturateInitialColor16, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar16 = lerp( desaturateInitialColor16, desaturateDot16.xxx, (1.0 + (_AlbedoSkinSaturation - 0.0) * (0.0 - 1.0) / (1.0 - 0.0)) );
				float3 ase_worldViewDir = ( _WorldSpaceCameraPos.xyz - WorldPosition );
				ase_worldViewDir = normalize(ase_worldViewDir);
				float3 tanNormal52 = Normal222;
				float temp_output_372_0 = (0.0 + (_PeachSkinValue - 0.0) * (5.0 - 0.0) / (1.0 - 0.0));
				float temp_output_144_0 = (10.0 + (_PeachSkinSpread - 0.0) * (2.0 - 10.0) / (1.0 - 0.0));
				float fresnelNdotV52 = dot( float3(dot(tanToWorld0,tanNormal52), dot(tanToWorld1,tanNormal52), dot(tanToWorld2,tanNormal52)), ase_worldViewDir );
				float fresnelNode52 = ( 0.0 + temp_output_372_0 * pow( max( 1.0 - fresnelNdotV52 , 0.0001 ), temp_output_144_0 ) );
				float layeredBlendVar505 = ( _Tattoo1Value * tex2DNode499.a );
				float4 layeredBlend505 = ( lerp( ( ( _SkinTint * float4( desaturateVar16 , 0.0 ) ) + float4( ( desaturateVar16 * fresnelNode52 ) , 0.0 ) ),tex2DNode499 , layeredBlendVar505 ) );
				float layeredBlendVar506 = ( _Tattoo2Value * tex2DNode501.a );
				float4 layeredBlend506 = ( lerp( layeredBlend505,tex2DNode501 , layeredBlendVar506 ) );
				half4 AlbedoSkinTx217 = layeredBlend506;
				float4 Translucency236 = ( _SubSurfaceScattering * _SubSurfaceScatteringColor );
				float3 DiffuseColor70_g15 = ( ( ( max( ( LightWrapVector47_g15 + ( ( 1.0 - LightWrapVector47_g15 ) * NDotL21_g15 ) ) , float3(0,0,0) ) * AttenuationColor8_g15 ) + (UNITY_LIGHTMODEL_AMBIENT).rgb ) * ( SubSurfaceScatter410 * AlbedoSkinTx217 * Translucency236 ).rgb );
				float3 normalizeResult77_g15 = normalize( _MainLightPosition.xyz );
				float3 normalizeResult28_g15 = normalize( ( normalizeResult77_g15 + ase_worldViewDir ) );
				float3 HalfDirection29_g15 = normalizeResult28_g15;
				float dotResult32_g15 = dot( HalfDirection29_g15 , CurrentNormal23_g15 );
				float SpecularPower14_g15 = exp2( ( ( 0.0 * 10.0 ) + 1.0 ) );
				float3 specularFinalColor42_g15 = ( AttenuationColor8_g15 * pow( max( dotResult32_g15 , 0.0 ) , SpecularPower14_g15 ) * 0.0 );
				float3 LightWrapVector47_g14 = (( 0.45 * 0.5 )).xxx;
				float3 tanNormal19_g14 = Normal222;
				float3 worldNormal19_g14 = normalize( float3(dot(tanToWorld0,tanNormal19_g14), dot(tanToWorld1,tanNormal19_g14), dot(tanToWorld2,tanNormal19_g14)) );
				float3 CurrentNormal23_g14 = worldNormal19_g14;
				float dotResult20_g14 = dot( CurrentNormal23_g14 , _MainLightPosition.xyz );
				float NDotL21_g14 = dotResult20_g14;
				float3 AttenuationColor8_g14 = ( _MainLightColor.rgb * ase_lightAtten );
				float3 DiffuseColor70_g14 = ( ( ( max( ( LightWrapVector47_g14 + ( ( 1.0 - LightWrapVector47_g14 ) * NDotL21_g14 ) ) , float3(0,0,0) ) * AttenuationColor8_g14 ) + (UNITY_LIGHTMODEL_AMBIENT).rgb ) * float3( 1,1,1 ) );
				float3 normalizeResult77_g14 = normalize( _MainLightPosition.xyz );
				float3 normalizeResult28_g14 = normalize( ( normalizeResult77_g14 + ase_worldViewDir ) );
				float3 HalfDirection29_g14 = normalizeResult28_g14;
				float dotResult32_g14 = dot( HalfDirection29_g14 , CurrentNormal23_g14 );
				float SpecularPower14_g14 = exp2( ( ( 0.0 * 10.0 ) + 1.0 ) );
				float3 specularFinalColor42_g14 = ( AttenuationColor8_g14 * pow( max( dotResult32_g14 , 0.0 ) , SpecularPower14_g14 ) * 0.0 );
				float3 desaturateInitialColor459 = ( DiffuseColor70_g15 + specularFinalColor42_g15 );
				float desaturateDot459 = dot( desaturateInitialColor459, float3( 0.299, 0.587, 0.114 ));
				float3 desaturateVar459 = lerp( desaturateInitialColor459, desaturateDot459.xxx, ( DiffuseColor70_g14 + specularFinalColor42_g14 ).x );
				float3 LightWrapVector47_g16 = (( 0.55 * 0.5 )).xxx;
				float3 tanNormal19_g16 = Normal222;
				float3 worldNormal19_g16 = normalize( float3(dot(tanToWorld0,tanNormal19_g16), dot(tanToWorld1,tanNormal19_g16), dot(tanToWorld2,tanNormal19_g16)) );
				float3 CurrentNormal23_g16 = worldNormal19_g16;
				float dotResult20_g16 = dot( CurrentNormal23_g16 , _MainLightPosition.xyz );
				float NDotL21_g16 = dotResult20_g16;
				float3 AttenuationColor8_g16 = ( _MainLightColor.rgb * ase_lightAtten );
				float3 DiffuseColor70_g16 = ( ( ( max( ( LightWrapVector47_g16 + ( ( 1.0 - LightWrapVector47_g16 ) * NDotL21_g16 ) ) , float3(0,0,0) ) * AttenuationColor8_g16 ) + (UNITY_LIGHTMODEL_AMBIENT).rgb ) * AlbedoSkinTx217.rgb );
				float3 normalizeResult77_g16 = normalize( _MainLightPosition.xyz );
				float3 normalizeResult28_g16 = normalize( ( normalizeResult77_g16 + ase_worldViewDir ) );
				float3 HalfDirection29_g16 = normalizeResult28_g16;
				float dotResult32_g16 = dot( HalfDirection29_g16 , CurrentNormal23_g16 );
				float SpecularPower14_g16 = exp2( ( ( 0.0 * 10.0 ) + 1.0 ) );
				float3 specularFinalColor42_g16 = ( AttenuationColor8_g16 * pow( max( dotResult32_g16 , 0.0 ) , SpecularPower14_g16 ) * 0.0 );
				float3 temp_output_439_0 = ( DiffuseColor70_g16 + specularFinalColor42_g16 );
				float3 LightWrapVector47_g8 = (( 0.54 * 0.5 )).xxx;
				float3 tanNormal19_g8 = Normal222;
				float3 worldNormal19_g8 = normalize( float3(dot(tanToWorld0,tanNormal19_g8), dot(tanToWorld1,tanNormal19_g8), dot(tanToWorld2,tanNormal19_g8)) );
				float3 CurrentNormal23_g8 = worldNormal19_g8;
				float dotResult20_g8 = dot( CurrentNormal23_g8 , _MainLightPosition.xyz );
				float NDotL21_g8 = dotResult20_g8;
				float3 AttenuationColor8_g8 = ( _MainLightColor.rgb * ase_lightAtten );
				float4 color398 = IsGammaSpace() ? float4(1,1,1,0) : float4(1,1,1,0);
				float3 DiffuseColor70_g8 = ( ( ( max( ( LightWrapVector47_g8 + ( ( 1.0 - LightWrapVector47_g8 ) * NDotL21_g8 ) ) , float3(0,0,0) ) * AttenuationColor8_g8 ) + (UNITY_LIGHTMODEL_AMBIENT).rgb ) * color398.rgb );
				float3 normalizeResult77_g8 = normalize( _MainLightPosition.xyz );
				float3 normalizeResult28_g8 = normalize( ( normalizeResult77_g8 + ase_worldViewDir ) );
				float3 HalfDirection29_g8 = normalizeResult28_g8;
				float dotResult32_g8 = dot( HalfDirection29_g8 , CurrentNormal23_g8 );
				float SpecularPower14_g8 = exp2( ( ( 0.0 * 10.0 ) + 1.0 ) );
				float3 specularFinalColor42_g8 = ( AttenuationColor8_g8 * pow( max( dotResult32_g8 , 0.0 ) , SpecularPower14_g8 ) * 0.0 );
				float3 temp_output_400_0 = ( DiffuseColor70_g8 + specularFinalColor42_g8 );
				float3 clampResult403 = clamp( ( temp_output_400_0 + temp_output_400_0 + temp_output_400_0 + temp_output_400_0 ) , float3( 0,0,0 ) , float3( 1,1,1 ) );
				float3 LightWrapVector47_g7 = (( 0.39 * 0.5 )).xxx;
				float3 tanNormal19_g7 = Normal222;
				float3 worldNormal19_g7 = normalize( float3(dot(tanToWorld0,tanNormal19_g7), dot(tanToWorld1,tanNormal19_g7), dot(tanToWorld2,tanNormal19_g7)) );
				float3 CurrentNormal23_g7 = worldNormal19_g7;
				float dotResult20_g7 = dot( CurrentNormal23_g7 , _MainLightPosition.xyz );
				float NDotL21_g7 = dotResult20_g7;
				float3 AttenuationColor8_g7 = ( _MainLightColor.rgb * ase_lightAtten );
				float3 DiffuseColor70_g7 = ( ( ( max( ( LightWrapVector47_g7 + ( ( 1.0 - LightWrapVector47_g7 ) * NDotL21_g7 ) ) , float3(0,0,0) ) * AttenuationColor8_g7 ) + (UNITY_LIGHTMODEL_AMBIENT).rgb ) * color398.rgb );
				float3 normalizeResult77_g7 = normalize( _MainLightPosition.xyz );
				float3 normalizeResult28_g7 = normalize( ( normalizeResult77_g7 + ase_worldViewDir ) );
				float3 HalfDirection29_g7 = normalizeResult28_g7;
				float dotResult32_g7 = dot( HalfDirection29_g7 , CurrentNormal23_g7 );
				float SpecularPower14_g7 = exp2( ( ( 0.0 * 10.0 ) + 1.0 ) );
				float3 specularFinalColor42_g7 = ( AttenuationColor8_g7 * pow( max( dotResult32_g7 , 0.0 ) , SpecularPower14_g7 ) * 0.0 );
				float3 temp_output_399_0 = ( DiffuseColor70_g7 + specularFinalColor42_g7 );
				float3 clampResult404 = clamp( ( temp_output_399_0 + temp_output_399_0 + temp_output_399_0 + temp_output_399_0 ) , float3( 0,0,0 ) , float3( 1,1,1 ) );
				float3 clampResult395 = clamp( ( clampResult403 - clampResult404 ) , float3( 0,0,0 ) , float3( 1,1,1 ) );
				float MetallicAlpha412 = tex2DNode409.a;
				float4 BloodRim406 = ( float4( clampResult395 , 0.0 ) * _BloodRimColor * _BloodRimScatterValue * MetallicAlpha412 );
				float4 Albedo515 = ( float4( desaturateVar459 , 0.0 ) + ( float4( temp_output_439_0 , 0.0 ) + BloodRim406 ) + AlbedoSkinTx217 );
				
				half AlbedoAlpha219 = tex2DNode162.a;
				
				
				float3 Albedo = Albedo515.rgb;
				float Alpha = AlbedoAlpha219;
				float AlphaClipThreshold = 0.5;

				half4 color = half4( Albedo, Alpha );

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				return color;
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "DepthNormals"
			Tags { "LightMode"="DepthNormals" }

			ZWrite On
			Blend One Zero
            ZTest LEqual
            ZWrite On

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 110000

			
			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS_DEPTHNORMALSONLY

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 worldPos : TEXCOORD0;
				#endif
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
				float4 shadowCoord : TEXCOORD1;
				#endif
				float3 worldNormal : TEXCOORD2;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _NormalMapSkin_ST;
			float4 _EmissiveColor;
			float4 _EmissiveTexture_ST;
			float4 _BloodRimColor;
			float4 _SubSurfaceScatteringColor;
			float4 _AlbedoTransparencySkin_ST;
			float4 _SkinTint;
			float4 _Tattoo1_ST;
			float4 _Tattoo2_ST;
			float4 _WrinkleNormal6_ST;
			float4 _WrinkleNormal5_ST;
			float4 _MetallicSkinAlpha_ST;
			float4 _WrinkleNormal4_ST;
			float4 _DetailNormal_ST;
			float4 _WrinkleNormal1_ST;
			float4 _WrinkleNormal3_ST;
			float4 _WrinkleNormal2_ST;
			float _NormalMapSkinValue;
			float _BloodRimScatterValue;
			float _DetailNormalValue;
			float _SubSurfaceScattering;
			float _PeachSkinSpread;
			float _PeachSkinValue;
			float _WrinkleNormalValue1;
			float _WrinkleNormalValue4;
			float _WrinkleNormalValue2;
			float _Tattoo1Value;
			float _Tattoo2Value;
			float _MetallicValue;
			float _WrinkleNormalValue6;
			float _WrinkleNormalValue3;
			float _WrinkleNormalValue5;
			float _AlbedoSkinSaturation;
			float _MetallicSmoothnessSkin;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			

			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif

				v.ase_normal = v.ase_normal;
				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 normalWS = TransformObjectToWorldNormal( v.ase_normal );
				float4 positionCS = TransformWorldToHClip( positionWS );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				o.worldPos = positionWS;
				#endif

				o.worldNormal = normalWS;

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					VertexPositionInputs vertexInput = (VertexPositionInputs)0;
					vertexInput.positionWS = positionWS;
					vertexInput.positionCS = positionCS;
					o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				o.clipPos = positionCS;
				return o;
			}

			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif
			half4 frag(	VertexOutput IN 
						#ifdef ASE_DEPTH_WRITE_ON
						,out float outputDepth : ASE_SV_DEPTH
						#endif
						 ) : SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( IN );

				#if defined(ASE_NEEDS_FRAG_WORLD_POSITION)
				float3 WorldPosition = IN.worldPos;
				#endif
				float4 ShadowCoords = float4( 0, 0, 0, 0 );

				#if defined(ASE_NEEDS_FRAG_SHADOWCOORDS)
					#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
						ShadowCoords = IN.shadowCoord;
					#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
						ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
					#endif
				#endif

				
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif
				
				#ifdef ASE_DEPTH_WRITE_ON
				outputDepth = DepthValue;
				#endif
				
				return float4(PackNormalOctRectEncode(TransformWorldToViewDir(IN.worldNormal, true)), 0.0, 0.0);
			}
			ENDHLSL
		}

		
		Pass
		{
			
			Name "GBuffer"
			Tags { "LightMode"="UniversalGBuffer" }
			
			Blend One Zero, One Zero
			ZWrite On
			ZTest LEqual
			Offset 0 , 0
			ColorMask RGBA
			

			HLSLPROGRAM
			
			#define _NORMAL_DROPOFF_TS 1
			#pragma multi_compile_instancing
			#pragma multi_compile _ LOD_FADE_CROSSFADE
			#pragma multi_compile_fog
			#define ASE_FOG 1
			#define _EMISSION
			#define _NORMALMAP 1
			#define ASE_SRP_VERSION 110000

			
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS
			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
			#pragma multi_compile _ _GBUFFER_NORMALS_OCT
			
			#pragma multi_compile _ DIRLIGHTMAP_COMBINED
			#pragma multi_compile _ LIGHTMAP_ON

			#pragma vertex vert
			#pragma fragment frag

			#define SHADERPASS SHADERPASS_GBUFFER

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"

			#if ASE_SRP_VERSION <= 70108
			#define REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR
			#endif

			#if defined(UNITY_INSTANCING_ENABLED) && defined(_TERRAIN_INSTANCED_PERPIXEL_NORMAL)
			    #define ENABLE_TERRAIN_PERPIXEL_NORMAL
			#endif

			

			struct VertexInput
			{
				float4 vertex : POSITION;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord1 : TEXCOORD1;
				float4 texcoord : TEXCOORD0;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct VertexOutput
			{
				float4 clipPos : SV_POSITION;
				float4 lightmapUVOrVertexSH : TEXCOORD0;
				half4 fogFactorAndVertexLight : TEXCOORD1;
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				float4 shadowCoord : TEXCOORD2;
				#endif
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 screenPos : TEXCOORD6;
				#endif
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			CBUFFER_START(UnityPerMaterial)
			float4 _NormalMapSkin_ST;
			float4 _EmissiveColor;
			float4 _EmissiveTexture_ST;
			float4 _BloodRimColor;
			float4 _SubSurfaceScatteringColor;
			float4 _AlbedoTransparencySkin_ST;
			float4 _SkinTint;
			float4 _Tattoo1_ST;
			float4 _Tattoo2_ST;
			float4 _WrinkleNormal6_ST;
			float4 _WrinkleNormal5_ST;
			float4 _MetallicSkinAlpha_ST;
			float4 _WrinkleNormal4_ST;
			float4 _DetailNormal_ST;
			float4 _WrinkleNormal1_ST;
			float4 _WrinkleNormal3_ST;
			float4 _WrinkleNormal2_ST;
			float _NormalMapSkinValue;
			float _BloodRimScatterValue;
			float _DetailNormalValue;
			float _SubSurfaceScattering;
			float _PeachSkinSpread;
			float _PeachSkinValue;
			float _WrinkleNormalValue1;
			float _WrinkleNormalValue4;
			float _WrinkleNormalValue2;
			float _Tattoo1Value;
			float _Tattoo2Value;
			float _MetallicValue;
			float _WrinkleNormalValue6;
			float _WrinkleNormalValue3;
			float _WrinkleNormalValue5;
			float _AlbedoSkinSaturation;
			float _MetallicSmoothnessSkin;
			#ifdef _TRANSMISSION_ASE
				float _TransmissionShadow;
			#endif
			#ifdef _TRANSLUCENCY_ASE
				float _TransStrength;
				float _TransNormal;
				float _TransScattering;
				float _TransDirect;
				float _TransAmbient;
				float _TransShadow;
			#endif
			#ifdef TESSELLATION_ON
				float _TessPhongStrength;
				float _TessValue;
				float _TessMin;
				float _TessMax;
				float _TessEdgeLength;
				float _TessMaxDisp;
			#endif
			CBUFFER_END
			

			
			VertexOutput VertexFunction( VertexInput v  )
			{
				VertexOutput o = (VertexOutput)0;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					float3 defaultVertexValue = v.vertex.xyz;
				#else
					float3 defaultVertexValue = float3(0, 0, 0);
				#endif
				float3 vertexValue = defaultVertexValue;
				#ifdef ASE_ABSOLUTE_VERTEX_POS
					v.vertex.xyz = vertexValue;
				#else
					v.vertex.xyz += vertexValue;
				#endif
				v.ase_normal = v.ase_normal;

				float3 positionWS = TransformObjectToWorld( v.vertex.xyz );
				float3 positionVS = TransformWorldToView( positionWS );
				float4 positionCS = TransformWorldToHClip( positionWS );

				VertexNormalInputs normalInput = GetVertexNormalInputs( v.ase_normal, v.ase_tangent );

				o.tSpace0 = float4( normalInput.normalWS, positionWS.x);
				o.tSpace1 = float4( normalInput.tangentWS, positionWS.y);
				o.tSpace2 = float4( normalInput.bitangentWS, positionWS.z);

				OUTPUT_LIGHTMAP_UV( v.texcoord1, unity_LightmapST, o.lightmapUVOrVertexSH.xy );
				OUTPUT_SH( normalInput.normalWS.xyz, o.lightmapUVOrVertexSH.xyz );

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					o.lightmapUVOrVertexSH.zw = v.texcoord;
					o.lightmapUVOrVertexSH.xy = v.texcoord * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

				half3 vertexLight = VertexLighting( positionWS, normalInput.normalWS );
				#ifdef ASE_FOG
					half fogFactor = ComputeFogFactor( positionCS.z );
				#else
					half fogFactor = 0;
				#endif
				o.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
				
				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
				VertexPositionInputs vertexInput = (VertexPositionInputs)0;
				vertexInput.positionWS = positionWS;
				vertexInput.positionCS = positionCS;
				o.shadowCoord = GetShadowCoord( vertexInput );
				#endif
				
				o.clipPos = positionCS;
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				o.screenPos = ComputeScreenPos(positionCS);
				#endif
				return o;
			}
			
			#if defined(TESSELLATION_ON)
			struct VertexControl
			{
				float4 vertex : INTERNALTESSPOS;
				float3 ase_normal : NORMAL;
				float4 ase_tangent : TANGENT;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct TessellationFactors
			{
				float edge[3] : SV_TessFactor;
				float inside : SV_InsideTessFactor;
			};

			VertexControl vert ( VertexInput v )
			{
				VertexControl o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
				o.vertex = v.vertex;
				o.ase_normal = v.ase_normal;
				o.ase_tangent = v.ase_tangent;
				o.texcoord = v.texcoord;
				o.texcoord1 = v.texcoord1;
				
				return o;
			}

			TessellationFactors TessellationFunction (InputPatch<VertexControl,3> v)
			{
				TessellationFactors o;
				float4 tf = 1;
				float tessValue = _TessValue; float tessMin = _TessMin; float tessMax = _TessMax;
				float edgeLength = _TessEdgeLength; float tessMaxDisp = _TessMaxDisp;
				#if defined(ASE_FIXED_TESSELLATION)
				tf = FixedTess( tessValue );
				#elif defined(ASE_DISTANCE_TESSELLATION)
				tf = DistanceBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, tessValue, tessMin, tessMax, GetObjectToWorldMatrix(), _WorldSpaceCameraPos );
				#elif defined(ASE_LENGTH_TESSELLATION)
				tf = EdgeLengthBasedTess(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams );
				#elif defined(ASE_LENGTH_CULL_TESSELLATION)
				tf = EdgeLengthBasedTessCull(v[0].vertex, v[1].vertex, v[2].vertex, edgeLength, tessMaxDisp, GetObjectToWorldMatrix(), _WorldSpaceCameraPos, _ScreenParams, unity_CameraWorldClipPlanes );
				#endif
				o.edge[0] = tf.x; o.edge[1] = tf.y; o.edge[2] = tf.z; o.inside = tf.w;
				return o;
			}

			[domain("tri")]
			[partitioning("fractional_odd")]
			[outputtopology("triangle_cw")]
			[patchconstantfunc("TessellationFunction")]
			[outputcontrolpoints(3)]
			VertexControl HullFunction(InputPatch<VertexControl, 3> patch, uint id : SV_OutputControlPointID)
			{
			   return patch[id];
			}

			[domain("tri")]
			VertexOutput DomainFunction(TessellationFactors factors, OutputPatch<VertexControl, 3> patch, float3 bary : SV_DomainLocation)
			{
				VertexInput o = (VertexInput) 0;
				o.vertex = patch[0].vertex * bary.x + patch[1].vertex * bary.y + patch[2].vertex * bary.z;
				o.ase_normal = patch[0].ase_normal * bary.x + patch[1].ase_normal * bary.y + patch[2].ase_normal * bary.z;
				o.ase_tangent = patch[0].ase_tangent * bary.x + patch[1].ase_tangent * bary.y + patch[2].ase_tangent * bary.z;
				o.texcoord = patch[0].texcoord * bary.x + patch[1].texcoord * bary.y + patch[2].texcoord * bary.z;
				o.texcoord1 = patch[0].texcoord1 * bary.x + patch[1].texcoord1 * bary.y + patch[2].texcoord1 * bary.z;
				
				#if defined(ASE_PHONG_TESSELLATION)
				float3 pp[3];
				for (int i = 0; i < 3; ++i)
					pp[i] = o.vertex.xyz - patch[i].ase_normal * (dot(o.vertex.xyz, patch[i].ase_normal) - dot(patch[i].vertex.xyz, patch[i].ase_normal));
				float phongStrength = _TessPhongStrength;
				o.vertex.xyz = phongStrength * (pp[0]*bary.x + pp[1]*bary.y + pp[2]*bary.z) + (1.0f-phongStrength) * o.vertex.xyz;
				#endif
				UNITY_TRANSFER_INSTANCE_ID(patch[0], o);
				return VertexFunction(o);
			}
			#else
			VertexOutput vert ( VertexInput v )
			{
				return VertexFunction( v );
			}
			#endif

			#if defined(ASE_EARLY_Z_DEPTH_OPTIMIZE)
				#define ASE_SV_DEPTH SV_DepthLessEqual  
			#else
				#define ASE_SV_DEPTH SV_Depth
			#endif
			FragmentOutput frag ( VertexOutput IN 
								#ifdef ASE_DEPTH_WRITE_ON
								,out float outputDepth : ASE_SV_DEPTH
								#endif
								 )
			{
				UNITY_SETUP_INSTANCE_ID(IN);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(IN);

				#ifdef LOD_FADE_CROSSFADE
					LODDitheringTransition( IN.clipPos.xyz, unity_LODFade.x );
				#endif

				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float2 sampleCoords = (IN.lightmapUVOrVertexSH.zw / _TerrainHeightmapRecipSize.zw + 0.5f) * _TerrainHeightmapRecipSize.xy;
					float3 WorldNormal = TransformObjectToWorldNormal(normalize(SAMPLE_TEXTURE2D(_TerrainNormalmapTexture, sampler_TerrainNormalmapTexture, sampleCoords).rgb * 2 - 1));
					float3 WorldTangent = -cross(GetObjectToWorldMatrix()._13_23_33, WorldNormal);
					float3 WorldBiTangent = cross(WorldNormal, -WorldTangent);
				#else
					float3 WorldNormal = normalize( IN.tSpace0.xyz );
					float3 WorldTangent = IN.tSpace1.xyz;
					float3 WorldBiTangent = IN.tSpace2.xyz;
				#endif
				float3 WorldPosition = float3(IN.tSpace0.w,IN.tSpace1.w,IN.tSpace2.w);
				float3 WorldViewDirection = _WorldSpaceCameraPos.xyz  - WorldPosition;
				float4 ShadowCoords = float4( 0, 0, 0, 0 );
				#if defined(ASE_NEEDS_FRAG_SCREEN_POSITION)
				float4 ScreenPos = IN.screenPos;
				#endif

				#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
					ShadowCoords = IN.shadowCoord;
				#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS)
					ShadowCoords = TransformWorldToShadowCoord( WorldPosition );
				#endif
	
				WorldViewDirection = SafeNormalize( WorldViewDirection );

				
				float3 Albedo = float3(0.5, 0.5, 0.5);
				float3 Normal = float3(0, 0, 1);
				float3 Emission = 0;
				float3 Specular = 0.5;
				float Metallic = 0;
				float Smoothness = 0.5;
				float Occlusion = 1;
				float Alpha = 1;
				float AlphaClipThreshold = 0.5;
				float AlphaClipThresholdShadow = 0.5;
				float3 BakedGI = 0;
				float3 RefractionColor = 1;
				float RefractionIndex = 1;
				float3 Transmission = 1;
				float3 Translucency = 1;
				#ifdef ASE_DEPTH_WRITE_ON
				float DepthValue = 0;
				#endif

				#ifdef _ALPHATEST_ON
					clip(Alpha - AlphaClipThreshold);
				#endif

				InputData inputData;
				inputData.positionWS = WorldPosition;
				inputData.viewDirectionWS = WorldViewDirection;
				inputData.shadowCoord = ShadowCoords;

				#ifdef _NORMALMAP
					#if _NORMAL_DROPOFF_TS
					inputData.normalWS = TransformTangentToWorld(Normal, half3x3( WorldTangent, WorldBiTangent, WorldNormal ));
					#elif _NORMAL_DROPOFF_OS
					inputData.normalWS = TransformObjectToWorldNormal(Normal);
					#elif _NORMAL_DROPOFF_WS
					inputData.normalWS = Normal;
					#endif
					inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
				#else
					inputData.normalWS = WorldNormal;
				#endif

				#ifdef ASE_FOG
					inputData.fogCoord = IN.fogFactorAndVertexLight.x;
				#endif

				inputData.vertexLighting = IN.fogFactorAndVertexLight.yzw;
				#if defined(ENABLE_TERRAIN_PERPIXEL_NORMAL)
					float3 SH = SampleSH(inputData.normalWS.xyz);
				#else
					float3 SH = IN.lightmapUVOrVertexSH.xyz;
				#endif

				inputData.bakedGI = SAMPLE_GI( IN.lightmapUVOrVertexSH.xy, SH, inputData.normalWS );
				#ifdef _ASE_BAKEDGI
					inputData.bakedGI = BakedGI;
				#endif

				BRDFData brdfData;
				InitializeBRDFData( Albedo, Metallic, Specular, Smoothness, Alpha, brdfData);
				half4 color;
				color.rgb = GlobalIllumination( brdfData, inputData.bakedGI, Occlusion, inputData.normalWS, inputData.viewDirectionWS);
				color.a = Alpha;

				#ifdef _TRANSMISSION_ASE
				{
					float shadow = _TransmissionShadow;
				
					Light mainLight = GetMainLight( inputData.shadowCoord );
					float3 mainAtten = mainLight.color * mainLight.distanceAttenuation;
					mainAtten = lerp( mainAtten, mainAtten * mainLight.shadowAttenuation, shadow );
					half3 mainTransmission = max(0 , -dot(inputData.normalWS, mainLight.direction)) * mainAtten * Transmission;
					color.rgb += Albedo * mainTransmission;
				
					#ifdef _ADDITIONAL_LIGHTS
						int transPixelLightCount = GetAdditionalLightsCount();
						for (int i = 0; i < transPixelLightCount; ++i)
						{
							Light light = GetAdditionalLight(i, inputData.positionWS);
							float3 atten = light.color * light.distanceAttenuation;
							atten = lerp( atten, atten * light.shadowAttenuation, shadow );
				
							half3 transmission = max(0 , -dot(inputData.normalWS, light.direction)) * atten * Transmission;
							color.rgb += Albedo * transmission;
						}
					#endif
				}
				#endif
				
				#ifdef _TRANSLUCENCY_ASE
				{
					float shadow = _TransShadow;
					float normal = _TransNormal;
					float scattering = _TransScattering;
					float direct = _TransDirect;
					float ambient = _TransAmbient;
					float strength = _TransStrength;
				
					Light mainLight = GetMainLight( inputData.shadowCoord );
					float3 mainAtten = mainLight.color * mainLight.distanceAttenuation;
					mainAtten = lerp( mainAtten, mainAtten * mainLight.shadowAttenuation, shadow );
				
					half3 mainLightDir = mainLight.direction + inputData.normalWS * normal;
					half mainVdotL = pow( saturate( dot( inputData.viewDirectionWS, -mainLightDir ) ), scattering );
					half3 mainTranslucency = mainAtten * ( mainVdotL * direct + inputData.bakedGI * ambient ) * Translucency;
					color.rgb += Albedo * mainTranslucency * strength;
				
					#ifdef _ADDITIONAL_LIGHTS
						int transPixelLightCount = GetAdditionalLightsCount();
						for (int i = 0; i < transPixelLightCount; ++i)
						{
							Light light = GetAdditionalLight(i, inputData.positionWS);
							float3 atten = light.color * light.distanceAttenuation;
							atten = lerp( atten, atten * light.shadowAttenuation, shadow );
				
							half3 lightDir = light.direction + inputData.normalWS * normal;
							half VdotL = pow( saturate( dot( inputData.viewDirectionWS, -lightDir ) ), scattering );
							half3 translucency = atten * ( VdotL * direct + inputData.bakedGI * ambient ) * Translucency;
							color.rgb += Albedo * translucency * strength;
						}
					#endif
				}
				#endif
				
				#ifdef _REFRACTION_ASE
					float4 projScreenPos = ScreenPos / ScreenPos.w;
					float3 refractionOffset = ( RefractionIndex - 1.0 ) * mul( UNITY_MATRIX_V, float4( WorldNormal, 0 ) ).xyz * ( 1.0 - dot( WorldNormal, WorldViewDirection ) );
					projScreenPos.xy += refractionOffset.xy;
					float3 refraction = SHADERGRAPH_SAMPLE_SCENE_COLOR( projScreenPos.xy ) * RefractionColor;
					color.rgb = lerp( refraction, color.rgb, color.a );
					color.a = 1;
				#endif
				
				#ifdef ASE_FINAL_COLOR_ALPHA_MULTIPLY
					color.rgb *= color.a;
				#endif
				
				#ifdef ASE_FOG
					#ifdef TERRAIN_SPLAT_ADDPASS
						color.rgb = MixFogColor(color.rgb, half3( 0, 0, 0 ), IN.fogFactorAndVertexLight.x );
					#else
						color.rgb = MixFog(color.rgb, IN.fogFactorAndVertexLight.x);
					#endif
				#endif
				
				#ifdef ASE_DEPTH_WRITE_ON
					outputDepth = DepthValue;
				#endif
				
				return BRDFDataToGbuffer(brdfData, inputData, Smoothness, Emission + color.rgb);
			}

			ENDHLSL
		}
		
	}
	
	CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
	Fallback "Hidden/InternalErrorShader"
	
}
/*ASEBEGIN
Version=18933
40;188;1654;804;-701.6746;-1771.724;2.465002;True;True
Node;AmplifyShaderEditor.CommentaryNode;295;-4416.595,1223.564;Inherit;False;1163.503;2740.573;Comment;20;302;301;303;281;277;280;276;279;272;274;278;275;273;269;271;270;265;268;267;266;WrinkeNormals;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;240;-2972.753,1676.923;Inherit;False;1372.586;713.6436;Comment;7;222;294;31;23;28;30;29;Normals;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;242;-4460.65,-3437.219;Inherit;False;4015.612;963.8835;Comment;21;161;162;20;16;19;119;219;499;500;503;505;57;372;52;144;380;55;139;223;520;521;Albedo;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;408;-4600.023,-435.1906;Inherit;False;2111.961;1230.582;Comment;10;418;417;416;415;414;413;412;411;410;409;Metallic;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;239;-2062.514,-253.8923;Inherit;False;1349.147;478.8339;Comment;4;236;47;7;48;Tanslucency;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;427;656.4189,1592.948;Inherit;False;3175.25;2741.462;Comment;26;461;459;457;456;455;454;453;451;450;449;448;447;446;445;444;443;442;441;439;438;437;433;430;429;515;522;Illum;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;383;-4583.506,-2253.656;Inherit;False;4074.626;1492.422;Comment;13;407;406;405;392;391;390;389;386;396;397;384;401;402;Scatter;1,1,1,1;0;0
Node;AmplifyShaderEditor.BlendNormalsNode;269;-3501.338,1427.264;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;397;-3580.128,-1720.671;Inherit;False;Constant;_floatB;float B;37;0;Create;True;0;0;0;False;0;False;0.39;0.39;-1;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;404;-2567.508,-1837.657;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;1,1,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BlendNormalsNode;278;-3488.785,1767.295;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;268;-4015.166,1585.312;Inherit;True;Property;_WrinkleNormal2;Wrinkle Normal 2;24;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;1,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;271;-4009.427,1942.221;Inherit;True;Property;_WrinkleNormal3;Wrinkle Normal 3;26;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;1,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;270;-4345.227,1959.741;Float;False;Property;_WrinkleNormalValue3;WrinkleNormal Value 3;27;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;28;-2641.467,2010.119;Inherit;True;Property;_DetailNormal;Detail Normal;10;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;1,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;399;-3143.508,-1869.657;Inherit;False;BlinnPhongLightWrap;-1;;7;139fed909c1bc1a42a96c42d8cf09006;0;5;1;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;44;FLOAT;0.5;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BlendNormalsNode;294;-2219.894,2073.02;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;274;-4018.634,2334.128;Inherit;True;Property;_WrinkleNormal4;Wrinkle Normal 4;28;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;1,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LayeredBlendNode;505;-727.5008,-3160.927;Inherit;False;6;0;FLOAT;1;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;400;-3159.508,-2077.656;Inherit;False;BlinnPhongLightWrap;-1;;8;139fed909c1bc1a42a96c42d8cf09006;0;5;1;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;44;FLOAT;0.5;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;266;-4334.248,1291.084;Float;False;Property;_WrinkleNormalValue1;WrinkleNormal Value 1;23;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;139;-3467.057,-2666.891;Float;False;Property;_PeachSkinSpread;Peach Skin Spread;4;0;Create;True;0;0;0;False;0;False;1;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;162;-3985.155,-3291.232;Inherit;True;Property;_TextureSample0;Texture Sample 0;23;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.BlendNormalsNode;280;-3507.02,2542.283;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.TFHCRemapNode;414;-4148.38,395.7169;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0.35;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;438;1299.4,2974.173;Inherit;False;Constant;_wrap;wrap;33;0;Create;True;0;0;0;False;0;False;0.55;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;393;-4177.179,-1741.41;Inherit;False;222;Normal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DesaturateOpNode;459;2472.706,2139.323;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.FresnelNode;52;-2592.895,-2838.071;Inherit;True;Standard;TangentNormal;ViewDir;False;True;5;0;FLOAT3;0,0,1;False;4;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;155;-3124.469,-3078.75;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;1;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;273;-4354.434,2351.648;Float;False;Property;_WrinkleNormalValue4;WrinkleNormal Value 4;29;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;275;-4366.595,2663.396;Float;False;Property;_WrinkleNormalValue5;WrinkleNormal Value 5;31;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.BlendNormalsNode;279;-3497.902,2191.259;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;384;-1908.985,-1838.085;Float;False;Property;_BloodRimColor;BloodRim Color;20;0;Create;True;0;0;0;False;0;False;1,0,0,0;1,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;402;-2800.565,-1756.138;Inherit;False;4;4;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;223;-2831.564,-2868.393;Inherit;False;222;Normal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;30;-2959.667,2060.233;Float;False;Property;_DetailNormalValue;Detail Normal Value;11;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.BlendNormalsNode;31;-2299.782,1839.927;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;380;-2365.739,-2588.166;Inherit;False;410;SubSurfaceScatter;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;20;-2805.69,-3387.219;Float;False;Property;_SkinTint;Skin Tint;0;0;Create;True;0;0;0;False;0;False;1,1,1,1;1,1,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;217;327.4575,-3166.736;Half;False;AlbedoSkinTx;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;272;-4035.352,2641.317;Inherit;True;Property;_WrinkleNormal5;Wrinkle Normal 5;30;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;1,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;7;-2012.513,-103.9173;Float;False;Property;_SubSurfaceScatteringColor;SubSurface Scattering Color;19;0;Create;True;0;0;0;False;0;False;1,1,1,1;0,0,0,0;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;516;1044.919,-66.9614;Inherit;False;515;Albedo;1;0;OBJECT;;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;412;-3724.035,-234.2079;Inherit;False;MetallicAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DesaturateOpNode;16;-2837.109,-3152.221;Inherit;True;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ColorNode;398;-3966.363,-2042.492;Inherit;False;Constant;_Color1;Color 1;35;0;Create;True;0;0;0;False;0;False;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;55;-3527.824,-2874.272;Float;False;Property;_PeachSkinValue;Peach Skin Value;3;0;Create;True;0;0;0;False;0;False;1;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;501;-562.5477,-3043.269;Inherit;True;Property;_Tattoo2;Tattoo2;14;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;401;-2772.488,-2207.482;Inherit;False;4;4;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;394;-2311.508,-1901.657;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;500;-1243.716,-3275.511;Inherit;False;Property;_Tattoo1Value;Tattoo 1 Value;13;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;502;-515.5141,-3330.427;Inherit;False;Property;_Tattoo2Value;Tattoo 2 Value;15;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;444;754.9789,2155.127;Inherit;False;236;Translucency;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;267;-4350.968,1602.832;Float;False;Property;_WrinkleNormalValue2;WrinkleNormal Value 2;25;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;48;-1608.138,120.818;Float;False;Property;_SubSurfaceScattering;SubSurface Scattering;18;0;Create;True;0;0;0;False;0;False;0.1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;29;-2955.148,1747.616;Float;False;Property;_NormalMapSkinValue;Normal Map Skin Value;9;0;Create;True;0;0;0;False;0;False;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;457;1941.596,2161.495;Inherit;False;BlinnPhongLightWrap;-1;;15;139fed909c1bc1a42a96c42d8cf09006;0;5;1;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;44;FLOAT;0.25;False;1;FLOAT3;0
Node;AmplifyShaderEditor.BlendNormalsNode;281;-3552.607,2884.187;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;23;-2644.734,1733.269;Inherit;True;Property;_NormalMapSkin;Normal Map Skin;8;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;1,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.AbsOpNode;521;-2840.234,-2551.538;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;57;-2122.289,-2992.814;Inherit;True;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;403;-2535.508,-2045.657;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;1,1,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;504;-212.7669,-3282.793;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;396;-3565.821,-2136.779;Inherit;False;Constant;_FloatA;Float A;34;0;Create;True;0;0;0;False;0;False;0.54;0.94;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;265;-3998.448,1273.564;Inherit;True;Property;_WrinkleNormal1;Wrinkle Normal 1;22;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;1,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TexturePropertyNode;161;-4404.928,-3298.966;Float;True;Property;_AlbedoTransparencySkin;Albedo Transparency Skin;1;0;Create;True;0;0;0;False;0;False;None;None;False;white;Auto;Texture2D;-1;0;2;SAMPLER2D;0;SAMPLERSTATE;1
Node;AmplifyShaderEditor.SimpleAddOpNode;461;3029.855,2589.495;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;222;-1869.446,2064.482;Float;False;Normal;-1;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;19;-2466.313,-3278.606;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;450;1318.315,1753.927;Inherit;False;222;Normal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;417;-2731.35,-185.2713;Float;False;Metallic;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;503;-885.0399,-3221.802;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;415;-3726.44,-108.7467;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;301;-4023.61,3378.715;Inherit;True;Property;_WrinkleNormal7;Wrinkle Normal 7;34;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;1,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;387;-1783.509,-1341.657;Inherit;True;Property;_EmissiveTexture;Emissive Texture;16;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LayeredBlendNode;506;-1.41626,-3197.696;Inherit;False;6;0;FLOAT;1;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;386;-1469.228,-1856.055;Inherit;False;4;4;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;405;-1575.509,-1613.657;Inherit;False;412;MetallicAlpha;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;447;1388.038,1941.829;Inherit;False;418;Glossiness;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;410;-3837.615,-363.1634;Float;False;SubSurfaceScatter;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;454;999.299,2355.904;Inherit;False;417;Metallic;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;453;1555.975,2039.928;Inherit;False;Constant;_miniwrap;miniwrap;36;0;Create;True;0;0;0;False;0;False;0.45;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;219;-3603.537,-3174.743;Half;False;AlbedoAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;392;-1063.508,-1373.657;Inherit;False;2;2;0;COLOR;0.01,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;439;1805.253,2711.764;Inherit;False;BlinnPhongLightWrap;-1;;16;139fed909c1bc1a42a96c42d8cf09006;0;5;1;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;44;FLOAT;0.25;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;13;-3570.222,-3065.869;Float;False;Property;_AlbedoSkinSaturation;Albedo Skin Saturation;2;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;236;-951.491,-112.6785;Float;False;Translucency;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;144;-3045.803,-2624.381;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;10;False;4;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;433;1407.094,2729.343;Inherit;False;222;Normal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;379;822.708,245.2081;Inherit;False;418;Glossiness;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;448;1442.372,2118.952;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;277;-4029.613,3002.784;Inherit;True;Property;_WrinkleNormal6;Wrinkle Normal 6;32;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;0,0;False;1;FLOAT2;1,0;False;2;FLOAT;1;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;237;1027.554,15.6273;Inherit;False;222;Normal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.ClampOpNode;395;-2087.509,-1917.657;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;1,1,1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;385;-2002.2,-1637.771;Float;False;Property;_BloodRimScatterValue;BloodRim Scatter Value;21;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;455;2513.184,2867.359;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;456;1884.922,1835.371;Inherit;False;BlinnPhongLightWrap;-1;;14;139fed909c1bc1a42a96c42d8cf09006;0;5;1;FLOAT3;1,1,1;False;4;FLOAT3;0,0,0;False;2;FLOAT;0;False;3;FLOAT;0;False;44;FLOAT;0.25;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;442;2259.421,3097.987;Inherit;False;406;BloodRim;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.BlendNormalsNode;302;-3546.604,3260.118;Inherit;False;0;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;303;-4359.412,3396.236;Float;False;Property;_WrinkleNormalValue7;WrinkleNormal Value 7;35;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;47;-1236.521,-129.7997;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;443;960.9869,2044.329;Inherit;False;410;SubSurfaceScatter;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;372;-3104.581,-2875.72;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0;False;4;FLOAT;5;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;449;986.037,2264.426;Inherit;False;222;Normal;1;0;OBJECT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.GetLocalVarNode;437;1400.84,2801.98;Inherit;False;417;Metallic;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;276;-4365.413,3020.305;Float;False;Property;_WrinkleNormalValue6;WrinkleNormal Value 6;33;0;Create;True;0;0;0;False;0;False;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;515;3355.652,2609.594;Inherit;False;Albedo;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;445;2144.503,2853.226;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;499;-1222.18,-3024.577;Inherit;True;Property;_Tattoo1;Tattoo1;12;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;black;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;388;-1719.509,-1101.656;Float;False;Property;_EmissiveColor;Emissive Color;17;1;[HDR];Create;True;0;0;0;False;0;False;0,0,0,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LightColorNode;391;-1095.508,-1517.657;Inherit;False;0;3;COLOR;0;FLOAT3;1;FLOAT;2
Node;AmplifyShaderEditor.GetLocalVarNode;446;1331.577,1845.405;Inherit;False;417;Metallic;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;220;770.1997,377.5216;Inherit;False;219;AlbedoAlpha;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;365;815.2141,168.4075;Inherit;False;417;Metallic;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;430;1380.488,2876.665;Inherit;False;418;Glossiness;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;389;-1367.509,-1261.657;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;119;-1681.603,-3084.368;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;416;-3745.676,274.9964;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;409;-4299.593,-385.1906;Inherit;True;Property;_MetallicSkinAlpha;Metallic Skin (Alpha);5;0;Create;True;0;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.GetLocalVarNode;228;755.35,67.84064;Inherit;False;407;Emissive;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;411;-4550.022,383.9325;Float;False;Property;_MetallicSmoothnessSkin;Metallic Smoothness Skin;7;0;Create;True;0;0;0;False;0;False;1;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;413;-4161.771,-33.71545;Inherit;False;Property;_MetallicValue;Metallic Value;6;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;441;955.4641,2121.841;Inherit;False;217;AlbedoSkinTx;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;418;-2730.102,293.522;Float;False;Glossiness;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;407;-739.5359,-1201.629;Float;False;Emissive;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ConditionalIfNode;390;-743.5083,-1421.657;Inherit;False;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;4;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;451;1055.76,2452.328;Inherit;False;418;Glossiness;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;429;1394.465,2649.541;Inherit;False;217;AlbedoSkinTx;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;406;-1045.678,-1859.456;Inherit;False;BloodRim;-1;True;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.AbsOpNode;520;-2909.258,-2758.61;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;522;2804.538,2980.499;Inherit;False;217;AlbedoSkinTx;1;0;OBJECT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;512;1482.885,38.78796;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Meta;0;4;Meta;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=Meta;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;513;1482.885,38.78796;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;Universal2D;0;5;Universal2D;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=Universal2D;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;518;1482.885,98.78796;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;GBuffer;0;7;GBuffer;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalGBuffer;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;509;1482.885,38.78796;Float;False;True;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;12;The Domaginarium/PBR Skin URP;94348b07e5e8bab40bd6c8a1e3df54cd;True;Forward;0;1;Forward;18;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;1;False;-1;0;False;-1;1;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;1;LightMode=UniversalForward;False;False;0;Hidden/InternalErrorShader;0;0;Standard;38;Workflow;1;0;Surface;0;0;  Refraction Model;0;0;  Blend;0;0;Two Sided;1;0;Fragment Normal Space,InvertActionOnDeselection;0;0;Transmission;0;0;  Transmission Shadow;0.5,False,-1;0;Translucency;0;0;  Translucency Strength;1,False,-1;0;  Normal Distortion;0.5,False,-1;0;  Scattering;2,False,-1;0;  Direct;0.9,False,-1;0;  Ambient;0.1,False,-1;0;  Shadow;0.5,False,-1;0;Cast Shadows;1;0;  Use Shadow Threshold;0;0;Receive Shadows;1;0;GPU Instancing;1;0;LOD CrossFade;1;0;Built-in Fog;1;0;_FinalColorxAlpha;0;0;Meta Pass;1;0;Override Baked GI;0;0;Extra Pre Pass;0;0;DOTS Instancing;0;0;Tessellation;0;0;  Phong;0;0;  Strength;0.5,False,-1;0;  Type;0;0;  Tess;16,False,-1;0;  Min;10,False,-1;0;  Max;25,False,-1;0;  Edge Length;16,False,-1;0;  Max Displacement;25,False,-1;0;Write Depth;0;0;  Early Z;0;0;Vertex Position,InvertActionOnDeselection;1;0;0;8;False;True;True;True;True;True;True;True;False;;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;508;1482.885,38.78796;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ExtraPrePass;0;0;ExtraPrePass;5;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;True;True;True;True;0;False;-1;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;True;1;False;-1;True;3;False;-1;True;True;0;False;-1;0;False;-1;True;0;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;517;1482.885,98.78796;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthNormals;0;6;DepthNormals;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;True;1;1;False;-1;0;False;-1;0;1;False;-1;0;False;-1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=DepthNormals;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;511;1482.885,38.78796;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;DepthOnly;0;3;DepthOnly;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;False;False;True;1;LightMode=DepthOnly;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;510;1482.885,38.78796;Float;False;False;-1;2;UnityEditor.ShaderGraph.PBRMasterGUI;0;1;New Amplify Shader;94348b07e5e8bab40bd6c8a1e3df54cd;True;ShadowCaster;0;2;ShadowCaster;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;True;0;False;-1;False;False;False;False;False;False;False;False;False;True;False;255;False;-1;255;False;-1;255;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;7;False;-1;1;False;-1;1;False;-1;1;False;-1;False;False;False;False;True;3;RenderPipeline=UniversalPipeline;RenderType=Opaque=RenderType;Queue=Geometry=Queue=0;True;0;True;17;d3d9;d3d11;glcore;gles;gles3;metal;vulkan;xbox360;xboxone;xboxseries;ps4;playstation;psp2;n3ds;wiiu;switch;nomrt;0;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;-1;False;False;False;True;False;False;False;False;0;False;-1;False;False;False;False;False;False;False;False;False;True;1;False;-1;True;3;False;-1;False;True;1;LightMode=ShadowCaster;False;False;0;Hidden/InternalErrorShader;0;0;Standard;0;False;0
WireConnection;269;0;265;0
WireConnection;269;1;268;0
WireConnection;404;0;402;0
WireConnection;278;0;269;0
WireConnection;278;1;271;0
WireConnection;268;5;267;0
WireConnection;271;5;270;0
WireConnection;28;5;30;0
WireConnection;399;1;398;0
WireConnection;399;4;393;0
WireConnection;399;44;397;0
WireConnection;294;0;31;0
WireConnection;294;1;281;0
WireConnection;274;5;273;0
WireConnection;505;0;503;0
WireConnection;505;1;119;0
WireConnection;505;2;499;0
WireConnection;400;1;398;0
WireConnection;400;4;393;0
WireConnection;400;44;396;0
WireConnection;162;0;161;0
WireConnection;280;0;279;0
WireConnection;280;1;272;0
WireConnection;414;0;411;0
WireConnection;459;0;457;0
WireConnection;459;1;456;0
WireConnection;52;0;223;0
WireConnection;52;2;372;0
WireConnection;52;3;144;0
WireConnection;155;0;13;0
WireConnection;279;0;278;0
WireConnection;279;1;274;0
WireConnection;402;0;399;0
WireConnection;402;1;399;0
WireConnection;402;2;399;0
WireConnection;402;3;399;0
WireConnection;31;0;23;0
WireConnection;31;1;28;0
WireConnection;217;0;506;0
WireConnection;272;5;275;0
WireConnection;412;0;409;4
WireConnection;16;0;162;0
WireConnection;16;1;155;0
WireConnection;401;0;400;0
WireConnection;401;1;400;0
WireConnection;401;2;400;0
WireConnection;401;3;400;0
WireConnection;394;0;403;0
WireConnection;394;1;404;0
WireConnection;457;1;448;0
WireConnection;457;4;449;0
WireConnection;457;44;453;0
WireConnection;281;0;280;0
WireConnection;281;1;277;0
WireConnection;23;5;29;0
WireConnection;521;0;144;0
WireConnection;57;0;16;0
WireConnection;57;1;52;0
WireConnection;403;0;401;0
WireConnection;504;0;502;0
WireConnection;504;1;501;4
WireConnection;265;5;266;0
WireConnection;461;0;459;0
WireConnection;461;1;455;0
WireConnection;461;2;522;0
WireConnection;222;0;294;0
WireConnection;19;0;20;0
WireConnection;19;1;16;0
WireConnection;417;0;415;0
WireConnection;503;0;500;0
WireConnection;503;1;499;4
WireConnection;415;0;409;4
WireConnection;415;1;413;0
WireConnection;301;5;303;0
WireConnection;506;0;504;0
WireConnection;506;1;505;0
WireConnection;506;2;501;0
WireConnection;386;0;395;0
WireConnection;386;1;384;0
WireConnection;386;2;385;0
WireConnection;386;3;405;0
WireConnection;410;0;409;0
WireConnection;219;0;162;4
WireConnection;392;1;389;0
WireConnection;439;1;429;0
WireConnection;439;4;433;0
WireConnection;439;44;438;0
WireConnection;236;0;47;0
WireConnection;144;0;139;0
WireConnection;448;0;443;0
WireConnection;448;1;441;0
WireConnection;448;2;444;0
WireConnection;277;5;276;0
WireConnection;395;0;394;0
WireConnection;455;0;439;0
WireConnection;455;1;442;0
WireConnection;456;4;450;0
WireConnection;456;44;453;0
WireConnection;302;0;281;0
WireConnection;302;1;301;0
WireConnection;47;0;48;0
WireConnection;47;1;7;0
WireConnection;372;0;55;0
WireConnection;515;0;461;0
WireConnection;445;0;439;0
WireConnection;445;1;439;0
WireConnection;445;2;439;0
WireConnection;389;0;387;0
WireConnection;389;1;388;0
WireConnection;119;0;19;0
WireConnection;119;1;57;0
WireConnection;416;0;409;0
WireConnection;416;1;414;0
WireConnection;418;0;416;0
WireConnection;407;0;389;0
WireConnection;390;0;391;2
WireConnection;390;2;392;0
WireConnection;390;3;389;0
WireConnection;390;4;389;0
WireConnection;406;0;386;0
WireConnection;520;0;372;0
WireConnection;509;0;516;0
WireConnection;509;1;237;0
WireConnection;509;2;228;0
WireConnection;509;3;365;0
WireConnection;509;4;379;0
WireConnection;509;6;220;0
ASEEND*/
//CHKSM=02981DB01D475CFD0F4BA96172FB530BDCDAEAEA