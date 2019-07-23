using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public enum InputCode
{
    Forward,
    Back,
    Left,
    Right,
    Jump,
    Run,
    Shoot,
    Aim,
    SwitchPerspective,
    ToggleBumperCar,
    Settings,
    Spectate,
    MiniMapZoomPlus,
    MiniMapZoomMinus,
}

public class InputManager : MonoBehaviour {

    public static InputManager Instance;

    Dictionary<InputCode, KeyCode> buttonKeys;
    public KeybindDialogBox KeybindDialogBox;

    private void OnEnable()
    {
        buttonKeys = new Dictionary<InputCode, KeyCode>();
        
        //Keep the dictionary in order of the UI elements for the KeybindDialogBox.cs to map correctly
        buttonKeys[InputCode.Forward] = KeyCode.W;
        buttonKeys[InputCode.Back] = KeyCode.S;
        buttonKeys[InputCode.Left] = KeyCode.A;
        buttonKeys[InputCode.Right] = KeyCode.D;
        buttonKeys[InputCode.Jump] = KeyCode.Space;
        buttonKeys[InputCode.Run] = KeyCode.LeftShift;
        buttonKeys[InputCode.Shoot] = KeyCode.Mouse0;
        buttonKeys[InputCode.Aim] = KeyCode.Mouse1;
        buttonKeys[InputCode.SwitchPerspective] = KeyCode.F1;
        buttonKeys[InputCode.ToggleBumperCar] = KeyCode.Mouse2;
        buttonKeys[InputCode.Settings] = KeyCode.Escape;
        buttonKeys[InputCode.Spectate] = KeyCode.Tab;
        buttonKeys[InputCode.MiniMapZoomPlus] = KeyCode.Equals;
        buttonKeys[InputCode.MiniMapZoomMinus] = KeyCode.Minus;
    }

    private void Awake()
    {
        Instance = this;
    }

    public bool GetKeyDown(InputCode inputCode)
    {
        //if (!buttonKeys.ContainsKey(inputCode))
        //{
        //    Debug.Log("InputManager::GetButtonDown -- No button Named: " + inputCode);
        //    return false;
        //}

        return Input.GetKeyDown(buttonKeys[inputCode]);
    }

    public bool GetKey(InputCode inputCode)
    {
        //if (!buttonKeys.ContainsKey(inputCode))
        //{
        //    Debug.Log("InputManager::GetButtonDown -- No button Named: " + inputCode);
        //    return false;
        //}

        return Input.GetKey(buttonKeys[inputCode]);
    }

    public InputCode[] GetButtonNames()
    {
        return buttonKeys.Keys.ToArray();
    }

    public void SetButtonForKey(InputCode inputCode, KeyCode keyCode)
    {
        buttonKeys[inputCode] = keyCode;
    }
}
