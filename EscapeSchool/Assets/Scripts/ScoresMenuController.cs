using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoresMenuController : MonoBehaviour
{
    [Header("UI")]
    public Transform contentParent;          // ScrollView/Viewport/Content
    public LeaderboardRow rowPrefab;         // prefab with LeaderboardRow
    public TMP_Text emptyLabel;              // optional: "No scores yet"

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (contentParent == null || rowPrefab == null) return;

        // Clear existing rows
        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Object.Destroy(contentParent.GetChild(i).gameObject);

        List<(string name, int bestScore)> data = LeaderboardManager.GetLeaderboardSorted();

        if (emptyLabel != null)
            emptyLabel.gameObject.SetActive(data.Count == 0);

        for (int i = 0; i < data.Count; i++)
        {
            var row = Object.Instantiate(rowPrefab, contentParent);
            row.Set(i + 1, data[i].name, data[i].bestScore);
        }
    }
}