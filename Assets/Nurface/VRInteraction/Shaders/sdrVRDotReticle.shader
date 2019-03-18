// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Copyright 2015 Google Inc. All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

Shader "Custom/VRDotReticle"
{
	Properties
	{
		_Color("Color", Color) = (1, 1, 1, 1)
		_Diameter("Diameter", Range(0.00872665, 10.0)) = 2.0
		_DistanceInMeters("DistanceInMeters", Range(0.0, 100.0)) = 2.0
	}

	SubShader
	{
		Tags{ "Queue" = "Overlay" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			AlphaTest Off
			Cull Back
			Lighting Off
			ZWrite Off
			ZTest Always

			Fog{ Mode Off }
			CGPROGRAM

#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

			uniform float4 _Color;
			uniform float _Diameter;
			uniform float _DistanceInMeters;

			struct vertexInput 
			{
				float4 vertex : POSITION;
			};

			struct fragmentInput 
			{
				float4 position : SV_POSITION;
			};

			fragmentInput vert(vertexInput i) 
			{
				float4 vert_out = float4(i.vertex.xy * _Diameter, _DistanceInMeters, 1.0);

				fragmentInput o;
				//o.position = mul(UNITY_MATRIX_MVP, vert_out);

				o.position = UnityObjectToClipPos(vert_out);
				return o;
			}

			fixed4 frag(fragmentInput i) : SV_Target
			{
				return _Color;
			}

			ENDCG
		}
	}
}