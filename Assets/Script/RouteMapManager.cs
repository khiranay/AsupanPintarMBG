using UnityEngine;
using UnityEngine.UI;

public class RouteMapManager : MonoBehaviour
{
    [Header("Popup Selesai Semua Level")]
    public GameObject popupSelesai; // assign popup di Inspector

    [Header("Force Top Canvas (PATCH)")]
    [Tooltip("Canvas utama RouteMap. Akan dipaksa ke sortingOrder tinggi " +
             "agar tidak terhalang Canvas scene lain yang persisten.")]
    public Canvas routeMapCanvas;

    void Start()
    {
        // PATCH: Paksa canvas ini ke sorting order tinggi
        ForceCanvasToTop();

        // Refresh semua LevelButton saat scene dimuat
        RefreshAllLevelButtons();

        if (popupSelesai == null) return;

        // Cek flag dari LevelFlowManager
        if (PlayerPrefs.GetInt("ShowSelesaiPopup", 0) == 1)
        {
            popupSelesai.SetActive(true);
            // Hapus flag agar tidak muncul lagi saat balik ke RouteMap berikutnya
            PlayerPrefs.SetInt("ShowSelesaiPopup", 0);
            PlayerPrefs.Save();
        }
        else
        {
            popupSelesai.SetActive(false);
        }
    }

    /// <summary>
    /// PATCH: Cari Canvas di scene ini dan naikkan sorting order-nya
    /// agar di atas Canvas persisten (mis. SceneLoader overlay).
    /// </summary>
    void ForceCanvasToTop()
    {
        // Jika user assign manual, pakai itu
        if (routeMapCanvas == null)
            routeMapCanvas = GetComponentInChildren<Canvas>(true);

        if (routeMapCanvas != null)
        {
            routeMapCanvas.overrideSorting = true;
            routeMapCanvas.sortingOrder = 100;
            Debug.Log($"[RouteMapManager] Canvas '{routeMapCanvas.name}' " +
                      $"paksa ke sortingOrder = 100");
        }
        else
        {
            Debug.LogWarning("[RouteMapManager] Tidak ada Canvas ditemukan. " +
                             "Tombol mungkin tertutup Canvas persisten lain.");
        }
    }

    void RefreshAllLevelButtons()
    {
        LevelButton[] allButtons = FindObjectsOfType<LevelButton>();
        foreach (var btn in allButtons)
        {
            btn.RefreshDisplay();
        }
        Debug.Log($"[RouteMapManager] Refreshed {allButtons.Length} LevelButtons");
    }

    public void TutupPopupSelesai()
    {
        if (popupSelesai != null)
            popupSelesai.SetActive(false);
    }
}
