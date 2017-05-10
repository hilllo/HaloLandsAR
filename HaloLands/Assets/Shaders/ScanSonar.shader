Shader "Custom/Sonar"
{
	Properties
	{
		_SonarBaseColor("Base Color",  Color) = (0.1, 0.1, 0.1, 0)
		_MainTex("Base Texture (RGB)", 2D) = "white" {}
        _SonarWaveColor  ("Wave Color",  Color)  = (1.0, 0.1, 0.1, 0)
        _SonarWaveParams ("Wave Params", Vector) = (1, 20, 20, 10)
        _SonarWaveVector ("Wave Vector", Vector) = (0, 0, 1, 0)
        _SonarAddColor   ("Add Color",   Color)  = (0, 0, 0, 0)
		_Lerp("Lerp", Range(0,100)) = 0.0

    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderType" = "TransparentCutout" "IgnoreProjector" = "True" }

		ZWrite On
			Blend SrcAlpha OneMinusSrcAlpha

			AlphaToMask On

        CGPROGRAM


        #pragma surface surf Lambert

        struct Input
        {
            float3 worldPos;
			float2 uv_MainTex;
        };

        float3 _SonarBaseColor;
        float3 _SonarWaveColor;
        float4 _SonarWaveParams; // Amp, Exp, Interval, Speed
        float3 _SonarWaveVector;
        float3 _SonarAddColor;
		sampler2D _MainTex;
		half _Lerp;
        void surf(Input IN, inout SurfaceOutput o)
        {
#ifdef SONAR_DIRECTIONAL
            float w = dot(IN.worldPos, _SonarWaveVector);
#else
            float w = length(IN.worldPos - _SonarWaveVector);
#endif

			float dist = distance(IN.worldPos,(0,0,0));


            // Moving wave.
            w -= _Time.y * _SonarWaveParams.w;

            // Get modulo (w % params.z / params.z)
            w /= _SonarWaveParams.z;
            w = w - floor(w);

            // Make the gradient steeper.
            float p = _SonarWaveParams.y;
            w = (pow(w, p) + pow(1 - w, p * 4)) * 0.5;

            // Amplify.
            w *= _SonarWaveParams.x;

            // Apply to the surface.
			//fixed4 c = tex2D(_MainTex,IN.uv_MainTex) * _SonarBaseColor;

			fixed4 from = (0, 0, 0, 1);
			fixed4 to = (1,1,1,0);

            //o.Albedo = tex2D(_MainTex, IN.uv_MainTex) * _SonarBaseColor;

			o.Albedo = lerp(tex2D(_MainTex, IN.uv_MainTex) * _SonarBaseColor, to,(dist-_Lerp) );
            o.Emission = _SonarWaveColor * w + _SonarAddColor;
        }

        ENDCG
    } 
    Fallback "Diffuse"
}
