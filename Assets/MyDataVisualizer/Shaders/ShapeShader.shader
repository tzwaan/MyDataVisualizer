Shader "Custom/ShapeShader" 
{
	Properties 
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Size ("Size", Range(0, 30)) = 0.5
		_MinSize("_MinSize",Float) = 0
		_MaxSize("_MaxSize",Float) = 0
		_MinX("_MinX",Range(0, 1)) = 0
		_MaxX("_MaxX",Range(0, 1)) = 1.0
		_MinY("_MinY",Range(0, 1)) = 0
		_MaxY("_MaxY",Range(0, 1)) = 1.0
		_MinZ("_MinZ",Range(0, 1)) = 0
		_MaxZ("_MaxZ",Range(0, 1)) = 1.0		
		_MinNormX("_MinNormX",Range(0, 1)) = 0.0
		_MaxNormX("_MaxNormX",Range(0, 1)) = 1.0
		_MinNormY("_MinNormY",Range(0, 1)) = 0.0
		_MaxNormY("_MaxNormY",Range(0, 1)) = 1.0
		_MinNormZ("_MinNormZ",Range(0, 1)) = 0.0
		_MaxNormZ("_MaxNormZ",Range(0, 1)) = 1.0
		_MySrcMode("_SrcMode", Float) = 5
		_MyDstMode("_DstMode", Float) = 10

		_Tween("_Tween", Range(0, 1)) = 1
		_TweenSize("_TweenSize", Range(0, 1)) = 1
	}

	SubShader 
	{
		Pass
		{	
			AlphaTest Greater 0
			Blend[_MySrcMode][_MyDstMode]
			ColorMaterial AmbientAndDiffuse
            Cull Off
			Lighting Off
			LOD 400
			ZWrite On
			ZTest [unity_GUIZTestMode]
			Tags
			{
				"LightMode" = "ForwardBase"
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
			}
			
			CGPROGRAM
				#pragma target 5.0
				#pragma vertex VS_Main
				#pragma fragment FS_Main
				#pragma geometry GS_Main
				#pragma multi_compile_instancing
				#include "UnityCG.cginc" 
				#include "UnityLightingCommon.cginc" // for _LightColor0
                #define sqrt2 0.707106f
                #define sqrt3 0.577350f
                #define norm1 0.325058
                #define norm2 0.888074

				// **************************************************************
				// Data structures												*
				// **************************************************************
				
		        struct VS_INPUT {
          		    float4 position : POSITION;
            		float4 color: COLOR;
					float3 normal: NORMAL;
					float4 uv_MainTex : TEXCOORD0; // index, vertex size, filtered, prev size
					
                    UNITY_VERTEX_INPUT_INSTANCE_ID
        		};

				struct v2g
				{
					float4 vertex : SV_POSITION;
					float4 color : COLOR;
					float3 normal : NORMAL;
					float  isBrushed : FLOAT;
					
					UNITY_VERTEX_INPUT_INSTANCE_ID 
					UNITY_VERTEX_OUTPUT_STEREO
				};

				struct g2f
				{
					float4 vertex : SV_POSITION;
					nointerpolation float4 color : COLOR;
					float2 tex0	: TEXCOORD0;
					float  isBrushed : FLOAT;
					
                    UNITY_VERTEX_OUTPUT_STEREO
				};

				struct f_output
				{
					float4 color : COLOR;
					float depth : SV_Depth;
				};
				
				// **************************************************************
				// Variables													*
				// **************************************************************

				UNITY_INSTANCING_BUFFER_START(Props)
					UNITY_DEFINE_INSTANCED_PROP(float, _Size)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MinSize)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MaxSize)
				
                    UNITY_DEFINE_INSTANCED_PROP(float, _MinX)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MaxX)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MinY)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MaxY)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MinZ)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MaxZ)
				
                    UNITY_DEFINE_INSTANCED_PROP(float, _MinNormX)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MaxNormX)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MinNormY)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MaxNormY)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MinNormZ)
                    UNITY_DEFINE_INSTANCED_PROP(float, _MaxNormZ)
					
                    UNITY_DEFINE_INSTANCED_PROP(float, _ShowBrush)
                    UNITY_DEFINE_INSTANCED_PROP(float4, _BrushColor)
					
                    UNITY_DEFINE_INSTANCED_PROP(float, _Tween)
                    UNITY_DEFINE_INSTANCED_PROP(float, _TweenSize)
				UNITY_INSTANCING_BUFFER_END(Props)
				
				float _DataWidth;
				float _DataHeight;
				float4x4 _VP;
				Texture2D _SpriteTex;
				SamplerState sampler_SpriteTex;
				sampler2D _BrushedTexture;

				//*********************************
				// Helper functions
				//*********************************

				float normaliseValue(float value, float i0, float i1, float j0, float j1)
				{
					float L = (j0 - j1) / (i0 - i1);
					return (j0 - (L * i0) + (L * value));
				}

                void addVertex(v2g p[1], g2f o, inout TriangleStream<g2f> triStream, float4 vertex, float2 tex0) {
                    o.vertex = UnityObjectToClipPos(vertex);
                    o.tex0 = tex0;
                    UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(p[0], 0);
                    triStream.Append(o);
                }

                float4 color_for_normal(float3 normal, float4 color) {
					half3 worldNormal = UnityObjectToWorldNormal(normal);
                    half nl = max(0, dot(worldNormal, _WorldSpaceLightPos0.xyz));
                    float4 result = float4(_LightColor0.rgb * nl, color.a);
                    result.rgb += ShadeSH9(half4(worldNormal, 1));
                    result.rgb *= color.rgb;
                    return result;
                }

				// **************************************************************
				// Shader Programs												*
				// **************************************************************
				
				// Vertex Shader ------------------------------------------------
				v2g VS_Main(VS_INPUT v)
				{
					v2g o;
					
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_OUTPUT(v2g, o);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);

					// Accessed instanced variables
					float Tween = UNITY_ACCESS_INSTANCED_PROP(Props, _Tween);
					float TweenSize = UNITY_ACCESS_INSTANCED_PROP(Props, _TweenSize);
                    float MinNormX = UNITY_ACCESS_INSTANCED_PROP(Props, _MinNormX);
                    float MaxNormX = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxNormX);
                    float MinNormY = UNITY_ACCESS_INSTANCED_PROP(Props, _MinNormY);
                    float MaxNormY = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxNormY);
                    float MinNormZ = UNITY_ACCESS_INSTANCED_PROP(Props, _MinNormZ);
                    float MaxNormZ = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxNormZ);
					float MinX = UNITY_ACCESS_INSTANCED_PROP(Props, _MinX);
                    float MaxX = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxX);
                    float MinY = UNITY_ACCESS_INSTANCED_PROP(Props, _MinY);
                    float MaxY = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxY);
                    float MinZ = UNITY_ACCESS_INSTANCED_PROP(Props, _MinZ);
                    float MaxZ = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxZ);

					float idx = v.uv_MainTex.x;
					float isFiltered = v.uv_MainTex.z;

                    // Check if vertex is brushed by looking up the texture
					float2 indexUV = float2((idx % _DataWidth) / _DataWidth, ((idx / _DataWidth) / _DataHeight));
					float4 brushValue = tex2Dlod(_BrushedTexture, float4(indexUV, 0.0, 0.0));
					o.isBrushed = brushValue.r;
				
                    // Lerp position and size values for animations
					float3 pos = lerp(v.normal, v.position, Tween);
					float size = lerp(v.uv_MainTex.w, v.uv_MainTex.y, TweenSize);

                    // Normalise values for min and max slider scaling
					float4 normalisedPosition = float4(
						normaliseValue(pos.x, MinNormX, MaxNormX, 0, 1),
						normaliseValue(pos.y, MinNormY, MaxNormY, 0, 1),
						normaliseValue(pos.z, MinNormZ, MaxNormZ, 0, 1),
						1.0);

					o.vertex = normalisedPosition;
					o.normal = float3(idx, size, isFiltered);
					o.color =  v.color;

                    // Filtering min and max ranges
					float epsilon = -0.00001; 
					if(normalisedPosition.x < (MinX + epsilon) ||
					   normalisedPosition.x > (MaxX - epsilon) || 
					   normalisedPosition.y < (MinY + epsilon) || 
					   normalisedPosition.y > (MaxY - epsilon) || 
					   normalisedPosition.z < (MinZ + epsilon) || 
					   normalisedPosition.z > (MaxZ - epsilon) ||
					   isFiltered)
					{
						o.color.w = 0;
					}

					return o;
				}
				
				// Geometry Shader -----------------------------------------------------
				[maxvertexcount(60)]
				void GS_Main(point v2g p[1], inout TriangleStream<g2f> triStream)
				{
					g2f o;
					
					UNITY_INITIALIZE_OUTPUT(g2f, o);
					UNITY_SETUP_INSTANCE_ID(p[0]);
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(p[0]);

					// Access instanced variables
                    float Size = UNITY_ACCESS_INSTANCED_PROP(Props, _Size);
                    float MinSize = UNITY_ACCESS_INSTANCED_PROP(Props, _MinSize);
                    float MaxSize = UNITY_ACCESS_INSTANCED_PROP(Props, _MaxSize);
					
					float sizeFactor = normaliseValue(p[0].normal.y, 0.0, 1.0, MinSize, MaxSize);
                    float shapeFactor = normaliseValue(p[0].normal.x, 0.0, 1.0, 0.0, 1.0);
					float halfS = 0.025f * (Size + (sizeFactor));
					float isBrushed = p[0].isBrushed;

					o.isBrushed = isBrushed;
					
					// Emit cube
					float3 position = p[0].vertex;
					float4 color = p[0].color;
					float xsize = halfS;// / (unity_ObjectToWorld[0].x / unity_ObjectToWorld[2].z);
					float ysize = halfS;// / (unity_ObjectToWorld[1].y / unity_ObjectToWorld[2].z);
					float zsize = halfS;// / (unity_ObjectToWorld[2].z / unity_ObjectToWorld[1].y);
                    float3 xyzsize = float3(xsize, ysize, zsize);

                    static float3 sphere_points[] = {
                        // Front face
                        float3(-sqrt3, -sqrt3, -sqrt3),
                        float3(  0.0f, -sqrt2, -sqrt2),
                        float3( sqrt3, -sqrt3, -sqrt3),
                        float3(-sqrt2,   0.0f, -sqrt2),
                        float3(  0.0f,   0.0f,  -1.0f),
                        float3( sqrt2,   0.0f, -sqrt2),
                        float3(-sqrt3,  sqrt3, -sqrt3),
                        float3(  0.0f,  sqrt2, -sqrt2),
                        float3( sqrt3,  sqrt3, -sqrt3),

                        // Middle spine
                        float3(-sqrt2, -sqrt2,  0.0f),
                        float3( 0.0f, -1.0f,  0.0f),
                        float3( sqrt2, -sqrt2,  0.0f),

                        float3(-1.0f,  0.0f,  0.0f),
                        float3( 1.0f,  0.0f,  0.0f),

                        float3(-sqrt2,  sqrt2,  0.0f),
                        float3( 0.0f,  1.0f,  0.0f),
                        float3( sqrt2,  sqrt2,  0.0f),

                        // Back face
                        float3(-sqrt3, -sqrt3,  sqrt3),
                        float3( 0.0f, -sqrt2,  sqrt2),
                        float3( sqrt3, -sqrt3,  sqrt3),
                        float3(-sqrt2,  0.0f,  sqrt2),
                        float3( 0.0f,  0.0f,  1.0f),
                        float3( sqrt2,  0.0f,  sqrt2),
                        float3(-sqrt3,  sqrt3,  sqrt3),
                        float3( 0.0f,  sqrt2,  sqrt2),
                        float3( sqrt3,  sqrt3,  sqrt3)
                    };

                    static float3 cube_points[] = {
                        // Front face
                        float3(-1.0f, -1.0f, -1.0f),
                        float3( 0.0f, -1.0f, -1.0f),
                        float3( 1.0f, -1.0f, -1.0f),
                        float3(-1.0f,  0.0f, -1.0f),
                        float3( 0.0f,  0.0f, -1.0f),
                        float3( 1.0f,  0.0f, -1.0f),
                        float3(-1.0f,  1.0f, -1.0f),
                        float3( 0.0f,  1.0f, -1.0f),
                        float3( 1.0f,  1.0f, -1.0f),

                        // Middle spine
                        float3(-1.0f, -1.0f,  0.0f),
                        float3( 0.0f, -1.0f,  0.0f),
                        float3( 1.0f, -1.0f,  0.0f),

                        float3(-1.0f,  0.0f,  0.0f),
                        float3( 1.0f,  0.0f,  0.0f),

                        float3(-1.0f,  1.0f,  0.0f),
                        float3( 0.0f,  1.0f,  0.0f),
                        float3( 1.0f,  1.0f,  0.0f),

                        // Back face
                        float3(-1.0f, -1.0f,  1.0f),
                        float3( 0.0f, -1.0f,  1.0f),
                        float3( 1.0f, -1.0f,  1.0f),
                        float3(-1.0f,  0.0f,  1.0f),
                        float3( 0.0f,  0.0f,  1.0f),
                        float3( 1.0f,  0.0f,  1.0f),
                        float3(-1.0f,  1.0f,  1.0f),
                        float3( 0.0f,  1.0f,  1.0f),
                        float3( 1.0f,  1.0f,  1.0f)
                    };
                    static float3 cube_normals[] = {
                        float3( 0,  0, -1),
                        float3( 0,  0, -1),
                        float3( 1,  0,  0),
                        float3( 1,  0,  0),
                        float3( 0,  0,  1),
                        float3( 0,  0,  1),
                        float3(-1,  0,  0),
                        float3(-1,  0,  0),

                        float3( 0,  0, -1),
                        float3( 0,  0, -1),
                        float3( 1,  0,  0),
                        float3( 1,  0,  0),
                        float3( 0,  0,  1),
                        float3( 0,  0,  1),
                        float3(-1,  0,  0),
                        float3(-1,  0,  0),

                        float3( 0, -1,  0),
                        float3( 0, -1,  0),
                        float3( 0, -1,  0),
                        float3( 0, -1,  0),

                        float3( 0,  1,  0),
                        float3( 0,  1,  0),
                        float3( 0,  1,  0),
                        float3( 0,  1,  0)
                    };
                    static float3 sphere_normals[] = {                            
                        float3(-norm1, -norm1, -norm2),
                        float3( norm1, -norm1, -norm2),
                        float3( norm2, -norm1, -norm1),
                        float3( norm2, -norm1,  norm1),
                        float3( norm1, -norm1,  norm2),
                        float3(-norm1, -norm1,  norm2),
                        float3(-norm2, -norm1,  norm1),
                        float3(-norm2, -norm1, -norm1),

                        float3(-norm1,  norm1, -norm2),
                        float3( norm1,  norm1, -norm2),
                        float3( norm2,  norm1, -norm1),
                        float3( norm2,  norm1,  norm1),
                        float3( norm1,  norm1,  norm2),
                        float3(-norm1,  norm1,  norm2),
                        float3(-norm2,  norm1,  norm1),
                        float3(-norm2,  norm1, -norm1),

                        float3(-norm1, -norm2,  norm1),
                        float3( norm1, -norm2,  norm1),
                        float3(-norm1, -norm2, -norm1),
                        float3( norm1, -norm2, -norm1),
                        float3(-norm1,  norm2,  norm1),
                        float3( norm1,  norm2,  norm1),
                        float3(-norm1,  norm2, -norm1),
                        float3( norm1,  norm2, -norm1),
                    };
                    float4 points[26];
                    for (int i=0; i<26; i++) {
                        points[i] = float4(position + lerp(cube_points[i], sphere_points[i], shapeFactor) * xyzsize, 1.0f);
                    }
                    float3 normals[24];
                    for (int i=0; i<24; i++) {
                        normals[i] = lerp(cube_normals[i], sphere_normals[i], shapeFactor);
                    }

                    o.color = color_for_normal(normals[0], color);
                    addVertex(p, o, triStream, points[ 0], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[ 3], float2(0.5f, 0.5f));
                    o.color = color_for_normal(normals[1], color);
                    addVertex(p, o, triStream, points[ 1], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[ 4], float2(0.5f, 0.5f));

                    o.color = color_for_normal(normals[2], color);
                    addVertex(p, o, triStream, points[ 2], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[ 5], float2(0.5f, 0.5f));
                    o.color = color_for_normal(normals[3], color);
                    addVertex(p, o, triStream, points[11], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[13], float2(0.5f, 0.5f));

                    o.color = color_for_normal(normals[4], color);
                    addVertex(p, o, triStream, points[19], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[22], float2(0.5f, 0.5f));
                    o.color = color_for_normal(normals[5], color);
                    addVertex(p, o, triStream, points[18], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[21], float2(0.5f, 0.5f));

                    o.color = color_for_normal(normals[6], color);
                    addVertex(p, o, triStream, points[17], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[20], float2(0.5f, 0.5f));
                    o.color = color_for_normal(normals[7], color);
                    addVertex(p, o, triStream, points[ 9], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[12], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[ 0], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[ 3], float2(0.5f, 0.5f));

					triStream.RestartStrip();
                    o.color = color_for_normal(normals[8], color);
                    addVertex(p, o, triStream, points[ 3], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[ 6], float2(0.5f, 0.5f));
                    o.color = color_for_normal(normals[9], color);
                    addVertex(p, o, triStream, points[ 4], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[ 7], float2(0.5f, 0.5f));

                    o.color = color_for_normal(normals[10], color);
                    addVertex(p, o, triStream, points[ 5], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[ 8], float2(0.5f, 0.5f));
                    o.color = color_for_normal(normals[11], color);
                    addVertex(p, o, triStream, points[13], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[16], float2(0.5f, 0.5f));

                    o.color = color_for_normal(normals[12], color);
                    addVertex(p, o, triStream, points[22], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[25], float2(0.5f, 0.5f));
                    o.color = color_for_normal(normals[13], color);
                    addVertex(p, o, triStream, points[21], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[24], float2(0.5f, 0.5f));

                    o.color = color_for_normal(normals[14], color);
                    addVertex(p, o, triStream, points[20], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[23], float2(0.5f, 0.5f));
                    o.color = color_for_normal(normals[15], color);
                    addVertex(p, o, triStream, points[12], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[14], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[ 3], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[ 6], float2(0.5f, 0.5f));

					triStream.RestartStrip();

                    o.color = color_for_normal(normals[16], color);
                    addVertex(p, o, triStream, points[17], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[ 9], float2(0.5f, 0.5f));
                    o.color = color_for_normal(normals[17], color);
                    addVertex(p, o, triStream, points[18], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[10], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[19], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[11], float2(0.5f, 0.5f));

					triStream.RestartStrip();

                    o.color = color_for_normal(normals[18], color);
                    addVertex(p, o, triStream, points[ 9], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[ 0], float2(0.5f, 0.5f));
                    o.color = color_for_normal(normals[19], color);
                    addVertex(p, o, triStream, points[10], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[ 1], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[11], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[ 2], float2(0.5f, 0.5f));

                    o.color = color_for_normal(normals[20], color);
                    addVertex(p, o, triStream, points[23], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[14], float2(0.5f, 0.5f));
                    o.color = color_for_normal(normals[21], color);
                    addVertex(p, o, triStream, points[24], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[15], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[25], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[16], float2(0.5f, 0.5f));

					triStream.RestartStrip();

                    o.color = color_for_normal(normals[22], color);
                    addVertex(p, o, triStream, points[14], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[ 6], float2(0.5f, 0.5f));
                    o.color = color_for_normal(normals[23], color);
                    addVertex(p, o, triStream, points[15], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[ 7], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[16], float2(0.5f, 0.5f));
                    addVertex(p, o, triStream, points[ 8], float2(0.5f, 0.5f));

					triStream.RestartStrip();
				}
				
				// Fragment Shader -----------------------------------------------
				f_output FS_Main(g2f input) : COLOR
				{
					f_output o;
					
					UNITY_INITIALIZE_OUTPUT(f_output, o);
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
					
					// Access instanced variables
					float4 BrushColor = UNITY_ACCESS_INSTANCED_PROP(Props, _BrushColor);
					float ShowBrush = UNITY_ACCESS_INSTANCED_PROP(Props, _ShowBrush);
					
					if (input.color.w == 0)
					{
						discard;
						o.color = float4(0.0, 0.0, 0.0, 0.0);
						o.depth = 0;
						return o;
					}
					else
					{
						float dx = input.tex0.x;
						float dy = input.tex0.y;

						if (dx > 0.99 || dx < 0.01 || dy < 0.01  || dy > 0.99 )
							o.color = float4(0.0, 0.0, 0.0, input.color.w);
						else if (input.isBrushed > 0.0 && ShowBrush > 0.0)
							o.color = BrushColor;
						else
							o.color = input.color;
						
						o.depth = input.vertex.z;
						return o;
					}
				}
			ENDCG
		}
	}

	 
}
