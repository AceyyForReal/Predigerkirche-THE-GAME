using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TitleMenu : MonoBehaviour
{
    public GameObject controlsBackground;
    public GameObject creditsBackground;
    public static string sceneToLoad;
    
    private bool controlWindowActive = false;

    public void StartGame(string sceneName)
    {
        sceneToLoad = "church level"; 
        SceneManager.LoadScene("loading level");
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void Credits()
    {
        SceneManager.LoadScene("credits level");
    }

    public void Controls()
    {
        controlWindowActive = !controlWindowActive;

        if (controlWindowActive)
        {
            controlsBackground.SetActive(true);
            creditsBackground.SetActive(false);
        }
        else
        {
            controlsBackground.SetActive(false);
        }
    }
}