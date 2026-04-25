using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach script ini ke setiap GameObject MateriLevel(N).
/// Atur subPanels di Inspector: drag sub-panel secara berurutan
/// (misal: CekBau, CekWarna, CekRasa untuk Level1).
/// </summary>
public class MateriNavigator : MonoBehaviour
{
    [Header("Sub-Panels dalam Materi ini (urut dari awal)")]
    public GameObject[] subPanels;

    [Header("Nomor Level ini (1-7)")]
    public int levelNumber = 1;

    [Header("Nama Scene Kuis")]
    public string kuisSceneName = "SceneKuis";

    private int currentIndex = 0;

    void OnEnable()
    {
        // Reset ke sub-panel pertama setiap kali panel ini dibuka
        currentIndex = 0;
        ShowPanel(currentIndex);
    }

    /// <summary>
    /// Tampilkan hanya panel di index tertentu, sembunyikan sisanya.
    /// </summary>
    private void ShowPanel(int index)
    {
        for (int i = 0; i < subPanels.Length; i++)
        {
            if (subPanels[i] != null)
                subPanels[i].SetActive(i == index);
        }
    }

    /// <summary>
    /// Hubungkan tombol Next di tiap sub-panel ke method ini.
    /// Jika masih ada sub-panel berikutnya → tampilkan.
    /// Jika sudah sub-panel terakhir → pindah ke Kuis.
    /// </summary>
    public void OnNextPressed()
    {
        currentIndex++;

        if (currentIndex < subPanels.Length)
        {
            // Masih ada sub-panel berikutnya
            ShowPanel(currentIndex);
        }
        else
        {
            // Semua sub-panel selesai → simpan level & pindah ke Kuis
            GoToKuis();
        }
    }

    /// <summary>
    /// Tombol Back di sub-panel: kembali ke sub-panel sebelumnya.
    /// Jika sudah di sub-panel pertama → kembali ke RouteMap.
    /// </summary>
    public void OnBackPressed()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            ShowPanel(currentIndex);
        }
        else
        {
            // Sub-panel pertama → kembali ke RouteMap
            LevelFlowManager.GoToRouteMap();
        }
    }

    private void GoToKuis()
    {
        // Simpan level yang sedang dimainkan
        PlayerPrefs.SetInt("CurrentLevel", levelNumber);
        PlayerPrefs.Save();

        SceneManager.LoadScene(kuisSceneName);
    }
}