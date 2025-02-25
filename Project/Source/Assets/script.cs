using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeybindingsManager : MonoBehaviour
{
    public Dictionary<string, KeyCode> keyBindings = new Dictionary<string, KeyCode>();
    public Text forwardKey, backwardKey, leftKey, rightKey;

    private string keyToRebind = "";

    void Start()
    {
        LoadDefaultKeys();
        UpdateUI();
    }

    void LoadDefaultKeys()
    {
        keyBindings["Forward"] = KeyCode.W;
        keyBindings["Backward"] = KeyCode.S;
        keyBindings["Left"] = KeyCode.A;
        keyBindings["Right"] = KeyCode.D;
    }

    void UpdateUI()
    {
        forwardKey.text = keyBindings["Forward"].ToString();
        backwardKey.text = keyBindings["Backward"].ToString();
        leftKey.text = keyBindings["Left"].ToString();
        rightKey.text = keyBindings["Right"].ToString();
    }

    public void StartRebinding(string key)
    {
        keyToRebind = key;
    }

    void Update()
    {
        if (!string.IsNullOrEmpty(keyToRebind))
        {
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    keyBindings[keyToRebind] = key;
                    keyToRebind = "";
                    UpdateUI();
                    break;
                }
            }
        }
    }
}
