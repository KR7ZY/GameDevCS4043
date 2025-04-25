using UnityEngine;
using UnityEngine.Audio;

public class Settings : MonoBehaviour
{
    public static Settings Instance { get; private set; }

    private const string MasterVolumeKey = "MasterVolume";
    private const string SfxVolumeKey = "SfxVolume";
    private const string MusicVolumeKey = "MusicVolume";
    private const string MouseSensitivityKey = "MouseSensitivity";

    [SerializeField] private AudioMixer mainMixer;
    [SerializeField] private string masterVolumeParam = "MasterVolume";
    [SerializeField] private string sfxVolumeParam = "SfxVolume";
    [SerializeField] private string musicVolumeParam = "MusicVolume";
    [SerializeField] private float fallbackMasterVolume = 1.0f;

    [SerializeField] private CursorLockMode defaultCursorLockMode = CursorLockMode.Locked;
    [SerializeField] private bool defaultCursorVisible = false;
    [SerializeField] private float mouseSensitivity = 2.0f;

    [SerializeField] private KeyCode temporaryUnlockCursorKey = KeyCode.LeftAlt;
    [SerializeField] private KeyCode additionalUnlockCursorKey = KeyCode.T;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadSettings();
        ApplySettings();
    }

    private void Update()
    {
        HandleCursorState();
    }

    private void LoadSettings()
    {
        float loadedMaster = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        float loadedSfx = PlayerPrefs.GetFloat(SfxVolumeKey, 1f);
        float loadedMusic = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        mouseSensitivity = PlayerPrefs.GetFloat(MouseSensitivityKey, 2.0f);
    }

    private void ApplySettings()
    {
        SetMasterVolume(PlayerPrefs.GetFloat(MasterVolumeKey, 1f));
        SetSfxVolume(PlayerPrefs.GetFloat(SfxVolumeKey, 1f));
        SetMusicVolume(PlayerPrefs.GetFloat(MusicVolumeKey, 1f));
        Cursor.lockState = defaultCursorLockMode;
        Cursor.visible = defaultCursorVisible;
    }

    public void SetMasterVolume(float linearVolume)
    {
        linearVolume = Mathf.Clamp01(linearVolume);
        if (mainMixer != null)
        {
            mainMixer.SetFloat(masterVolumeParam, Mathf.Log10(linearVolume) * 20);
        }
        else
        {
            fallbackMasterVolume = linearVolume;
            AudioListener.volume = fallbackMasterVolume;
        }
        PlayerPrefs.SetFloat(MasterVolumeKey, linearVolume);
    }

    public void SetSfxVolume(float linearVolume)
    {
        linearVolume = Mathf.Clamp01(linearVolume);
        if (mainMixer != null)
        {
            mainMixer.SetFloat(sfxVolumeParam, Mathf.Log10(linearVolume) * 20);
            PlayerPrefs.SetFloat(SfxVolumeKey, linearVolume);
        }
    }

    public void SetMusicVolume(float linearVolume)
    {
        linearVolume = Mathf.Clamp01(linearVolume);
        if (mainMixer != null)
        {
            mainMixer.SetFloat(musicVolumeParam, Mathf.Log10(linearVolume) * 20);
            PlayerPrefs.SetFloat(MusicVolumeKey, linearVolume);
        }
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        mouseSensitivity = Mathf.Clamp(sensitivity, 0.1f, 10f);
        PlayerPrefs.SetFloat(MouseSensitivityKey, mouseSensitivity);
    }

    private void HandleCursorState()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Introduction")
        {
            if (Input.GetKey(temporaryUnlockCursorKey) || Input.GetKey(additionalUnlockCursorKey))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}