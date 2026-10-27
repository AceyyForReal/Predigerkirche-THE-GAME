using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Components;

public class TypewriterEffect : MonoBehaviour
{
    [Header("Localization / Output")]
    public LocalizeStringEvent localizedStringEvent;
    public TMP_Text dialogueText;

    [Header("Timing")]
    public float delay = 0.05f;

    [Header("Overflow")]
    public int maxLines = 2;

    private Coroutine typingCoroutine;

    void Awake()
    {
        if (localizedStringEvent == null)
            localizedStringEvent = GetComponent<LocalizeStringEvent>();

        if (dialogueText == null)
            dialogueText = GetComponent<TMP_Text>();

        if (localizedStringEvent != null)
            localizedStringEvent.OnUpdateString.AddListener(OnLocalizedStringChanged);
    }

    private void OnDestroy()
    {
        if (localizedStringEvent != null)
            localizedStringEvent.OnUpdateString.RemoveListener(OnLocalizedStringChanged);
    }

    private void OnLocalizedStringChanged(string newText)
    {
        StartTyping(newText);
    }

    public void StartTyping(string fullText)
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(fullText));
    }

    public void StopTyping(bool clearText = true)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        if (dialogueText != null && clearText)
            dialogueText.text = "";
    }

    private IEnumerator TypeText(string fullText)
    {
        if (dialogueText == null) yield break;

        dialogueText.text = "";
        StringBuilder sb = new StringBuilder();

        foreach (char c in fullText)
        {
            sb.Append(c);
            dialogueText.text = sb.ToString();

            dialogueText.ForceMeshUpdate();
            int lineCount = dialogueText.textInfo.lineCount;

            if (lineCount > maxLines)
            {
                int firstVisibleCharIndex = dialogueText.textInfo.lineInfo[lineCount - maxLines].firstCharacterIndex;
                sb.Clear();
                for (int i = firstVisibleCharIndex; i < sb.Length; i++)
                {
                    sb.Append(fullText[i]);
                }
                dialogueText.text = sb.ToString();
            }
            yield return new WaitForSeconds(delay);
        }
        typingCoroutine = null;
    }
}