using UnityEngine;

public class RouteMapManager : MonoBehaviour
{
    [Header("Popup Selesai Semua Level")]
    public GameObject popupSelesai; // assign popup di Inspector

    void Start()
    {
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

    public void TutupPopupSelesai()
    {
        if (popupSelesai != null)
            popupSelesai.SetActive(false);
    }
}