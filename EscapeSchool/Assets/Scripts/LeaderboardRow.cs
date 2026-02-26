using TMPro;
using UnityEngine;

public class LeaderboardRow : MonoBehaviour
{
    public TMP_Text rankText;
    public TMP_Text nameText;
    public TMP_Text scoreText;

    public void Set(int rank, string name, int score)
    {
        if (rankText != null) rankText.text = rank.ToString();
        if (nameText != null) nameText.text = name;
        if (scoreText != null) scoreText.text = score.ToString();
    }
}