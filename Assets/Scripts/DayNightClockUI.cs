using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DayNightClockUI : MonoBehaviour
{
    [Header("Core Reference")]
    [SerializeField] private DayNightCycle dayNightCycle;

    [Header("Clock Hand")]
    [SerializeField] private RectTransform clockHand;

    [Header("Label")]
    [SerializeField] private TextMeshProUGUI nightCounterLabel;

    private void Awake()
    {
        DayNightCycle.OnDayStart += HandleDayStart;
        DayNightCycle.OnNightStart += HandleNightStart;

        RefreshNightCounter();
    }

    private void OnDestroy()
    {
        DayNightCycle.OnDayStart -= HandleDayStart;
        DayNightCycle.OnNightStart -= HandleNightStart;
    }

    private void Update()
    {
        if (dayNightCycle == null) return;

        float t = dayNightCycle.timeOfDay;

        if (clockHand != null)
            clockHand.localEulerAngles = new Vector3(0f, 0f, -(t * 360f));
    }

    private void RefreshNightCounter()
    {
        if (nightCounterLabel != null)
            nightCounterLabel.text = NightCounter.currentNight > 0
                ? $"Night {NightCounter.currentNight}"
                : "Day";
        if(nightCounterLabel != null)
        {
            nightCounterLabel.text = NightCounter.currentNight == 20
                ? "Eternal Night"
                : nightCounterLabel.text;
        }
    }

    private void HandleDayStart()
    {
        RefreshNightCounter();
    }

    private void HandleNightStart()
    {
        RefreshNightCounter();
    }
}