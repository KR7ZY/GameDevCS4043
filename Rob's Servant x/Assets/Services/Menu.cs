using System.Collections; // Required for IEnumerator
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] private Animator animator; // Reference to the Animator component

    public void NewGame() { StartCoroutine(LoadSceneWithFade("Introduction")); }

    public void Continue() { StartCoroutine(LoadSceneWithFade("LastSavedScene")); }

    public void Settings() { StartCoroutine(LoadSceneWithFade("Settings")); }

    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        animator.SetTrigger("Fade"); // Trigger the fade animation
        yield return new WaitForSeconds(1.5f); // Wait for 1 second (fade duration)
        SceneManager.LoadScene(sceneName); // Load the specified scene
    }
}