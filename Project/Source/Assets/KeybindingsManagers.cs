using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class KeybindingsManager : MonoBehaviour
{
    public Text forwardKey;
    public Text backwardKey;
    public Text leftKey;
    public Text rightKey;

    private Dictionary<string, KeyCode> keys = new Dictionary<string, KeyCode>();

    void Start()
    {
        keys["Forward"] = KeyCode.W;
        keys["Backward"] = KeyCode.S;
        keys["Left"] = KeyCode.A;
        keys["Right"] = KeyCode.D;

        forwardKey.text = keys["Forward"].ToString();
        backwardKey.text = keys["Backward"].ToString();
        leftKey.text = keys["Left"].ToString();
        rightKey.text = keys["Right"].ToString();
    }

    public void StartRebinding(string key) => StartCoroutine(WaitForKeyPress(key));

    private IEnumerator WaitForKeyPress(string key)
    {
        while (!Input.anyKeyDown)
        {
            yield return null;
        }

        foreach (KeyCode newKey in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(newKey))
            {
                keys[key] = newKey;
                UpdateKeyText(key);
                break;
            }
        }
    }

    private void UpdateKeyText(string key)
    {
        switch (key)
        {
            case "Forward":
                forwardKey.text = keys["Forward"].ToString();
                break;
            case "Backward":
                backwardKey.text = keys["Backward"].ToString();
                break;
            case "Left":
                leftKey.text = keys["Left"].ToString();
                break;
            case "Right":
                rightKey.text = keys["Right"].ToString();
                break;
        }
    }
}
