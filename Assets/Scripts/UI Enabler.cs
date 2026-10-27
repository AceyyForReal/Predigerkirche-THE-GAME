using UnityEngine;

public class UIEnabler : MonoBehaviour
{
    [SerializeField]private GameObject UI;
    [SerializeField]private GameObject DialogueManager;
    
    void Start()
    {
        UI.SetActive(true);
        DialogueManager.SetActive(true);
    }
}