using UnityEngine;
using UnityEngine.SceneManagement;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public void PlayGame() => SceneManager.LoadScene("Game");
    public void Quit() => Application.Quit();
}
