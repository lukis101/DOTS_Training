
// Volumetric light beam shader by Dj Lukis.LT
// For use with special cone mesh

Shader "DJL/BeamShader v4"
{
Properties 
{
	_BaseColor ("Base Color", Color) = (1,1,1,1)
	_SecondColor ("Modifier Color", Color) = (1,1,1,1)
	_Intensity ("Intensity", Range(0.0, 10.0)) = 1.0
	_XFade ("Color Crossfade", Range(0,1)) = 0
    
	[Header(Beam parameters)]
	_attenType ("Attenuation type", Range(0.0, 1.0)) = 1.0
	_Falloff ("Angle falloff", Range(0.0, 1.0)) = 0.5
	_Scattering ("Scattering(Volumetry)", Range(0.0, 10.0)) = 1.0
	_Blinding ("Blinding", Range(0.0, 1.0)) = 0.5
	
	[Toggle(_)] _Noise("Noise(smoke simulation)", Int) = 0
	_NoiseStr ("Noise Strength", Range(0,1)) = 0.7
	_NoiseScale ("Noise scale", Float) = 1

	[Header(Movement)]
	_Pan ("Pan", Range(0.0, 1.0)) = 0.5
	_Tilt ("Tilt", Range(0.0, 1.0)) = 0.5
	_Focus ("Focus", Range(0.0, 1.0)) = 0.5

	[Header(Effects)]
	_HueOffset ("Hue shift",  Range( 0.0, 1.0)) = 0.0
	_Desaturate ("Desaturate",  Range( 0.0, 1.0)) = 0.0
}
SubShader 
{
	Tags {
        "Queue" = "Transparent"
        "RenderType"="Overlay"
        "IgnoreProjector"="True"
        "DisableBatching"="True"
        "ForceNoShadowCasting "="True"
    }
	Cull Front
	ZWrite Off
    ZTest Always
	Blend One One
	//ColorMask RGBA

	Pass
	{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag
		//#pragma shader_feature _NOISE_ON
		#pragma multi_compile_instancing
		#pragma target 3.0

		#include "UnityCG.cginc"
		#include "ColorFuncs.cginc"
		#include "matrix.cginc"
        #define UMP UNITY_MATRIX_P

        UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);

		uniform float _attenType;
		uniform float _Scattering;
		uniform float _Falloff;
		uniform float _Blinding;

		UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float3, _BaseColor)
            UNITY_DEFINE_INSTANCED_PROP(half, _Intensity)
            UNITY_DEFINE_INSTANCED_PROP(float3, _SecondColor)
            UNITY_DEFINE_INSTANCED_PROP(half, _XFade)
            UNITY_DEFINE_INSTANCED_PROP(half, _Pan)
            UNITY_DEFINE_INSTANCED_PROP(half, _Tilt)
            UNITY_DEFINE_INSTANCED_PROP(half, _Focus)
        UNITY_INSTANCING_BUFFER_END(Props)

		uniform float _NoiseStr;
		uniform float _NoiseScale;

		struct v2f
		{
			float4 pos : SV_Position;
			float4 col : COLOR;
			//float3 uv : TEXCOORD1;
			float4 objpos : TEXCOORD1;
			nointerpolation float3 objcampos : TEXCOORD2;
            float4 originPos : TEXCOORD3;
			float4 grabPos : TEXCOORD4;
			//float4 wpos : TEXCOORD5;
			//float3 viewDir : TEXCOORD6;
			//float3 worldDirection : TEXCOORD7;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

        inline bool IsInMirror()
        {
            return UMP._31 != 0.f || UMP._32 != 0.f;
        }
        float getIntensity(in float3 pos)
        {
            float pdist = pow(saturate(pos.y)*2.0, 1);
            //pdist = saturate(length(pos))*2.0;

            //float atten = pow(1 - pdist, 1); // diminish with distance
            float atten = 1/(0.25 + 1.0*pow(pdist*1.0, 2)); // distance fade
            atten *= 1-pow(pos.y, 2); // limit to object space
            float falloff = saturate((1- length(pos.xz)/pos.y*2)/_Falloff); // angular fade
            //return falloff;
            return falloff*atten;
        }

        float3 getLine2ConeIntersection2(float3 fragPos, float3 CamPos, float3 camDir)
        {
            const static float angcos = 0.79999; //cos(halfangle)^2, tan(halfangle) = 0.5
            const static float3 axis = float3(0,1,0);
            float3 tocam = CamPos;
            float dotcamaxis = dot(camDir,axis);
            float a = pow(dotcamaxis, 2) - angcos;
            float b = 2*( dotcamaxis * dot(tocam,axis) - dot(camDir,tocam)*angcos );
            float c = pow(dot(tocam,axis),2) - dot(tocam,tocam)*angcos;

            float Discriminant = pow(b,2) - (4.f*a*c);
            //if (Discriminant >= 0)
            {
                float t1 = (((-b) - sqrt(Discriminant))/(2*a));
                // t2 = 
                //float t2 = (((-b) + sqrt(Discriminant))/(2*a));
                float3 pt1 = CamPos + t1*camDir;
                //float3 pt2 = CamPos + t2*camDir; // pt2 = fragPos

                if ((pt1.y > 1.0) || (pt1.y <0)) // outside of cone range, do plane intersection
                {
                    float denom = abs(dot(axis, camDir));
                    float3 p0l0 = axis - CamPos; 
                    t1 = dot(p0l0, axis) / denom;
                    pt1 = CamPos + camDir * t1;
                }

                float thickness = distance(pt1, fragPos);
                float camdist = distance(CamPos, fragPos);
                //if(camdist <= t1) clip(-1);
                
                if (camdist < thickness) // camera inside the cone
                {
                    //clip(-1);
                    //t1 = 0;
                    thickness = camdist;
                    pt1 = CamPos;
                }
                return pt1;
            }
            //return fragPos;
        }

        inline float4 UnityObjectToClipPos_(in float4x4 m_o2w, in float3 pos)
        {
            // More efficient than computing M*VP matrix product
            return mul(UNITY_MATRIX_VP, mul(m_o2w, float4(pos, 1.0)));
        }

        void rebuildMatrices(in float4x4 m, out float4x4 o2w, out float4x4 w2o,
            in float pan, in float tilt, in float focus)
        {
            o2w = IDENTITY_MATRIX;
            w2o = IDENTITY_MATRIX;

            float sx = length(float3(m[0][0], m[1][0], m[2][0]));
            float sy = length(float3(m[0][1], m[1][1], m[2][1]));
            float sz = length(float3(m[0][2], m[1][2], m[2][2]));
            // if determine is negative, we need to invert one scale
            float det = determinant(m);
            if (det < 0) {
                sx = -sx;
            }

            float3 position = float3(m[0][3], m[1][3], m[2][3]);

            // descale the rotation part
            float invSX = 1.0 / sx;
            float invSY = 1.0 / sy;
            float invSZ = 1.0 / sz;

            o2w[0][0] = m[0][0] * invSX;
            o2w[1][0] = m[1][0] * invSX;
            o2w[2][0] = m[2][0] * invSX;

            o2w[0][1] = m[0][1] * invSY;
            o2w[1][1] = m[1][1] * invSY;
            o2w[2][1] = m[2][1] * invSY;

            o2w[0][2] = m[0][2] * invSZ;
            o2w[1][2] = m[1][2] * invSZ;
            o2w[2][2] = m[2][2] * invSZ;

            // Object->world
            float4x4 scaling = scaleMatrix(float3(sx*focus, sy, sz*focus));
            float4x4 rotation = mul(o2w, euler_to_matrix(0, pan, tilt));

            o2w = mul(rotation, scaling);
            o2w = m_translate(o2w, position);

            // world->Object
            // reconstructing from inversed transforms is faster than full matrix inversion
            float4x4 invrotation = transpose(rotation);
            w2o = m_translate(w2o, -position);
            w2o = mul(invrotation, w2o);
            w2o = m_scale(w2o, float3(invSX/focus, invSY, invSZ/focus));
        }

		//--- Vertex shader ---//
		v2f vert(appdata_full v)
		{
			v2f o;
			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_TRANSFER_INSTANCE_ID(v, o);

            half pan   = (UNITY_ACCESS_INSTANCED_PROP(Props, _Pan) *2 -1) * 1.5708;
            half tilt  = (UNITY_ACCESS_INSTANCED_PROP(Props, _Tilt)*2 -1) * 1.5708;
            half focus = UNITY_ACCESS_INSTANCED_PROP(Props, _Focus);
            //float focusbrightness = pow(1.5 - focus, 2);
            focus = focus*0.8 + 0.2;

            float4x4 m_o2w, m_w2o;
            rebuildMatrices(unity_ObjectToWorld, m_o2w, m_w2o, pan, tilt, focus);

			o.objpos = v.vertex;
			o.pos = UnityObjectToClipPos_(m_o2w, o.objpos);
			o.col = UNITY_ACCESS_INSTANCED_PROP(Props, _Intensity) / pow(focus,1.5)
                * float4(LerpHSV(UNITY_ACCESS_INSTANCED_PROP(Props, _BaseColor).rgb, UNITY_ACCESS_INSTANCED_PROP(Props, _SecondColor).rgb, UNITY_ACCESS_INSTANCED_PROP(Props, _XFade)), 1);

            o.objcampos = mul(m_w2o, float4(_WorldSpaceCameraPos.xyz, 1.0));
            o.originPos = ComputeGrabScreenPos(UnityObjectToClipPos_(unity_ObjectToWorld, float4(0,0,0,1)));
            //o.wpos = mul(m_scaled, o.objpos);
			o.grabPos = ComputeGrabScreenPos(o.pos);
			//o.worldDirection = (o.wpos.xyz - _WorldSpaceCameraPos);

			return o;
		}

		//--- Fragment shader ---//
		fixed4 frag (v2f i) : SV_Target
		{
			UNITY_SETUP_INSTANCE_ID(i);
			float4 col = i.col;
			float finalatten = 0;

            float3 fpos = i.objpos.xyz;// / i.objpos.w;
            float3 objcampos = i.objcampos;
            float3 objcamdir = normalize(objcampos-fpos);

			float perspectiveDivide = 1.0f / i.grabPos.w;
			float3 direction = (fpos - objcampos) * perspectiveDivide;
            float2 screenPos = i.grabPos.xy * perspectiveDivide;
			float  depth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenPos));
			float3 depthobjpos = direction * depth + objcampos.xyz;
            //return float4(frac(depthobjpos), 1);
            float3 frontpoint = getLine2ConeIntersection2(fpos, objcampos, objcamdir);

			float origindepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, i.originPos));
            bool originoccluded = i.originPos.w > origindepth;
            bool backoccluded = false;
            //if (originoccluded) return float4(0.0,0,0.5,1);

            //float d_back  = distance(objcampos, fpos);
            float d_front = distance(objcampos, frontpoint);
            float d_depth = distance(objcampos, depthobjpos);
            //return(frac(float4(0,d_depth-d_front,0,1)));

            // check if back face is occluded
            if (depth < i.grabPos.w)
            //if (d_depth < d_back)
            {
                if (d_depth <= d_front) // front occluded too
                    discard;
                backoccluded = true;
                fpos = depthobjpos;
                //d_back = d_depth;
                //return float4(0.0,0,0.5,1);

                // Illuminate geometry inside volume
                fixed3 objNormal;
				if (IsInMirror()) // VRChat mirrors render with GL.invertCulling = true;
					objNormal = cross(ddx(depthobjpos), ddy(depthobjpos));
				else
					objNormal = cross(-ddx(depthobjpos), ddy(depthobjpos));
				objNormal = normalize(objNormal);
                //return float4(objNormal,1);

                finalatten = 0;//getIntensity(depthobjpos) * saturate(dot(objNormal, float3(0,-1,0)));
                //return float4(0.0,0,finalatten,1);
            }
            float thickness = distance(fpos, frontpoint);
            if ((thickness > 2) || (thickness <= 0) || isnan(thickness))
                discard;

            // Scattering (volumetry)
            float scatter = 0;
            const static uint steps = 8;
            float stepsize = thickness/steps;
            float3 stepvec = -(fpos-frontpoint) / steps;
            float previntens = 0;
            for (uint i=1; i<=steps; i++)
            {
                float3 samppoint = fpos + stepvec*i;
                float targetintens = getIntensity(samppoint);
                scatter += (previntens+targetintens)*0.5;
                previntens = targetintens;
            }
            //scatter = getIntensity(CamPos+ camDir*(t1-thickness/2));
            scatter = (scatter/steps)*thickness * _Scattering;
            //scatter*=(1 +saturate(dot(normalize(fpos), dot(normalize(objcampos),float3(0,1,0)))));
            //scatter = getIntensity(fpos);
            finalatten += scatter;

            //return thickness;
            //return getIntensity(fpos);
            //return scatter;

            //if(i.uv.y > 0.95) inters = getIntensity(fpos); //spot angle debug

            /*float blinding = 1-saturate(length(objcampos.xz)*2); // TODO use same falloff function?
            if (originoccluded) blinding=0;
            float lookat = saturate(dot(normalize(objcampos), float3(0,1,0))); // TODO attenuate/only if inside
            finalatten += blinding *lookat *_Blinding;*/

            //if (originoccluded) finalatten *= 1-lookat;

		    return float4(col.rgb * pow(finalatten, 2.2), 1);
			//return col;
		}
		ENDCG
	}
} 
//Fallback "Diffuse"
}