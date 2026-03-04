using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public static class LeaderboardManager
{
    private const string CurrentNameKey = "current_player_name";
    private const string LeaderboardKey = "leaderboard_v1"; // stored as JSON
    public  const string DefaultName    = "Anonymous";
    public  const int    MaxNameLength  = 15;

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

    /// <summary>Returns the saved name, or "Anonymous" if none is set.</summary>
    public static string GetCurrentPlayerName()
    {
        string n = PlayerPrefs.GetString(CurrentNameKey, "");
        return string.IsNullOrEmpty(n) ? DefaultName : n;
    }

    /// <summary>
    /// Validates then saves the name. Returns null on success, or an error string to show the user.
    /// Passing empty/null resets back to Anonymous.
    /// </summary>
    public static string TrySetCurrentPlayerName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            PlayerPrefs.SetString(CurrentNameKey, "");
            PlayerPrefs.Save();
            return null;
        }
        string error = ValidateName(name);
        if (error != null) return error;
        PlayerPrefs.SetString(CurrentNameKey, name.Trim());
        PlayerPrefs.Save();
        return null;
    }

    /// <summary>Legacy setter — used internally. Silently ignores invalid names.</summary>
    public static void SetCurrentPlayerName(string name) => TrySetCurrentPlayerName(name);

    /// <summary>
    /// Returns null if valid, or a short error message.
    /// Rules: letters + digits only, no spaces, 1–15 chars.
    /// </summary>
    public static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return null; // empty = reset
        name = name.Trim();
        if (name.Length > MaxNameLength)  return $"Max {MaxNameLength} characters";
        if (name.Contains(" "))           return "No spaces allowed";
        if (!Regex.IsMatch(name, @"^[A-Za-z0-9]+$")) return "Letters and numbers only";
        return null;
    }

    // ---------- Scores ----------
    public static void SubmitScore(int score)
    {
        string current = GetCurrentPlayerName(); // always at least "Anonymous"
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


}