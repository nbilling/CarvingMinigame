using UnityEngine;

public static class MathUtil
{
    public static int Mod(int a, int m)
    {
        return ((a % m) + m) % m;
    }

    public static float RoundToDecimalPlaces(float d, uint n)
    {
        float tenToNthPower = Mathf.Pow(10, n);
        return Mathf.Round(d *tenToNthPower ) / tenToNthPower;
    }

    public static Vector3 RoundToDecimalPlaces(Vector3 v, uint n)
    {
        return new Vector3(RoundToDecimalPlaces(v.x, n), RoundToDecimalPlaces(v.y, n), RoundToDecimalPlaces(v.z, n));
    }

    public static float TriangleArea(Vector3 a, Vector3 b, Vector3 c)
    {
        // TODO: possibly more efficient calculation for this?
        return TriangleArea((a - b).magnitude, (b - c).magnitude, (c - a).magnitude);
    }

    public static float TriangleArea(float a, float b, float c)
    {
        float p = (a + b + c) / 2f;
        return Mathf.Sqrt(p * (p - a) * (p - b) * (p - c));
    }
}
