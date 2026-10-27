using UnityEngine;
using TMPro;
using UnityEngine.Localization;

public class DiscoveredObjects : MonoBehaviour
{
    public int discoveredObjects;
    public int quotesAmount;
    private InteractionManager interactionManager;
    public TextMeshProUGUI discoveredObjectsText;
    [SerializeField] private LocalizedString discoveredObjectsLocalizedString;

    void Start()
    {
        interactionManager = GetComponent<InteractionManager>();
        discoveredObjects = 0;

        if (discoveredObjectsText != null)
        {
            discoveredObjectsLocalizedString.StringChanged += UpdateText;
            discoveredObjectsLocalizedString.RefreshString();
        }
    }

    private void OnDestroy()
    {
        if (discoveredObjectsLocalizedString != null)
        {
            discoveredObjectsLocalizedString.StringChanged -= UpdateText;
        }
    }

    public void IncrementDiscoveredObjects()
    {
        discoveredObjects++;
        discoveredObjectsLocalizedString.RefreshString();

        if (discoveredObjects == interactionManager.clickableObjects.Length - 5)
        {
            interactionManager.QueueEndDialogue();
        }
    }

    private void UpdateText(string newValue)
    {
        discoveredObjectsText.text = $"{newValue}: {discoveredObjects} / {interactionManager.clickableObjects.Length - 5}";
    }
}