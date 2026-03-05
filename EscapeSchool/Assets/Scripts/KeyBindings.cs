using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Stores the player's custom key bindings using PlayerPrefs.
/// Arrow keys (left/right) are the remappable primary keys.
/// A/D act as fixed secondary keys that always work.
/// </summary>
public static class KeyBindings
{
    private const Key DefaultLeft  = Key.LeftArrow;
    private const Key DefaultRight = Key.RightArrow;

    public static Key LeftKey
    {
        get => (Key)PlayerPrefs.GetInt("KeyLeft",  (int)DefaultLeft);
        set { PlayerPrefs.SetInt("KeyLeft",  (int)value); PlayerPrefs.Save(); }
    }

    public static Key RightKey
    {
        get => (Key)PlayerPrefs.GetInt("KeyRight", (int)DefaultRight);
        set { PlayerPrefs.SetInt("KeyRight", (int)value); PlayerPrefs.Save(); }
    }

    // Fixed secondary keys (always active, cannot be rebound)
    private static readonly Key LeftAlt  = Key.A;
    private static readonly Key RightAlt = Key.D;

    public static bool IsLeftPressed()
    {
        var kb = Keyboard.current;
        if (kb == null) return false;
        bool primary = false;
        try { primary = kb[LeftKey].isPressed; } catch { }
        return primary || kb[LeftAlt].isPressed;
    }

    public static bool IsRightPressed()
    {
        var kb = Keyboard.current;
        if (kb == null) return false;
        bool primary = false;
        try { primary = kb[RightKey].isPressed; } catch { }
        return primary || kb[RightAlt].isPressed;
    }
}
