using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightCycle : MonoBehaviour
{
    [Header("Time Settings")]
    public float dayLengthInMinutes = 5f;
    [Range(0f, 1f)] public float timeOfDay = 0f;
    private bool isNight = false;

    [Header("Lighting settings")]
    public Light2D globalLight;
    public float dayIntensity = 1f;
    public float nightIntensity = 0.2f;

    public static event Action OnNightStart;
    public static event Action OnDayStart;

    void Update()
    {
        if (!globalLight) return;

        timeOfDay += Time.deltaTime / (dayLengthInMinutes * 60);
        if (timeOfDay >= 1)
            timeOfDay = 0;

        float intensity = Mathf.Lerp(dayIntensity, nightIntensity, Mathf.Sin(timeOfDay * Mathf.PI));
        globalLight.intensity = intensity;

        bool currentlyNight = timeOfDay > 0.5f;
        if (currentlyNight != isNight)
        {
            isNight = currentlyNight;
            if (isNight)
                OnNightStart?.Invoke();
            else
                OnDayStart?.Invoke();
        }
    }
}
