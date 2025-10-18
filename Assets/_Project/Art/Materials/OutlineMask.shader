Shader "Hidden/OutlineMask"
{
    Properties
    {
    }
    
    SubShader
    {
        Tags { "Queue" = "Transparent+110" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        
        Pass
        {
            Name "Mask"
            Cull Off
            ZTest Always
            ZWrite Off
            ColorMask 0
            
            Stencil
            {
                Ref 1
                Pass Replace
            }
        }
    }
}