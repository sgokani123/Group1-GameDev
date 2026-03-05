using TMPro;
using UnityEngine;

public class LeaderboardRow : MonoBehaviour
{
    public TMP_Text rankText;
    public TMP_Text nameText;
    public TMP_Text scoreText;

    public void Set(int rank, string playerName, int score)
    {
        if (rankText  != null) rankText.text  = rank.ToString();
        if (nameText  != null) nameText.text  = playerName;
        if (scoreText != null) scoreText.text = score.ToString();
    }

    public void SetEmpty(int rank)
    {
        if (rankText  != null) rankText.text  = rank.ToString();
        if (nameText  != null) nameText.text  = "";
        if (scoreText != null) scoreText.text = "";
        Highlight(false);
    }

    /// <summary>Visually highlights this row (e.g. the current player's entry).</summary>
    public void Highlight(bool on)
    {
        Color c = on ? new Color(1f, 0.85f, 0.1f) : Color.white;  // gold vs white
        if (rankText  != null) rankText.color  = c;
        if (nameText  != null) nameText.color  = c;
        if (scoreText != null) scoreText.color = c;
    }
}