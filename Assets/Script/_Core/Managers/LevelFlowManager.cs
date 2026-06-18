using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelFlowManager
{
    public const string RouteMapScene = "RouteMap";
    public const string KuisScene = "Kuis";
    public const string GameScene = "Game";
    public const string MateriScene = "Materi";
    public const string KeyCurrentLevel = "CurrentLevel";

    public static void OnKuisSelesai()
    {
        SceneLoader.LoadScene(GameScene);
    }

    public static void OnGameSelesai()
    {
        int level = PlayerPrefs.GetInt(KeyCurrentLevel, 1);
        LevelProgressManager.CompleteMiniGame(level);

        int highestUnlocked = PlayerPrefs.GetInt("HighestUnlocked", 1);
        if (level >= highestUnlocked)
        {
            PlayerPrefs.SetInt("HighestUnlocked", level + 1);
            PlayerPrefs.Save();
        }

        // ← HARUS ADA
        LevelProgressManager.CheckAndSetAllLevelsCompleted();

        GoToRouteMap();
    }

    public static void GoToNextLevelMateri(int totalLevel = 7)
    {
        int level = PlayerPrefs.GetInt(KeyCurrentLevel, 1);
        LevelProgressManager.CompleteMiniGame(level);

        int highestUnlocked = PlayerPrefs.GetInt("HighestUnlocked", 1);
        if (level >= highestUnlocked)
        {
            PlayerPrefs.SetInt("HighestUnlocked", level + 1);
            PlayerPrefs.Save();
        }

        // ← HARUS ADA
        LevelProgressManager.CheckAndSetAllLevelsCompleted();

        if (level >= totalLevel)
        {
            GoToRouteMap();
            return;
        }

        PlayerPrefs.SetInt(KeyCurrentLevel, level + 1);
        PlayerPrefs.Save();
        SceneLoader.LoadScene(MateriScene);
    }

    public static void GoToRouteMap()
    {
        SceneLoader.LoadScene(RouteMapScene);
    }

    public static int GetCurrentLevel()
    {
        return PlayerPrefs.GetInt(KeyCurrentLevel, 1);
    }
}