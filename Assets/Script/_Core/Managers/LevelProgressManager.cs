using UnityEngine;

public class LevelProgressManager : MonoBehaviour
{
    // Panggil ini dari scene Materi saat selesai
    public static void CompleteMateri(int levelIndex)
    {
        int current = PlayerPrefs.GetInt("Level_" + levelIndex + "_Stars", 0);
        if (current < 1)
        {
            PlayerPrefs.SetInt("Level_" + levelIndex + "_Stars", 1);
            PlayerPrefs.Save();
        }
    }

    // Panggil ini dari scene Kuis saat selesai
    public static void CompleteKuis(int levelIndex)
    {
        int current = PlayerPrefs.GetInt("Level_" + levelIndex + "_Stars", 0);
        if (current < 2)
        {
            PlayerPrefs.SetInt("Level_" + levelIndex + "_Stars", 2);
            PlayerPrefs.Save();
        }
    }

    // Panggil ini dari scene Mini Game saat selesai
    public static void CompleteMiniGame(int levelIndex)
    {
        PlayerPrefs.SetInt("Level_" + levelIndex + "_Stars", 3);
        PlayerPrefs.Save();
    }

    public static int GetStars(int levelIndex)
    {
        return PlayerPrefs.GetInt("Level_" + levelIndex + "_Stars", 0);
    }
}