#ifndef _TRIPLANA_CUSTOM_FUNCTIONS_HLSL_
#define _TRIPLANA_CUSTOM_FUNCTIONS_HLSL_

void TriplanarMapping_float(
    // INPUT PARAMETERS
    UnityTexture2D WallTexture,
    UnityTexture2D GroundTexture,
    float Scale,
    float BlendFactor,
    float3 WorldPos,
    float3 WorldNormal,

    // OUTPUT PARAMETERS
    out float4 Out
)
{
    float4 c = tex2D(GroundTexture, WorldPos.xz * Scale);
    float4 c2 = tex2D(WallTexture, WorldPos.xy * Scale);
    float4 c3 = tex2D(WallTexture, WorldPos.yz * Scale);

    float3 normal = pow(abs(WorldNormal), BlendFactor);
    float auxNormal = normal.x + normal.y + normal.z;
    normal = normal / auxNormal;
    float4 res = c * normal.y + c2 * normal.z + c3 * normal.x;
    
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

    Out = lerp(color, Snow, snowFactor);
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

    WorldPos += WorldNormal * 3 * snowFactor * SnowScale;

    Out =  float4(WorldPos, 1);
}


#endif