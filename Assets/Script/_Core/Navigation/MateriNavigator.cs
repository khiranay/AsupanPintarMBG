using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach script ini ke setiap GameObject MateriLevel(N).
/// Atur subPanels di Inspector: drag sub-panel secara berurutan.
/// Isi levelNumber sesuai level panel ini (1-7).
/// Script akan otomatis mematikan dirinya jika bukan level yang sedang dimainkan.
/// </summary>
public class MateriNavigator : MonoBehaviour
{
    [Header("Nomor Level panel ini (1-7)")]
    public int levelNumber = 1;

    [Header("Sub-Panels dalam Materi ini (urut dari awal)")]
    public GameObject[] subPanels;

    private int currentIndex = 0;

    void OnEnable()
    {
        int currentLevel = LevelFlowManager.GetCurrentLevel();

        // Jika bukan panel untuk level ini, matikan diri sendiri
        if (levelNumber != currentLevel)
        {
            gameObject.SetActive(false);
            return;
        }

        // Reset ke sub-panel pertama
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
    /// Jika sudah sub-panel terakhir → simpan bintang 1 + pindah ke Kuis.
    /// </summary>
    public void OnNextPressed()
    {
        currentIndex++;

        if (currentIndex < subPanels.Length)
        {
            ShowPanel(currentIndex);
        }
        else
        {
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
            LevelFlowManager.GoToRouteMap();
        }
    }

    private void GoToKuis()
    {
        // Simpan bintang 1 setelah materi selesai
        LevelProgressManager.CompleteMateri(levelNumber);

        // Pindah ke scene Kuis
        SceneLoader.LoadScene(LevelFlowManager.KuisScene);
    }
}
