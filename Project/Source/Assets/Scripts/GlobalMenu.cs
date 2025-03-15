using UnityEngine;

public class GlobalMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        AudioSource Audio = GetComponent<AudioSource>();
        Audio.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
