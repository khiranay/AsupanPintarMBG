using UnityEngine;

public class AllLevelsCompletionManager : MonoBehaviour
{
    private const string ALL_LEVELS_COMPLETED_KEY = "AllLevelsCompleted";
    private const int TOTAL_LEVELS = 7; // Sesuaikan dengan jumlah level Anda

    /// <summary>
    /// Periksa apakah semua level sudah selesai (minimal 1 bintang)
    /// </summary>
    public static bool AreAllLevelsCompleted()
    {
        for (int i = 1; i <= TOTAL_LEVELS; i++)
        {
            int stars = LevelProgressManager.GetStars(i);
            if (stars < 1) // Level belum selesai (0 bintang)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Set flag bahwa semua level sudah selesai
    /// </summary>
    public static void SetAllLevelsCompleted()
    {
        PlayerPrefs.SetInt(ALL_LEVELS_COMPLETED_KEY, 1);
        PlayerPrefs.Save();
        Debug.Log("[AllLevelsCompletionManager] Flag reward diset!");
    }

    /// <summary>
    /// Periksa apakah flag semua level selesai aktif
    /// </summary>
    public static bool IsAllLevelsCompletedFlagSet()
    {
        return PlayerPrefs.GetInt(ALL_LEVELS_COMPLETED_KEY, 0) == 1;
    }

    /// <summary>
    /// Hapus flag (dipanggil setelah popup reward ditampilkan)
    /// </summary>
    public static void ClearAllLevelsCompletedFlag()
    {
        PlayerPrefs.SetInt(ALL_LEVELS_COMPLETED_KEY, 0);
        PlayerPrefs.Save();
        Debug.Log("[AllLevelsCompletionManager] Flag reward dihapus!");
    }
}