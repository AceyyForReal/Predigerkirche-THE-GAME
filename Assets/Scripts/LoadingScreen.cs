using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    public Slider progressBar;
    public TMP_Text progressText; 

    void Start()
    {
        if (!string.IsNullOrEmpty(TitleMenu.sceneToLoad))
        {
            LoadScene(TitleMenu.sceneToLoad);
        }
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (progressBar != null)
                progressBar.value = progress;

            if (operation.progress >= 0.9f)
            {
                if (progressBar != null)
                progressBar.value = 1f;

                operation.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}