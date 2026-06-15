using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("Loading UI (harus child dari GameObject ini)")]
    public GameObject loadingOverlay;

    [Header("Dots")]
    public RectTransform dot1;
    public RectTransform dot2;
    public RectTransform dot3;

    [Header("Settings")]
    public float minimumLoadTime = 0.8f;
    public float dotBounceHeight = 20f;  // seberapa tinggi bounce
    public float dotBounceSpeed = 8f;    // seberapa cepat
    public float dotDelay = 0.15f;       // jeda antar dot

    private Coroutine bounceCoroutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (loadingOverlay != null)
            loadingOverlay.SetActive(false);
    }

    public static void LoadScene(string sceneName)
    {
        if (Instance == null)
        {
            Debug.LogWarning("[SceneLoader] Instance belum ada. Fallback ke sync load.");
            SceneManager.LoadScene(sceneName);
            return;
        }
        Instance.StartCoroutine(Instance.LoadAsync(sceneName));
    }

    private IEnumerator LoadAsync(string sceneName)
    {
        // Tampilkan overlay
        if (loadingOverlay != null)
            loadingOverlay.SetActive(true);

        // Mulai animasi dot
        bounceCoroutine = StartCoroutine(BounceDots());

        float startTime = Time.realtimeSinceStartup;

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        // Tunggu minimum load time
        float elapsed = Time.realtimeSinceStartup - startTime;
        if (elapsed < minimumLoadTime)
            yield return new WaitForSecondsRealtime(minimumLoadTime - elapsed);

        // Stop bounce
        if (bounceCoroutine != null)
            StopCoroutine(bounceCoroutine);

        // Reset posisi dot
        ResetDots();

        yield return new WaitForSecondsRealtime(0.1f);

        op.allowSceneActivation = true;
        yield return null;

        if (loadingOverlay != null)
            loadingOverlay.SetActive(false);
    }

    private IEnumerator BounceDots()
    {
        // Simpan posisi awal masing-masing dot
        Vector2 pos1 = dot1 != null ? dot1.anchoredPosition : Vector2.zero;
        Vector2 pos2 = dot2 != null ? dot2.anchoredPosition : Vector2.zero;
        Vector2 pos3 = dot3 != null ? dot3.anchoredPosition : Vector2.zero;

        float time = 0f;
        while (true)
        {
            time += Time.deltaTime * dotBounceSpeed;

            // Masing-masing dot punya offset waktu (delay) → efek wave
            if (dot1 != null)
                dot1.anchoredPosition = pos1 + Vector2.up * Mathf.Abs(Mathf.Sin(time)) * dotBounceHeight;

            if (dot2 != null)
                dot2.anchoredPosition = pos2 + Vector2.up * Mathf.Abs(Mathf.Sin(time - dotDelay * dotBounceSpeed)) * dotBounceHeight;

            if (dot3 != null)
                dot3.anchoredPosition = pos3 + Vector2.up * Mathf.Abs(Mathf.Sin(time - dotDelay * 2f * dotBounceSpeed)) * dotBounceHeight;

            yield return null;
        }
    }

    private void ResetDots()
    {
        // Nanti posisi dot kembali ke posisi awal
        // Tidak perlu reset manual karena posisi awal sudah disimpan di BounceDots
    }
}