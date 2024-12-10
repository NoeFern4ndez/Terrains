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
        // Flowers
        _FlowerColor ("Flower Color", Color) = (1,1,1,1)
        _FlowerTex ("Flower Texture", 2D) = "white" {}
        // Snow
        _SnowColor ("Snow Color", Color) = (1,1,1,1)
        _SnowTex ("Snow Texture", 2D) = "white" {}
        // Water
        _WaterColor ("Water Color", Color) = (1,1,1,1)  
        _WaterTex ("Water Texture", 2D) = "white" {}
        // Rock properties
        _RockThreshold ("Rock Slope", Range(0,1)) = 0.5
        _RockClamp ("Rock Clamp", Range(0,1)) = 0.5
        // Snow properties
        _SnowScale ("Snow Displacement Scale", Range(0,1)) = 0.5
        _SnowSlope ("Snow Slope", Range(0,1)) = 0.5
        _SnowClamp ("Snow Clamp", Range(0,1)) = 0.5
        _SnowThreshold ("Snow Height", Float) = 500
        _SnowHeightVariation ("Snow Height Variation", Float) = 100
        // Grass properties
        _GrassClamp ("Grass Clamp", Range(0,1)) = 0.5
        _GrassHeight ("Grass Height", Float) = 300
        _GrassHeightVariation ("Grass Height Variation", Float) = 100
        // Un texel de hierba tiene flores si los vecinos en texeles a distancia x,y son de hierba
        // y el texel actual es de hierba
        _FlowerScale ("Flower Displacement Scale", Range(0,1)) = 0.5
        _FlowerSlope ("Flower Slope", Range(0,1)) = 0.5
        _FlowerDistanceX ("Flower Spread in X", Float) = 1
        _FlowerDistanceY ("Flower Spread in Y", Float) = 1
        _FlowerClamp ("Flower Clamp", Range(0,1)) = 0.5
        // Water properties
        _WaterScale ("Water Displacement Scale", Range(0,1)) = 0.5
        _WaterSlope ("Water Slope", Range(0,1)) = 0.5
        _WaterClamp ("Water Clamp", Range(0,1)) = 0.5
        _WaterHeight ("Water Height", Float) = 0
        _WaterHeightVariation ("Water Height Variation", Float) = 100
        _WaterVelocity ("Water Velocity", Float) = 0.1
        _WaterAngle ("Water Angle", Range(0,360)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "TerrainCompatible"="true" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma vertex vert

        #pragma target 4.0

        sampler2D _GroundTex;
        sampler2D _RockTex;
        sampler2D _GrassTex;
        sampler2D _SnowTex;
        sampler2D _WaterTex;
        sampler2D _FlowerTex;

        fixed4 _GroundColor;
        fixed4 _RockColor;
        fixed4 _GrassColor;
        fixed4 _SnowColor;
        fixed4 _WaterColor;
        fixed4 _FlowerColor;

        half _Metallic;
        half _Glossiness;

        half _RockThreshold;
        half _SnowThreshold;
        half _SnowSlope;
        half _GrassHeight;
        half _GrassHeightVariation;
        half _SnowHeightVariation;
        half _WaterSlope;
        half _WaterHeight;
        half _WaterHeightVariation;
        half _WaterVelocity;
        half _RockClamp;
        half _GrassClamp;
        half _SnowClamp;
        half _WaterClamp;
        int _WaterAngle;
        float _FlowerDistanceX; 
        float _FlowerDistanceY;
        float _FlowerClamp;
        float _FlowerSlope;

        float _WaterScale;
        float _SnowScale;
        float _FlowerScale;

        struct VertexInput
        {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            float4 tangent : TANGENT;
            float2 texcoord : TEXCOORD0;
        };

        struct VertexOutput
        {
            float4 pos : SV_POSITION;
            float2 uv_GroundTex : TEXCOORD0;
            float2 uv_RockTex : TEXCOORD1;
            float2 uv_GrassTex : TEXCOORD2;
            float2 uv_WaterTex : TEXCOORD3;
            float2 uv_FlowerTex : TEXCOORD4;
            float3 worldPos : TEXCOORD5;
            float3 worldNormal : TEXCOORD6;
        };

        VertexOutput vert(inout VertexInput v)
        {
            VertexOutput o;
            
            float4 pos = v.vertex;

            float slope = 1 - abs(dot(v.normal, float3(0, 1, 0)));
            float slopeTop = 1 - abs(dot(v.normal, float3(0, _FlowerDistanceY, 0)));
            float slopeBottom = 1 - abs(dot(v.normal, float3(0, -_FlowerDistanceY, 0)));
            float slopeLeft = 1 - abs(dot(v.normal, float3(-_FlowerDistanceX, 0, 0)));
            float slopeRight = 1 - abs(dot(v.normal, float3(_FlowerDistanceX, 0, 0)));

            float maxSnowHeight = _SnowThreshold + _SnowHeightVariation;
            float minSnowHeight = _SnowThreshold - _SnowHeightVariation;
            float maxGrassHeight = _GrassHeight + _GrassHeightVariation;
            float minGrassHeight = _GrassHeight - _GrassHeightVariation;
            float maxWaterHeight = _WaterHeight + _WaterHeightVariation;
            float minWaterHeight = _WaterHeight - _WaterHeightVariation;

            float snowFactor = smoothstep(minSnowHeight, maxSnowHeight, pos.y);
            snowFactor *= (1 - smoothstep(0, _SnowSlope, slope));
            snowFactor = smoothstep(0, _SnowClamp, snowFactor);

            float flowerFactor = 0;
            float flowerSlopeFactor = smoothstep(_FlowerSlope, _FlowerClamp * 0.5, slopeTop) ;
            flowerSlopeFactor *= smoothstep(_FlowerSlope, _FlowerClamp * 0.5, slopeBottom);
            flowerSlopeFactor *= smoothstep(_FlowerSlope, _FlowerClamp * 0.5, slopeLeft);
            flowerSlopeFactor * smoothstep(_FlowerSlope, _FlowerClamp * 0.5, slopeRight);
            flowerSlopeFactor *= smoothstep(_FlowerSlope, _FlowerClamp * 0.5, slope);
            flowerFactor = smoothstep(maxGrassHeight, minGrassHeight, pos.y);
            flowerFactor *= smoothstep(0, _FlowerClamp, (1 - slope));
            flowerFactor *= smoothstep(_WaterHeight, maxWaterHeight, pos.y);
            flowerFactor *= flowerSlopeFactor;

           // v.vertex.y = max(v.vertex.y, _WaterHeight);
            v.vertex += float4(v.normal, 1.0) * 3 * snowFactor * _SnowScale;
            v.vertex += float4(0.0, 1.0, 0.0, 1.0) * 10 * snowFactor * _SnowScale;

            v.vertex += float4(v.normal, 1.0) * 10 * flowerFactor * _FlowerScale;
            
            float normalFactor = smoothstep(minWaterHeight, _WaterHeight, v.vertex.y) * _WaterScale;
            if(v.vertex.y <= _WaterHeight  && v.vertex.y >= minWaterHeight)
            {   
                v.normal = normalize(lerp(v.normal, float3(0, 1, 0), normalFactor));
            }
            
            o.pos = UnityObjectToClipPos(v.vertex);
            o.worldPos = mul(unity_ObjectToWorld,  v.vertex);
            o.worldNormal = mul(unity_ObjectToWorld, v.normal);
            o.uv_GroundTex = v.texcoord;
            o.uv_RockTex = v.texcoord;
            o.uv_GrassTex = v.texcoord;
            o.uv_WaterTex = v.texcoord;
            o.uv_FlowerTex = v.texcoord;

            return o;
        }

        struct Input
        {
            float2 uv_GroundTex; 
            float2 uv_RockTex; 
            float2 uv_GrassTex; 
            float2 uv_WaterTex;
            float2 uv_FlowerTex;
            float3 worldPos;
            float3 worldNormal;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            float3 normal = normalize(IN.worldNormal);

            float2 groundUV = IN.uv_GroundTex; 
            float2 rockUV = IN.uv_RockTex; 
            float2 grassUV = IN.uv_GrassTex;
            float2 snowUV = IN.uv_WaterTex;
            float2 waterUV = IN.uv_WaterTex;

            float2 flowerUV = IN.uv_FlowerTex;

            float slope = 1 - abs(dot(normal, float3(0, 1, 0)));
            float slopeTop = 1 - abs(dot(normal, float3(0, _FlowerDistanceY, 0)));
            float slopeBottom = 1 - abs(dot(normal, float3(0, -_FlowerDistanceY, 0)));
            float slopeLeft = 1 - abs(dot(normal, float3(-_FlowerDistanceX, 0, 0)));
            float slopeRight = 1 - abs(dot(normal, float3(_FlowerDistanceX, 0, 0)));

            fixed4 groundColor = tex2D(_GroundTex, groundUV) * _GroundColor;
            fixed4 rockColor = tex2D(_RockTex, rockUV) * _RockColor;
            fixed4 grassColor = tex2D(_GrassTex, grassUV) * _GrassColor;
            fixed4 flowerColor = tex2D(_FlowerTex, flowerUV) * _FlowerColor;
            fixed4 snowColor = tex2D(_SnowTex, snowUV) * _SnowColor;

            float angleRad = radians(_WaterAngle);
            float2x2 rotationMatrix = float2x2(cos(angleRad), -sin(angleRad), sin(angleRad), cos(angleRad));
            float2 rotatedUV = mul(rotationMatrix, waterUV);
            fixed4 waterColor = tex2D(_WaterTex, rotatedUV + _WaterVelocity * 0.01 * _Time) * _WaterColor;

            float maxSnowHeight = _SnowThreshold + _SnowHeightVariation;
            float minSnowHeight = _SnowThreshold - _SnowHeightVariation;
            float maxGrassHeight = _GrassHeight + _GrassHeightVariation;
            float minGrassHeight = _GrassHeight - _GrassHeightVariation;
            float maxWaterHeight = _WaterHeight + _WaterHeightVariation;
            float minWaterHeight = _WaterHeight - _WaterHeightVariation;

            float grassFactor = smoothstep(maxGrassHeight, minGrassHeight, IN.worldPos.y);
            grassFactor *= smoothstep(0, _GrassClamp, (1 - slope));
            
            float flowerFactor = 0;
            float flowerSlopeFactor = smoothstep(_FlowerSlope, _FlowerClamp * 0.5, slopeTop) ;
            flowerSlopeFactor *= smoothstep(_FlowerSlope, _FlowerClamp * 0.5, slopeBottom);
            flowerSlopeFactor *= smoothstep(_FlowerSlope, _FlowerClamp * 0.5, slopeLeft);
            flowerSlopeFactor * smoothstep(_FlowerSlope, _FlowerClamp * 0.5, slopeRight);
            flowerSlopeFactor *= smoothstep(_FlowerSlope, _FlowerClamp * 0.5, slope);
            flowerFactor = smoothstep(maxGrassHeight, minGrassHeight, IN.worldPos.y);
            flowerFactor *= smoothstep(0, _FlowerClamp, (1 - slope));
            flowerFactor *= smoothstep(_WaterHeight, maxWaterHeight, IN.worldPos.y);
            flowerFactor *= flowerSlopeFactor;

            float rockFactor = smoothstep(_RockThreshold - _RockClamp, _RockThreshold + _RockClamp, slope);
            
            float snowFactor = smoothstep(minSnowHeight, maxSnowHeight, IN.worldPos.y);
            snowFactor *= (1 - smoothstep(0, _SnowSlope, slope));
            snowFactor = smoothstep(0, _SnowClamp, snowFactor);

            float waterFactor = smoothstep(maxWaterHeight, minWaterHeight, IN.worldPos.y);
            waterFactor *= smoothstep(0, _WaterSlope, (1 - slope));
            waterFactor = smoothstep(0, _WaterClamp, waterFactor);

            fixed4 terrainColor = groundColor;
            terrainColor = lerp(terrainColor, grassColor, grassFactor);
            terrainColor = lerp(terrainColor, rockColor, rockFactor);
            terrainColor = lerp(terrainColor, snowColor, snowFactor);
            terrainColor = lerp(terrainColor, waterColor, waterFactor);
            terrainColor = lerp(terrainColor, flowerColor, flowerFactor);

            o.Albedo = terrainColor.rgb;
            //o.Albedo = float4(normal, 1.0);
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
