Shader "Custom/DepthOccluder"
{
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry-1"
        }

        Pass
        {
            ZWrite On
            ColorMask 0
        }
    }
}
