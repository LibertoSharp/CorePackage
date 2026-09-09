using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct InterpolationInfo
{
    public float StartValue;
    public float TargetValue;
    public float Seconds;
    public Interpolations.Type InterpolationType;
    public Action<float> UpdateValue;
    public float WaitSeconds;
    public Action<float> OnInterpolationEnd;
    public string? StringID;
}

public class Interpolations : LazySingleton<Interpolations>
{
    public enum Type
    {
        LINEAR,
        EASE_IN,
        EASE_OUT,
        SMOOTH_STEP
    }

    private Dictionary<string, Coroutine> _activeCoroutines = new Dictionary<string, Coroutine>();

    public void Reach(InterpolationInfo info)
    {
    bool hasId = !string.IsNullOrEmpty(info.StringID);

        if (hasId && _activeCoroutines.TryGetValue(info.StringID, out var existingCoroutine))
            StopCoroutine(existingCoroutine);

        Coroutine newCoroutine = StartCoroutine(ReachCoroutine(info));

        if (hasId)
            _activeCoroutines[info.StringID] = newCoroutine;
        
    }

    private IEnumerator ReachCoroutine(InterpolationInfo info)
    {
        if (info.WaitSeconds > 0f)
        {
            info.UpdateValue?.Invoke(info.StartValue);
            yield return new WaitForSeconds(info.WaitSeconds);
        }

        float elapsed = 0f;
        while (elapsed < info.Seconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / info.Seconds);
            float t2 = ApplyEasing(t, info.InterpolationType);
            info.UpdateValue?.Invoke(Mathf.Lerp(info.StartValue, info.TargetValue, t2));
            yield return null;
        }

        info.UpdateValue?.Invoke(info.TargetValue);
        info.OnInterpolationEnd?.Invoke(info.TargetValue);
        if (!string.IsNullOrEmpty(info.StringID))
            _activeCoroutines.Remove(info.StringID);
        
    }

    private float ApplyEasing(float t, Type type)
    {
        return type switch
        {
            Type.LINEAR => t,
            Type.EASE_IN => t * t,
            Type.EASE_OUT => 1f - (1f - t) * (1f - t),
            Type.SMOOTH_STEP => t * t * (3f - 2f * t),
            _ => t,
        };
    }

    public static float Damp(float source, float target, float seconds, float dt)
    {
        float f = Mathf.Pow(0.01f, 1f / seconds);
        source = Mathf.Lerp(source, target, 1f - Mathf.Pow(f, dt));
        return source;
    }

    public static Vector3 Damp(Vector3 source, Vector3 target, float seconds, float dt)
    {
        float f = Mathf.Pow(0.01f, 1f / seconds);
        source.x = Mathf.Lerp(source.x, target.x, 1f - Mathf.Pow(f, dt));
        source.y = Mathf.Lerp(source.y, target.y, 1f - Mathf.Pow(f, dt));
        source.z = Mathf.Lerp(source.z, target.z, 1f - Mathf.Pow(f, dt));
        return source;
    }
}