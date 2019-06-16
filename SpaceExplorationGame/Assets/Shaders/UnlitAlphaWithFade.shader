Shader "Unlit/UnlitAlphaWithFade"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	Category
	{
		Lighting Off
		ZWrite Off
		
		Cull back
		Blend SrcAlpha OneMinusSrcAlpha

		Tags {Queue=Transparent}

		SubShader
		{
			Pass
			{
				SetTexture [_MainTex]
				{
					ConstantColor [_Color]
					Combine Texture * constant
				}
			}
		}
	}
}