// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/Outline Only" 
{
	Properties
	{
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_Outline("Outline width", Range(0.0, 0.3)) = .005
		_zDepthOffset("Z Depth Outline Offset", float) = 0
	}

CGINCLUDE
#include "UnityCG.cginc"

	struct appdata {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	struct v2f {
		float4 pos : POSITION;
		float4 color : COLOR;
	};

	uniform float _Outline;
	uniform float4 _OutlineColor;
	uniform float _zDepthOffset;

	v2f vert(appdata v)
	{
		v2f o;

		float3 normal = v.normal;

		float4 normalOffset = float4(normal*_Outline, 0);

		float4 viewT = float4(normalize(ObjSpaceViewDir(v.vertex + normalOffset)), 0);

		o.pos = UnityObjectToClipPos(v.vertex + normalOffset - viewT *_zDepthOffset);

		o.color = _OutlineColor;
		return o;
	}
ENDCG

	SubShader
	{
		Tags{ "Queue" = "Transparent" }

		Pass{
		Name "BASE"
		Cull Back
		Blend Zero One

		SetTexture[_OutlineColor]
		{
			ConstantColor(0,0,0,0)
			Combine constant
		}
	}

		// note that a vertex shader is specified here but its using the one above
	Pass
	{
		Name "OUTLINE"
		Tags{ "LightMode" = "Always" }
		Cull Front

		// you can choose what kind of blending mode you want for the outline
		Blend SrcAlpha OneMinusSrcAlpha // Normal
		//Blend One One // Additive
		//Blend One OneMinusDstColor // Soft Additive
		//Blend DstColor Zero // Multiplicative
		//Blend DstColor SrcColor // 2x Multiplicative

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

		half4 frag(v2f i) :COLOR
		{
			return i.color;
		}
		ENDCG
	}


	}

		Fallback "Diffuse"
}