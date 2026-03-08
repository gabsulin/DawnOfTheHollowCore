using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject settingsMenu;
    private void Start()
    {
        if(AudioManager.Instance != null)
        {
            Debug.Log("Playing menu music.");
            AudioManager.Instance.PlayMusic("Normal", false);
        } else
        {
            Debug.LogWarning("AudioManager instance not found. Music will not play.");
        }
    }
    public void SendPlayerToTutorial()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Tutorial");
    }
    public void SendPlayerToGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }
    public void OpenSettings()
    {
        settingsMenu.SetActive(true);
    }
    public void CloseSettings()
    {
        settingsMenu.SetActive(false);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
