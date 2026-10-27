using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsScroll : MonoBehaviour
{
    public RectTransform creditsContent;
    public float scrollSpeed = 20f;

    void Update()
    {
        creditsContent.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;
    }

    public void BackToTitleScreen()
    {
        SceneManager.LoadScene("title level");
    }
}
