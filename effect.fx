matrix World;
matrix View;
matrix Projection;

Texture2D tex2D;

float AspectRatio;

SamplerState linearSampler
{
    Filter = MIN_MAG_MIP_LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
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
	float2 newcoords = psInput.TexCoord;

	if (AspectRatio > 1){
		newcoords.y = AspectRatio * (newcoords.y - 0.5) + 0.5;
	} else {
		newcoords.x = (newcoords.x - 0.5) / AspectRatio + 0.5;
	}

	float4 sample = tex2D.Sample(linearSampler, newcoords);

	if (newcoords.x < 0 || newcoords.y < 0 || newcoords.x > 1 || newcoords.y > 1){
		return float4(1, 1, 1, 1);
	} else {
		return sample;
	}
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
