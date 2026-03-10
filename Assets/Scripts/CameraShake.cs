using UnityEngine;
using System.Collections;
using Unity.Cinemachine;


public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    public bool useCoroutine = true;

    private CinemachineImpulseSource impulseSource;
    private Coroutine shakeCoroutine;

    void Awake()
    {
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void StartShake(float duration = 0.2f, float interval = 0.05f, float force = 1f)
    {
        if (!useCoroutine)
        {
            impulseSource.GenerateImpulse(force);
            return;
        }

        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(ShakeRoutine(duration, interval, force));
    }

    public void StopShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }
    }

    private IEnumerator ShakeRoutine(float duration, float interval, float force)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            impulseSource.GenerateImpulse(force);
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        shakeCoroutine = null;
    }
}
