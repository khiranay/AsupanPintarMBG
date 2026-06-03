using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Singleton untuk memuat scene secara asinkron.
///
/// CARA SETUP (PENTING - baca semua):
/// 1. Buat GameObject kosong di scene pertama (Home/RouteMap), beri nama "SceneLoader".
/// 2. Attach script ini ke GameObject tersebut.
/// 3. (Opsional) Untuk loading overlay:
///    - Buat Panel (Image gelap) sebagai CHILD dari GameObject SceneLoader ini.
///    - Buat Image (Image Type = Filled) sebagai CHILD dari Panel tersebut (progress bar).
///    - Assign keduanya ke field loadingOverlay dan loadingBar di Inspector.
///    - PENTING: overlay HARUS menjadi child dari SceneLoader agar ikut DontDestroyOnLoad.
///      Jika overlay bukan child, ia akan hancur bersama scene dan tidak muncul di scene berikutnya.
/// 4. Panggil SceneLoader.LoadScene("NamaScene") dari script manapun.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("Loading UI (Opsional — harus child dari GameObject ini)")]
    [Tooltip("Panel/Image gelap yang muncul saat loading. HARUS child dari SceneLoader agar ikut DontDestroyOnLoad.")]
    public GameObject loadingOverlay;

    [Tooltip("Image (Image Type = Filled) sebagai progress bar. HARUS child dari SceneLoader.")]
    public Image loadingBar;

    [Tooltip("Waktu minimum loading screen tampil (detik)")]
    public float minimumLoadTime = 0.3f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // BUG FIX #4: Validasi bahwa loadingOverlay adalah child dari SceneLoader.
        // Jika bukan child, setelah scene berganti overlay akan ikut hancur bersama
        // scene lama dan tidak akan muncul pada scene berikutnya.
        if (loadingOverlay != null && loadingOverlay.transform.parent != this.transform)
        {
            Debug.LogWarning(
                "[SceneLoader] loadingOverlay bukan child dari SceneLoader! " +
                "Overlay akan hancur bersama scene saat navigasi. " +
                "Pindahkan loadingOverlay menjadi child dari GameObject SceneLoader " +
                "agar overlay bertahan lintas scene (DontDestroyOnLoad).");
        }

        // Pastikan overlay tersembunyi saat awal
        if (loadingOverlay != null)
            loadingOverlay.SetActive(false);
    }

    /// <summary>
    /// Muat scene secara async. Aman dipanggil dari script manapun.
    /// </summary>
    public static void LoadScene(string sceneName)
    {
        if (Instance == null)
        {
            Debug.LogWarning("[SceneLoader] Instance belum ada di scene ini. " +
                             "Pastikan ada GameObject dengan SceneLoader di scene pertama. " +
                             "Fallback ke synchronous load.");
            SceneManager.LoadScene(sceneName);
            return;
        }

        Instance.StartCoroutine(Instance.LoadAsync(sceneName));
    }

    private IEnumerator LoadAsync(string sceneName)
    {
        // Tampilkan overlay — hanya jika masih valid (child dari SceneLoader, ikut DontDestroyOnLoad)
        bool hasOverlay = loadingOverlay != null;
        if (hasOverlay)
        {
            loadingOverlay.SetActive(true);
            if (loadingBar != null) loadingBar.fillAmount = 0f;
        }

        float startTime = Time.realtimeSinceStartup;

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        // Update progress bar selama loading berlangsung
        while (op.progress < 0.9f)
        {
            if (hasOverlay && loadingBar != null)
                loadingBar.fillAmount = Mathf.Clamp01(op.progress / 0.9f);

            yield return null;
        }

        // Pastikan minimum load time terpenuhi
        float elapsed = Time.realtimeSinceStartup - startTime;
        if (elapsed < minimumLoadTime)
            yield return new WaitForSecondsRealtime(minimumLoadTime - elapsed);

        if (hasOverlay && loadingBar != null)
            loadingBar.fillAmount = 1f;

        yield return new WaitForSecondsRealtime(0.1f);

        // Aktifkan scene baru
        op.allowSceneActivation = true;

        // Tunggu satu frame setelah scene aktif, lalu sembunyikan overlay
        yield return null;

        if (loadingOverlay != null)
            loadingOverlay.SetActive(false);
    }
}
