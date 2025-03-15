using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void PlayGame() => SceneManager.LoadScene("Game");

    public void Quit() => Application.Quit();

    public void Settings()
    {
        Application.Quit();
        Debug.Log("Game is quitting");
    }
}
