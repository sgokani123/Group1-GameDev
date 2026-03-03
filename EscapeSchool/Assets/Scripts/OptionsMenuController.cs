using TMPro;
using UnityEngine;

// Controls the options UI. When enabled, ensure background is fixed to the camera
// (so the game background looks static) and make text larger for readability.
public class OptionsMenuController : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField nameInput;
    public TMP_Text currentNameLabel;

    [Header("Appearance")]
    [Tooltip("Font size applied to the options UI text when the panel opens.")]
    public float optionFontSize = 36f;

    // Background ajust component (if present in the scene)
    private Ajust backgroundAjust;
    private bool prevFollowCamera;

    void OnEnable()
    {
        // Make text larger
        ApplyFontSizes();

        // Fix background to camera while options are open
        backgroundAjust = FindObjectOfType<Ajust>();
        if (backgroundAjust != null)
        {
            prevFollowCamera = backgroundAjust.followCamera;
            backgroundAjust.followCamera = true;
        }

        SyncFromSaved();
    }

    void OnDisable()
    {
        // Restore background behavior
        if (backgroundAjust != null)
            backgroundAjust.followCamera = prevFollowCamera;
    }

    void ApplyFontSizes()
    {
        if (currentNameLabel != null)
            currentNameLabel.fontSize = optionFontSize;

        if (nameInput != null && nameInput.textComponent != null)
            nameInput.textComponent.fontSize = optionFontSize;

        // Try to set placeholder font size if it's a TMP_Text
        if (nameInput != null && nameInput.placeholder is TMP_Text placeholderText)
            placeholderText.fontSize = optionFontSize;
    }

    public void SyncFromSaved()
    {
        string current = LeaderboardManager.GetCurrentPlayerName();

        if (nameInput != null)
            nameInput.SetTextWithoutNotify(current);

        if (currentNameLabel != null)
            currentNameLabel.text = string.IsNullOrWhiteSpace(current)
                ? "Current: (not set)"
                : "Current: " + current;
    }

    // Hook this to: NameInput -> OnEndEdit (recommended) OR a Save button
    public void SaveName()
    {
        if (nameInput == null) return;
        SoundManager.Instance.PlaySFX(0); //  click sound

        LeaderboardManager.SetCurrentPlayerName(nameInput.text);
        SyncFromSaved();
    }
}