using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class SpotTheDifference : MonoBehaviour, IGameManager
{
    [Header("Gambar")]
    public Image imageBComponent;

    [Header("Area Perbedaan (Manual)")]
    public List<RectTransform> differenceAreas;

    [Header("UI Game")]
    public GameObject highlightPrefab;
    public Transform highlightParent;

    [Header("UI Timer & Countdown")]
    [Tooltip("TextMeshProUGUI untuk tampilan 3-2-1-GO! (opsional)")]
    public TextMeshProUGUI teksCountdown;
    [Tooltip("TextMeshProUGUI untuk tampilan waktu tersisa (format MM:SS)")]
    public TextMeshProUGUI teksWaktu;

    [Header("Popup Hasil")]
    public GameObject popup;
    public TextMeshProUGUI teksHasil;   // teks skor misal "4/4"
    public TextMeshProUGUI teksBenar;   // teks jumlah benar
    public TextMeshProUGUI teksSalah;   // teks jumlah salah

    private List<bool> foundList = new List<bool>();
    private int jumlahBenar = 0;
    private int jumlahSalah = 0;

    // Timer
    private float timeLeft = 30f;
    private bool gameActive = false;  // true setelah countdown selesai

    // Implementasi IGameManager — mulai countdown lalu aktifkan interaksi
    public void MulaiGame()
    {
        gameActive = false;
        timeLeft = 30f;
        StartCoroutine(CountdownHelper.Hitung(teksCountdown, () =>
        {
            gameActive = true;
            StartCoroutine(TimerCountdown());
        }));
    }

    void Start()
    {
        foundList.Clear();
        foreach (var area in differenceAreas)
            foundList.Add(false);

        if (popup != null)
            popup.SetActive(false);

        UpdateTimerUI();
    }

    public void OnClickImageB(PointerEventData pointerData)
    {
        if (!gameActive) return;   // abaikan klik sebelum countdown selesai
        Vector2 screenPos = pointerData.position;
        CheckClick(screenPos);
    }

    void CheckClick(Vector2 screenPos)
    {
        for (int i = 0; i < differenceAreas.Count; i++)
        {
            if (foundList[i]) continue;

            RectTransform area = differenceAreas[i];

            if (RectTransformUtility.RectangleContainsScreenPoint(area, screenPos))
            {
                foundList[i] = true;
                jumlahBenar++;

                SpawnHighlight(area.position, true);
                CheckAllFound();
                return;
            }
        }

        // Klik salah
        jumlahSalah++;
        SpawnHighlight(screenPos, false);
    }

    void SpawnHighlight(Vector2 screenPos, bool isCorrect)
    {
        if (highlightPrefab == null || highlightParent == null)
        {
            Debug.LogError("Highlight Prefab / Parent belum diisi!");
            return;
        }

        GameObject obj = Instantiate(highlightPrefab, highlightParent);

        RectTransform parentRect = highlightParent as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            screenPos,
            null,
            out Vector2 localPos
        );

        obj.GetComponent<RectTransform>().localPosition = localPos;

        Image img = obj.GetComponent<Image>();
        if (img != null)
            img.color = isCorrect ? Color.green : Color.red;

        if (!isCorrect) Destroy(obj, 1f);
    }

    void CheckAllFound()
    {
        foreach (bool found in foundList)
            if (!found) return;

        // Semua perbedaan ditemukan → menang!
        gameActive = false;
        StopAllCoroutines();
        TampilkanPopup(true);
    }

    IEnumerator TimerCountdown()
    {
        while (timeLeft > 0 && gameActive)
        {
            timeLeft -= Time.deltaTime;
            UpdateTimerUI();
            yield return null;
        }

        // Waktu habis
        if (gameActive)
        {
            gameActive = false;
            TampilkanPopup(false);
        }
    }

    void UpdateTimerUI()
    {
        if (teksWaktu != null)
        {
            int detik = Mathf.CeilToInt(Mathf.Max(0f, timeLeft));
            teksWaktu.text = string.Format("{0:00}:{1:00}", detik / 60, detik % 60);
        }
    }

    void TampilkanPopup(bool menang)
    {
        // Isi teks hasil
        if (teksHasil != null)
        {
            if (menang)
                teksHasil.text = "BERHASIL!";
            else
                teksHasil.text = "WAKTU HABIS!";
        }

        if (teksBenar != null)
            teksBenar.text = jumlahBenar.ToString();

        if (teksSalah != null)
            teksSalah.text = jumlahSalah.ToString();

        // Simpan bintang 3 ke PlayerPrefs → tampil di RouteMap
        int level = LevelFlowManager.GetCurrentLevel();
        LevelProgressManager.CompleteMiniGame(level);

        if (popup != null)
            popup.SetActive(true);
    }

    /// <summary>
    /// Hubungkan ke LanjutButton di popup.
    /// Unlock level berikutnya dan kembali ke RouteMap.
    /// </summary>
    public void OnTombolLanjut()
    {
        LevelFlowManager.OnGameSelesai();
    }

    /// <summary>
    /// Hubungkan ke CobaLagiButton di popup.
    /// Reset game dari awal.
    /// </summary>
    public void OnTombolCobaLagi()
    {
        jumlahBenar = 0;
        jumlahSalah = 0;
        timeLeft = 30f;

        foundList.Clear();
        foreach (var area in differenceAreas)
            foundList.Add(false);

        // Hapus semua highlight
        foreach (Transform child in highlightParent)
            Destroy(child.gameObject);

        if (popup != null)
            popup.SetActive(false);

        // Mulai ulang game
        MulaiGame();
    }

    /// <summary>
    /// Hubungkan ke HomeButton di popup.
    /// Kembali ke RouteMap tanpa unlock level berikutnya.
    /// </summary>
    public void OnTombolHome()
    {
        LevelFlowManager.GoToRouteMap();
    }
}
