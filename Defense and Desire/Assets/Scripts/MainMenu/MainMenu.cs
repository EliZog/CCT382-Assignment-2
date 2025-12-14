using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject overlay;
    public GameObject optionsPanel;

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(0);
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(1);
    }

    public void Options()
    {
        overlay.SetActive(true);
        optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        overlay.SetActive(false);
        optionsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("QUIT GAME!");
    }
}
