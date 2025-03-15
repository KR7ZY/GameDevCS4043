using UnityEngine;

public class TransitionScene : MonoBehaviour
{
    public Animator animator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TransitionSceneIndex(1);
        }
    }

    public void TransitionSceneIndex (int SceneIndex)
    {
        animator.SetTrigger("FadeOut");
    }
}
