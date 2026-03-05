public static class StoreManager
{
    private const string RocketKey    = "store_rocket";
    private const string CharacterKey = "store_character";

    public static int GetRocketIndex()    => UnityEngine.PlayerPrefs.GetInt(RocketKey, 0);
    public static int GetCharacterIndex() => UnityEngine.PlayerPrefs.GetInt(CharacterKey, 0);

    public static void SetRocketIndex(int i)
    {
        UnityEngine.PlayerPrefs.SetInt(RocketKey, i);
        UnityEngine.PlayerPrefs.Save();
    }

    public static void SetCharacterIndex(int i)
    {
        UnityEngine.PlayerPrefs.SetInt(CharacterKey, i);
        UnityEngine.PlayerPrefs.Save();
    }
}