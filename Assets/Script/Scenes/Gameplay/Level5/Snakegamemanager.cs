using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Game Manager untuk Level 5: Ular Makan Pintar
/// Letakkan di GameObject "SnakeGameManager" di scene Game (panel Level 5)
/// </summary>
public class SnakeGameManager : MonoBehaviour
{
    // ─── Grid Settings ───────────────────────────────────────────────────────
    [Header("Grid")]
    public int gridWidth  = 8;
    public int gridHeight = 5;
    public float cellSize = 120f;       // ukuran cell dalam pixel (sesuaikan di inspector)
    public RectTransform gridContainer; // Panel kosong sebagai area grid

    // ─── Snake ───────────────────────────────────────────────────────────────
    [Header("Snake")]
    public GameObject snakeHeadPrefab;    // Prefab kepala ular (dengan sprite detektif)
    public GameObject snakeBodyPrefab;    // Prefab badan ular
    public float moveInterval = 0.35f;   // Detik per langkah (makin kecil = makin cepat)

    // ─── Food ────────────────────────────────────────────────────────────────
    [Header("Food")]
    public FoodItemDataLv5[] daftarMakanan; // Isi di Inspector dengan ScriptableObjects
    public GameObject foodPrefab;           // Prefab food (Image + FoodItemLv5 component)
    public int jumlahFoodAwal = 6;         // Berapa food yang di-spawn di awal

    // ─── UI ──────────────────────────────────────────────────────────────────
    [Header("UI")]
    public TextMeshProUGUI teksSkor;
    public Slider progressBar;              // Rainbow progress bar
    public int targetSkor = 100;            // Skor target untuk menang
    public GameObject popupHasil;
    public TextMeshProUGUI teksHasilSkor;
    public TextMeshProUGUI teksHasilPesan;
    public GameObject popupPerintah;        // Popup instruksi sebelum mulai

    [Header("Hindari Panel")]
    public Image[] iconHindari;            // 3 icon makanan yang harus dihindari
    public TextMeshProUGUI[] teksHindari;  // Nama makanan yang harus dihindari

    [Header("Dialog Detektif")]
    public TextMeshProUGUI teksDialog;     // Speech bubble detektif
    public float dialogDuration = 2.5f;

    // ─── Private State ───────────────────────────────────────────────────────
    private List<Vector2Int> snakeBody = new List<Vector2Int>(); // posisi tiap segment
    private List<GameObject> snakeObjects = new List<GameObject>();
    private Vector2Int direction = Vector2Int.right;
    private Vector2Int nextDirection = Vector2Int.right;

    private Dictionary<Vector2Int, GameObject> foodObjects = new Dictionary<Vector2Int, GameObject>();
    private HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

    private int skor = 0;
    private bool isPlaying = false;
    private Coroutine moveCoroutine;
    private Coroutine dialogCoroutine;

    // Dialog hints
    private string[] dialogSegar  = { "Makan yang segar!", "Bagus! Lanjutkan!", "Detektif hebat!" };
    private string[] dialogBahaya = { "Hati-hati! Itu meragukan!", "Jangan dimakan!", "Awas bahaya!" };

    // ─────────────────────────────────────────────────────────────────────────

    void Start()
    {
        // Tampilkan popup perintah sebelum mulai
        if (popupPerintah != null)
            popupPerintah.SetActive(true);

        // Isi panel Hindari
        IsiPanelHindari();
    }

    // Dipanggil dari tombol "Mulai" di popup perintah
    public void MulaiGame()
    {
        if (popupPerintah != null)
            popupPerintah.SetActive(false);

        StartCoroutine(CountdownMulai());
    }

    IEnumerator CountdownMulai()
    {
        TampilkanDialog("Siap?");
        yield return new WaitForSeconds(1f);
        TampilkanDialog("3...");
        yield return new WaitForSeconds(1f);
        TampilkanDialog("2...");
        yield return new WaitForSeconds(1f);
        TampilkanDialog("1...");
        yield return new WaitForSeconds(1f);
        TampilkanDialog("MULAI!");

        InitSnake();
        SpawnFoodAwal();

        isPlaying = true;
        moveCoroutine = StartCoroutine(GameLoop());
    }

    // ─── Inisialisasi Snake ──────────────────────────────────────────────────

    void InitSnake()
    {
        snakeBody.Clear();
        foreach (var obj in snakeObjects) Destroy(obj);
        snakeObjects.Clear();
        occupiedCells.Clear();

        // Spawn kepala di tengah grid
        Vector2Int startPos = new Vector2Int(gridWidth / 2, gridHeight / 2);
        snakeBody.Add(startPos);
        occupiedCells.Add(startPos);

        GameObject head = Instantiate(snakeHeadPrefab, gridContainer);
        SetPositionUI(head.GetComponent<RectTransform>(), startPos);
        snakeObjects.Add(head);

        direction = Vector2Int.right;
        nextDirection = Vector2Int.right;
    }

    // ─── Spawn Food ──────────────────────────────────────────────────────────

    void SpawnFoodAwal()
    {
        for (int i = 0; i < jumlahFoodAwal; i++)
            SpawnSatuFood();
    }

    void SpawnSatuFood()
    {
        if (daftarMakanan == null || daftarMakanan.Length == 0) return;

        Vector2Int pos = GetRandomEmptyCell();
        if (pos == -Vector2Int.one) return; // grid penuh

        FoodItemDataLv5 data = daftarMakanan[Random.Range(0, daftarMakanan.Length)];
        GameObject foodObj = Instantiate(foodPrefab, gridContainer);

        FoodItemLv5 foodComp = foodObj.GetComponent<FoodItemLv5>();
        foodComp.Setup(data);

        SetPositionUI(foodObj.GetComponent<RectTransform>(), pos);
        foodObjects[pos] = foodObj;
        occupiedCells.Add(pos);
    }

    // ─── Game Loop ───────────────────────────────────────────────────────────

    IEnumerator GameLoop()
    {
        while (isPlaying)
        {
            yield return new WaitForSeconds(moveInterval);
            if (isPlaying) MoveSnake();
        }
    }

    void MoveSnake()
    {
        direction = nextDirection;
        Vector2Int newHead = snakeBody[0] + direction;

        // Cek tabrakan dinding
        if (newHead.x < 0 || newHead.x >= gridWidth ||
            newHead.y < 0 || newHead.y >= gridHeight)
        {
            GameOver(false);
            return;
        }

        // Cek tabrakan badan sendiri
        if (snakeBody.Count > 1 && snakeBody.Contains(newHead))
        {
            GameOver(false);
            return;
        }

        bool tumbuh = false;

        // Cek apakah ada makanan
        if (foodObjects.ContainsKey(newHead))
        {
            FoodItemLv5 food = foodObjects[newHead].GetComponent<FoodItemLv5>();

            if (food.data.isSegar)
            {
                // Makanan segar: tambah skor, ular tumbuh
                TambahSkor(10);
                tumbuh = true;
                TampilkanDialog(dialogSegar[Random.Range(0, dialogSegar.Length)]);
            }
            else
            {
                // Makanan meragukan: kurang skor
                TambahSkor(-5);
                TampilkanDialog(dialogBahaya[Random.Range(0, dialogBahaya.Length)]);
            }

            // Hapus food lama, spawn food baru
            Destroy(foodObjects[newHead]);
            foodObjects.Remove(newHead);
            occupiedCells.Remove(newHead);
            SpawnSatuFood();
        }

        // Gerakkan snake: tambah kepala baru
        snakeBody.Insert(0, newHead);
        occupiedCells.Add(newHead);

        // Tambah GameObject kepala baru
        GameObject newHeadObj = Instantiate(snakeHeadPrefab, gridContainer);
        SetPositionUI(newHeadObj.GetComponent<RectTransform>(), newHead);

        // Ubah kepala lama jadi badan
        if (snakeObjects.Count > 0)
        {
            Destroy(snakeObjects[0]);
            snakeObjects[0] = null; // akan diganti
        }

        snakeObjects.Insert(0, newHeadObj);

        // Buat badan di posisi kedua (kalau ada)
        if (snakeBody.Count > 1)
        {
            GameObject bodyObj = Instantiate(snakeBodyPrefab, gridContainer);
            SetPositionUI(bodyObj.GetComponent<RectTransform>(), snakeBody[1]);
            if (snakeObjects.Count > 1)
            {
                Destroy(snakeObjects[1]);
                snakeObjects[1] = bodyObj;
            }
            else
            {
                snakeObjects.Add(bodyObj);
            }
        }

        // Kalau tidak tumbuh, hapus ekor
        if (!tumbuh)
        {
            Vector2Int tail = snakeBody[snakeBody.Count - 1];
            occupiedCells.Remove(tail);
            snakeBody.RemoveAt(snakeBody.Count - 1);

            if (snakeObjects.Count > 0)
            {
                int lastIdx = snakeObjects.Count - 1;
                Destroy(snakeObjects[lastIdx]);
                snakeObjects.RemoveAt(lastIdx);
            }
        }

        // Cek menang
        if (skor >= targetSkor)
        {
            GameOver(true);
        }
    }

    // ─── Input D-Pad ─────────────────────────────────────────────────────────
    // Assign ke tombol D-Pad di Inspector

    public void OnTombolAtas()
    {
        if (direction != Vector2Int.down)
            nextDirection = Vector2Int.up;
    }

    public void OnTombolBawah()
    {
        if (direction != Vector2Int.up)
            nextDirection = Vector2Int.down;
    }

    public void OnTombolKiri()
    {
        if (direction != Vector2Int.right)
            nextDirection = Vector2Int.left;
    }

    public void OnTombolKanan()
    {
        if (direction != Vector2Int.left)
            nextDirection = Vector2Int.right;
    }

    // ─── Skor & UI ───────────────────────────────────────────────────────────

    void TambahSkor(int nilai)
    {
        skor = Mathf.Max(0, skor + nilai);
        teksSkor.text = "SKOR: " + skor;

        if (progressBar != null)
            progressBar.value = (float)skor / targetSkor;
    }

    void TampilkanDialog(string pesan)
    {
        if (teksDialog == null) return;
        if (dialogCoroutine != null) StopCoroutine(dialogCoroutine);
        dialogCoroutine = StartCoroutine(DialogCoroutine(pesan));
    }

    IEnumerator DialogCoroutine(string pesan)
    {
        teksDialog.text = pesan;
        teksDialog.gameObject.SetActive(true);
        yield return new WaitForSeconds(dialogDuration);
        teksDialog.text = "Cari makanan segar!\nHindari yang meragukan!";
    }

    void IsiPanelHindari()
    {
        if (daftarMakanan == null) return;

        int idx = 0;
        foreach (var item in daftarMakanan)
        {
            if (!item.isSegar && idx < iconHindari.Length)
            {
                iconHindari[idx].sprite = item.spriteItem;
                if (teksHindari.Length > idx)
                    teksHindari[idx].text = (idx + 1) + ". " + item.namaItem;
                idx++;
            }
        }
    }

    // ─── Game Over ───────────────────────────────────────────────────────────

    void GameOver(bool menang)
    {
        isPlaying = false;
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);

        // Tandai level selesai
        int level = PlayerPrefs.GetInt("CurrentLevel", 1);
        LevelProgressManager.CompleteMiniGame(level);

        // Tampilkan popup hasil
        if (popupHasil != null)
        {
            popupHasil.SetActive(true);
            teksHasilSkor.text = skor.ToString();
            teksHasilPesan.text = menang
                ? "Hebat! Kamu Detektif Gizi Terbaik!"
                : "Jangan menyerah, coba lagi!";
        }
    }

    public void OnTombolSelesai()
    {
        LevelFlowManager.OnGameSelesai();
    }

    public void OnTombolUlang()
    {
        UnityEngine.SceneManagement.SceneManager
            .LoadScene(UnityEngine.SceneManagement.SceneManager
            .GetActiveScene().name);
    }

    // ─── Helper ──────────────────────────────────────────────────────────────

    void SetPositionUI(RectTransform rect, Vector2Int gridPos)
    {
        // Posisi berdasarkan grid (pivot center)
        float x = gridPos.x * cellSize;
        float y = gridPos.y * cellSize;
        rect.anchoredPosition = new Vector2(x, y);
        rect.sizeDelta = new Vector2(cellSize, cellSize);
    }

    Vector2Int GetRandomEmptyCell()
    {
        List<Vector2Int> emptyCells = new List<Vector2Int>();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                if (!occupiedCells.Contains(cell))
                    emptyCells.Add(cell);
            }
        }

        if (emptyCells.Count == 0) return -Vector2Int.one;
        return emptyCells[Random.Range(0, emptyCells.Count)];
    }
}