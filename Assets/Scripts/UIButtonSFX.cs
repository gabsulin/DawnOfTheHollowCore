using UnityEngine;
using UnityEngine.UI;

public class UIButtonSFX : MonoBehaviour
{
    [SerializeField] private string sfxName = "UIButton";

    private void Start()
    {
        Button[] buttons = GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            button.onClick.AddListener(() => AudioManager.Instance?.PlaySFX(sfxName));
        }
    }
}