using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;

public class Settings : MonoBehaviour
{
    // Audio ------------------------
    public Slider musicVolumeSlider;
    public Slider dialogueVolumeSlider;
    public Slider effectsVolumeSlider;
    public AudioSource musicSource;
    public AudioSource[] dialogueSource;
    public AudioSource[] effectsSource;

    // % Texts ------------------------
    public TextMeshProUGUI musicVolumeText;
    public TextMeshProUGUI dialogueVolumeText;
    public TextMeshProUGUI effectsVolumeText;

    // Graphics ----------------------
    private string[] resolutions = { "3840x2160", "2560x1440", "1920x1080", "1600x900", "1366x768" };
    private int currentResolutionIndex = 2;
    public Button nextRes;
    public Button prevRes;
    public TextMeshProUGUI resText;

    // Anti-Aliasing ---------------
    private int[] aaLevels = { 1, 2, 4, 8 };
    private string[] aaLabels = { "Off", "2x", "4x", "8x" };
    private int currentAAIndex = 1;
    public Button nextAA;
    public Button prevAA;
    public TextMeshProUGUI aaText;

    // Render Scale -------------------
    public Slider renderScaleSlider;
    public TextMeshProUGUI renderScaleText;

    // Misc -------------------
    public Toggle fpsToggle;
    public GameObject fps;
    public Toggle fullscreenToggle;
    public Toggle moblurToggle;
    public Toggle bloomToggle;
    public Toggle DoFToggle;
    public Volume postProcessVolume;
    public UniversalRenderPipelineAsset urpAsset;
    public ScriptableRendererFeature ssaoFeature;

    private DepthOfField depthOfField;
    private Bloom bloom;
    private MotionBlur motionBlur;

    void Start()
    {
        InitializePostProcessing();
        SetResolutionText();

        if (urpAsset != null)
        {
            int msaa = urpAsset.msaaSampleCount;
            int idx = System.Array.IndexOf(aaLevels, msaa);
            currentAAIndex = idx >= 0 ? idx : 0;
        }
        ApplyAA();

        if (renderScaleSlider != null)
        {
            renderScaleSlider.minValue = 0.5f;
            renderScaleSlider.maxValue = 1f;
            renderScaleSlider.wholeNumbers = false;

            float initial = urpAsset != null ? urpAsset.renderScale : 1f;
            initial = Mathf.Clamp(initial, 0.5f, 1f);
            renderScaleSlider.value = initial;

            renderScaleSlider.onValueChanged.AddListener(OnRenderScaleChanged);

            UpdateRenderScaleDisplay(initial);
            if (urpAsset != null) urpAsset.renderScale = initial;
        }

        // Music
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 1f;
            musicVolumeSlider.wholeNumbers = false;

            if (musicSource != null) musicVolumeSlider.value = musicSource.volume;
            UpdateMusicVolumeDisplay(musicVolumeSlider.value);
            ApplyMusicVolume(musicVolumeSlider.value);

            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        }

        // Dialogue
        if (dialogueVolumeSlider != null)
        {
            dialogueVolumeSlider.minValue = 0f;
            dialogueVolumeSlider.maxValue = 1f;
            dialogueVolumeSlider.wholeNumbers = false;

            if (dialogueSource != null && dialogueSource.Length > 0) dialogueVolumeSlider.value = dialogueSource[0].volume;
            UpdateDialogueVolumeDisplay(dialogueVolumeSlider.value);
            ApplyDialogueVolume(dialogueVolumeSlider.value);

            dialogueVolumeSlider.onValueChanged.AddListener(OnDialogueVolumeChanged);
        }

        // Effects
        if (effectsVolumeSlider != null)
        {
            effectsVolumeSlider.minValue = 0f;
            effectsVolumeSlider.maxValue = 1f;
            effectsVolumeSlider.wholeNumbers = false;

            if (effectsSource != null && effectsSource.Length > 0) effectsVolumeSlider.value = effectsSource[0].volume;
            UpdateEffectsVolumeDisplay(effectsVolumeSlider.value);
            ApplyEffectsVolume(effectsVolumeSlider.value);

            effectsVolumeSlider.onValueChanged.AddListener(OnEffectsVolumeChanged);
        }
    }

    void OnDestroy()
    {
        // remove listeners
        if (renderScaleSlider != null) renderScaleSlider.onValueChanged.RemoveListener(OnRenderScaleChanged);
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
        if (dialogueVolumeSlider != null) dialogueVolumeSlider.onValueChanged.RemoveListener(OnDialogueVolumeChanged);
        if (effectsVolumeSlider != null) effectsVolumeSlider.onValueChanged.RemoveListener(OnEffectsVolumeChanged);
    }

    private void InitializePostProcessing()
    {
        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGet(out depthOfField);
            postProcessVolume.profile.TryGet(out bloom);
            postProcessVolume.profile.TryGet(out motionBlur);
        }
    }

    public void OnMusicVolumeChanged(float value)
    {
        ApplyMusicVolume(value);
        UpdateMusicVolumeDisplay(value);
    }
    private void ApplyMusicVolume(float value)
    {
        if (musicSource != null) musicSource.volume = value;
    }
    private void UpdateMusicVolumeDisplay(float value)
    {
        if (musicVolumeText != null) musicVolumeText.text = Mathf.RoundToInt(value * 100f).ToString() + "%";
    }

    public void OnDialogueVolumeChanged(float value)
    {
        ApplyDialogueVolume(value);
        UpdateDialogueVolumeDisplay(value);
    }
    private void ApplyDialogueVolume(float value)
    {
        if (dialogueSource != null)
        {
            for (int i = 0; i < dialogueSource.Length; i++)
            {
                if (dialogueSource[i] != null) dialogueSource[i].volume = value;
            }
        }
    }
    private void UpdateDialogueVolumeDisplay(float value)
    {
        if (dialogueVolumeText != null) dialogueVolumeText.text = Mathf.RoundToInt(value * 100f).ToString() + "%";
    }

    public void OnEffectsVolumeChanged(float value)
    {
        ApplyEffectsVolume(value);
        UpdateEffectsVolumeDisplay(value);
    }
    private void ApplyEffectsVolume(float value)
    {
        if (effectsSource != null)
        {
            for (int i = 0; i < effectsSource.Length; i++)
            {
                if (effectsSource[i] != null) effectsSource[i].volume = value;
            }
        }
    }
    private void UpdateEffectsVolumeDisplay(float value)
    {
        if (effectsVolumeText != null) effectsVolumeText.text = Mathf.RoundToInt(value * 100f).ToString() + "%";
    }

    // ---- Post Processing Toggles ----
    public void ToggleDepthOfField(bool enabled)
    {
        if (depthOfField != null) depthOfField.active = enabled;
    }

    public void ToggleBloom(bool enabled)
       {
        if (bloom != null) bloom.active = enabled;
    }

    public void ToggleMotionBlur(bool enabled)
    {
        if (motionBlur != null) motionBlur.active = enabled;
    }

    // ---- Misc Toggles ----
    public void ToggleFPS(bool enabled)
    {
        if (fps != null) fps.SetActive(enabled);
    }

    public void ToggleFullScreen(bool enabled)
    {
        Screen.fullScreen = enabled;
    }

    public void ToggleSSAO(bool enabled)
    {
        if (ssaoFeature != null) ssaoFeature.SetActive(enabled);
    }

    // ---- Resolution ----
    public void NextResolution()
    {
        currentResolutionIndex = (currentResolutionIndex + 1) % resolutions.Length;
        ApplyResolution();
    }

    public void PrevResolution()
    {
        Debug.Log("Skibidi");
        currentResolutionIndex = (currentResolutionIndex - 1 + resolutions.Length) % resolutions.Length;
        ApplyResolution();
    }

    private void ApplyResolution()
    {
        Screen.SetResolution(GetWidth(), GetHeight(), Screen.fullScreen);
        SetResolutionText();
    }

    private int GetWidth()
    {
        string[] split = resolutions[currentResolutionIndex].Split('x');
        return int.Parse(split[0]);
    }

    private int GetHeight()
    {
        string[] split = resolutions[currentResolutionIndex].Split('x');
        return int.Parse(split[1]);
    }

    private void SetResolutionText()
    {
        if (resText != null) resText.text = resolutions[currentResolutionIndex];
    }

    // ---- Anti-Aliasing ----
    public void NextAA()
    {
        currentAAIndex = (currentAAIndex + 1) % aaLevels.Length;
        ApplyAA();
    }

    public void PrevAA()
    {
        currentAAIndex = (currentAAIndex - 1 + aaLevels.Length) % aaLevels.Length;
        ApplyAA();
    }

    private void ApplyAA()
    {
        if (urpAsset != null)
        {
            urpAsset.msaaSampleCount = aaLevels[currentAAIndex];
        }

        if (aaText != null) aaText.text = aaLabels[currentAAIndex];
    }

    // ---- Render Scale ----
    public void OnRenderScaleChanged(float value)
    {
        float v = Mathf.Clamp(value, 0.5f, 1f);
        if (urpAsset != null) urpAsset.renderScale = v;
        UpdateRenderScaleDisplay(v);
    }

    public void OnRenderScaleChanged()
    {
        if (renderScaleSlider != null)
        {
            OnRenderScaleChanged(renderScaleSlider.value);
        }
    }

    private void UpdateRenderScaleDisplay(float value)
    {
        if (renderScaleText != null)
        {
            int percent = Mathf.RoundToInt(value * 100f);
            renderScaleText.text = percent.ToString() + "%";
        }
    }
}