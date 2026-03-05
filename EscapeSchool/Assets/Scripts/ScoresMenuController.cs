using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoresMenuController : MonoBehaviour
{
    [Header("UI")]
    public Transform contentParent;          // ScrollView/Viewport/Content
    public LeaderboardRow rowPrefab;         // prefab with LeaderboardRow
    public TMP_Text emptyLabel;              // optional: "No scores yet"
    public int maxRows = 10;                 // top N to display

    void OnEnable() { Refresh(); }

    public void Refresh()
    {
        if (contentParent == null || rowPrefab == null) return;

        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Object.Destroy(contentParent.GetChild(i).gameObject);

        List<(string name, int score)> data = LeaderboardManager.GetLeaderboardSorted();
        string currentPlayer = LeaderboardManager.GetCurrentPlayerName();

        // Always spawn exactly maxRows rows so the panel looks full
        for (int i = 0; i < maxRows; i++)
        {
            var row = Object.Instantiate(rowPrefab, contentParent);
            if (i < data.Count)
            {
                row.Set(i + 1, data[i].name, data[i].score);
                row.Highlight(data[i].name == currentPlayer);
            }
            else
            {
                row.SetEmpty(i + 1); // rank shown, name/score blank
            }
        }
    }
}