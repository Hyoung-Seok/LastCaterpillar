using UnityEngine;

public static class GoldenSpiral
{
    private const float GoldenAngleRad = 2.39996323f; 
    
    public static Vector3 GetOffset(int index, float spreadScale, float yOffset = 0)
    {
        var angle = index * GoldenAngleRad;
        var radius = Mathf.Sqrt(index) * spreadScale;
        
        var x = Mathf.Cos(angle) * radius;
        var z = Mathf.Sin(angle) * radius;
        
        return new Vector3(x, yOffset, z);
    }
}
