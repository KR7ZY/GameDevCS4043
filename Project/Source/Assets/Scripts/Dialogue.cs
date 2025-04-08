using System.Collections;
using TMPro;
using UnityEngine;

public class Dialogue : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI dialogueText; // Reference to the TextMeshPro object
    [SerializeField] private float letterDelay = 0.05f; // Delay between each letter appearing
    [SerializeField] private float displayDurationMultiplier = 0.5f; // Multiplier to calculate how long the text stays before disappearing

    private Coroutine currentCoroutine;

    // Function to set the dialogue text gradually
    public void SetDialogue(string text)
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine); // Stop any ongoing coroutine
        }

        currentCoroutine = StartCoroutine(DisplayTextGradually(text));
    }

    // Coroutine to display text one letter at a time
    private IEnumerator DisplayTextGradually(string text)
    {
        dialogueText.text = ""; // Clear the text
        dialogueText.gameObject.SetActive(true); // Ensure the text object is visible

        // Display letters one by one
        foreach (char letter in text)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(letterDelay);
        }

        // Wait for a duration based on the text length
        float displayDuration = text.Length * displayDurationMultiplier;
        yield return new WaitForSeconds(displayDuration);

        // Start disappearing letters one by one
        yield return StartCoroutine(HideTextGradually());
    }

    // Coroutine to hide text one letter at a time
    private IEnumerator HideTextGradually()
    {
        string currentText = dialogueText.text;

        for (int i = currentText.Length - 1; i >= 0; i--)
        {
            dialogueText.text = currentText.Substring(0, i);
            yield return new WaitForSeconds(letterDelay);
        }

        dialogueText.gameObject.SetActive(false); // Hide the text object
    }
}