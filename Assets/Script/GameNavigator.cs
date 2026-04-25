using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// ═══════════════════════════════════════════════════════════════
//  GameNavigator
//  Attach ke Manager object di Scene Game.
//  Mengatur panel game per level, dan tombol Next setelah game.
// ═══════════════════════════════════════════════════════════════
public class GameNavigator : MonoBehaviour
{
    [Header("Panel Game per Level (index 0 = Level 1, dst.)")]
    public GameObject[] gamePanels;

    void Start()
    {
        int level = LevelFlowManager.GetCurrentLevel();
        ShowGameForLevel(level);
    }

    private void ShowGameForLevel(int level)
    {
        int index = level - 1;
        for (int i = 0; i < gamePanels.Length; i++)
        {
            if (gamePanels[i] != null)
                gamePanels[i].SetActive(i == index);
        }
    }

    /// <summary>
    /// Hubungkan tombol "Selesai / Next" di panel Game ke method ini.
    /// </summary>
    public void OnGameSelesai()
    {
        LevelFlowManager.OnGameSelesai();
    }

    /// <summary>
    /// Tombol Back di Game → kembali ke RouteMap.
    /// </summary>
    public void OnBackPressed()
    {
        LevelFlowManager.GoToRouteMap();
    }
}