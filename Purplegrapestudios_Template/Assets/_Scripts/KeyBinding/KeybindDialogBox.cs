using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeybindDialogBox : MonoBehaviour {

    InputCode _keyToRebind;
    Dictionary <InputCode, Text> buttonToLabel;
    public bool wishToBind;
    public bool hasAppliedSettings;
    int bindCount = 0;

    public GameObject [] keyButtons;
    IEnumerator UpdateKeyBindCoroutine;


    // Use this for initialization
    void Start () {
        buttonToLabel = new Dictionary<InputCode, Text>();

        InputCode[] buttonNames = InputManager.Instance.GetButtonNames();
        int buttonIndex = 0;

        foreach (GameObject key in keyButtons)
        {
            InputCode inputCodeToRebind = buttonNames[buttonIndex];
            key.name = buttonNames[buttonIndex].ToString();
            buttonToLabel[buttonNames[buttonIndex]] = key.GetComponentInChildren<Text>();
            key.GetComponent<Button>().onClick.AddListener(() => { StartRebindFor(inputCodeToRebind); });
            buttonIndex++;
        }
    }
	
	// Update is called once per frame
	IEnumerator UpdateKeyBind_CO () {

        while (wishToBind)
        {
            if (_keyToRebind != null)
            {
                if (Input.anyKeyDown)
                {
                    //Which key was pressed down?

                    //Loop through all possible keys and see if it was pressed down
                    KeyCode[] keyCodes = (KeyCode[])Enum.GetValues(typeof(KeyCode));

                    foreach (KeyCode kc in keyCodes)
                    {
                        if (Input.GetKeyDown(kc))
                        {
                            //Rebind the key here
                            bindCount = 1;
                            if (bindCount == 1 && kc == KeyCode.Escape) break;
                            if (_keyToRebind == InputCode.Settings && kc == KeyCode.Mouse0) break;

                            InputManager.Instance.SetButtonForKey(_keyToRebind, kc);
                            buttonToLabel[_keyToRebind].text = kc.ToString();
                            wishToBind = false;
                            break;
                        }
                    }
                }
            }
            yield return null;
        }
        yield break;

	}

    void StartRebindFor(InputCode buttonName)
    {
        //Debug.Log(buttonName);
        _keyToRebind = buttonName;
    }

    public void OnClick_SetBind()
    {
        wishToBind = true;

        if (UpdateKeyBindCoroutine != null) {
            StopCoroutine(UpdateKeyBindCoroutine);
        }
        UpdateKeyBindCoroutine = (UpdateKeyBind_CO());
        StartCoroutine(UpdateKeyBindCoroutine);
    }

}
