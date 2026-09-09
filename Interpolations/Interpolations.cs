using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;

public class Interpolations : LazySingleton<Interpolations>
{
	 public enum Type
    {
        LINEAR,
        EASE_IN,
        EASE_OUT,
        SMOOTH_STEP
    }
	private Dictionary<string, Coroutine> _activeCoroutines = new();
	
	public void Reach(float start, float target, float seconds, Type type, Action<float> onUpdate, float waitSeconds = 0)
	{
		StartCoroutine(ReachCoroutine(null, start, target, seconds, type, onUpdate, waitSeconds));
	}

	public void Reach(string id, float start, float target, float seconds, Type type, Action<float> onUpdate, float waitSeconds = 0)
    {
        if (_activeCoroutines.TryGetValue(id, out Coroutine existingCoroutine))
        {
            StopCoroutine(existingCoroutine);
        }

        _activeCoroutines[id] = StartCoroutine(ReachCoroutine(id, start, target, seconds, type, onUpdate, waitSeconds));
    }

	private IEnumerator ReachCoroutine(string id, float start, float target, float seconds, Type type, Action<float> onUpdate, float waitSeconds)
    {
		yield return new WaitForSeconds(waitSeconds);
        float elapsed = 0f;

        while (elapsed < seconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / seconds);
            float easedT = ApplyEasing(t, type);
            
            onUpdate?.Invoke(Mathf.Lerp(start, target, easedT));
            yield return null;
        }

        onUpdate?.Invoke(target);
        
		if (!string.IsNullOrEmpty(id))
			_activeCoroutines.Remove(id);
		
    }

    private float ApplyEasing(float t, Type type)
    {
        return type switch
        {
            Type.LINEAR => t,
            Type.EASE_IN => t * t,
            Type.EASE_OUT => 1f - (1f - t) * (1f - t),
            Type.SMOOTH_STEP => t * t * (3f - 2f * t),
            _ => t
        };
    }
	
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