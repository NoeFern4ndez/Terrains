Shader "Terrains/Terrain"
{
    Properties
    {
        // General
        _Metallic ("Metallic", Range(0,1)) = 0.5
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        // Ground
        _GroundColor ("Ground Color", Color) = (1,1,1,1)
        _GroundTex ("Ground Texture", 2D) = "white" {}
        // Rock
        _RockColor ("Rock Color", Color) = (1,1,1,1)
        _RockTex ("Rock Texture", 2D) = "white" {}
        // Grass
        _GrassColor ("Grass Color", Color) = (1,1,1,1)
        _GrassTex ("Grass Texture", 2D) = "white" {}
        // Snow
        _SnowColor ("Snow Color", Color) = (1,1,1,1)
        _SnowTex ("Snow Texture", 2D) = "white" {}
        // Thresholds
        _RockThreshold ("Rock Slope", Range(0,1)) = 0.5
        _SnowSlope ("Snow Slope", Range(0,1)) = 0.5
        _SnowThreshold ("Snow Height", Float) = 500
        _GrassHeight ("Grass Height", Float) = 300
        // Clamping factors
        _RockClamp ("Rock Clamp", Range(0,1)) = 0.5
        _GrassClamp ("Grass Clamp", Range(0,1)) = 0.5
        _SnowClamp ("Snow Clamp", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "TerrainCompatible"="true" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows

        #pragma target 3.0

        sampler2D _GroundTex;
        sampler2D _RockTex;
        sampler2D _GrassTex;
        sampler2D _SnowTex;

        fixed4 _GroundColor;
        fixed4 _RockColor;
        fixed4 _GrassColor;
        fixed4 _SnowColor;

        half _Metallic;
        half _Glossiness;

        half _RockThreshold;
        half _SnowThreshold;
        half _SnowSlope;
        half _GrassHeight;

        half _RockClamp;
        half _GrassClamp;
        half _SnowClamp;

        struct Input
        {
            float2 uv_GroundTex; 
            float2 uv_RockTex; 
            float2 uv_GrassTex; 
            float2 uv_SnowTex;
            float3 worldPos;
            float3 worldNormal;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float3 normal = normalize(IN.worldNormal);

            float slope = 1 - abs(dot(normal, float3(0, 1, 0)));

            float2 groundUV = IN.uv_GroundTex; 
            float2 rockUV = IN.uv_RockTex; 
            float2 grassUV = IN.uv_GrassTex;
            float2 snowUV = IN.uv_SnowTex;

            fixed4 groundColor = tex2D(_GroundTex, groundUV) * _GroundColor;
            fixed4 rockColor = tex2D(_RockTex, rockUV) * _RockColor;
            fixed4 grassColor = tex2D(_GrassTex, grassUV) * _GrassColor;
            fixed4 snowColor = tex2D(_SnowTex, snowUV) * _SnowColor;

            float grassFactor = smoothstep(_GrassHeight + _GrassClamp, _GrassHeight - _GrassClamp, IN.worldPos.y) * smoothstep(0, _GrassClamp, (1 - slope));
            float rockFactor = smoothstep(_RockThreshold - _RockClamp, _RockThreshold + _RockClamp, slope);
            float snowFactor = smoothstep(_SnowThreshold - _SnowClamp, _SnowThreshold + _SnowClamp, IN.worldPos.y) * (1 - smoothstep(0, _SnowSlope, slope));
            snowFactor = smoothstep(0, _SnowClamp, snowFactor);

            fixed4 terrainColor = groundColor;
            terrainColor = lerp(terrainColor, grassColor, grassFactor);
            terrainColor = lerp(terrainColor, rockColor, rockFactor);
            terrainColor = lerp(terrainColor, snowColor, snowFactor);

            o.Albedo = terrainColor.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
