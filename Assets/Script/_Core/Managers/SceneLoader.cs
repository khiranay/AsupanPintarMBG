using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ═══════════════════════════════════════════════════════════════════════════
///  SceneLoader - REWRITE dengan IMGUI (OnGUI)
/// ═══════════════════════════════════════════════════════════════════════════
///
///  PENDEKATAN BARU:
///  - Loading screen pakai IMGUI (OnGUI), BUKAN Canvas/UI
///  - IMGUI render langsung ke screen, TIDAK bisa "di luar canvas"
///  - Tidak butuh RectTransform, Canvas, sorting order, dll
///  - PASTI tampil selama transisi scene
///
///  CARA PAKAI:
///  1. Delete SceneLoader lama dari scene (kalau ada)
///  2. Taruh script ini di GameObject manapun di scene
///  3. Atau biarin kosong - auto-create via RuntimeInitializeOnLoadMethod
///  4. Panggil: SceneLoader.LoadScene("NamaScene")
///
///  DEPENDENCY: TIDAK ADA. Tidak butuh Canvas, EventSystem, atau UI apapun.
/// ═══════════════════════════════════════════════════════════════════════════
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("Settings")]
    [Tooltip("Durasi minimum loading tampil (detik). Default 0.8s.")]
    public float minimumLoadTime = 0.8f;

    [Tooltip("Teks loading (contoh: 'Tunggu sebentar', 'Memuat...').")]
    public string loadingText = "Tunggu sebentar";

    [Tooltip("Warna background overlay (alpha 0-1, 0=transparan, 1=hitam pekat).")]
    [Range(0f, 1f)]
    public float overlayAlpha = 0.75f;

    [Tooltip("Warna spinner/dots.")]
    public Color spinnerColor = new Color(1f, 0.65f, 0.2f); // orange

    [Tooltip("Warna teks loading.")]
    public Color textColor = Color.white;

    // State
    private bool isLoading = false;
    private float loadStartTime;
    private string currentSceneName = "";

    // ═══════════════════════════════════════════════════════════════════
    //  AUTO-CREATE: Pastikan SceneLoader selalu ada sejak awal.
    //  Ini handle kasus "Instance null setelah scene reload".
    // ═══════════════════════════════════════════════════════════════════
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void AutoCreateSceneLoader()
    {
        if (Instance != null) return;
        var go = new GameObject("[SceneLoader]");
        go.AddComponent<SceneLoader>();
        // Instance akan di-set di Awake
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ═══════════════════════════════════════════════════════════════════
    //  PUBLIC API
    // ═══════════════════════════════════════════════════════════════════
    public static void LoadScene(string sceneName)
    {
        if (Instance == null)
        {
            Debug.LogWarning("[SceneLoader] Instance null, fallback ke sync load");
            SceneManager.LoadScene(sceneName);
            return;
        }
        Instance.StartCoroutine(Instance.LoadAsync(sceneName));
    }

    private IEnumerator LoadAsync(string sceneName)
    {
        isLoading = true;
        currentSceneName = sceneName;
        loadStartTime = Time.realtimeSinceStartup;

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        // Tunggu sampai scene siap (progress >= 0.9) DAN minimum load time tercapai
        while (op.progress < 0.9f ||
               (Time.realtimeSinceStartup - loadStartTime) < minimumLoadTime)
        {
            yield return null;
        }

        // Aktifkan scene
        op.allowSceneActivation = true;
        yield return null; // Tunggu 1 frame agar scene fully active

        isLoading = false;
    }

    // ═══════════════════════════════════════════════════════════════════
    //  IMGUI RENDERING — selalu tampil, tidak bisa "di luar canvas"
    // ═══════════════════════════════════════════════════════════════════
    void OnGUI()
    {
        if (!isLoading) return;

        // Simpan warna & matrix
        Color prevColor = GUI.color;
        Matrix4x4 prevMatrix = GUI.matrix;

        // 1) Background semi-transparan (full screen)
        GUI.color = new Color(0, 0, 0, overlayAlpha);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);

        // 2) Spinner sederhana: 3 dots yang bounce
        GUI.color = spinnerColor;
        DrawBouncingDots(Screen.width / 2f, Screen.height / 2f - 30f);

        // 3) Teks loading dengan animasi titik
        GUI.color = textColor;
        int dotCount = Mathf.FloorToInt(Time.realtimeSinceStartup * 2f) % 4;
        string dots = new string('.', dotCount);

        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = 36,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = textColor }
        };

        GUI.Label(new Rect(0, Screen.height / 2f + 40f, Screen.width, 80),
                  loadingText + dots, style);

        // Restore
        GUI.color = prevColor;
        GUI.matrix = prevMatrix;
    }

    /// <summary>
    /// Gambar 3 dots yang bounce-bounce (efek loading klasik).
    /// </summary>
    void DrawBouncingDots(float centerX, float centerY)
    {
        int numDots = 3;
        float dotSize = 18f;
        float spacing = 35f;
        float totalWidth = (numDots - 1) * spacing;
        float startX = centerX - totalWidth / 2f;

        for (int i = 0; i < numDots; i++)
        {
            // Bounce phase: tiap dot delay 0.2 detik
            float phase = (Time.realtimeSinceStartup * 4f) - (i * 0.3f);
            float bounce = Mathf.Abs(Mathf.Sin(phase)) * 20f;

            var rect = new Rect(
                startX + i * spacing - dotSize / 2f,
                centerY - bounce - dotSize / 2f,
                dotSize, dotSize);

            GUI.DrawTexture(rect, Texture2D.whiteTexture);
        }
    }
}
