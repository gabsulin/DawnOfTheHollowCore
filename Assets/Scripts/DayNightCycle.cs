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

    private int bossFightNight = 20;
    private bool nightIncremented = false;
    private bool eternalNight = false;

    void Update()
    {
        if (!globalLight) return;

        float previousTimeOfDay = timeOfDay;

        if (!eternalNight)
        {
            timeOfDay += Time.deltaTime / (dayLengthInMinutes * 60);
            if (timeOfDay >= 1f)
                timeOfDay = 0;
        }

        float intensity = Mathf.Lerp(dayIntensity, nightIntensity, Mathf.Sin(timeOfDay * Mathf.PI));
        globalLight.intensity = intensity;

        bool currentlyNight = timeOfDay > 0.5f;

        if (currentlyNight != isNight)
        {

            isNight = currentlyNight;

            if (isNight)
            {
                if (!nightIncremented)
                {
                    NightCounter.currentNight++;
                    nightIncremented = true;

                    OnNightStart?.Invoke();

                    if (NightCounter.currentNight >= bossFightNight)
                    {
                        eternalNight = true;
                        Debug.Log("Eternal night has begun!");
                    }
                }
            }
            else
            {
                OnDayStart?.Invoke();
                nightIncremented = false;
            }
        }
    }
}

partial class NightCounter
{
    public static int currentNight = 0;
}