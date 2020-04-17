#ifndef COLOR_FUNCS
#define COLOR_FUNCS

const static float Epsilon = 1e-10;

float3 LerpRGB(in float3 c1, in float3 c2, in float interp)
{
	return lerp(c1, c2, interp);
}

// ----- HSV operations ----- //
float3 HUEtoRGB(in float H)
{
	float R = abs(H * 6 - 3) - 1;
	float G = 2 - abs(H * 6 - 2);
	float B = 2 - abs(H * 6 - 4);
	return saturate(float3(R,G,B));
}
float3 RGBtoHCV(in float3 RGB)
{
	// Based on work by Sam Hocevar and Emil Persson
	float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0/3.0) : float4(RGB.gb, 0.0, -1.0/3.0);
	float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
	float C = Q.x - min(Q.w, Q.y);
	float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
	return float3(H, C, Q.x);
}
float3 RGBtoHSV(in float3 RGB)
{
	float3 HCV = RGBtoHCV(RGB);
	float S = HCV.y / (HCV.z + Epsilon);
	return float3(HCV.x, S, HCV.z);
}
float3 HSVtoRGB(in float3 HSV)
{
	float3 RGB = HUEtoRGB(HSV.x);
	return ((RGB - 1) * HSV.y + 1) * HSV.z;
}

float HueLerp(in float h1, in float h2, in float interp)
{
	float h;
	float d = h2 - h1;
	if (h1 > h2)
	{
		// Swap hues
		float temp = h2;
		h2 = h1;
		h1 = temp;

		d = -d;
		interp = 1 - interp;
	}

	if (d > 0.5) // 180deg
	{
		h1 = h1 + 1; // 360deg
		h = ( h1 + interp * (h2 - h1) ) % 1; // 360deg
	}
	if (d <= 0.5) // 180deg
	{
		h = h1 + interp * d;
	}
	return h;
}

float3 LerpHSV(in float3 c1, in float3 c2, in float interp)
{
	float3 hsv1 = RGBtoHSV(c1);
	float3 hsv2 = RGBtoHSV(c2);	
	float3 hsvout = float3(HueLerp(hsv1.x,hsv2.x,interp), lerp(hsv1.y,hsv2.y,interp), lerp(hsv1.z,hsv2.z,interp));
	if (hsvout.x > 1.0)
		hsvout.x -= 1.0;
	return HSVtoRGB(hsvout);
}
float3 LerpHSV_Simple(in float3 c1, in float3 c2, in float value)
{
	float3 hsv1 = RGBtoHSV(c1);
	float3 hsv2 = RGBtoHSV(c2);
	float hue = lerp(hsv1.x,hsv2.x,value);
	float3 hsvout = float3(hue,lerp(hsv1.y,hsv2.y,value),lerp(hsv1.z,hsv2.z,value));
	if (hsvout.x > 1.0)
		hsvout.x -= 1.0;
	return HSVtoRGB(hsvout);
}

// ----- HCY operations ----- //

// The weights of RGB contributions to luminance.
// Should sum to unity.
float3 HCYwts = float3(0.299, 0.587, 0.114);

float3 HCYtoRGB(in float3 HCY)
{
	float3 RGB = HUEtoRGB(HCY.x);
	float Z = dot(RGB, HCYwts);
	if (HCY.z < Z)
	{
		HCY.y *= HCY.z / Z;
	}
	else if (Z < 1)
	{
		HCY.y *= (1 - HCY.z) / (1 - Z);
	}
	return (RGB - Z) * HCY.y + HCY.z;
}
float3 RGBtoHCY(in float3 RGB)
{
	// Corrected by David Schaeffer
	float3 HCV = RGBtoHCV(RGB);
	float Y = dot(RGB, HCYwts);
	float Z = dot(HUEtoRGB(HCV.x), HCYwts);
	if (Y < Z)
	{
		HCV.y *= Z / (Epsilon + Y);
	}
	else
	{
		HCV.y *= (1 - Z) / (Epsilon + 1 - Y);
	}
	return float3(HCV.x, HCV.y, Y);
}
float3 LerpHCY(in float3 c1, in float3 c2, in float interp)
{
	float3 hcy1 = RGBtoHCY(c1);
	float3 hcy2 = RGBtoHCY(c2);
	float3 hcyout = float3(HueLerp(hcy1.x,hcy2.x,interp), lerp(hcy1.y,hcy2.y,interp), lerp(hcy1.z,hcy2.z,interp));
	return HCYtoRGB(hcyout);
}

#endif
