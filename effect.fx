matrix World;
matrix View;
matrix Projection;

Texture2D tex2D;

SamplerState linearSampler
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct PS_INPUT
{
	float4 Pos : SV_POSITION;
	float2 TexCoord : TEXCOORD0;
};

PS_INPUT VS( float4 Pos : POSITION, float2 TexCoord : TEXCOORD )
{
	PS_INPUT psInput;

	Pos = mul( Pos, World );
	Pos = mul( Pos, View );

	psInput.Pos = mul( Pos, Projection );
	psInput.TexCoord = TexCoord;

    return psInput;
}

float4 PS( PS_INPUT psInput ) : SV_Target
{
    return tex2D.Sample( linearSampler, psInput.TexCoord );
}

technique10 Render
{
    pass P0
    {
        SetVertexShader( CompileShader( vs_4_0, VS() ) );
        SetGeometryShader( NULL );
        SetPixelShader( CompileShader( ps_4_0, PS() ) );
    }
}
