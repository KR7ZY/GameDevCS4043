using UnityEngine;
using UnityEngine.UI; // Required if you link directly to UI components like Sliders in the Inspector
using UnityEngine.Audio; // Required if you use AudioMixers (recommended for better audio control)

public class Settings : MonoBehaviour
{
    // --- Singleton Pattern ---
    // Makes the Settings instance easily accessible from other scripts via Settings.Instance
    public static Settings Instance { get; private set; }

    // --- Constants for PlayerPrefs Keys ---
    // Using constants avoids typos when saving/loading
    private const string MasterVolumeKey = "MasterVolume";
    private const string SfxVolumeKey = "SfxVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string MouseSensitivityKey = "MouseSensitivity";
    // Add more keys for other settings as needed

    [Header("Audio Settings")]
    // If using AudioMixer (recommended)
    [SerializeField] private AudioMixer mainMixer; // Assign your main AudioMixer asset here
    [SerializeField] private string masterVolumeParam = "MasterVolume"; // MUST match the Exposed Parameter name in the AudioMixer
    [SerializeField] private string sfxVolumeParam = "SfxVolume";       // MUST match the Exposed Parameter name in the AudioMixer
    [SerializeField] private string musicVolumeParam = "MusicVolume";   // MUST match the Exposed Parameter name in the AudioMixer

    // Fallback if not using AudioMixer (controls global listener volume)
    [Range(0.0001f, 1f)] // Use Range for a slider in the inspector, avoid 0 for logarithmic conversion
    [SerializeField] private float fallbackMasterVolume = 1.0f;

    [Header("Mouse Settings")]
    [SerializeField] private CursorLockMode defaultCursorLockMode = CursorLockMode.Locked;
    [SerializeField] private bool defaultCursorVisible = false;
    [Range(0.1f, 10f)]
    [SerializeField] private float mouseSensitivity = 2.0f;

    [Header("Keybindings")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    [SerializeField] private KeyCode inventoryKey = KeyCode.I;
    [SerializeField] private KeyCode temporaryUnlockCursorKey = KeyCode.LeftAlt; // Key to temporarily show cursor

    // --- Public Properties (Read-only access) ---
    // Other scripts can read these values if needed
    public float MouseSensitivity => mouseSensitivity;
    public KeyCode InteractionKey => interactionKey;
    public KeyCode InventoryKey => inventoryKey;

    // --- Unity Lifecycle Methods ---
    private void Awake()
    {
        // Implement Singleton Pattern
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Duplicate Settings instance found. Destroying new one.");
            Destroy(gameObject);
            return; // Stop execution here
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Keep the settings manager alive across scene changes

        LoadSettings(); // Load saved settings from PlayerPrefs
        ApplySettings(); // Apply the loaded (or default) settings
    }

    private void Update()
    {
        // Handle temporary cursor unlock/lock
        HandleCursorState();

        // Example: Check for keybinding presses (other scripts would typically do this)
        // CheckForExampleKeyPresses(); // You might move input checks to relevant player/UI scripts
    }

    // --- Settings Loading and Saving ---
    private void LoadSettings()
    {
        Debug.Log("Loading settings...");

        // Load Audio Settings
        // For AudioMixer, load the slider value (linear), default to 1 (max)
        float loadedMaster = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        float loadedSfx = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
        float loadedMusic = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        // Store these loaded linear values to potentially set UI sliders correctly
        // We'll apply them logarithmically in ApplySettings()

        // Load Mouse Sensitivity
        mouseSensitivity = PlayerPrefs.GetFloat(MouseSensitivityKey, 2.0f); // Default to 2.0f

        // Load Keybindings (Example - could use PlayerPrefs.GetString and KeyCode.Parse)
        // For simplicity, we're currently using Inspector values, but you could save/load key strings.
        // interactionKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("InteractionKey", KeyCode.E.ToString()));
        // inventoryKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("InventoryKey", KeyCode.I.ToString()));

        Debug.Log($"Loaded Settings: MasterVol={loadedMaster}, SfxVol={loadedSfx}, MusicVol={loadedMusic}, MouseSens={mouseSensitivity}");
    }

    private void ApplySettings()
    {
        Debug.Log("Applying settings...");

        // Apply Audio Settings
        // Important: AudioMixer volumes are typically logarithmic (dB). A linear slider value (0-1)
        // needs conversion. A common formula is: dB = log10(linearValue) * 20
        // Set the initial values based on loaded data
        SetMasterVolume(PlayerPrefs.GetFloat(MasterVolumeKey, 1f));
        SetSfxVolume(PlayerPrefs.GetFloat(SfxVolumeKey, 1f));
        SetMusicVolume(PlayerPrefs.GetFloat(MusicVolumeKey, 1f));

        // Apply Mouse Sensitivity (other scripts like player controllers would read Instance.MouseSensitivity)

        // Apply Initial Cursor State (based on defaults, not the temp key)
        Cursor.lockState = defaultCursorLockMode;
        Cursor.visible = defaultCursorVisible;

        Debug.Log("Settings Applied.");
    }

    private void SaveSettings()
    {
        // PlayerPrefs.Save() is called automatically on quit, but can be called manually if needed.
        Debug.Log("Settings saved (implicitly by PlayerPrefs on modification/quit).");
    }


    // --- Public Functions Callable from UI (or other scripts) ---

    // Set Master Volume (expects a value from 0.0 to 1.0, typically from a UI Slider)
    public void SetMasterVolume(float linearVolume)
    {
        linearVolume = Mathf.Clamp01(linearVolume); // Ensure value is between 0 and 1

        if (mainMixer != null)
        {
            // Convert linear volume (0-1) to dB for the AudioMixer (-80dB to 0dB)
            mainMixer.SetFloat(masterVolumeParam, Mathf.Log10(linearVolume) * 20);
        }
        else
        {
            // Fallback: Control the global AudioListener volume (linear)
            fallbackMasterVolume = linearVolume;
            AudioListener.volume = fallbackMasterVolume;
        }
        // Save the linear value (easier for sliders)
        PlayerPrefs.SetFloat(MasterVolumeKey, linearVolume);
        // SaveSettings(); // Optional: save immediately
    }

    // Set SFX Volume
    public void SetSfxVolume(float linearVolume)
    {
        linearVolume = Mathf.Clamp01(linearVolume);
        if (mainMixer != null)
        {
            mainMixer.SetFloat(sfxVolumeParam, Mathf.Log10(linearVolume) * 20);
            PlayerPrefs.SetFloat(SfxVolumeKey, linearVolume);
            // SaveSettings();
        } else {
             Debug.LogWarning("SFX Volume control requires an AudioMixer assigned.");
        }
    }

    // Set Music Volume
    public void SetMusicVolume(float linearVolume)
    {
        linearVolume = Mathf.Clamp01(linearVolume);
        if (mainMixer != null)
        {
            mainMixer.SetFloat(musicVolumeParam, Mathf.Log10(linearVolume) * 20);
            PlayerPrefs.SetFloat(MusicVolumeKey, linearVolume);
            // SaveSettings();
        } else {
             Debug.LogWarning("Music Volume control requires an AudioMixer assigned.");
        }
    }

    // Set Mouse Sensitivity
    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = Mathf.Clamp(sensitivity, 0.1f, 10f); // Clamp to reasonable limits
        PlayerPrefs.SetFloat(MouseSensitivityKey, mouseSensitivity);
        // SaveSettings();
    }

    // --- Internal Logic ---

    private void HandleCursorState()
    {
        // If the temporary unlock key is held down
        if (Input.GetKey(temporaryUnlockCursorKey))
        {
            // Unlock and show the cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            // Revert to the default lock state and visibility
            // This ensures that opening/closing menus doesn't fight with this temp key
            if (Cursor.lockState != defaultCursorLockMode || Cursor.visible != defaultCursorVisible)
            {
                 // Only apply if it's different, prevents spamming console in some cases
                 // But also check if a menu *might* want the cursor unlocked (more complex state needed)
                 // For now, this assumes Alt overrides everything else.
                Cursor.lockState = defaultCursorLockMode;
                Cursor.visible = defaultCursorVisible;
            }
        }
    }

    // Example function to show how other scripts might check keys
    private void CheckForExampleKeyPresses()
    {
        if (Input.GetKeyDown(interactionKey))
        {
            Debug.Log("Interaction Key Pressed!");
            // Send message, call function, etc.
        }
        if (Input.GetKeyDown(inventoryKey))
        {
            Debug.Log("Inventory Key Pressed!");
            // Toggle inventory UI, etc.
        }
    }

     // --- Getters for UI Initialization ---
     // These help set the initial state of UI elements (like sliders) when the settings menu opens.

    public float GetMasterVolumeLinear()
    {
        return PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
    }

    public float GetSfxVolumeLinear()
    {
        return PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
    }

     public float GetMusicVolumeLinear()
    {
        return PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
    }

     public float GetMouseSensitivity()
    {
        return PlayerPrefs.GetFloat(MouseSensitivityKey, 2.0f);
    }


    // Optional: Reset Settings to Default
    public void ResetSettingsToDefault()
    {
        Debug.Log("Resetting settings to default...");
        PlayerPrefs.DeleteAll(); // Deletes ALL player prefs for this project! Use with caution.
                                 // Or delete specific keys:
                                 // PlayerPrefs.DeleteKey(MasterVolumeKey);
                                 // ... etc.

        // Reload default values from Inspector/code defaults (or re-apply hardcoded defaults)
        // For simplicity, let's just reload the scene or restart the game after reset usually
        // Or manually reset variables and apply:
        mouseSensitivity = 2.0f;
        // ... reset other variables ...

        LoadSettings(); // Reload (will now get defaults)
        ApplySettings(); // Apply the defaults

        // You might need to manually update UI elements (sliders etc.) after reset
        // FindObjectOfType<YourSettingsUI>()?.UpdateUIFields(); // Example
    }


    // Note: The original OnApplicationQuit is usually not necessary,
    // as Unity handles quitting. PlayerPrefs are also saved automatically on quit.
    // private void OnApplicationQuit()
    // {
    //     // Ensure the application quits completely (usually redundant)
    //     // Application.Quit();
    //     // PlayerPrefs.Save(); // Usually redundant
    // }
}