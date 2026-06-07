using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using TMPro;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

/// <summary>
/// Game Manager untuk Level 7 - Word Search (Cari Kata).
///
/// SETUP DI UNITY EDITOR:
/// 1. Buat Panel GridLayoutGroup → assign ke gridContainer
/// 2. Buat Cell Prefab (Image + TextMeshProUGUI + WordSearchCell) → assign ke cellPrefab
/// 3. Buat Highlight Prefab (Image rounded/rectangle) → assign ke highlightPrefab
/// 4. Buat Panel kosong sebagai parent highlight → assign ke highlightContainer
/// 5. Isi gridRows (tiap string = satu baris huruf kapital, panjang sama)
/// 6. Isi kataYangDicari dengan kata-kata tersembunyi di grid
/// </summary>
public class WordSearchManager : MonoBehaviour, IGameManager
{
    [Header("Grid Data")]
    [Tooltip("Tiap string = satu baris huruf KAPITAL. Semua baris harus panjang sama.")]
    public string[] gridRows = {
        "ILQABHMZBHSRHS",
        "PUSINGQSASNBBR",
        "PMUALONKEHRYHC",
        "AFORSAKITPERUT",
        "IGULLXNMUNTAHH",
        "IGULWXNREMQZTZ"
    };

    [Tooltip("Daftar kata yang harus ditemukan pemain (kapital)")]
    public string[] kataYangDicari = {
        "SAKIT", "PERUT", "MUAL", "PUSING", "MUNTAH"
    };

    [Header("Prefabs & References")]
    [Tooltip("Prefab sel huruf — kosongkan jika sel sudah ada di gridContainer")]
    public GameObject cellPrefab;
    [Tooltip("Panel yang berisi sel-sel huruf (GridLayoutGroup atau manual)")]
    public Transform gridContainer;
    [Tooltip("Prefab Image untuk highlight bar")]
    public GameObject highlightPrefab;
    [Tooltip("Panel parent untuk semua highlight (di atas grid)")]
    public Transform highlightContainer;

    [Header("UI")]
    public TextMeshProUGUI teksSkor;
    public TextMeshProUGUI teksWaktu;
    public TextMeshProUGUI teksCountdown;
    [Tooltip("Teks daftar kata yang belum ditemukan")]
    public TextMeshProUGUI teksKataTarget;
    public GameObject popupHasil;
    public TextMeshProUGUI teksHasilSkor;
    public GameObject popupPerintah;

    [Header("Timer")]
    public float durasiTimer = 120f;

    [Header("Scoring")]
    [Tooltip("Poin per kata yang ditemukan")]
    public int poinPerKata = 10;

    [Header("Warna Highlight")]
    [Tooltip("Warna saat sedang memilih (sementara)")]
    public Color warnaPilihan = new Color(1f, 0.25f, 0.25f, 0.55f);
    [Tooltip("Warna saat kata ditemukan (permanen)")]
    public Color warnaBenar   = new Color(0.2f, 0.85f, 0.2f, 0.65f);

    // ─── Private ──────────────────────────────────────────────────────
    private WordSearchCell[,] grid;
    private int rowCount, colCount;

    private bool isDragging = false;
    private List<WordSearchCell> selectedCells = new List<WordSearchCell>();
    private Vector2Int selectionDir = Vector2Int.zero;
    private GameObject currentHighlight;

    private HashSet<string> foundWords = new HashSet<string>();

    private int skor = 0;
    private float timeLeft;
    private bool isPlaying = false;

    // ─── Lifecycle ────────────────────────────────────────────────────

    void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    void Start()
    {
        skor = 0;
        timeLeft = durasiTimer;
        isPlaying = false;

        if (popupHasil != null) popupHasil.SetActive(false);

        UpdateScoreUI();
        UpdateTimerUI();
        UpdateWordListUI();

        BuildGrid();

        // Jika ada popup perintah, tampilkan dan tunggu tombol MulaiGame()
        // Jika tidak ada popup, langsung mulai game
        if (popupPerintah != null)
            popupPerintah.SetActive(true);
        else
            MulaiGame(); // auto-start jika tidak ada popup
    }

    public void MulaiGame()
    {
        if (popupPerintah != null) popupPerintah.SetActive(false);

        // Pastikan HighlightLayer tidak memblokir input ke grid
        if (highlightContainer != null)
        {
            Image hlImg = highlightContainer.GetComponent<Image>();
            if (hlImg != null) hlImg.raycastTarget = false;
        }

        StartCoroutine(CountdownHelper.Hitung(teksCountdown, () =>
        {
            isPlaying = true;
            StartCoroutine(TimerCountdown());
        }));
    }

    // ─── Grid Building ────────────────────────────────────────────────

    void BuildGrid()
    {
        if (gridRows == null || gridRows.Length == 0)
        {
            Debug.LogError("[WordSearch] gridRows kosong!");
            return;
        }

        rowCount = gridRows.Length;
        colCount = gridRows[0].Length;
        grid    = new WordSearchCell[rowCount, colCount];

        // MODE A: Sel sudah ada di scene (cellPrefab kosong)
        if (cellPrefab == null || gridContainer.childCount >= rowCount * colCount)
        {
            InitDariExistingCells();
        }
        else
        {
            // MODE B: Generate sel baru dari prefab
            foreach (Transform child in gridContainer)
                Destroy(child.gameObject);

            for (int r = 0; r < rowCount; r++)
                for (int c = 0; c < colCount; c++)
                {
                    char letter = (c < gridRows[r].Length) ? gridRows[r][c] : ' ';
                    GameObject cellObj = Instantiate(cellPrefab, gridContainer);
                    cellObj.name = $"Cell_{r}_{c}";
                    WordSearchCell cell = cellObj.GetComponent<WordSearchCell>()
                                      ?? cellObj.AddComponent<WordSearchCell>();
                    cell.Initialize(r, c, letter);
                    grid[r, c] = cell;

                    // Pastikan Image punya Raycast Target = true agar bisa dideteksi
                    Image img = cellObj.GetComponent<Image>();
                    if (img != null) img.raycastTarget = true;
                }

            Debug.Log($"[WordSearch] Grid {rowCount}×{colCount} dibuat dari prefab.");
        }
    }

    /// <summary>
    /// Pakai sel yang sudah ada di gridContainer sebagai children.
    /// Urutan children harus: kiri-ke-kanan, atas-ke-bawah
    /// (sama seperti urutan GridLayoutGroup).
    /// </summary>
    void InitDariExistingCells()
    {
        int totalDibutuhkan = rowCount * colCount;
        int totalAda        = gridContainer.childCount;

        if (totalAda < totalDibutuhkan)
        {
            Debug.LogWarning($"[WordSearch] Jumlah child ({totalAda}) kurang dari yang dibutuhkan ({totalDibutuhkan}).");
        }

        int idx = 0;
        for (int r = 0; r < rowCount; r++)
        {
            for (int c = 0; c < colCount; c++, idx++)
            {
                if (idx >= totalAda) break;

                Transform t = gridContainer.GetChild(idx);

                // Tambah WordSearchCell otomatis jika belum ada
                WordSearchCell cell = t.GetComponent<WordSearchCell>()
                                  ?? t.gameObject.AddComponent<WordSearchCell>();

                char letter = (c < gridRows[r].Length) ? gridRows[r][c] : ' ';
                cell.Initialize(r, c, letter);

                // Pastikan Image punya Raycast Target = true agar bisa dideteksi
                Image img = t.GetComponent<Image>();
                if (img != null) img.raycastTarget = true;

                grid[r, c] = cell;
            }
        }

        Debug.Log($"[WordSearch] Grid {rowCount}×{colCount} dari {totalAda} existing cells.");
    }

    // ─── Input Handling (Update + Raycast) ───────────────────────────

    void Update()
    {
        if (!isPlaying) return;

        bool pressing = false;
        Vector2 inputPos = Vector2.zero;

        // Cek touch (mobile)
        if (Touchscreen.current != null && Touch.activeTouches.Count > 0)
        {
            var t = Touch.activeTouches[0];
            pressing = t.phase != UnityEngine.InputSystem.TouchPhase.Ended
                    && t.phase != UnityEngine.InputSystem.TouchPhase.Canceled;
            inputPos = t.screenPosition;
        }
        // Cek mouse (editor / PC)
        else if (Mouse.current != null)
        {
            pressing = Mouse.current.leftButton.isPressed;
            inputPos = Mouse.current.position.ReadValue();
        }

        if (!pressing && isDragging)
        {
            FinishSelection();
            return;
        }

        if (pressing)
        {
            WordSearchCell cellUnder = GetCellAtScreenPos(inputPos);
            if (cellUnder == null) return;

            if (!isDragging)
                StartSelection(cellUnder);
            else
                TryExtendSelection(cellUnder);
        }
    }

    WordSearchCell GetCellAtScreenPos(Vector2 screenPos)
    {
        var pointerData = new PointerEventData(EventSystem.current) { position = screenPos };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var r in results)
        {
            var cell = r.gameObject.GetComponent<WordSearchCell>();
            if (cell != null) return cell;
        }
        return null;
    }

    // ─── Selection Logic ──────────────────────────────────────────────

    void StartSelection(WordSearchCell cell)
    {
        isDragging = true;
        selectedCells.Clear();
        selectionDir = Vector2Int.zero;
        selectedCells.Add(cell);

        if (currentHighlight != null) Destroy(currentHighlight);
        currentHighlight = CreateHighlight(warnaPilihan);
        if (currentHighlight == null)
        {
            Debug.LogError("[WordSearch] FAILED to create highlight! Check highlightPrefab and highlightContainer in Inspector.");
            isDragging = false;
            selectedCells.Clear();
            return;
        }
        RefreshHighlight(currentHighlight, selectedCells);
    }

    void TryExtendSelection(WordSearchCell cell)
    {
        if (selectedCells.Count == 0) return;

        // Backtrack: kalau cell sudah ada di seleksi, potong ke sana
        int existingIdx = selectedCells.IndexOf(cell);
        if (existingIdx >= 0)
        {
            if (existingIdx < selectedCells.Count - 1)
            {
                selectedCells = selectedCells.GetRange(0, existingIdx + 1);
                if (selectedCells.Count <= 1) selectionDir = Vector2Int.zero;
                RefreshHighlight(currentHighlight, selectedCells);
            }
            return;
        }

        WordSearchCell last = selectedCells[selectedCells.Count - 1];
        int dr = cell.row - last.row;
        int dc = cell.col - last.col;

        // Hanya satu langkah per arah (termasuk diagonal)
        if (Mathf.Abs(dr) > 1 || Mathf.Abs(dc) > 1) return;
        if (dr == 0 && dc == 0) return;

        Vector2Int newDir = new Vector2Int(dc, dr);

        if (selectionDir == Vector2Int.zero)
        {
            selectionDir = newDir;
        }
        else if (newDir != selectionDir)
        {
            // Arah tidak konsisten, abaikan
            return;
        }

        selectedCells.Add(cell);
        RefreshHighlight(currentHighlight, selectedCells);
    }

    void FinishSelection()
    {
        isDragging = false;

        if (selectedCells.Count < 2)
        {
            ClearCurrentHighlight();
            selectedCells.Clear();
            selectionDir = Vector2Int.zero;
            return;
        }

        string selected = BuildSelectedString();
        string reversed = ReverseString(selected);

        string matchedWord = null;
        foreach (string word in kataYangDicari)
        {
            string w = word.ToUpper().Trim();
            if ((selected == w || reversed == w) && !foundWords.Contains(w))
            {
                matchedWord = w;
                break;
            }
        }

        if (matchedWord != null)
        {
            // ✅ Kata ditemukan — highlight permanen
            foundWords.Add(matchedWord);
            TambahSkor(poinPerKata);
            UpdateWordListUI();

            // Buat highlight permanen hijau
            GameObject permanentHL = CreateHighlight(warnaBenar);
            RefreshHighlight(permanentHL, selectedCells);

            // Hapus highlight sementara
            ClearCurrentHighlight();

            Debug.Log($"[WordSearch] Kata ditemukan: {matchedWord}! Skor: {skor}");

            if (foundWords.Count >= kataYangDicari.Length)
            {
                StartCoroutine(TungguLaluGameOver(true));
            }
        }
        else
        {
            // ❌ Salah — hapus highlight
            ClearCurrentHighlight();
            Debug.Log($"[WordSearch] Kata tidak valid: {selected}");
        }

        selectedCells.Clear();
        selectionDir = Vector2Int.zero;
    }

    IEnumerator TungguLaluGameOver(bool menang)
    {
        yield return new WaitForSeconds(0.5f);
        GameOver(menang);
    }

    void ClearCurrentHighlight()
    {
        if (currentHighlight != null)
        {
            Destroy(currentHighlight);
            currentHighlight = null;
        }
    }

    string BuildSelectedString()
    {
        string s = "";
        foreach (var cell in selectedCells)
            s += cell.letter;
        return s;
    }

    static string ReverseString(string s)
    {
        var arr = s.ToCharArray();
        System.Array.Reverse(arr);
        return new string(arr);
    }

    // ─── Highlight Drawing ────────────────────────────────────────────

    GameObject CreateHighlight(Color color)
    {
        if (highlightPrefab == null)
        {
            Debug.LogError("[WordSearch] highlightPrefab is NULL! Assign in Inspector.");
            return null;
        }
        if (highlightContainer == null)
        {
            Debug.LogError("[WordSearch] highlightContainer is NULL! Assign in Inspector.");
            return null;
        }


        GameObject hl = Instantiate(highlightPrefab, highlightContainer);
        hl.name = "Highlight_" + Time.time.ToString("F2");


        // Ensure highlight renders on TOP of cells
        hl.transform.SetAsLastSibling();

        Image img = hl.GetComponent<Image>();
        if (img != null) img.color = color;

        Debug.Log($"[WordSearch] Highlight created: {hl.name}, pos={hl.transform.position}, container={highlightContainer.name}");
        return hl;
    }

    void RefreshHighlight(GameObject highlight, List<WordSearchCell> cells)
    {
        if (highlight == null || cells == null || cells.Count == 0)
        {
            Debug.LogError($"[WordSearch] RefreshHighlight skipped: highlight={highlight != null}, cells={(cells != null ? cells.Count.ToString() : "null")}");
            return;
        }

        RectTransform hlRect = highlight.GetComponent<RectTransform>();
        if (hlRect == null)
        {
            Debug.LogError("[WordSearch] Highlight has no RectTransform!");
            return;
        }

        RectTransform firstRect = cells[0].GetComponent<RectTransform>();
        RectTransform lastRect  = cells[cells.Count - 1].GetComponent<RectTransform>();
        if (firstRect == null || lastRect == null)
        {
            Debug.LogError("[WordSearch] Cell missing RectTransform!");
            return;
        }

        Vector3 firstWorld = firstRect.position;
        Vector3 lastWorld  = lastRect.position;

        // Posisi tengah di world space
        hlRect.position = (firstWorld + lastWorld) * 0.5f;


        // Ukuran sel (world scale)
        float cellW = firstRect.rect.width  * firstRect.lossyScale.x;
        float cellH = firstRect.rect.height * firstRect.lossyScale.y;


        // Hitung panjang actual: jumlah sel × lebar sel (lebar cell saja, tidak perlu +dist)
        // Ini lebih akurat untuk semua arah (horizontal, vertikal, diagonal)
        float hlWorld_W = cells.Count * cellW;
        float hlWorld_H = cellH * 0.85f;

        // Konversi ke local size (bagi parent scale)
        float pScaleX = highlightContainer.lossyScale.x > 0 ? highlightContainer.lossyScale.x : 1f;
        float pScaleY = highlightContainer.lossyScale.y > 0 ? highlightContainer.lossyScale.y : 1f;
        hlRect.sizeDelta = new Vector2(hlWorld_W / pScaleX, hlWorld_H / pScaleY);

        // Rotasi sesuai arah seleksi
        if (cells.Count > 1)
        {
            Vector3 dir = lastWorld - firstWorld;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            hlRect.rotation = Quaternion.Euler(0f, 0f, angle);
            Debug.Log($"[WordSearch] Highlight refresh: cells={cells.Count}, pos={hlRect.position}, angle={angle:F1}°, size={hlRect.sizeDelta}");
        }
        else
        {
            hlRect.rotation = Quaternion.identity;
            Debug.Log($"[WordSearch] Highlight refresh: 1 cell, pos={hlRect.position}, size={hlRect.sizeDelta}");
        }
    }

    // ─── Score & Timer ────────────────────────────────────────────────

    void TambahSkor(int nilai)
    {
        skor += nilai;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (teksSkor != null) teksSkor.text = skor.ToString();
    }

    void UpdateTimerUI()
    {
        if (teksWaktu == null) return;
        int detik = Mathf.CeilToInt(Mathf.Max(0f, timeLeft));
        teksWaktu.text = string.Format("{0:00}:{1:00}", detik / 60, detik % 60);
    }

    void UpdateWordListUI()
    {
        if (teksKataTarget == null) return;

        var remaining = new List<string>();
        foreach (string w in kataYangDicari)
            if (!foundWords.Contains(w.ToUpper().Trim()))
                remaining.Add(w);

        teksKataTarget.text = remaining.Count > 0
            ? "Cari: " + string.Join("  |  ", remaining)
            : "Semua kata ditemukan! 🎉";
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
            GameOver(false);
    }

    void GameOver(bool menang)
    {
        if (!isPlaying) return;
        isPlaying = false;

        ClearCurrentHighlight();
        StopAllCoroutines();

        LevelProgressManager.CompleteMiniGame(PlayerPrefs.GetInt("CurrentLevel", 1));

        if (popupHasil != null)
        {
            popupHasil.SetActive(true);
            if (teksHasilSkor != null) teksHasilSkor.text = skor.ToString();
        }

        Debug.Log($"[WordSearch] GameOver menang={menang}, skor={skor}");
    }

    // ─── Tombol Popup ─────────────────────────────────────────────────

    public void OnTombolLanjut()   => LevelFlowManager.OnGameSelesai();
    public void OnTombolCobaLagi() => SceneLoader.LoadScene(
        UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    public void OnTombolHome()     => LevelFlowManager.GoToRouteMap();
}
