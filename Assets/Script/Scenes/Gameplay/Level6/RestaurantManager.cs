using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Game Manager untuk Level 6 - Restaurant Serving Game.
/// Satu item makanan muncul di meja, player drag ke customer atau tong sampah.
/// Setelah diproses, item baru muncul otomatis.
/// </summary>
public class RestaurantManager : MonoBehaviour, IGameManager
{
    [Header("Game Settings")]
    [Tooltip("Durasi game dalam detik")]
    public float durasiTimer = 60f;
    [Tooltip("Target skor untuk menang")]
    public int targetSkor = 100;

    [Header("Food Spawn Settings")]
    [Tooltip("Satu titik spawn di tengah meja")]
    public Transform foodSpawnPoint;
    [Tooltip("Daftar makanan yang bisa muncul")]
    public FoodItemDataLv5[] daftarMakanan;
    [Tooltip("Prefab untuk food yang bisa di-drag")]
    public GameObject draggableFoodPrefab;
    [Tooltip("Delay (detik) sebelum item berikutnya muncul")]
    public float respawnDelay = 0.6f;

    [Header("Food Spawn Ratio")]
    [Tooltip("Persentase chance spawn makanan SEGAR (0-100)")]
    [Range(0, 100)]
    public int chanceSpawnSegar = 70;

    [Header("Customer Character")]
    [Tooltip("Reference ke CustomerCharacter component")]
    public CustomerCharacter customer;

    [Header("UI")]
    [Tooltip("TextMeshProUGUI untuk tampilan skor")]
    public TextMeshProUGUI teksSkor;
    [Tooltip("TextMeshProUGUI untuk tampilan timer (format MM:SS)")]
    public TextMeshProUGUI teksWaktu;
    [Tooltip("TextMeshProUGUI untuk countdown 3-2-1-GO! (opsional)")]
    public TextMeshProUGUI teksCountdown;

    [Header("UI Popup")]
    public GameObject popupHasil;
    public TextMeshProUGUI teksHasilSkor;
    [Tooltip("Teks angka BENAR di popup hasil (hijau)")]
    public TextMeshProUGUI teksBenar;
    [Tooltip("Teks angka SALAH di popup hasil (merah)")]
    public TextMeshProUGUI teksSalah;
    public GameObject popupPerintah;

    [Header("Scoring")]
    [Tooltip("Poin saat kasih makanan segar ke customer / buang makanan busuk")]
    public int poinSegar = 10;
    [Tooltip("Poin dikurangi saat kasih makanan busuk ke customer / buang makanan segar")]
    public int poinBusuk = -5;

    // Private
    private int skor = 0;
    private int jumlahBenar = 0;
    private int jumlahSalah = 0;
    private float timeLeft;
    private bool isPlaying = false;
    private GameObject currentFoodObject;   // Hanya 1 item sekaligus
    private bool isSpawning = false;        // Guard supaya tidak double-spawn

    void Start()
    {
        skor = 0;
        jumlahBenar = 0;
        jumlahSalah = 0;
        isPlaying = false;
        timeLeft = durasiTimer;

        if (popupPerintah != null) popupPerintah.SetActive(true);
        if (popupHasil != null) popupHasil.SetActive(false);

        UpdateScoreUI();
        UpdateTimerUI();

        // Validasi setup
        Debug.Log($"[Restaurant] Start() - SpawnPoint: {(foodSpawnPoint != null ? foodSpawnPoint.name : "NULL")}");
        Debug.Log($"[Restaurant] Daftar Makanan: {(daftarMakanan != null ? daftarMakanan.Length : 0)}");
        Debug.Log($"[Restaurant] Prefab: {(draggableFoodPrefab != null ? draggableFoodPrefab.name : "NULL")}");
    }

    public void MulaiGame()
    {
        Debug.Log("[Restaurant] MulaiGame() dipanggil");

        Time.timeScale = 1f;
        if (popupPerintah != null) popupPerintah.SetActive(false);
        if (popupHasil != null) popupHasil.SetActive(false);

        StartCoroutine(CountdownHelper.Hitung(teksCountdown, () =>
        {
            isPlaying = true;
            timeLeft = durasiTimer;

            SpawnFood();
            StartCoroutine(TimerCountdown());
        }));
    }

    // ─── Spawn 1 item makanan ──────────────────────────────────────────────

    void SpawnFood()
    {
        if (!isPlaying) return;
        if (isSpawning) return;

        if (foodSpawnPoint == null)
        {
            Debug.LogError("[Restaurant] foodSpawnPoint belum di-assign di Inspector!");
            return;
        }

        FoodItemDataLv5 data = PickFoodByRatio();
        if (data == null)
        {
            Debug.LogError("[Restaurant] Tidak ada data makanan!");
            return;
        }

        isSpawning = true;

        GameObject foodObj = Instantiate(
            draggableFoodPrefab,
            foodSpawnPoint.position,
            Quaternion.identity,
            foodSpawnPoint
        );

        DraggableFood draggable = foodObj.GetComponent<DraggableFood>();
        if (draggable != null)
        {
            draggable.Initialize(data, this, 0);
        }
        else
        {
            Debug.LogError("[Restaurant] DraggableFood tidak ditemukan di prefab!");
            Destroy(foodObj);
            isSpawning = false;
            return;
        }

        currentFoodObject = foodObj;
        isSpawning = false;

        Debug.Log($"[Restaurant] Spawn: {data.namaItem} | Segar: {data.isSegar}");
    }

    IEnumerator SpawnFoodDelayed()
    {
        yield return new WaitForSeconds(respawnDelay);
        if (isPlaying)
            SpawnFood();
    }

    // ─── Dipanggil oleh DraggableFood saat drop ke Customer ───────────────

    public void OnFoodServed(FoodItemDataLv5 food, GameObject foodObject, int spawnIndex)
    {
        if (!isPlaying) return;

        Debug.Log($"[Restaurant] Disajikan: {food.namaItem} | Segar: {food.isSegar}");

        if (food.isSegar)
        {
            // Makanan segar → benar, customer senang
            TambahSkor(poinSegar);
            CatatBenar();
            if (customer != null) customer.ReactToFood(true);
        }
        else
        {
            // Makanan busuk → salah, customer sakit
            TambahSkor(poinBusuk);
            CatatSalah();
            if (customer != null) customer.ReactToFood(false);
        }

        ClearCurrentFood(foodObject);
        StartCoroutine(SpawnFoodDelayed());
    }

    // ─── Dipanggil oleh DraggableFood saat drop ke TrashArea ──────────────

    public void OnFoodDiscarded(FoodItemDataLv5 food, GameObject foodObject, int spawnIndex)
    {
        if (!isPlaying) return;

        Debug.Log($"[Restaurant] Dibuang: {food.namaItem} | Segar: {food.isSegar}");

        if (!food.isSegar)
        {
            // Makanan busuk dibuang → benar, customer senang
            TambahSkor(poinSegar);
            CatatBenar();
            if (customer != null) customer.ReactToFood(true);
            Debug.Log("[Restaurant] Buang busuk - BENAR! +poin");
        }
        else
        {
            // Makanan segar dibuang sia-sia → salah
            TambahSkor(poinBusuk);
            CatatSalah();
            if (customer != null) customer.ReactToFood(false);
            Debug.Log("[Restaurant] Buang segar - SALAH! -poin");
        }

        ClearCurrentFood(foodObject);
        StartCoroutine(SpawnFoodDelayed());
    }

    // ─── Helper ───────────────────────────────────────────────────────────

    void CatatBenar()
    {
        jumlahBenar++;
        if (teksBenar != null)
            teksBenar.text = jumlahBenar.ToString("00");
    }

    void CatatSalah()
    {
        jumlahSalah++;
        if (teksSalah != null)
            teksSalah.text = jumlahSalah.ToString("00");
    }

    void ClearCurrentFood(GameObject foodObject)
    {
        currentFoodObject = null;
        if (foodObject != null)
            Destroy(foodObject);
    }

    FoodItemDataLv5 PickFoodByRatio()
    {
        if (daftarMakanan == null || daftarMakanan.Length == 0)
        {
            Debug.LogError("[Restaurant] daftarMakanan kosong!");
            return null;
        }

        var segarList = new List<FoodItemDataLv5>();
        var busukList = new List<FoodItemDataLv5>();

        foreach (var item in daftarMakanan)
        {
            if (item == null) continue;
            if (item.isSegar) segarList.Add(item);
            else busukList.Add(item);
        }

        if (segarList.Count == 0 && busukList.Count == 0)
            return daftarMakanan[0];

        if (segarList.Count == 0)
            return busukList[Random.Range(0, busukList.Count)];

        if (busukList.Count == 0)
            return segarList[Random.Range(0, segarList.Count)];

        return Random.Range(0, 100) < chanceSpawnSegar
            ? segarList[Random.Range(0, segarList.Count)]
            : busukList[Random.Range(0, busukList.Count)];
    }

    void TambahSkor(int nilai)
    {
        skor = Mathf.Clamp(skor + nilai, 0, targetSkor);
        UpdateScoreUI();

        if (skor >= targetSkor)
        {
            Debug.Log($"[Restaurant] Target tercapai! {skor}/{targetSkor}");
            GameOver(true);
        }
    }

    void UpdateScoreUI()
    {
        if (teksSkor != null)
            teksSkor.text = $"SKOR: {skor} / {targetSkor}";
    }

    IEnumerator TimerCountdown()
    {
        while (timeLeft > 0 && isPlaying)
        {
            timeLeft -= Time.deltaTime;
            UpdateTimerUI();
            yield return null;
        }

        if (isPlaying)
        {
            Debug.Log("[Restaurant] Waktu habis!");
            GameOver(false);
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

    void GameOver(bool menang)
    {
        if (!isPlaying) return;
        isPlaying = false;

        StopAllCoroutines();

        // Hapus makanan yang masih ada
        if (currentFoodObject != null)
        {
            Destroy(currentFoodObject);
            currentFoodObject = null;
        }

        LevelProgressManager.CompleteMiniGame(PlayerPrefs.GetInt("CurrentLevel", 1));

        if (popupHasil != null)
        {
            popupHasil.SetActive(true);

            // Skor akhir = targetSkor - (jumlahSalah × denda per salah)
            // Misal: 3 salah → 100 - (3×5) = 85
            int denda = Mathf.Abs(poinBusuk);
            int skorAkhir = Mathf.Clamp(targetSkor - (jumlahSalah * denda), 0, targetSkor);

            if (teksHasilSkor != null)
                teksHasilSkor.text = skorAkhir.ToString();
            if (teksBenar != null)
                teksBenar.text = jumlahBenar.ToString("00");
            if (teksSalah != null)
                teksSalah.text = jumlahSalah.ToString("00");
        }

        Debug.Log($"[Restaurant] GameOver - menang:{menang}, skor:{skor}");
    }

    public void OnTombolLanjut()   => LevelFlowManager.OnGameSelesai();
    public void OnTombolCobaLagi() => SceneLoader.LoadScene(
        UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    public void OnTombolHome()     => LevelFlowManager.GoToRouteMap();
}
