using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Rob : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [SerializeField] private Dialogue dialogue; // Reference to the Dialogue component
    [SerializeField] private List<string> dialogueTrees; // List of dialogue trees based on game stages
    [SerializeField] private int gameStage; // Current game stage (can be loaded from save data)

    [Header("Floating Sphere Settings")]
    [SerializeField] private GameObject floatingSphere; // Reference to the floating sphere
    [SerializeField] private Transform headTransform; // Reference to the character's head
    [SerializeField] private float floatSpeed = 1f; // Speed of floating
    [SerializeField] private float floatHeight = 0.2f; // Height of floating

    [Header("Interaction Prompt")]
    [SerializeField] private TextMeshProUGUI interactionPrompt; // Reference to the TMP text for interaction
    [SerializeField] private float interactionDistance = 2f; // Distance within which the player can interact

    [Header("Player Attachment")]
    [SerializeField] private Transform playerTransform; // Reference to the player's transform

    private bool isPlayerNearby = false; // Tracks if the player is within interaction range
    private Vector3 initialSpherePosition; // Initial position of the floating sphere

    private void Start()
    {
        // Cache the initial position of the floating sphere
        if (floatingSphere != null)
        {
            initialSpherePosition = floatingSphere.transform.position;
        }

        // Hide the interaction prompt initially
        if (interactionPrompt != null)
        {
            interactionPrompt.gameObject.SetActive(false);
        }

        // If the playerTransform is not assigned, try to find the player by tag
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
    }

    private void Update()
    {
        HandleFloatingSphere();
        CheckPlayerProximity();
        HandleInteraction();
    }

    private void HandleFloatingSphere()
    {
        if (floatingSphere != null && headTransform != null)
        {
            // Make the sphere float up and down above the head
            float newY = initialSpherePosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
            floatingSphere.transform.position = new Vector3(headTransform.position.x, newY, headTransform.position.z);
        }
    }

    private void CheckPlayerProximity()
    {
        if (playerTransform == null) return;

        // Check if the player is within interaction distance
        float distance = Vector3.Distance(playerTransform.position, transform.position);
        isPlayerNearby = distance <= interactionDistance;

        // Show or hide the interaction prompt
        if (interactionPrompt != null)
        {
            interactionPrompt.gameObject.SetActive(isPlayerNearby);
        }
    }

    private void HandleInteraction()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            // Set the dialogue based on the current game stage
            if (dialogue != null && gameStage >= 0 && gameStage < dialogueTrees.Count)
            {
                dialogue.SetDialogue(dialogueTrees[gameStage]);
            }
        }
    }

    // Function to set the game stage (can be called from save data logic)
    public void SetGameStage(int stage)
    {
        gameStage = stage;
    }
}