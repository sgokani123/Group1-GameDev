using TMPro;
using UnityEngine;

public class OptionsMenuController : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField nameInput;
    public TMP_Text currentNameLabel;


    void OnEnable()
    {
        SyncFromSaved();
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