using UnityEngine;

public static class Interpolations
{
    public static float Damp(float source, float target, float seconds, float dt)
    {
        float smoothing = Mathf.Pow(0.01f, 1 / seconds);
        source = Mathf.Lerp(source, target, 1 - Mathf.Pow(smoothing, dt));
        return source;
    }

    public static Vector3 Damp(Vector3 source, Vector3 target, float seconds, float dt)
    {
        float smoothing = Mathf.Pow(0.01f, 1 / seconds);
        source.x = Mathf.Lerp(source.x, target.x, 1 - Mathf.Pow(smoothing, dt));
        source.y = Mathf.Lerp(source.y, target.y, 1 - Mathf.Pow(smoothing, dt));
        source.z = Mathf.Lerp(source.z, target.z, 1 - Mathf.Pow(smoothing, dt));
        return source;
    }
}
