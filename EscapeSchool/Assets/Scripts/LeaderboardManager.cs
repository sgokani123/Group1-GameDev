using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class LeaderboardManager
{
    private const string CurrentNameKey = "current_player_name";
    private const string LeaderboardKey = "leaderboard_v1"; // stored as JSON

    [Serializable]
    private class Entry
    {
        public string name;
        public int bestScore;
    }

    [Serializable]
    private class EntryList
    {
        public List<Entry> entries = new List<Entry>();
    }

    // ---------- Name ----------
    public static string GetCurrentPlayerName()
    {
        return PlayerPrefs.GetString(CurrentNameKey, "");
    }

    public static void SetCurrentPlayerName(string name)
    {
        name = SanitizeName(name);
        PlayerPrefs.SetString(CurrentNameKey, name);
        PlayerPrefs.Save();
    }

    // ---------- Scores ----------
    public static void SubmitScore(int score)
    {
        string current = GetCurrentPlayerName();
        if (string.IsNullOrWhiteSpace(current)) return; // no name set => don't save

        var list = Load();

        // Case-sensitive match (caps allowed; not treated as “sensitive”)
        var entry = list.entries.FirstOrDefault(e => e.name == current);
        if (entry == null)
        {
            entry = new Entry { name = current, bestScore = score };
            list.entries.Add(entry);
        }
        else
        {
            if (score > entry.bestScore)
                entry.bestScore = score;
        }

        Save(list);
    }

    public static int GetBestForCurrentPlayer()
    {
        string current = GetCurrentPlayerName();
        if (string.IsNullOrWhiteSpace(current)) return 0;

        var list = Load();
        var entry = list.entries.FirstOrDefault(e => e.name == current);
        return entry != null ? entry.bestScore : 0;
    }

    public static int GetGlobalBest()
    {
        var list = Load();
        if (list.entries == null || list.entries.Count == 0) return 0;
        return list.entries.Max(e => e.bestScore);
    }

    public static List<(string name, int bestScore)> GetLeaderboardSorted()
    {
        var list = Load();
        return list.entries
            .OrderByDescending(e => e.bestScore)
            .ThenBy(e => e.name)
            .Select(e => (e.name, e.bestScore))
            .ToList();
    }

    // ---------- Storage ----------
    private static EntryList Load()
    {
        string json = PlayerPrefs.GetString(LeaderboardKey, "");
        if (string.IsNullOrEmpty(json))
            return new EntryList();

        try
        {
            var data = JsonUtility.FromJson<EntryList>(json);
            return data ?? new EntryList();
        }
        catch
        {
            return new EntryList();
        }
    }

    private static void Save(EntryList list)
    {
        if (list == null) list = new EntryList();
        string json = JsonUtility.ToJson(list);
        PlayerPrefs.SetString(LeaderboardKey, json);
        PlayerPrefs.Save();
    }

    private static string SanitizeName(string name)
    {
        if (name == null) return "";
        name = name.Trim();

        // prevent newlines / weird whitespace
        name = name.Replace("\n", " ").Replace("\r", " ").Replace("\t", " ");

        // optional: limit length so UI doesn’t explode
        if (name.Length > 16) name = name.Substring(0, 16);

        return name;
    }
}