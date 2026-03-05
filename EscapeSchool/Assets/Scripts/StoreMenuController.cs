using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Carousel-style store. 
/// In the Inspector wire up two Image components (previews) and the sprite arrays.
/// Hook the Prev/Next buttons directly to the methods below via onClick.
/// </summary>
public class StoreMenuController : MonoBehaviour
{
    [Header("Rocket options (fly1 ... fly7)")]
    public Sprite[]  rocketSprites;
    /// <summary>
    /// Attachment offset for each rocket when parented to the player.
    /// X/Y in local player space. Z should stay small (e.g. 0.1).
    /// Examples: balloon = (0, 1.5, 0.1)  hat = (0, 0.6, 0.1)  shoe = (0, -0.5, 0.1)
    /// </summary>
    public Vector3[] rocketOffsets;  // same length as rocketSprites

    [Header("Character options (Student1, 2, 3)")]
    public Sprite[] characterSprites;

    [Header("UI - Rocket Carousel")]
    public Image  rocketPreviewImage;   // large Image showing the current rocket
    public Button rocketPrevButton;
    public Button rocketNextButton;

    [Header("UI - Character Carousel")]
    public Image  charPreviewImage;     // large Image showing the current character
    public Button charPrevButton;
    public Button charNextButton;

    private int _rocketIndex;
    private int _charIndex;

    void OnEnable()
    {
        _rocketIndex = StoreManager.GetRocketIndex();
        _charIndex   = StoreManager.GetCharacterIndex();
        RefreshRocket();
        RefreshChar();
    }

    // ── Rocket ───────────────────────────────────────────────────────────────
    public void RocketPrev()
    {
        if (rocketSprites == null || rocketSprites.Length == 0) return;
        _rocketIndex = (_rocketIndex - 1 + rocketSprites.Length) % rocketSprites.Length;
        StoreManager.SetRocketIndex(_rocketIndex);
        RefreshRocket();
    }

    public void RocketNext()
    {
        if (rocketSprites == null || rocketSprites.Length == 0) return;
        _rocketIndex = (_rocketIndex + 1) % rocketSprites.Length;
        StoreManager.SetRocketIndex(_rocketIndex);
        RefreshRocket();
    }

    void RefreshRocket()
    {
        if (rocketPreviewImage == null || rocketSprites == null) return;
        if (_rocketIndex < rocketSprites.Length)
            rocketPreviewImage.sprite = rocketSprites[_rocketIndex];
        rocketPreviewImage.preserveAspect = true;
    }

    // ── Character ────────────────────────────────────────────────────────────
    public void CharPrev()
    {
        if (characterSprites == null || characterSprites.Length == 0) return;
        _charIndex = (_charIndex - 1 + characterSprites.Length) % characterSprites.Length;
        StoreManager.SetCharacterIndex(_charIndex);
        RefreshChar();
        GameManager.Instance?.ApplyCharacterSkin();
    }

    public void CharNext()
    {
        if (characterSprites == null || characterSprites.Length == 0) return;
        _charIndex = (_charIndex + 1) % characterSprites.Length;
        StoreManager.SetCharacterIndex(_charIndex);
        RefreshChar();
        GameManager.Instance?.ApplyCharacterSkin();
    }

    void RefreshChar()
    {
        if (charPreviewImage == null || characterSprites == null) return;
        if (_charIndex < characterSprites.Length)
            charPreviewImage.sprite = characterSprites[_charIndex];
        charPreviewImage.preserveAspect = true;
    }

    // ── Accessors used by GameManager and Rocket ──────────────────────────────
    public Sprite GetSelectedRocketSprite()
    {
        int i = StoreManager.GetRocketIndex();
        if (rocketSprites == null || i >= rocketSprites.Length) return null;
        return rocketSprites[i];
    }

    public Vector3 GetSelectedRocketOffset(Vector3 fallback)
    {
        int i = StoreManager.GetRocketIndex();
        if (rocketOffsets == null || i >= rocketOffsets.Length) return fallback;
        return rocketOffsets[i];
    }

    public Sprite GetSelectedCharacterSprite()
    {
        int i = StoreManager.GetCharacterIndex();
        if (characterSprites == null || i >= characterSprites.Length) return null;
        return characterSprites[i];
    }
}