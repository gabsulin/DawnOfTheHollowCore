using UnityEngine;

public class Ability : ScriptableObject
{
    public new string name;
    public float coolDownTime;
    public float activeTime;

    [HideInInspector] public float baseCooldownTime;
    [HideInInspector] public float baseActiveTime;

    public virtual void Activate(GameObject parent) { }

    private void OnEnable()
    {
        if (baseCooldownTime == 0f) baseCooldownTime = coolDownTime;
        if (baseActiveTime == 0f) baseActiveTime = activeTime;
    }

    public void ResetValues()
    {
        coolDownTime = baseCooldownTime;
        activeTime = baseActiveTime;
    }
}
