using System;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardManager : MonoBehaviour, IKeyboard
{
    static KeyboardManager instance;
    public static IKeyboard Instance 
    {
        get 
        {
            if (instance == null) 
            {
                instance =  FindObjectOfType(typeof (KeyboardManager)) as KeyboardManager;
                if (instance == null)
                    throw new InvalidOperationException("No instance in scene!");
            }
            return instance;
        }
    }
    void OnApplicationQuit() 
    {
        instance = null;
    }

    readonly Dictionary<KeyCode, ComplexButtonState> keyStates = new Dictionary<KeyCode, ComplexButtonState>(KeyCodeEqualityComparer.Default);

    readonly List<KeyCode> down = new List<KeyCode>();
    readonly List<KeyCode> released = new List<KeyCode>();
    readonly List<KeyCode> pressed = new List<KeyCode>();

    readonly List<KeyCode> registeredKeys = new List<KeyCode>();

    public void RegisterKey(KeyCode key)
    {
        if (!registeredKeys.Contains(key))
            registeredKeys.Add(key);
    }

    public ComplexButtonState GetKeyState(KeyCode key)
    {
        ComplexButtonState state;
        if (!keyStates.TryGetValue(key, out state))
            state = ComplexButtonState.Up;

        return state;
    }

    public IEnumerable<KeyCode> Down { get { return down; } }
    public IEnumerable<KeyCode> Released { get { return released; } }
    public IEnumerable<KeyCode> Pressed { get { return pressed; } }

    readonly List<KeyCode> newlyPressed = new List<KeyCode>();
    readonly List<KeyCode> werePressedOrDown = new List<KeyCode>();

    void Update()
    {
        foreach (var key in released)
            keyStates.Remove(key);
        released.Clear();

        newlyPressed.Clear();
        foreach (var key in registeredKeys)
            if (Input.GetKey(key))
                newlyPressed.Add(key);

        werePressedOrDown.Clear();
        werePressedOrDown.AddRange(pressed);
        werePressedOrDown.AddRange(down);

        foreach (var key in newlyPressed)
        {
            ComplexButtonState formerState;
            if (keyStates.TryGetValue(key, out formerState))
            {
                if (formerState == ComplexButtonState.Pressed)
                {
                    keyStates.Remove(key);
                    pressed.Remove(key);
                    released.Remove(key);
                    down.Add(key);
                    keyStates.Add(key, ComplexButtonState.Down);
                }
            }
            else
            {
                pressed.Add(key);
                keyStates.Add(key, ComplexButtonState.Pressed);
            }
        }

        if (werePressedOrDown.Count > 0 && newlyPressed.Count > 0)
            newlyPressed.Sort(KeyCodeComparer.Default);

        foreach (var key in werePressedOrDown)
            if (newlyPressed.BinarySearch(key, KeyCodeComparer.Default) == -1)
            {
                pressed.Remove(key);
                down.Remove(key);
                keyStates.Remove(key);
                released.Add(key);
                keyStates.Add(key, ComplexButtonState.Released);
            }
    }
}

public interface IKeyboard
{
    void RegisterKey(KeyCode key);

    ComplexButtonState GetKeyState(KeyCode key);

    IEnumerable<KeyCode> Down { get; }
    IEnumerable<KeyCode> Released { get; }
    IEnumerable<KeyCode> Pressed { get; }
}

public class KeyCodeEqualityComparer : IEqualityComparer<KeyCode>
{
    public static readonly KeyCodeEqualityComparer Default = new KeyCodeEqualityComparer();
    public bool Equals(KeyCode x, KeyCode y) { return x == y; }
    public int GetHashCode(KeyCode obj) { return (int)obj; }
}
public class KeyCodeComparer : IComparer<KeyCode>
{
    public static readonly KeyCodeComparer Default = new KeyCodeComparer();
    public int Compare(KeyCode x, KeyCode y)
    {
        if (x < y) return -1;
        if (x > y) return 1;
        return 0;
    }
}