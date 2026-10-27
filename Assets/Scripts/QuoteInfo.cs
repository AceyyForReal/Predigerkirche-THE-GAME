using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class QuoteInfo : MonoBehaviour
{
    public Image iconImage;
    public DiscoveredObjects discoveredObjects;
    public Transform lookTarget;
    public System.Action OnQuoteRead;
    
    private Transform playerTransform;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }

    void Update()
    {
        if (playerTransform == null || iconImage == null || !iconImage.gameObject.activeSelf) return;

        Vector3 directionToPlayer = playerTransform.position - transform.position;
        directionToPlayer.y = 0;
        iconImage.transform.rotation = Quaternion.LookRotation(directionToPlayer);
    }
}