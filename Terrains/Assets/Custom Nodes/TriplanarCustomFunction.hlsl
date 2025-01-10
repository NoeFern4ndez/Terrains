#ifndef _TRIPLANA_CUSTOM_FUNCTIONS_HLSL_
#define _TRIPLANA_CUSTOM_FUNCTIONS_HLSL_

void TriplanarMapping_float(
    // INPUT PARAMETERS
    float4 Wall,
    UnityTexture2D WallTexture,
    float4 Ground,
    UnityTexture2D GroundTexture,
    float4 Grass,
    UnityTexture2D GrassTexture,
    float GrassHeight,
    float GrassHeightVariation,
    float Scale,
    float BlendFactor,
    float3 WorldPos,
    float3 WorldNormal,

    // OUTPUT PARAMETERS
    out float4 Out
)
{
    float4 c = tex2D(GroundTexture, WorldPos.xz * Scale) * Ground;    
    float4 c2 = tex2D(WallTexture, WorldPos.xy * Scale) * Wall;
    float4 c3 = tex2D(WallTexture, WorldPos.yz * Scale) * Wall;
    float4 c4 = tex2D(GrassTexture, WorldPos.xz * Scale) * Grass;

    float3 normal = pow(abs(WorldNormal), BlendFactor);
    float auxNormal = normal.x + normal.y + normal.z;
    normal = normal / auxNormal;

    float grassFactor = smoothstep(GrassHeight + GrassHeightVariation, GrassHeight - GrassHeightVariation, WorldPos.y);
    float4 res = lerp(c, c4, grassFactor) * normal.y + c2 * normal.z + c3 * normal.x;
    
    Out = res;
}

void SnowFragment_float(
    // INPUT PARAMETERS
    float4 color,
    float4 Snow,
    float SnowHeight,
    float SnowHeightVariation,
    float SnowFactor,
    float SnowSlope,
    float3 WorldPos,
    float3 WorldNormal,

    // OUTPUT PARAMETERS
    out float4 Out
)
{
    float maxSnowHeight = SnowHeight + SnowHeightVariation;
    float minSnowHeight = SnowHeight - SnowHeightVariation;

    float3 normal = pow(abs(WorldNormal), SnowFactor);
    float slope = 1 - abs(dot(normal, float3(0, 1, 0)));

    float snowFactor = smoothstep(minSnowHeight, maxSnowHeight, WorldPos.y);
    snowFactor *= (1 - smoothstep(0, SnowSlope, slope));
    snowFactor = smoothstep(0, SnowFactor, snowFactor);
    snowFactor *= step(0, dot(WorldNormal, float3(0, 1, 0)));
    snowFactor = max(0, snowFactor);

    Out = lerp(color, Snow, snowFactor);
}

void WaterFragment_float(
    // INPUT PARAMETERS
    float4 WaterTexture,
    float4 color,
    float4 Water,
    float WaterHeight,
    float WaterHeightVariation,
    float WaterFactor,
    float WaterSlope,
    float3 WorldPos,
    float3 WorldNormal,

    // OUTPUT PARAMETERS
    out float4 Out
)
{
    float maxWaterHeight = WaterHeight + WaterHeightVariation;
    float minWaterHeight = WaterHeight - WaterHeightVariation;

    float slope = 1 - abs(dot(WorldNormal, float3(0, 1, 0)));
    float4 waterColor = Water * WaterTexture;

    float waterFactor = smoothstep(maxWaterHeight, minWaterHeight, WorldPos.y);
        waterFactor *= smoothstep(0, WaterSlope,(1 - slope));
        waterFactor = smoothstep(0, WaterFactor, waterFactor);

    Out = lerp(color, waterColor, waterFactor);
}

void SnowDisplacement_float(
    // INPUT PARAMETERS
    float SnowScale,
    float SnowHeight,
    float SnowHeightVariation,
    float SnowFactor,
    float SnowSlope,
    float3 WorldPos,
    float3 WorldNormal,

    // OUTPUT PARAMETERS
    out float4 Position,
    out float3 outNormals
)
{
    float maxSnowHeight = SnowHeight + SnowHeightVariation;
    float minSnowHeight = SnowHeight - SnowHeightVariation;

    float3 normal = pow(abs(WorldNormal), SnowFactor);
    float slope = 1 - abs(dot(normal, float3(0, 1, 0)));

    float snowFactor = smoothstep(minSnowHeight, maxSnowHeight, WorldPos.y);
    snowFactor *= (1 - smoothstep(0, SnowSlope, slope));
    snowFactor = smoothstep(0, SnowFactor, snowFactor);
    snowFactor *= step(0, dot(WorldNormal, float3(0, 1, 0)));
    snowFactor = max(0, snowFactor);

    WorldPos += WorldNormal * snowFactor * SnowScale;

    outNormals = WorldNormal;
    Position =  float4(WorldPos, 1);
}

void WaterDisplacement_float(
    // INPUT PARAMETERS
    float WaterHeight,
    float WaterHeightVariation,
    float3 WorldPos,
    float3 WorldNormal,
    // OUTPUT PARAMETERS
    out float4 Position,
    out float3 outNormals
)
{
    float maxWaterHeight = WaterHeight + WaterHeightVariation;
    float minWaterHeight = WaterHeight - WaterHeightVariation;

    WorldPos.y = max(WaterHeight, WorldPos.y);
    outNormals = WorldNormal;

    float normalFactor = smoothstep(minWaterHeight, WaterHeight, WorldPos.y);
    if(WorldPos.y <= WaterHeight  && WorldPos.y >= minWaterHeight)
    {   
        outNormals = normalize(lerp(WorldNormal, float3(0, 1, 0), normalFactor));
    }


    Position =  float4(WorldPos, 1);
}


#endif