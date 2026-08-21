using UnityEngine;
using System.Collections.Generic;

public class ChatPersistence : MonoBehaviour
{
    private const string CHAT_HISTORY_KEY = "3DAI_ChatHistory";
    private const string PREFERENCES_KEY = "3DAI_Preferences";
    private const int MAX_HISTORY_ENTRIES = 50;

    [System.Serializable]
    private class ChatEntry
    {
        public string role;
        public string text;
        public long timestamp;
    }

    [System.Serializable]
    private class ChatHistory
    {
        public List<ChatEntry> entries = new List<ChatEntry>();
    }

    [System.Serializable]
    private class Preferences
    {
        public string avatarPath;
        public bool lipSyncEnabled;
        public bool expressionsEnabled;
    }

    private ChatHistory _history = new ChatHistory();

    void Start()
    {
        LoadChatHistory();
        Debug.Log("[Persistence] Loaded " + _history.entries.Count + " chat entries");
    }

    public void AddMessage(string role, string text)
    {
        var entry = new ChatEntry
        {
            role = role,
            text = text,
            timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };

        _history.entries.Add(entry);

        // Trim old entries if over limit
        while (_history.entries.Count > MAX_HISTORY_ENTRIES)
            _history.entries.RemoveAt(0);

        SaveChatHistory();
    }

    public string GetChatHistoryAsText()
    {
        string result = "";
        foreach (var entry in _history.entries)
        {
            string label = entry.role == "user" ? "You" : "AI";
            result += label + ": " + entry.text + "\n";
        }
        return result;
    }

    public List<string> GetRecentMessages(int count)
    {
        var messages = new List<string>();
        int start = Mathf.Max(0, _history.entries.Count - count);
        for (int i = start; i < _history.entries.Count; i++)
            messages.Add(_history.entries[i].text);
        return messages;
    }

    public void ClearHistory()
    {
        _history.entries.Clear();
        SaveChatHistory();
        Debug.Log("[Persistence] Chat history cleared");
    }

    private void SaveChatHistory()
    {
        try
        {
            string json = JsonUtility.ToJson(_history);
            PlayerPrefs.SetString(CHAT_HISTORY_KEY, json);
            PlayerPrefs.Save();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[Persistence] Failed to save: " + e.Message);
        }
    }

    private void LoadChatHistory()
    {
        try
        {
            if (PlayerPrefs.HasKey(CHAT_HISTORY_KEY))
            {
                string json = PlayerPrefs.GetString(CHAT_HISTORY_KEY);
                _history = JsonUtility.FromJson<ChatHistory>(json);
                if (_history == null) _history = new ChatHistory();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[Persistence] Failed to load: " + e.Message);
            _history = new ChatHistory();
        }
    }

    public void SavePreference(string key, string value)
    {
        PlayerPrefs.SetString(PREFERENCES_KEY + "_" + key, value);
        PlayerPrefs.Save();
    }

    public string LoadPreference(string key, string defaultValue = "")
    {
        return PlayerPrefs.GetString(PREFERENCES_KEY + "_" + key, defaultValue);
    }

    public int MessageCount => _history.entries.Count;
}
