using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;
    public AudioClip[] clips;

    public AudioClip GetRandomClip()
    {
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning("No clips assigned for sound: " + name);
            return null;
        }

        return clips[Random.Range(0, clips.Length)];
    }
}
