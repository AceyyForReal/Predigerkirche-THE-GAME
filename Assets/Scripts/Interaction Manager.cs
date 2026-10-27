using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Settings;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    private bool isTutorialDialogue = false;

    [Header("General UI")]
    public GameObject pauseMenu;
    public GameObject hudUI;
    public GameObject pressE;
    
    [Header("New Input System Actions")]
    public InputActionProperty interactAction;
    public InputActionProperty returnAction;
    public InputActionProperty endDialogueAction;

    [Header("Dialogue UI")]
    public RectTransform topBar;
    public RectTransform bottomBar;
    public TextMeshProUGUI titleTextUI;
    public TextMeshProUGUI subtitleTextUI;

    [Header("Dialogue Settings")]
    public float moveAmount = 100f;
    public float speed = 5f;
    public float dialogueDuration = 5f;

    [Header("Camera Rotation")]
    public Transform cameraTransform;
    public Transform lookTarget;
    public float rotationSpeed = 5f;

    [Header("Player Control")]
    public MonoBehaviour playerMovementScript;
    public AquamanController aqmControllerScript;

    [Header("Interaction Settings")]
    public float interactionRange = 1.5f;
    public GameObject[] clickableObjects;
    public int hoverLayer = 6;
    public int defaultLayer = 7;

    [Header("Quote Camera Lerp")]
    public float quoteLerpDuration = 1.0f;

    [Header("Tutorial (Start)")]
    public bool playTutorialOnStart = true;
    public LocalizedString tutorialTitle;
    public LocalizedString tutorialDescription;

    [Header("End Dialogue")]
    public LocalizedString endTitle;
    public LocalizedString endDescription;
    public AudioClip endDialogueClip;
    public AudioSource audioSource;
    public float endDialogueDuration = 5f;

    private bool queuedEndDialogue = false;
    public LocalizedString emptyDialogue;
    public TypewriterEffect subtitleTypewriter;

    private Renderer[] objectRenderers;
    private ObjectInfo[] objectInfos;
    private QuoteInfo[] quoteInfos;
    private GameObject lastHoveredObject = null;
    private int activeObjectIndex = -1;
    private Camera mainCamera;
    private float lastRaycastTime = 0f;
    private const float RAYCAST_THROTTLE = 0.1f;
    private bool isHovering = false;
    private bool dialogueActive = false;
    private bool tutorialLaunched = false;
    private bool quoteLerping = false;
    private bool atQuoteTarget = false;
    private bool quoteActive = false;

    private Vector2 topOriginal;
    private Vector2 bottomOriginal;
    private Vector2 topTarget;
    private Vector2 bottomTarget;

    private Quaternion originalRotation;
    private float dialogueTimer;
    private Vector3 savedStartPos;
    private Quaternion savedStartRot;
    private Vector3 savedTargetPos;
    private Quaternion savedTargetRot;
    private QuoteInfo currentQuoteSource = null;

    private Coroutine quoteCoroutine = null;

    private void OnEnable()
    {
        interactAction.action.Enable();
        interactAction.action.performed += OnInteractPerformed;

        returnAction.action.Enable();
        returnAction.action.performed += OnReturnPerformed;

        endDialogueAction.action.Enable();
        endDialogueAction.action.performed += OnEndDialoguePerformed;
    }

    private void OnDisable()
    {
        // avoid memory leaks
        interactAction.action.performed -= OnInteractPerformed;
        returnAction.action.performed -= OnReturnPerformed;
        endDialogueAction.action.performed -= OnEndDialoguePerformed;

        interactAction.action.Disable();
        returnAction.action.Disable();
        endDialogueAction.action.Disable();
    }

    void Start()
    {
        int numObjects = clickableObjects != null ? clickableObjects.Length : 0;
        objectRenderers = new Renderer[numObjects];
        objectInfos = new ObjectInfo[numObjects];
        quoteInfos = new QuoteInfo[numObjects];
        mainCamera = Camera.main;

        for (int i = 0; i < numObjects; i++)
        {
            if (clickableObjects[i] != null)
            {
                objectRenderers[i] = clickableObjects[i].GetComponent<Renderer>();
                objectInfos[i] = clickableObjects[i].GetComponent<ObjectInfo>();
                quoteInfos[i] = clickableObjects[i].GetComponent<QuoteInfo>();
            }
        }

        if (topBar != null) topOriginal = topBar.anchoredPosition;
        if (bottomBar != null) bottomOriginal = bottomBar.anchoredPosition;
        topTarget = topOriginal;
        bottomTarget = bottomOriginal;

        if (cameraTransform != null)
        {
            originalRotation = cameraTransform.rotation;
        }

        if (subtitleTypewriter == null && subtitleTextUI != null)
            subtitleTypewriter = subtitleTextUI.GetComponent<TypewriterEffect>();

        if (playTutorialOnStart)
            StartCoroutine(LaunchTutorialOnce());
    }

    private IEnumerator LaunchTutorialOnce()
    {
        yield return null;
        if (tutorialLaunched) yield break;

        string tTitle = tutorialTitle != null ? tutorialTitle.GetLocalizedString() : "";
        string tDesc = tutorialDescription != null ? tutorialDescription.GetLocalizedString() : "";

        if (!string.IsNullOrEmpty(tTitle) || !string.IsNullOrEmpty(tDesc))
        {
            tutorialLaunched = true;
            isTutorialDialogue = true;
            StartDialogue(tTitle, tDesc);
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (pauseMenu.activeSelf || dialogueActive || quoteActive || quoteLerping) return;

        if (lastHoveredObject != null)
        {
            for (int i = 0; i < clickableObjects.Length; i++)
            {
                if (lastHoveredObject == clickableObjects[i])
                {
                    HandleInteractionTrigger(i);
                    break;
                }
            }
        }
    }

    private void OnReturnPerformed(InputAction.CallbackContext context)
    {
        if (atQuoteTarget && !pauseMenu.activeSelf)
        {
            ReturnFromQuote();
        }
    }

    private void OnEndDialoguePerformed(InputAction.CallbackContext context)
    {
        if (dialogueActive)
        {
            EndDialogue();
        }
    }

    void Update()
    {
        HandleHoverVisualsOnly();

        if (dialogueActive)
        {
            dialogueTimer -= Time.deltaTime;
            if (dialogueTimer <= 0f)
                EndDialogue();
        }

        if (topBar != null)
            topBar.anchoredPosition = Vector2.Lerp(topBar.anchoredPosition, topTarget, Time.deltaTime * speed);

        if (bottomBar != null)
            bottomBar.anchoredPosition = Vector2.Lerp(bottomBar.anchoredPosition, bottomTarget, Time.deltaTime * speed);

        if (dialogueActive && cameraTransform != null && lookTarget != null)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookTarget.position - cameraTransform.position);
            cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    private void HandleHoverVisualsOnly()
    {
        if (quoteLerping || atQuoteTarget) return;

        if (Time.time - lastRaycastTime < RAYCAST_THROTTLE) return;
        lastRaycastTime = Time.time;

        if (mainCamera == null) return;
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange))
        {
            GameObject hitObject = hit.collider.gameObject;
            for (int i = 0; i < clickableObjects.Length; i++)
            {
                if (hitObject == clickableObjects[i])
                {
                    if (!isHovering || lastHoveredObject != hitObject)
                    {
                        ResetLastHoveredObject();
                        isHovering = true;
                        lastHoveredObject = hitObject;
                        hitObject.layer = hoverLayer;
                        if (pressE != null) pressE.SetActive(true);
                    }
                    return;
                }
            }
        }
        ResetLastHoveredObject();
    }

    private void HandleInteractionTrigger(int index)
    {
        activeObjectIndex = index;
        GameObject hitObject = clickableObjects[index];
        ObjectInfo objInfo = objectInfos[index];
        QuoteInfo quoteInfo = quoteInfos[index];

        if (objInfo != null)
        {
            string localizedTitle = objInfo.titleText != null ? objInfo.titleText.GetLocalizedString() : "";
            string localizedDescription = objInfo.infoText != null ? objInfo.infoText.GetLocalizedString() : "";

            StartDialogue(localizedTitle, localizedDescription, objInfo.duration);
            objInfo.HideExclamation();
            objInfo.Narrator();
        }
        else if (quoteInfo != null)
        {
            if (quoteInfo.lookTarget != null && cameraTransform != null)
            {
                if (quoteCoroutine != null) StopCoroutine(quoteCoroutine);
                quoteCoroutine = StartCoroutine(PerformQuoteCameraLerpToTarget(quoteInfo.lookTarget, quoteInfo));
            }
            else
            {
                quoteInfo.OnQuoteRead?.Invoke();
            }
        }
    }

    private IEnumerator PerformQuoteCameraLerpToTarget(Transform targetLookTransform, QuoteInfo sourceQuote)
    {
        quoteLerping = true;
        quoteActive = true;
        atQuoteTarget = false;
        currentQuoteSource = sourceQuote;

        savedStartPos = cameraTransform.position;
        savedStartRot = cameraTransform.rotation;
        savedTargetPos = targetLookTransform.position;
        savedTargetRot = targetLookTransform.rotation;

        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (aqmControllerScript != null) aqmControllerScript.isTalkingToAQM = true;
        if (hudUI != null) hudUI.SetActive(false);
        if (pressE != null) pressE.SetActive(false);

        float t = 0f;
        while (t < quoteLerpDuration)
        {
            t += Time.deltaTime;
            float frac = Mathf.Clamp01(t / quoteLerpDuration);
            cameraTransform.position = Vector3.Lerp(savedStartPos, savedTargetPos, frac);
            cameraTransform.rotation = Quaternion.Slerp(savedStartRot, savedTargetRot, frac);
            yield return null;
        }

        cameraTransform.position = savedTargetPos;
        cameraTransform.rotation = savedTargetRot;
        quoteLerping = false;
        atQuoteTarget = true;
    }

    public void ReturnFromQuote()
    {
        if (!atQuoteTarget || quoteLerping) return;
        if (quoteCoroutine != null) StopCoroutine(quoteCoroutine);
        quoteCoroutine = StartCoroutine(PerformQuoteCameraLerpReturn());
    }

    private IEnumerator PerformQuoteCameraLerpReturn()
    {
        quoteLerping = true;
        float t = 0f;
        while (t < quoteLerpDuration)
        {
            t += Time.deltaTime;
            float frac = Mathf.Clamp01(t / quoteLerpDuration);
            cameraTransform.position = Vector3.Lerp(savedTargetPos, savedStartPos, frac);
            cameraTransform.rotation = Quaternion.Slerp(savedTargetRot, savedStartRot, frac);
            yield return null;
        }

        cameraTransform.position = savedStartPos;
        cameraTransform.rotation = savedStartRot;

        if (aqmControllerScript != null) aqmControllerScript.isTalkingToAQM = false;
        if (playerMovementScript != null) playerMovementScript.enabled = true;
        if (hudUI != null) hudUI.SetActive(true);

        currentQuoteSource?.OnQuoteRead?.Invoke();
        currentQuoteSource = null;
        quoteLerping = false;
        atQuoteTarget = false;
        quoteActive = false;
    }

    private void ResetLastHoveredObject()
    {
        if (lastHoveredObject != null)
        {
            lastHoveredObject.layer = defaultLayer;
            lastHoveredObject = null;
        }
        isHovering = false;
        if (pressE != null) pressE.SetActive(false);
    }

    public void StartDialogue(string title, string subtitle, float? customDuration = null)
    {
        dialogueActive = true;
        dialogueTimer = customDuration ?? dialogueDuration;

        if (aqmControllerScript != null) aqmControllerScript.isTalkingToAQM = true;
        if (playerMovementScript != null) playerMovementScript.enabled = false;
        if (hudUI != null) hudUI.SetActive(false);

        if (titleTextUI != null) titleTextUI.text = title;
        if (subtitleTypewriter != null) subtitleTypewriter.StartTyping(subtitle);
        else if (subtitleTextUI != null) subtitleTextUI.text = subtitle;

        topTarget = topOriginal + new Vector2(0, -moveAmount);
        bottomTarget = bottomOriginal - new Vector2(0, -moveAmount);
    }

    public void EndDialogue()
    {
        dialogueActive = false;

        if (activeObjectIndex >= 0 && activeObjectIndex < objectInfos.Length)
        {
            ObjectInfo objInfo = objectInfos[activeObjectIndex];
            if (objInfo != null && objInfo.audioSource != null) objInfo.audioSource.Stop();
        }

        if (aqmControllerScript != null) aqmControllerScript.isTalkingToAQM = false;
        if (playerMovementScript != null) playerMovementScript.enabled = true;
        if (hudUI != null) hudUI.SetActive(true);

        if (titleTextUI != null) titleTextUI.text = "";
        if (subtitleTypewriter != null) subtitleTypewriter.StopTyping(true);
        else if (subtitleTextUI != null) subtitleTextUI.text = "";

        topTarget = topOriginal;
        bottomTarget = bottomOriginal;

        if (cameraTransform != null) cameraTransform.rotation = originalRotation;

        if (isTutorialDialogue && subtitleTextUI != null)
        {
            var localizeEvent = subtitleTextUI.GetComponent<LocalizeStringEvent>();
            if (localizeEvent != null)
            {
                localizeEvent.StringReference = emptyDialogue;
                localizeEvent.RefreshString();
            }
            isTutorialDialogue = false;
        }

        if (queuedEndDialogue)
        {
            queuedEndDialogue = false;
            string eTitle = endTitle != null ? endTitle.GetLocalizedString() : "";
            string eDesc = endDescription != null ? endDescription.GetLocalizedString() : "";
            StartDialogue(eTitle, eDesc, endDialogueDuration);

            if (endDialogueClip != null)
            {
                if (audioSource != null) audioSource.PlayOneShot(endDialogueClip);
                else AudioSource.PlayClipAtPoint(endDialogueClip, cameraTransform.position);
            }
        }
    }

    public void QueueEndDialogue() => queuedEndDialogue = true;
}