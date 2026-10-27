using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; 

public class Menu : MonoBehaviour
{
    [Header("Input Settings")]
    [SerializeField] private InputActionProperty toggleMenuAction;

    [Header("UI GameObjects")]
    [SerializeField] private GameObject hud;
    [SerializeField] private GameObject settings;
    [SerializeField] private GameObject objectInfoBox;
    
    [Header("Settings Sub-Menus")]
    [SerializeField] private GameObject soundSettings;
    [SerializeField] private GameObject graphicSettings;
    [SerializeField] private GameObject languageSettings;
    [SerializeField] private GameObject controlSettings;

    [Header("References")]
    [SerializeField] private MonoBehaviour playerMovementScript;
    [SerializeField] private AquamanController aquamanController;

    private bool menuOpen;

    private void OnEnable()
    {
        toggleMenuAction.action.Enable();
        toggleMenuAction.action.performed += OnToggleMenu;
    }

    private void OnDisable()
    {
        toggleMenuAction.action.performed -= OnToggleMenu;
        toggleMenuAction.action.Disable();
    }

    private void OnToggleMenu(InputAction.CallbackContext context)
    {
        if (objectInfoBox != null && objectInfoBox.activeSelf) return;
        if (aquamanController != null && aquamanController.isTalkingToAQM) return;

        if (menuOpen)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void PauseGame()
    {
        menuOpen = true;
        settings.SetActive(true);
        hud.SetActive(false);
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.Confined;
        
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        
    }

    public void ResumeGame()
    {
        menuOpen = false;
        settings.SetActive(false);
        hud.SetActive(true);
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        
        if (playerMovementScript != null) playerMovementScript.enabled = true;

    }
    
    public void GraphicSection()
    {
        SetAllSectionsInactive();
        graphicSettings.SetActive(true);
    }

    public void ControlsSection()
    {
        SetAllSectionsInactive();
        controlSettings.SetActive(true);
    }

    public void LanguageSection()
    {
        SetAllSectionsInactive();
        languageSettings.SetActive(true);
    }

    public void SoundSection()
    {
        SetAllSectionsInactive();
        soundSettings.SetActive(true);
    }

    private void SetAllSectionsInactive()
    {
        languageSettings.SetActive(false);
        graphicSettings.SetActive(false);
        soundSettings.SetActive(false);
        controlSettings.SetActive(false);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("title level");
    }
}