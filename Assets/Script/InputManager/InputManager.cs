using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class InputManager : InputManagerClasses
{
    /*
    public static InputManager instance { get; protected set; }

    private void Awake()
    { instance = this; }
    */
    


    static Dictionary<string, InputKey> _inputKeys = new();
    static Dictionary<string, InputDirection> _inputDirections = new();
    static Dictionary<string, InputVector> _inputVectors = new();

    public static Dictionary<string, InputKey> inputKeys { get => _inputKeys; }
    public static Dictionary<string, InputDirection> inputDirections { get => _inputDirections; }
    public static Dictionary<string, InputVector> inputVectors { get => _inputVectors; }


    public static void UpdateInputs()
    {
        foreach (KeyValuePair<string, InputKey> kvp in _inputKeys)
        { kvp.Value.Update(); }
    }


    public static T GetInputKey<T>(string name) where T : InputKey
    { return (T)_inputKeys[name]; }
    public static InputDirection GetInputDirection(string name)
    { return _inputDirections[name]; }
    public static T GetInputVector<T>(string name) where T : InputVector 
    { return (T)_inputVectors[name]; }

    public static bool TryGetInputKey<T>(string name, out T inputKey) where T : InputKey
    {
        if (_inputKeys.ContainsKey(name))
        {
            inputKey = (T)_inputKeys[name];
            return true;
        }
        inputKey = default;
        return false;
    }
    public static bool TryGetInputDirection(string name, out InputDirection inputDirection)
    { return _inputDirections.TryGetValue(name, out inputDirection); }
    public static bool TryGetInputVector<T>(string name, out T inputKey) where T : InputVector
    {
        if (_inputVectors.ContainsKey(name))
        {
            inputKey = (T)_inputVectors[name];
            return true;
        }
        inputKey = default;
        return false;
    }


    // SET KEYS AND BUTTONS AND ALL THE STUFF
    // input keys and buttons
    public static InputKeyCode SetInputKey(string name, KeyCode keycode)
    {
        if (_inputKeys.ContainsKey(name))
        { _inputKeys[name] = new InputKeyCode(name, keycode); }
        else
        { _inputKeys.Add(name, new InputKeyCode(name, keycode)); }
        return (InputKeyCode)_inputKeys[name];
    }
    public static InputButton SetInputButton(string name, string button)
    {
        if (_inputKeys.ContainsKey(name))
        { _inputKeys[name] = new InputButton(name, button); }
        else
        { _inputKeys.Add(name, new InputButton(name, button)); }
        return (InputButton)_inputKeys[name];
    }

    // input directions
    public static InputDirection SetInputDirection(string name, InputKey positive, InputKey negative)
    {
        if (_inputDirections.ContainsKey(name))
        { _inputDirections[name] = new InputDirection(name, positive, negative); }
        else
        { _inputDirections.Add(name, new InputDirection(name, positive, negative)); }
        return _inputDirections[name];
    }

    // input vectors
    public static InputVector SetInputVector(string name, params InputDirection[] input_directions)
    {
        if (_inputVectors.ContainsKey(name))
        { _inputVectors[name] = new InputVector(name, input_directions); }
        else
        { _inputVectors.Add(name, new InputVector(name, input_directions)); }
        return _inputVectors[name];
    }
    public static InputVector2 SetInputVector2(string name, InputDirection x, InputDirection y)
    {
        if (_inputVectors.ContainsKey(name))
        { _inputVectors[name] = new InputVector2(name, x, y); }
        else
        { _inputVectors.Add(name, new InputVector2(name, x, y)); }
        return (InputVector2)_inputVectors[name];
    }
    public static InputVector3 SetInputVector3(string name, InputDirection x, InputDirection y, InputDirection z)
    {
        if (_inputVectors.ContainsKey(name))
        { _inputVectors[name] = new InputVector3(name, x, y, z); }
        else
        { _inputVectors.Add(name, new InputVector3(name, x, y, z)); }
        return (InputVector3)_inputVectors[name];
    }









}
