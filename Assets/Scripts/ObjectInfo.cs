using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
using System.Collections;

public class ObjectInfo : MonoBehaviour
{
    [Header("Localization")]
    public LocalizedString titleText;
    public LocalizedString infoText;

    [Header("UI")]
    public Image iconImage;

    [Header("Audio")]
    public AudioClip interactionSound;
    public AudioClip narratorClip;
    public AudioSource audioSource;

    [Header("References")]
    public DiscoveredObjects discoveredObjects;
    public Transform narratorSpawnPoint;
    public Transform playerSpawnPoint;
    public Transform narrator;
    public Transform player;
    public Transform camTransform;
    public PlayerController playerScript;
    public Animator aqmController;

    public float duration;

    [Header("Camera Settings")]
    public float camLerpSpeed = 2f;

    private Transform playerTransform;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    private void Update()
    {
        if (playerTransform == null || iconImage == null || !iconImage.gameObject.activeSelf) return;

        Vector3 directionToPlayer = playerTransform.position - transform.position;
        directionToPlayer.y = 0f;
        iconImage.transform.rotation = Quaternion.LookRotation(directionToPlayer);
    }

    public void Narrator()
    {
        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.SetPositionAndRotation(playerSpawnPoint.position, playerSpawnPoint.rotation);

        if (cc != null) cc.enabled = true;

        narrator.SetPositionAndRotation(narratorSpawnPoint.position, narratorSpawnPoint.rotation);

        playerScript.isTalkingToAquaman = true;

        if (!audioSource.isPlaying && narratorClip != null)
        {
            aqmController.SetBool("isTalking", true);
            audioSource.clip = narratorClip;
            audioSource.Play();
        }

        StartCoroutine(LerpToNarrator());
    }

    private IEnumerator LerpToNarrator()
    {
        Vector3 directionToNarrator = narrator.position - camTransform.position;
        Quaternion targetRotation = Quaternion.LookRotation(directionToNarrator);

        while (Quaternion.Angle(camTransform.rotation, targetRotation) > 0.1f)
        {
            camTransform.rotation = Quaternion.Lerp(
                camTransform.rotation,
                targetRotation,
                camLerpSpeed * Time.deltaTime
            );
            yield return null;
        }

        camTransform.rotation = targetRotation; 
    }

    public void ShowExclamation()
    {
        if (iconImage != null)
        {
            iconImage.gameObject.SetActive(true);
        }
    }

    public void HideExclamation()
    {
        if (iconImage != null && iconImage.gameObject.activeSelf)
        {
            iconImage.gameObject.SetActive(false);

            if (audioSource != null && interactionSound != null)
            {
                audioSource.PlayOneShot(interactionSound);
            }

            if (discoveredObjects != null)
            {
                discoveredObjects.IncrementDiscoveredObjects();
            }
        }
    }
}
