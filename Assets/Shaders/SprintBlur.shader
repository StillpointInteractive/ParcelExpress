
Shader "Custom/SprintBlur"
{
    Properties
    {
        _BlurStrength ("Blur Strength", Range(0, 1)) = 0
        _BlurRadius ("Blur Radius", Range(0, 0.03)) = 0.008
        _EdgeStart ("Edge Start", Range(0, 1)) = 0.35
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        ZTest Always
        ZWrite Off
        Cull Off

        Pass
        {
            Name "SprintBlur"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _BlurStrength;
            float _BlurRadius;
            float _EdgeStart;

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord.xy;

                half4 original = SAMPLE_TEXTURE2D_X(
                    _BlitTexture, sampler_LinearClamp, uv);

                // 0 in centre, 1 at left/right edges.
                float distanceFromCentre = abs(uv.x - 0.5) * 2.0;

                float edgeMask = smoothstep(
                    _EdgeStart, 1.0, distanceFromCentre);

                float intensity = edgeMask * _BlurStrength;

                if (intensity <= 0.001)
                    return original;

                // Blur outwards from the centre of the screen.
                float2 direction = uv - float2(0.5, 0.5);
                float2 offset = direction * _BlurRadius;

                half4 blurred = original;

                blurred += SAMPLE_TEXTURE2D_X(
                    _BlitTexture, sampler_LinearClamp, uv + offset);
                blurred += SAMPLE_TEXTURE2D_X(
                    _BlitTexture, sampler_LinearClamp, uv - offset);
                blurred += SAMPLE_TEXTURE2D_X(
                    _BlitTexture, sampler_LinearClamp, uv + offset * 2.0);
                blurred += SAMPLE_TEXTURE2D_X(
                    _BlitTexture, sampler_LinearClamp, uv - offset * 2.0);
                blurred += SAMPLE_TEXTURE2D_X(
                    _BlitTexture, sampler_LinearClamp, uv + offset * 3.0);
                blurred += SAMPLE_TEXTURE2D_X(
                    _BlitTexture, sampler_LinearClamp, uv - offset * 3.0);

                blurred /= 7.0;

                return lerp(original, blurred, intensity);
            }

            ENDHLSL
        }
    }
}
