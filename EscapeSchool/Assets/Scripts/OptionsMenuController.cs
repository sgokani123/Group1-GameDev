using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Controls the options UI. When enabled, ensure background is fixed to the camera
// (so the game background looks static) and make text larger for readability.
public class OptionsMenuController : MonoBehaviour
{
    // ── Name ────────────────────────────────────────────────────────────────
    [Header("UI - Name")]
    public TMP_InputField nameInput;
    [Tooltip("Label that shows validation errors in red. Optional.")]
    public TMP_Text nameValidationLabel;

    // ── Sound ────────────────────────────────────────────────────────────────
    [Header("UI - Sound")]
    [Tooltip("The Image component on the Sound button (its sprite will swap).")]
    public Image soundButtonImage;
    [Tooltip("Sprite shown when sound is ON  → assign Botton on.png")]
    public Sprite soundOnSprite;
    [Tooltip("Sprite shown when sound is OFF → assign Botton off.png")]
    public Sprite soundOffSprite;
    // ── Speed slider ──────────────────────────────────────────────────────────
    [Header("UI - Speed")]
    [Tooltip("The Slider (0-100) that controls player move speed.")]
    public UnityEngine.UI.Slider speedSlider;
    [Tooltip("Label showing the current speed value, e.g. '60'.")]
    public TMP_Text speedValueLabel;
    // ── Key Binds ────────────────────────────────────────────────────────────
    [Header("UI - Key Binds")]
    [Tooltip("Text showing the current left-move key.")]
    public TMP_Text leftKeyLabel;
    [Tooltip("Text showing the current right-move key.")]
    public TMP_Text rightKeyLabel;
    [Tooltip("Button that triggers left-key rebind.")]
    public Button rebindLeftButton;
    [Tooltip("Button that triggers right-key rebind.")]
    public Button rebindRightButton;
    [Tooltip("Status label shown while waiting for a key press.")]
    public TMP_Text rebindStatusLabel;

    // ── How To Play ──────────────────────────────────────────────────────────
    [Header("UI - How To Play")]
    [Tooltip("Panel GameObject that holds the How to Play content.")]
    public GameObject howToPlayPanel;

    // ── Appearance ───────────────────────────────────────────────────────────
    [Header("Appearance")]
    [Tooltip("Font size applied to the options UI text when the panel opens.")]
    public float optionFontSize = 36f;

    // ── Internals ────────────────────────────────────────────────────────────
    private Ajust backgroundAjust;
    private bool prevFollowCamera;
    private bool isRebinding = false;

    // ════════════════════════════════════════════════════════════════════════
    //  Unity lifecycle
    // ════════════════════════════════════════════════════════════════════════

    void OnEnable()
    {
        ApplyFontSizes();

        // Fix background to camera while options are open
        backgroundAjust = FindFirstObjectByType<Ajust>();
        if (backgroundAjust != null)
        {
            prevFollowCamera = backgroundAjust.followCamera;
            backgroundAjust.followCamera = true;
        }

        SyncFromSaved();
        RefreshSoundLabel();
        RefreshKeyLabels();
        RefreshSpeedSlider();

        if (howToPlayPanel    != null) howToPlayPanel.SetActive(false);
        if (rebindStatusLabel != null) rebindStatusLabel.gameObject.SetActive(false);
    }

    void OnDisable()
    {
        if (backgroundAjust != null)
            backgroundAjust.followCamera = prevFollowCamera;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Name
    // ════════════════════════════════════════════════════════════════════════

    public void SyncFromSaved()
    {
        string current = LeaderboardManager.GetCurrentPlayerName(); // always at least "Anonymous"

        // Pre-fill the input with the current name so the user can edit it directly
        if (nameInput != null)
            nameInput.SetTextWithoutNotify(current);

        if (nameValidationLabel != null)
            nameValidationLabel.gameObject.SetActive(false);
    }

    /// <summary>Hook this to the Save button or NameInput -> OnEndEdit.</summary>
    public void SaveName()
    {
        if (nameInput == null) return;

        string input = nameInput.text.Trim();
        string error = LeaderboardManager.TrySetCurrentPlayerName(input);

        if (error != null)
        {
            // Show validation error
            if (nameValidationLabel != null)
            {
                nameValidationLabel.text  = error;
                nameValidationLabel.color = new UnityEngine.Color(0.85f, 0.1f, 0.1f);
                nameValidationLabel.gameObject.SetActive(true);
            }
            return;
        }

        SoundManager.Instance.PlaySFX(0);
        SyncFromSaved();
        // Push new name to HUD immediately if game is running
        GameManager.Instance?.RefreshLoggedInLabels();
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Sound toggle
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Hook this to the Sound On/Off button's OnClick.</summary>
    public void ToggleSound()
    {
        bool nowMuted = !SoundManager.Instance.IsMuted;
        SoundManager.Instance.SetMuted(nowMuted);
        PlayerPrefs.SetInt("SoundMuted", nowMuted ? 1 : 0);
        PlayerPrefs.Save();
        RefreshSoundLabel();
    }

    private void RefreshSoundLabel()
    {
        if (soundButtonImage == null) return;
        soundButtonImage.sprite = SoundManager.Instance.IsMuted ? soundOffSprite : soundOnSprite;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Speed slider
    // ════════════════════════════════════════════════════════════════════════

    private void RefreshSpeedSlider()
    {
        if (speedSlider == null) return;
        float saved = PlayerPrefs.GetFloat("PlayerSpeed", 38f);
        // Update slider without triggering OnValueChanged callback
        speedSlider.SetValueWithoutNotify(saved);
        if (speedValueLabel != null) speedValueLabel.text = Mathf.RoundToInt(saved).ToString();
    }

    /// <summary>Hook to the Slider's OnValueChanged event.</summary>
    public void OnSpeedChanged(float value)
    {
        // Always persist — Player.Start() will pick this up when the game begins
        PlayerPrefs.SetFloat("PlayerSpeed", value);
        PlayerPrefs.Save();

        // Also apply immediately if Player is already active in the scene (in-game options)
        // includeInactive:true so we find it even when it's disabled
        Player player = FindAnyObjectByType<Player>(FindObjectsInactive.Include);
        if (player != null) player.SetSpeedFromSlider(value);

        if (speedValueLabel != null) speedValueLabel.text = Mathf.RoundToInt(value).ToString();
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Key binds
    // ════════════════════════════════════════════════════════════════════════

    private void RefreshKeyLabels()
    {
        if (leftKeyLabel  != null) leftKeyLabel.text  = "← Move Left:  " + KeyBindings.LeftKey;
        if (rightKeyLabel != null) rightKeyLabel.text = "→ Move Right: " + KeyBindings.RightKey;
    }

    /// <summary>Hook to the Rebind Left button's OnClick.</summary>
    public void StartRebindLeft()  => StartCoroutine(RebindRoutine("Left"));

    /// <summary>Hook to the Rebind Right button's OnClick.</summary>
    public void StartRebindRight() => StartCoroutine(RebindRoutine("Right"));

    private IEnumerator RebindRoutine(string action)
    {
        if (isRebinding) yield break;
        isRebinding = true;
        SetRebindButtonsInteractable(false);

        if (rebindStatusLabel != null)
        {
            rebindStatusLabel.gameObject.SetActive(true);
            rebindStatusLabel.text = $"Press a key for [{action}]...  (Esc = cancel)";
        }

        // Skip two frames so the button-click key doesn't register immediately.
        yield return null;
        yield return null;

        Key capturedKey = Key.None;
        float elapsed   = 0f;

        while (elapsed < 10f && capturedKey == Key.None)
        {
            elapsed += Time.unscaledDeltaTime;

            if (Keyboard.current != null)
            {
                foreach (Key k in System.Enum.GetValues(typeof(Key)))
                {
                    if (k == Key.None) continue;
                    try
                    {
                        if (Keyboard.current[k].wasPressedThisFrame)
                        {
                            capturedKey = k;
                            break;
                        }
                    }
                    catch { /* key doesn't exist on this keyboard layout */ }
                }
            }

            yield return null;
        }

        // Apply (ignore Escape — treat as cancel)
        if (capturedKey != Key.None && capturedKey != Key.Escape)
        {
            if (action == "Left")  KeyBindings.LeftKey  = capturedKey;
            if (action == "Right") KeyBindings.RightKey = capturedKey;
        }

        RefreshKeyLabels();
        SetRebindButtonsInteractable(true);
        if (rebindStatusLabel != null) rebindStatusLabel.gameObject.SetActive(false);
        isRebinding = false;
    }

    private void SetRebindButtonsInteractable(bool value)
    {
        if (rebindLeftButton  != null) rebindLeftButton.interactable  = value;
        if (rebindRightButton != null) rebindRightButton.interactable = value;
    }

    // ════════════════════════════════════════════════════════════════════════
    //  How To Play
    // ════════════════════════════════════════════════════════════════════════

    /// <summary>Hook to the "How to Play" button's OnClick.</summary>
    public void ShowHowToPlay()
    {
        if (howToPlayPanel != null) howToPlayPanel.SetActive(true);
    }

    /// <summary>Hook to the Close button inside the How to Play panel.</summary>
    public void HideHowToPlay()
    {
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
    }

    // ════════════════════════════════════════════════════════════════════════
    //  Helpers
    // ════════════════════════════════════════════════════════════════════════

    private void ApplyFontSizes()
    {
        if (nameInput != null && nameInput.textComponent != null)
            nameInput.textComponent.fontSize = optionFontSize;

        if (nameInput != null && nameInput.placeholder is TMP_Text placeholderText)
            placeholderText.fontSize = optionFontSize;
    }
}
