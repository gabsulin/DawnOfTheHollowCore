using UnityEngine;

public class MenuController : MonoBehaviour
{
    [SerializeField] private GameObject settingsMenu;
    private void Start()
    {
        AudioManager.Instance.PlayMusic("Normal", false);
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
