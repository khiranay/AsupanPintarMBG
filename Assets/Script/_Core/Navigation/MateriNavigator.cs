using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

/// <summary>
/// Attach script ini ke setiap GameObject MateriLevel(N).
/// Atur subPanels di Inspector: drag sub-panel secara berurutan.
/// Isi levelNumber sesuai level panel ini (1-7).
/// Script akan otomatis mematikan dirinya jika bukan level yang sedang dimainkan.
///
/// PATCH: Transisi slide sekarang pakai coroutine agar tidak ada
/// "frame terakhir slide sebelumnya" yang muncul sesaat di slide baru.
/// - Menunggu 1 frame penuh setelah slide lama dimatikan
/// - Membersihkan RawImage/VideoPlayer slide lama (kosongkan texture,
///   stop video) sebelum di-hide
/// - Opsional: fade in/out lewat CanvasGroup jika slide punya
/// </summary>
public class MateriNavigator : MonoBehaviour
{
    [Header("Nomor Level panel ini (1-7)")]
    public int levelNumber = 1;

    [Header("Sub-Panels dalam Materi ini (urut dari awal)")]
    public GameObject[] subPanels;

    [Header("Transition (PATCH)")]
    [Tooltip("Aktifkan fade in/out halus saat transisi (menggunakan CanvasGroup).")]
    [SerializeField] private bool useFadeTransition = true;

    [Tooltip("Durasi fade in/out dalam detik (0 = hard cut, default 0.1).")]
    [Range(0f, 0.5f)]
    [SerializeField] private float fadeDuration = 0.1f;

    [Tooltip("Jeda tambahan (detik) antara slide lama dimatikan dan slide baru diaktifkan. " +
             "Mencegah 'flash' frame terakhir webm. Default 0.05 (1-2 frame).")]
    [Range(0f, 0.3f)]
    [SerializeField] private float bufferBetweenSlides = 0.05f;

    [Header("Loading Indicator (saat transisi)")]
    [Tooltip("Teks loading (contoh: 'Memuat', 'Tunggu sebentar').")]
    [SerializeField] private string loadingText_ = "Tunggu sebentar";

    [Tooltip("Warna background overlay (alpha < 1 = semi-transparan).")]
    [SerializeField] private Color overlayColor = new Color(0, 0, 0, 0.75f);

    [Tooltip("Warna spinner / loader.")]
    [SerializeField] private Color spinnerColor = new Color(1f, 0.65f, 0.2f); // orange

    [Tooltip("Warna teks loading.")]
    [SerializeField] private Color textColor = Color.white;

    [Tooltip("Kecepatan rotasi spinner (derajat/detik).")]
    [SerializeField] private float spinnerSpeed = 280f;

    [Tooltip("Ukuran spinner (pixel).")]
    [SerializeField] private float spinnerSize = 100f;

    [Tooltip("Sprite spinner custom (kosongkan untuk otomatis buat lingkaran).")]
    [SerializeField] private Sprite customSpinnerSprite;

    [Header("Auto-Wire Navigation Buttons")]
    [Tooltip("Nama scene Home (untuk tombol Back → Home). " +
             "Default 'Home' sesuai LevelFlowManager / HomeButton.")]
    [SerializeField] private string homeSceneName = "Home";

    [Tooltip("Prefix button yang dianggap 'Back' (case-insensitive). " +
             "Dipakai oleh AutoWireNavigationButtons.")]
    [SerializeField] private string[] backButtonKeywords = new string[]
    {
        "back", "kembali", "home", "btn_back", "btnback", "Back Button"
    };

    [Tooltip("Prefix button yang dianggap 'Next' (case-insensitive). " +
             "Dipakai oleh AutoWireNavigationButtons.")]
    [SerializeField] private string[] nextButtonKeywords = new string[]
    {
        "next", "lanjut", "quiz", "kuis", "btn_next", "btnnext", "Next Button"
    };

    private int currentIndex = 0;
    private bool isTransitioning = false;

    // PATCH: Loading overlay — container semi-transparan dengan spinner
    // dan teks "Tunggu sebentar" yang muncul sesaat saat transisi.
    private Image blankingOverlay;
    private Canvas blankingCanvas;
    private RectTransform spinnerRect;
    private TextMeshProUGUI loadingTextUI;
    private Coroutine textAnimCoroutine;

    /// <summary>
    /// PATCH: Dipanggil oleh Unity saat script ini pertama kali di-attach
    /// ke GameObject, atau saat user klik kanan → Reset di Inspector.
    /// Memastikan field baru punya default value yang benar (mengatasi
    /// Unity gotcha di mana field baru diinisialisasi ke default(T)).
    /// </summary>
    void Reset()
    {
        homeSceneName = "Home";
        backButtonKeywords = new string[]
        {
            "back", "kembali", "home", "btn_back", "btnback"
        };
        nextButtonKeywords = new string[]
        {
            "next", "lanjut", "quiz", "kuis", "btn_next", "btnnext"
        };
        useFadeTransition = true;
        fadeDuration = 0.1f;
        bufferBetweenSlides = 0.05f;
    }

    void OnEnable()
    {
        int currentLevel = LevelFlowManager.GetCurrentLevel();

        // Jika bukan panel untuk level ini, matikan diri sendiri
        if (levelNumber != currentLevel)
        {
            gameObject.SetActive(false);
            return;
        }

        // PATCH: Siapkan blanking overlay (Image hitam fullscreen)
        // untuk menutupi 'loncat' visual saat transisi.
        EnsureBlankingOverlay();

        // PATCH: Auto-wire button Back/Next yang OnClick-nya masih kosong.
        // Lihat AutoWireNavigationButtons() untuk detail.
        AutoWireNavigationButtons();

        // Reset ke sub-panel pertama
        currentIndex = 0;
        // Saat inisialisasi awal, langsung show tanpa animasi
        ShowPanelImmediate(currentIndex);
    }

    /// <summary>
    /// PATCH: Buat Canvas + Image semi-transparan + spinner + teks loading
    /// (jika belum ada) yang akan diaktifkan sesaat saat transisi. Lebih
    /// bagus dari blanking hitam polos.
    ///
    /// FIX (bug #4): Loading overlay harus di-parent ke TOPMOST CANVAS
    /// (yang fullscreen), bukan ke MateriNavigator. Jika parent ke
    /// MateriNavigator yang ukurannya kecil, overlay hanya jadi kotak
    /// hitam kecil di tengah — bukan fullscreen.
    ///
    /// FIX (bug #5 - 'di luar canvas'): Blanking overlay dibuat sebagai
    /// ROOT GameObject dengan Canvas sendiri (ScreenSpaceOverlay) dan
    /// DontDestroyOnLoad. Sebelumnya di-parent ke topmostCanvas sebagai
    /// nested Canvas — kalau parent Canvas bermasalah (render mode world
    /// space, atau di luar viewport), nested Canvas ikut kena imbas.
    /// </summary>
    private void EnsureBlankingOverlay()
    {
        if (blankingOverlay != null) return;

        // PENTING: Buat sebagai ROOT GameObject (BUKAN child topmostCanvas)
        // agar RectTransform anchor (0,0)-(1,1) bisa resolve dengan benar
        // dan Canvas pasti render di ScreenSpaceOverlay.
        var bg = new GameObject("__BlankingOverlay_" + levelNumber, typeof(RectTransform));
        // JANGAN setParent. Biarkan root.

        // RectTransform full-screen
        var bgRT = (RectTransform)bg.transform;
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        bgRT.pivot = new Vector2(0.5f, 0.5f);
        bgRT.sizeDelta = Vector2.zero;
        bgRT.anchoredPosition = Vector2.zero;

        // Canvas sendiri dengan ScreenSpaceOverlay + sortingOrder MAX
        blankingCanvas = bg.AddComponent<Canvas>();
        blankingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        blankingCanvas.overrideSorting = true;
        blankingCanvas.sortingOrder = short.MaxValue; // 32767

        // CanvasScaler untuk responsive UI
        var bgScaler = bg.AddComponent<CanvasScaler>();
        bgScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        bgScaler.referenceResolution = new Vector2(1920, 1080);
        bgScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        bgScaler.matchWidthOrHeight = 0.5f;

        // GraphicRaycaster (perlu untuk Canvas, walaupun raycast off)
        bg.AddComponent<GraphicRaycaster>();
        var bgCG = bg.AddComponent<CanvasGroup>();
        bgCG.alpha = 1f;
        bgCG.blocksRaycasts = false;
        bgCG.interactable = false;

        // Background Image
        blankingOverlay = bg.AddComponent<Image>();
        blankingOverlay.color = overlayColor;
        blankingOverlay.raycastTarget = false;

        // PATCH: Make DontDestroyOnLoad agar persist (sebab root GameObject)
        Object.DontDestroyOnLoad(bg);

        // 2) SPINNER (Image berputar) — child dari root Canvas
        var spinner = new GameObject("Spinner", typeof(RectTransform));
        spinner.transform.SetParent(bg.transform, false);
        spinnerRect = (RectTransform)spinner.transform;
        spinnerRect.anchorMin = new Vector2(0.5f, 0.5f);
        spinnerRect.anchorMax = new Vector2(0.5f, 0.5f);
        spinnerRect.pivot = new Vector2(0.5f, 0.5f);
        spinnerRect.sizeDelta = new Vector2(spinnerSize, spinnerSize);
        spinnerRect.anchoredPosition = new Vector2(0f, 25f);

        var spinnerImage = spinner.AddComponent<Image>();
        spinnerImage.sprite = customSpinnerSprite != null
            ? customSpinnerSprite
            : CreateRingSprite();
        spinnerImage.color = spinnerColor;
        spinnerImage.raycastTarget = false;
        spinnerImage.type = Image.Type.Filled;
        spinnerImage.fillMethod = Image.FillMethod.Radial360;
        spinnerImage.fillAmount = 0.75f;
        spinnerImage.fillClockwise = true;

        // 3) TEKS LOADING (TextMeshPro) — child dari root Canvas
        var textGo = new GameObject("LoadingText", typeof(RectTransform));
        textGo.transform.SetParent(bg.transform, false);
        var textRT = (RectTransform)textGo.transform;
        textRT.anchorMin = new Vector2(0.5f, 0.5f);
        textRT.anchorMax = new Vector2(0.5f, 0.5f);
        textRT.pivot = new Vector2(0.5f, 0.5f);
        textRT.sizeDelta = new Vector2(400f, 60f);
        textRT.anchoredPosition = new Vector2(0f, -60f);

        loadingTextUI = textGo.AddComponent<TextMeshProUGUI>();
        loadingTextUI.text = loadingText_;
        loadingTextUI.fontSize = 32f;
        loadingTextUI.alignment = TextAlignmentOptions.Center;
        loadingTextUI.color = textColor;
        loadingTextUI.fontStyle = FontStyles.Bold;
        loadingTextUI.raycastTarget = false;

        bg.SetActive(false);

        Debug.Log($"[MateriNavigator] Blanking overlay dibuat (root Canvas, " +
                  $"sortingOrder={blankingCanvas.sortingOrder}) untuk Level {levelNumber}");
    }

    /// <summary>
    /// PATCH: Buat sprite 'ring' (lingkaran dengan lubang tengah) secara
    /// prosedural, sehingga kita tidak perlu import sprite tambahan.
    /// Bisa diganti dengan customSpinnerSprite di Inspector.
    /// </summary>
    private Sprite CreateRingSprite()
    {
        const int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.name = "__GeneratedRingSpinner";
        tex.filterMode = FilterMode.Bilinear;

        float center = size / 2f;
        float outerRadius = size / 2f - 2f;
        float ringThickness = 16f;
        float innerRadius = outerRadius - ringThickness;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center + 0.5f;
                float dy = y - center + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist <= outerRadius && dist >= innerRadius)
                {
                    tex.SetPixel(x, y, Color.white);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }

        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    /// <summary>
    /// PATCH: Animasikan spinner berputar di Update().
    /// </summary>
    void Update()
    {
        if (spinnerRect != null && spinnerRect.gameObject.activeInHierarchy)
        {
            spinnerRect.Rotate(0f, 0f, -spinnerSpeed * Time.unscaledDeltaTime);
        }
    }

    /// <summary>
    /// PATCH: Coroutine animasi titik pada teks loading
    /// (contoh: "Tunggu sebentar" → "Tunggu sebentar." → "Tunggu sebentar.." → ...).
    /// </summary>
    private IEnumerator AnimateLoadingText()
    {
        int dotCount = 0;
        while (true)
        {
            if (loadingTextUI != null)
            {
                loadingTextUI.text = loadingText_ + new string('.', dotCount);
            }
            dotCount = (dotCount + 1) % 4; // 0,1,2,3 titik lalu loop
            yield return new WaitForSecondsRealtime(0.35f);
        }
    }

    /// <summary>
    /// FIX: Cari Canvas paling atas (topmost) di hierarchy scene.
    /// Digunakan untuk parent blanking overlay agar fullscreen.
    /// </summary>
    private Canvas GetTopmostCanvas()
    {
        Canvas topCanvas = null;
        Canvas current = GetComponentInParent<Canvas>();
        if (current == null) return null;

        // Walk up hierarchy sampai tidak ada parent Canvas lagi
        while (current != null)
        {
            topCanvas = current;
            if (current.transform.parent == null) break;
            current = current.transform.parent.GetComponentInParent<Canvas>();
        }
        return topCanvas;
    }

    /// <summary>
    /// PATCH: Tampilkan loading overlay (spinner + teks) untuk menutupi
    /// 'loncat' visual saat transisi.
    /// </summary>
    private void ShowBlankingOverlay()
    {
        if (blankingOverlay == null) return;

        var cg = blankingOverlay.GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = 1f;
        blankingOverlay.gameObject.SetActive(true);

        // Mulai animasi titik
        if (textAnimCoroutine != null) StopCoroutine(textAnimCoroutine);
        textAnimCoroutine = StartCoroutine(AnimateLoadingText());
    }

    /// <summary>
    /// PATCH: Sembunyikan loading overlay setelah transisi selesai.
    /// </summary>
    private void HideBlankingOverlay()
    {
        if (blankingOverlay == null) return;

        blankingOverlay.gameObject.SetActive(false);

        if (textAnimCoroutine != null)
        {
            StopCoroutine(textAnimCoroutine);
            textAnimCoroutine = null;
        }
    }

    /// <summary>
    /// Tampilkan panel di index tertentu, sembunyikan sisanya.
    /// Versi ini: langsung (untuk inisialisasi), tanpa transisi.
    /// </summary>
    private void ShowPanelImmediate(int index)
    {
        for (int i = 0; i < subPanels.Length; i++)
        {
            if (subPanels[i] != null)
            {
                bool shouldBeActive = (i == index);
                subPanels[i].SetActive(shouldBeActive);

                if (shouldBeActive)
                {
                    // PATCH: Pastikan visual & video di-restore saat init
                    RestorePanelVisual(subPanels[i]);
                }
            }
        }
    }

    /// <summary>
    /// Tampilkan panel di index tertentu dengan transisi yang halus.
    /// Dipanggil oleh OnNextPressed / OnBackPressed.
    /// </summary>
    private void ShowPanel(int index)
    {
        if (isTransitioning) return; // Abaikan tap cepat
        if (index < 0 || index >= subPanels.Length) return;

        StartCoroutine(TransitionToPanel(currentIndex, index));
    }

    /// <summary>
    /// PATCH: Coroutine transisi yang menjamin tidak ada "flash" frame lama.
    /// Algoritma:
    ///   1. Fade out slide lama (opsional)
    ///   2. Stop VideoPlayer & kosongkan RawImage pada slide lama
    ///   3. SetActive(false) pada slide lama
    ///   4. Tunggu 1-2 frame (yield) agar render pipeline clear
    ///   5. SetActive(true) pada slide baru
    ///   6. Fade in slide baru (opsional)
    /// </summary>
    private IEnumerator TransitionToPanel(int fromIndex, int toIndex)
    {
        isTransitioning = true;

        GameObject oldPanel = (fromIndex >= 0 && fromIndex < subPanels.Length)
            ? subPanels[fromIndex] : null;
        GameObject newPanel = (toIndex >= 0 && toIndex < subPanels.Length)
            ? subPanels[toIndex] : null;

        // PATCH: Tampilkan blanking overlay (hitam fullscreen) untuk
        // menutupi 'loncat' visual dari frame lama. Muncul 1 frame
        // sebelum SetActive(false) sehingga user tidak melihat transisi.
        ShowBlankingOverlay();
        yield return null; // 1 frame agar overlay ter-render

        // 1) Fade out slide lama
        if (useFadeTransition && fadeDuration > 0f && oldPanel != null)
        {
            yield return StartCoroutine(FadePanel(oldPanel, 1f, 0f, fadeDuration));
        }

        // 2) Bersihkan visual konten slide lama (STOP VIDEO + CLEAR RT)
        if (oldPanel != null)
        {
            CleanupPanelVisual(oldPanel);

            // 3) SetActive(false) pada slide lama
            oldPanel.SetActive(false);
        }

        // 4) Tunggu 1-2 frame + buffer agar render pipeline benar-benar clear
        if (bufferBetweenSlides > 0f)
            yield return new WaitForSeconds(bufferBetweenSlides);
        yield return null; // 1 frame extra
        yield return null; // 1 frame extra biar aman

        // 5) Aktifkan slide baru
        if (newPanel != null)
        {
            newPanel.SetActive(true);

            // PATCH: Restore visual & eksplisit play ulang video.
            // Tanpa ini, panel yang diaktifkan ke-2x (mis. setelah
            // Next → Previous) hanya menampilkan audio, bukan video.
            RestorePanelVisual(newPanel);

            // 6) Fade in slide baru
            if (useFadeTransition && fadeDuration > 0f)
            {
                yield return StartCoroutine(FadePanel(newPanel, 0f, 1f, fadeDuration));
            }
        }

        // PATCH: Sembunyikan blanking overlay SETELAH slide baru
        // sudah mulai fade in, sehingga user melihat transisi yang halus
        // dan overlay hilang perlahan di atas slide baru.
        yield return null;
        HideBlankingOverlay();

        currentIndex = toIndex;
        isTransitioning = false;
    }

    /// <summary>
    /// PATCH: Stop semua VideoPlayer pada panel yang akan disembunyikan,
    /// lalu CLEAR RenderTexture-nya untuk benar-benar menghapus frame
    /// terakhir. NON-DESTRUCTIVE: tidak memutus referensi.
    ///
    /// CATATAN PENTING (bug fix #2):
    /// Jangan panggil `vp.targetTexture = null` atau `raw.texture = null`
    /// di sini. Jika diputus, saat panel diaktifkan ulang VideoPlayer
    /// tidak punya target untuk render, sehingga audio jalan tapi
    /// video tidak muncul.
    ///
    /// CATATAN PENTING (bug fix #3 — "frame leak / loncat"):
    /// Saat VideoPlayer.Stop() dipanggil, frame terakhir masih tertinggal
    /// di RenderTexture target. Jika di frame yang sama slide baru
    /// diaktifkan, RawImage dari panel lain yang SHARE RenderTexture
    /// yang sama akan me-render frame lama tersebut → kelihatan
    /// seperti "loncat" / stutter. Solusinya: panggil GL.Clear() pada
    /// RenderTexture sebelum SetActive(false).
    /// </summary>
    private void CleanupPanelVisual(GameObject panel)
    {
        if (panel == null) return;

        // 1) Stop VideoPlayer untuk hentikan audio & reset currentTime ke 0
        var videoPlayers = panel.GetComponentsInChildren<VideoPlayer>(true);
        foreach (var vp in videoPlayers)
        {
            if (vp == null) continue;
            try
            {
                if (vp.isPlaying) vp.Stop();
                // JANGAN: vp.targetTexture = null;
            }
            catch { /* ignore */ }
        }

        // 2) CLEAR RenderTexture pada RawImage — INI KUNCI FIX FRAME LEAK
        // GL.Clear menghapus pixel frame terakhir dari RT, sehingga
        // panel lain yang share RT tidak akan render frame lama.
        var rawImages = panel.GetComponentsInChildren<RawImage>(true);
        foreach (var raw in rawImages)
        {
            if (raw == null || raw.texture == null) continue;
            if (raw.texture is RenderTexture rt)
            {
                ClearRenderTexture(rt);
            }
        }

        // 3) Hide visual via CanvasGroup (non-destruktif)
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;
    }

    /// <summary>
    /// PATCH: Hapus semua pixel dari RenderTexture dengan GL.Clear.
    /// Mencegah "frame leak" (frame terakhir video lama terbawa ke slide baru).
    /// </summary>
    private void ClearRenderTexture(RenderTexture rt)
    {
        if (rt == null) return;

        try
        {
            var prevActive = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(true, true, Color.clear); // clear color + depth + stencil
            RenderTexture.active = prevActive;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[MateriNavigator] Gagal clear RenderTexture: {ex.Message}");
        }
    }

    /// <summary>
    /// PATCH: Saat panel diaktifkan ulang, restore visual dan
    /// eksplisit play ulang VideoPlayer. Ini safety net supaya
    /// animasi selalu muncul (bukan cuma audio).
    /// </summary>
    private void RestorePanelVisual(GameObject panel)
    {
        if (panel == null) return;

        // Restore CanvasGroup agar panel bisa dilihat & diinteraksi
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
        cg.blocksRaycasts = true;
        cg.interactable = true;

        // Eksplisit play ulang semua VideoPlayer di panel
        var videoPlayers = panel.GetComponentsInChildren<VideoPlayer>(true);
        foreach (var vp in videoPlayers)
        {
            if (vp == null) continue;
            try
            {
                // Cek apakah VideoPlayer punya target
                if (vp.targetTexture == null && vp.renderMode == VideoRenderMode.RenderTexture)
                {
                    Debug.LogWarning(
                        $"[MateriNavigator] VideoPlayer '{vp.name}' targetTexture null. " +
                        "Video tidak akan render. Pastikan Inspector Render Mode & " +
                        "Target Texture sudah di-assign.");
                }

                // Reset ke awal lalu play
                vp.time = 0d;
                vp.Play();
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[MateriNavigator] Gagal Play VideoPlayer '{vp.name}': {ex.Message}");
            }
        }
    }

    /// <summary>
    /// PATCH: Fade CanvasGroup dari nilai alpha start ke end.
    /// Jika panel tidak punya CanvasGroup, tambahkan otomatis.
    /// </summary>
    private IEnumerator FadePanel(GameObject panel, float fromAlpha, float toAlpha, float duration)
    {
        if (panel == null) yield break;

        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();

        // Supaya raycast mengikuti alpha (opsional, tidak block saat alpha 0)
        cg.interactable = (toAlpha > 0.5f);
        cg.blocksRaycasts = (toAlpha > 0.5f);

        if (duration <= 0f)
        {
            cg.alpha = toAlpha;
            yield break;
        }

        float elapsed = 0f;
        cg.alpha = fromAlpha;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            cg.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            yield return null;
        }

        cg.alpha = toAlpha;
    }

    /// <summary>
    /// Hubungkan tombol Next di tiap sub-panel ke method ini.
    /// Jika masih ada sub-panel berikutnya → tampilkan.
    /// Jika sudah sub-panel terakhir → simpan bintang 1 + pindah ke Kuis.
    /// </summary>
    public void OnNextPressed()
    {
        if (isTransitioning) return;

        int nextIndex = currentIndex + 1;

        if (nextIndex < subPanels.Length)
        {
            ShowPanel(nextIndex);
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
        if (isTransitioning) return;

        if (currentIndex > 0)
        {
            ShowPanel(currentIndex - 1);
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

    // ═════════════════════════════════════════════════════════════════
    //  PATCH: Direct Navigation (Back → Home, Next → Kuis)
    //  Metode ini dipakai oleh tombol Back/Next yang OnClick-nya
    //  di-wire manual ATAU otomatis oleh AutoWireNavigationButtons().
    // ═════════════════════════════════════════════════════════════════

    /// <summary>
    /// PATCH: Tombol Back yang langsung ke Home scene.
    /// BEDA dari OnBackPressed(): OnBackPressed() adalah navigasi
    /// slide-per-slide (ke slide sebelumnya), sedangkan OnBackToHome()
    /// selalu langsung ke Home scene.
    ///
    /// Cara wire manual di Inspector:
    ///   1. Pilih Button Back di Hierarchy
    ///   2. Di Inspector → Button → On Click (), klik +
    ///   3. Drag GameObject MateriNavigator ke slot
    ///   4. Pilih MateriNavigator → OnBackToHome()
    /// </summary>
    public void OnBackToHome()
    {
        if (isTransitioning) return;

        Debug.Log($"[MateriNavigator] Back → Home scene (Level {levelNumber})");
        SceneLoader.LoadScene(homeSceneName);
    }

    /// <summary>
    /// PATCH: Tombol Next yang langsung ke Kuis (untuk level ini).
    /// BEDA dari OnNextPressed(): OnNextPressed() adalah navigasi
    /// slide-per-slide, sedangkan OnNextToQuiz() selalu langsung ke Kuis.
    ///
    /// Cara wire manual di Inspector:
    ///   1. Pilih Button Next di Hierarchy
    ///   2. Di Inspector → Button → On Click (), klik +
    ///   3. Drag GameObject MateriNavigator ke slot
    ///   4. Pilih MateriNavigator → OnNextToQuiz()
    /// </summary>
    public void OnNextToQuiz()
    {
        if (isTransitioning) return;

        Debug.Log($"[MateriNavigator] Next → Kuis Level {levelNumber}");
        GoToKuis();
    }

    /// <summary>
    /// PATCH: Auto-wire Button Back/Next yang OnClick-nya masih kosong.
    /// - Scan semua Button di GameObject ini dan child-nya
    /// - Skip button yang sudah punya persistent listener (sudah di-wire manual)
    /// - Jika nama button mengandung salah satu backButtonKeywords
    ///   (mis. "back", "kembali", "home") → wire ke OnBackToHome()
    /// - Jika nama button mengandung salah satu nextButtonKeywords
    ///   (mis. "next", "lanjut", "quiz", "kuis") → wire ke OnNextToQuiz()
    ///
    /// Log hasil wiring ke Console.
    /// </summary>
    [ContextMenu("Auto-Wire Navigation Buttons Now")]
    public void AutoWireNavigationButtons()
    {
        // PATCH: Selalu jalan (tidak ada flag). Tombol yang sudah di-wire
        // manual akan di-skip via GetPersistentEventCount().

        var buttons = GetComponentsInChildren<Button>(true);
        int wired = 0;

        foreach (var btn in buttons)
        {
            if (btn == null) continue;

            // Skip button yang sudah punya persistent listener
            // (sudah di-wire manual di Inspector, jangan ditimpa)
            if (btn.onClick.GetPersistentEventCount() > 0) continue;

            string lowerName = btn.name.ToLower();
            UnityEngine.Events.UnityAction targetAction = null;

            // Cek apakah nama mengandung keyword back/home
            if (MatchesAnyKeyword(lowerName, backButtonKeywords))
            {
                targetAction = OnBackToHome;
            }
            // Cek apakah nama mengandung keyword next/quiz
            else if (MatchesAnyKeyword(lowerName, nextButtonKeywords))
            {
                targetAction = OnNextToQuiz;
            }

            if (targetAction != null)
            {
                btn.onClick.AddListener(targetAction);
                wired++;
                Debug.Log($"[MateriNavigator] Auto-wired Button '{btn.name}' " +
                          $"→ {targetAction.Method.Name}");
            }
        }

        if (wired > 0)
        {
            Debug.Log($"[MateriNavigator] Auto-wire selesai: {wired} button berhasil di-wire. " +
                      $"Cek Console di atas untuk detail tiap button.");
        }
    }

    /// <summary>
    /// Helper: cek apakah name mengandung salah satu keyword.
    /// </summary>
    private bool MatchesAnyKeyword(string name, string[] keywords)
    {
        if (keywords == null) return false;
        foreach (var kw in keywords)
        {
            if (string.IsNullOrEmpty(kw)) continue;
            if (name.Contains(kw.ToLower())) return true;
        }
        return false;
    }
}
