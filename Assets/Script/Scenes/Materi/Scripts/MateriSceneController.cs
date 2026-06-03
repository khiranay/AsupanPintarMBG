using UnityEngine;

/// <summary>
/// Controller terpusat untuk scene Materi.
///
/// CARA SETUP:
/// 1. Buat GameObject kosong di root scene Materi, beri nama "MateriSceneController".
/// 2. Attach script ini ke GameObject tersebut.
/// 3. Assign field "materiRoot" → drag Canvas atau parent GameObject yang
///    berisi semua MateriLevel(N) di dalamnya.
///
/// KENAPA DIBUTUHKAN:
/// MateriNavigator lama mengandalkan OnEnable() untuk inisialisasi diri.
/// Masalahnya: OnEnable() hanya jalan jika GameObject AKTIF saat scene load.
/// Jika panel Level 2, 3, dst. dalam keadaan inactive di Hierarchy,
/// OnEnable() tidak pernah dipanggil → konten tidak muncul, hanya background.
///
/// Script ini menyelesaikan masalah tersebut dengan mencari semua MateriNavigator
/// (termasuk yang inactive) dan mengaktifkan yang sesuai current level.
/// </summary>
public class MateriSceneController : MonoBehaviour
{
    [Tooltip("Parent GameObject yang berisi semua MateriLevel(N). " +
             "Biasanya Canvas atau root panel di scene Materi.")]
    public GameObject materiRoot;

    void Start()
    {
        if (materiRoot == null)
        {
            Debug.LogError("[MateriSceneController] 'materiRoot' belum di-assign di Inspector! " +
                           "Drag Canvas atau parent dari semua MateriLevel ke field ini.");
            return;
        }

        int currentLevel = LevelFlowManager.GetCurrentLevel();

        // Cari SEMUA MateriNavigator di bawah materiRoot,
        // termasuk yang sedang inactive (parameter true)
        MateriNavigator[] semuaNavigator =
            materiRoot.GetComponentsInChildren<MateriNavigator>(true);

        if (semuaNavigator.Length == 0)
        {
            Debug.LogError("[MateriSceneController] Tidak ada MateriNavigator ditemukan " +
                           $"di bawah '{materiRoot.name}'. Pastikan setiap MateriLevel " +
                           "memiliki komponen MateriNavigator.");
            return;
        }

        bool levelDitemukan = false;

        foreach (MateriNavigator nav in semuaNavigator)
        {
            bool isLevelIni = (nav.levelNumber == currentLevel);

            // Aktifkan hanya panel yang sesuai level, nonaktifkan sisanya.
            // Ketika SetActive(true) dipanggil, OnEnable() di MateriNavigator
            // akan otomatis berjalan dan menampilkan sub-panel pertama.
            nav.gameObject.SetActive(isLevelIni);

            if (isLevelIni)
            {
                levelDitemukan = true;

                // Validasi: peringatkan jika subPanels kosong
                if (nav.subPanels == null || nav.subPanels.Length == 0)
                {
                    Debug.LogError(
                        $"[MateriSceneController] MateriNavigator untuk Level {currentLevel} " +
                        $"ditemukan ('{nav.gameObject.name}'), tapi array 'subPanels' kosong! " +
                        "Drag sub-panel konten ke array subPanels di Inspector.");
                }
            }
        }

        if (!levelDitemukan)
        {
            Debug.LogError(
                $"[MateriSceneController] Tidak ada MateriNavigator dengan levelNumber = " +
                $"{currentLevel} ditemukan! Pastikan ada panel Materi untuk Level {currentLevel} " +
                "dan field 'levelNumber'-nya sudah diisi dengan benar di Inspector.");
        }
    }
}
