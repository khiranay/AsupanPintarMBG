using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class SnakeGameManager : MonoBehaviour, IGameManager
{
    [Header("Grid")]
    [Tooltip("Jumlah kolom grid (horizontal). Recommended: 10-12")]
    public int   gridWidth   = 10;
    [Tooltip("Jumlah baris grid (vertikal). Recommended: 6-8 untuk gameplay nyaman")]
    public int   gridHeight  = 6;
    [Tooltip("AUTO: Centang untuk hitung cell size otomatis sesuai panel. MANUAL: Uncheck dan set cellSize manual.")]
    public bool autoFitToPanel = true;
    [Tooltip("Ukuran setiap cell dalam pixels (hanya dipakai jika autoFitToPanel = false)")]
    public float cellSize    = 90f;
    public RectTransform gridContainer;

    private float actualCellSize; // Cell size yang dipakai (hasil kalkulasi atau manual)

    [Header("Snake Pemain")]
    public GameObject snakeHeadPrefab;
    public GameObject snakeBodyPrefab;
    public float baseMoveInterval = 0.5f; // Diperlambat dari 0.35 ke 0.5

    [Header("Rotasi Sprite")]
    [Tooltip("Arah default sprite kepala ular di prefab: 0=Kanan, 1=Atas, 2=Kiri, 3=Bawah")]
    public int spriteDefaultDirection = 1; // 0=Right, 1=Up, 2=Left, 3=Down

    [Header("Dinding/Boundaries")]
    [Tooltip("Jika TRUE: ular nabrak dinding = game over. Jika FALSE: ular keluar dari satu sisi dan muncul di sisi lain (wrap around)")]
    public bool enableWalls = false; // Default false = wrap around mode

    [Header("Snake Growth")]
    [Tooltip("Jika TRUE: ular bertambah panjang saat makan. Jika FALSE: ular tetap panjang awal (tidak tumbuh), fokus ke skor saja.")]
    public bool enableGrowth = false; // Default false = tidak tumbuh
    [Tooltip("Panjang awal ular (jumlah segment). Min: 1 (hanya kepala), Recommended: 2-3 (kepala + ekor)")]
    public int initialSnakeLength = 2; // Default 2 = kepala + 1 ekor

    [Header("Difficulty Scaling")]
    public float minMoveInterval = 0.15f;
    public float speedUpRate = 0.02f;
    public int skorPerSpeedUp = 20;

    [Header("Ular Musuh (opsional)")]
    public bool       aktifkanUlarMusuh = true;
    public GameObject enemyHeadPrefab;
    public GameObject enemyBodyPrefab;
    public float      enemyMoveInterval = 0.55f;
    public int        jarakSpawnMusuh   = 5;
    public int        maxEnemyLength    = 5;

    [Header("Food — Poisson Disk Sampling")]
    public FoodItemDataLv5[] daftarMakanan;
    public GameObject        foodPrefab;
    public int   jumlahFoodAwal      = 5;
    public float poissonMinRadius    = 2f;
    public float poissonMinDariPlayer = 2f;
    public int   poissonK            = 30;

    [Header("Food Spawn Ratio")]
    [Tooltip("Persentase chance spawn makanan SEGAR (0-100). Contoh: 70 = 70% segar, 30% tidak segar.")]
    [Range(0, 100)]
    public int chanceSpawnSegar = 70; // Default 70% segar, 30% tidak segar

    [Header("UI Skor & Timer")]
    public TextMeshProUGUI teksSkor;
    public int             targetSkor = 100;
    [Tooltip("TextMeshProUGUI untuk tampilan waktu tersisa (format MM:SS)")]
    public TextMeshProUGUI teksWaktu;
    public float           durasiTimer = 60f; // Durasi game dalam detik (default 60 detik)

    [Header("UI Countdown & Effects")]
    [Tooltip("TextMeshProUGUI untuk tampilan 3-2-1-GO! (opsional)")]
    public TextMeshProUGUI teksCountdown;
    public GameObject      floatingTextPrefab; // Prefab UI Text untuk efek skor melayang

    [Header("UI Popup")]
    public GameObject      popupHasil;
    public TextMeshProUGUI teksHasilSkor;
    public GameObject      popupPerintah;

    [Header("Panel Hindari")]
    public Image[]           iconHindari;
    public TextMeshProUGUI[] teksHindari;

    private List<Vector2Int> playerBody    = new List<Vector2Int>();
    private List<GameObject> playerObjects = new List<GameObject>();
    private HashSet<Vector2Int> playerBodySet = new HashSet<Vector2Int>();

    private Vector2Int playerDir     = Vector2Int.right;
    private Vector2Int nextPlayerDir = Vector2Int.right;

    private List<Vector2Int> enemyBody    = new List<Vector2Int>();
    private List<GameObject> enemyObjects = new List<GameObject>();
    private HashSet<Vector2Int> enemyBodySet = new HashSet<Vector2Int>();

    private Dictionary<Vector2Int, GameObject>      foodObjects = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, FoodItemDataLv5> foodData    = new Dictionary<Vector2Int, FoodItemDataLv5>();

    private int  skor      = 0;
    private bool isPlaying = false;
    private float currentMoveInterval;
    private float timeLeft;  // Waktu tersisa

    private System.Random rng = new System.Random();

    private const int POIN_SEGAR = 10;
    private const int POIN_BAHAYA = -5;

    void Start()
    {
        // Reset state
        skor = 0;
        isPlaying = false;

        // Hitung cell size otomatis jika enabled
        CalculateCellSize();

        // Setup UI
        if (popupPerintah != null) popupPerintah.SetActive(true);
        if (popupHasil != null) popupHasil.SetActive(false); // Pastikan popup hasil tersembunyi

        IsiPanelHindari();
        currentMoveInterval = baseMoveInterval;
        timeLeft = durasiTimer;
        UpdateScoreUI();
        UpdateTimerUI();

        Debug.Log($"[Snake] Start() - Timer: {durasiTimer}s, isPlaying: {isPlaying}, CellSize: {actualCellSize}");
    }

    /// <summary>
    /// Hitung cell size otomatis agar grid pas dengan ukuran panel container.
    /// </summary>
    void CalculateCellSize()
    {
        if (autoFitToPanel && gridContainer != null)
        {
            // Ambil ukuran panel container
            float panelWidth = gridContainer.rect.width;
            float panelHeight = gridContainer.rect.height;

            // Hitung cell size berdasarkan constraint yang paling ketat
            float cellSizeByWidth = panelWidth / gridWidth;
            float cellSizeByHeight = panelHeight / gridHeight;

            // Gunakan yang lebih kecil agar muat di kedua dimensi
            actualCellSize = Mathf.Min(cellSizeByWidth, cellSizeByHeight);

            // Tambahkan sedikit padding agar tidak mepet
            actualCellSize *= 0.95f;

            Debug.Log($"[Snake] Auto Cell Size - Panel: {panelWidth}x{panelHeight}, Grid: {gridWidth}x{gridHeight}, Cell: {actualCellSize}px");
        }
        else
        {
            // Gunakan manual cell size
            actualCellSize = cellSize;
            Debug.Log($"[Snake] Manual Cell Size: {actualCellSize}px");
        }
    }

    void Update()
    {
        if (!isPlaying) return;

        // Input Keyboard dinonaktifkan sementara untuk menghindari konflik dengan Input System
        // Game menggunakan tombol UI untuk kontrol
        // Jika ingin aktifkan keyboard, ganti Active Input Handling ke "Input Manager (Old)" atau "Both"
        // di Project Settings > Player > Other Settings

        /*
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            OnTombolAtas();
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            OnTombolBawah();
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            OnTombolKiri();
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            OnTombolKanan();
        */
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }

    public void MulaiGame()
    {
        Debug.Log("[Snake] MulaiGame() dipanggil");

        Time.timeScale = 1f;
        if (popupPerintah != null) popupPerintah.SetActive(false);
        if (popupHasil != null) popupHasil.SetActive(false);

        StartCoroutine(CountdownHelper.Hitung(teksCountdown, () =>
        {
            Debug.Log($"[Snake] Countdown selesai, mulai game. Timer: {durasiTimer}s");

            InitPlayerSnake();
            SpawnSemuaFood();
            if (aktifkanUlarMusuh) InitEnemySnake();

            isPlaying = true;
            timeLeft = durasiTimer; // Reset timer

            StartCoroutine(PlayerLoop());
            StartCoroutine(TimerCountdown());
            if (aktifkanUlarMusuh) StartCoroutine(EnemyLoop());
        }));
    }

    HashSet<Vector2Int> GetOccupiedCells()
    {
        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>(foodObjects.Keys);
        occupied.UnionWith(playerBodySet);
        if (aktifkanUlarMusuh) occupied.UnionWith(enemyBodySet);
        return occupied;
    }

    void SpawnSemuaFood()
    {
        List<Vector2Int> posisiFood = CustomPoissonDiskSampler.Generate(
            gridWidth, gridHeight,
            minRadius         : poissonMinRadius,
            jumlah            : jumlahFoodAwal,
            excluded          : GetOccupiedCells(),
            playerHead        : playerBody[0],
            minJarakPlayer    : poissonMinDariPlayer,
            k                 : poissonK,
            rand              : rng
        );
        foreach (var pos in posisiFood)
            SpawnFoodDiCell(pos);
    }

    void SpawnSatuFood()
    {
        List<Vector2Int> emptyCells = new List<Vector2Int>();
        HashSet<Vector2Int> occupied = GetOccupiedCells();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                if (!occupied.Contains(cell))
                    emptyCells.Add(cell);
            }
        }

        if (emptyCells.Count > 0)
        {
            Vector2Int randomPos = emptyCells[Random.Range(0, emptyCells.Count)];
            SpawnFoodDiCell(randomPos);
        }
    }

    void SpawnFoodDiCell(Vector2Int pos)
    {
        if (daftarMakanan == null || daftarMakanan.Length == 0)
        {
            Debug.LogError("[SnakeGame] daftarMakanan kosong! Assign FoodItemDataLv5 di Inspector.");
            return;
        }

        // Pilih makanan berdasarkan ratio segar vs tidak segar
        FoodItemDataLv5 data = PickFoodByRatio();

        // Pilih prefab: gunakan prefabKhusus jika ada, jika tidak gunakan foodPrefab default
        GameObject prefabToSpawn = data.prefabKhusus != null ? data.prefabKhusus : foodPrefab;

        if (prefabToSpawn == null)
        {
            Debug.LogError($"[SnakeGame] Tidak ada prefab untuk '{data.namaItem}'! " +
                           "Isi 'Prefab Khusus' di FoodItemData atau 'Food Prefab' di GameManager.");
            return;
        }

        GameObject obj = Instantiate(prefabToSpawn, gridContainer);

        FoodItemLv5 foodComp = obj.GetComponent<FoodItemLv5>();
        if (foodComp == null)
        {
            Debug.LogError($"[SnakeGame] Prefab '{prefabToSpawn.name}' tidak punya komponen FoodItemLv5! " +
                           "Tambahkan script FoodItemLv5 ke prefab makanan.");
            Destroy(obj);
            return;
        }

        foodComp.Setup(data);
        SetUIPosition(obj.GetComponent<RectTransform>(), pos);
        foodObjects[pos] = obj;
        foodData[pos] = data;
    }

    /// <summary>
    /// Pilih makanan berdasarkan ratio segar vs tidak segar.
    /// </summary>
    FoodItemDataLv5 PickFoodByRatio()
    {
        // Pisahkan makanan segar dan tidak segar
        List<FoodItemDataLv5> segarList = new List<FoodItemDataLv5>();
        List<FoodItemDataLv5> tidakSegarList = new List<FoodItemDataLv5>();

        foreach (var item in daftarMakanan)
        {
            if (item.isSegar)
                segarList.Add(item);
            else
                tidakSegarList.Add(item);
        }

        // Jika salah satu list kosong, fallback ke random biasa
        if (segarList.Count == 0 && tidakSegarList.Count == 0)
        {
            Debug.LogError("[Snake] Tidak ada makanan di daftarMakanan!");
            return daftarMakanan[0];
        }

        if (segarList.Count == 0)
            return tidakSegarList[Random.Range(0, tidakSegarList.Count)];

        if (tidakSegarList.Count == 0)
            return segarList[Random.Range(0, segarList.Count)];

        // Random berdasarkan ratio
        int roll = Random.Range(0, 100);

        if (roll < chanceSpawnSegar)
        {
            // Spawn makanan segar
            return segarList[Random.Range(0, segarList.Count)];
        }
        else
        {
            // Spawn makanan tidak segar
            return tidakSegarList[Random.Range(0, tidakSegarList.Count)];
        }
    }

    void InitPlayerSnake()
    {
        // Spawn lebih ke tengah bawah agar punya ruang lebih ke atas
        Vector2Int start = new Vector2Int(gridWidth / 2, Mathf.Max(1, gridHeight / 3));

        Debug.Log($"[Snake] InitPlayerSnake - Grid: {gridWidth}x{gridHeight}, Start pos: {start}, Length: {initialSnakeLength}");

        playerBody.Clear();
        playerBodySet.Clear();
        foreach (GameObject obj in playerObjects)
            if (obj != null) Destroy(obj);
        playerObjects.Clear();

        // Spawn kepala
        playerBody.Add(start);
        playerBodySet.Add(start);
        GameObject head = Instantiate(snakeHeadPrefab, gridContainer);
        SetUIPosition(head.GetComponent<RectTransform>(), start);
        playerObjects.Add(head);
        playerDir = nextPlayerDir = Vector2Int.right;

        // Set rotasi awal kepala ular
        UpdateSnakeHeadRotation(head, playerDir);

        // Spawn body/ekor (initialSnakeLength - 1 segment)
        // Body akan spawn di belakang kepala (berlawanan arah)
        Vector2Int oppositeDir = -playerDir; // Jika kepala ke kanan, body ke kiri dari kepala

        for (int i = 1; i < initialSnakeLength; i++)
        {
            Vector2Int bodyPos = start + (oppositeDir * i);

            // Pastikan posisi body valid (wrap jika perlu)
            if (!InBounds(bodyPos))
                bodyPos = WrapPosition(bodyPos);

            playerBody.Add(bodyPos);
            playerBodySet.Add(bodyPos);

            GameObject bodyObj = Instantiate(snakeBodyPrefab, gridContainer);
            SetUIPosition(bodyObj.GetComponent<RectTransform>(), bodyPos);
            playerObjects.Add(bodyObj);
        }

        // Update rotasi semua body segments
        UpdateAllBodyRotations(playerBody, playerObjects);
    }

    IEnumerator PlayerLoop()
    {
        // Delay sebelum ular mulai bergerak (beri waktu player untuk siap)
        yield return new WaitForSeconds(1.5f); // Diperpanjang dari 0.5 ke 1.5 detik

        while (isPlaying)
        {
            yield return new WaitForSeconds(currentMoveInterval);
            if (isPlaying) MovePlayer();
        }
    }

    void MovePlayer()
    {
        playerDir = nextPlayerDir;

        // Rotasi kepala ular sesuai arah
        UpdateSnakeHeadRotation(playerObjects[0], playerDir);

        Vector2Int newHead = playerBody[0] + playerDir;

        // Wrap around jika keluar dari bounds (kecuali enableWalls = true)
        if (!InBounds(newHead))
        {
            if (enableWalls)
            {
                // Mode dengan dinding: game over
                Debug.LogError($"[Snake] OUT OF BOUNDS! Pos: {newHead}, Grid: {gridWidth}x{gridHeight}");
                GameOver(false, "Menabrak dinding!");
                return;
            }
            else
            {
                // Mode wrap around: teleport ke sisi lain
                newHead = WrapPosition(newHead);
                Debug.Log($"[Snake] Wrap Around: {playerBody[0]} -> {newHead} (wrapped)");
            }
        }
        else
        {
            Debug.Log($"[Snake] Move: {playerBody[0]} -> {newHead}, Dir: {playerDir}, Bounds: {gridWidth}x{gridHeight}");
        }

        // Cek nabrak badan sendiri (hanya jika ular punya body, yaitu panjang > 1)
        if (playerBody.Count > 1)
        {
            bool tailWillMove = !foodObjects.ContainsKey(newHead);
            Vector2Int currentTail = playerBody[playerBody.Count - 1];
            if (playerBodySet.Contains(newHead) && (!tailWillMove || newHead != currentTail))
                { GameOver(false, "Menabrak badan sendiri!"); return; }
        }

        if (aktifkanUlarMusuh && enemyBodySet.Contains(newHead))
            { GameOver(false, "Ketangkap ular musuh!"); return; }

        bool tumbuh = false;

        if (foodObjects.ContainsKey(newHead))
        {
            FoodItemDataLv5 item = foodData[newHead];
            Vector3 posisiMakan = foodObjects[newHead].transform.position; // Ambil posisi dunia dari item

            if (item.isSegar)
            {
                TambahSkor(POIN_SEGAR);
                // Ular hanya tumbuh jika enableGrowth = true
                tumbuh = enableGrowth;
                MunculkanFloatingText("+" + POIN_SEGAR, Color.green, posisiMakan);
            }
            else
            {
                TambahSkor(POIN_BAHAYA);
                StartCoroutine(FlashRedEffect(playerObjects[0]));
                MunculkanFloatingText(POIN_BAHAYA.ToString(), Color.red, posisiMakan);
            }

            Destroy(foodObjects[newHead]);
            foodObjects.Remove(newHead);
            foodData.Remove(newHead);
            SpawnSatuFood();
        }

        GerakkanSnakePool(playerBody, playerBodySet, playerObjects, snakeBodyPrefab, newHead, tumbuh);

        if (skor >= targetSkor) GameOver(true, "Kamu Detektif Gizi Terbaik!");
    }

    IEnumerator FlashRedEffect(GameObject headObj)
    {
        Image img = headObj.GetComponent<Image>();
        if (img != null)
        {
            Color orig = img.color;
            img.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            img.color = orig;
        }
    }

    void MunculkanFloatingText(string teks, Color warna, Vector3 posisi)
    {
        if (floatingTextPrefab == null) return;

        // Spawn di parent yang sama dengan grid agar tampilannya benar di UI
        GameObject popObj = Instantiate(floatingTextPrefab, gridContainer.parent);
        popObj.transform.position = posisi;

        TextMeshProUGUI tmp = popObj.GetComponent<TextMeshProUGUI>();
        if (tmp == null) tmp = popObj.GetComponentInChildren<TextMeshProUGUI>();

        if (tmp != null)
        {
            tmp.text = teks;
            tmp.color = warna;
        }

        StartCoroutine(AnimasiFloatingText(popObj));
    }

    IEnumerator AnimasiFloatingText(GameObject obj)
    {
        float durasi = 1f;
        float timer = 0f;
        Vector3 startPos = obj.transform.position;
        // Bergerak ke atas sebanyak 80 pixel/unit
        Vector3 endPos = startPos + Vector3.up * 80f;

        CanvasGroup cg = obj.GetComponent<CanvasGroup>();
        if (cg == null) cg = obj.AddComponent<CanvasGroup>();

        while (timer < durasi)
        {
            if (obj == null) break;
            timer += Time.deltaTime;
            float t = timer / durasi;

            obj.transform.position = Vector3.Lerp(startPos, endPos, t);
            cg.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        if (obj != null) Destroy(obj);
    }

    public void OnTombolAtas()  { if (playerDir != Vector2Int.down)  nextPlayerDir = Vector2Int.up;    }
    public void OnTombolBawah() { if (playerDir != Vector2Int.up)    nextPlayerDir = Vector2Int.down;  }
    public void OnTombolKiri()  { if (playerDir != Vector2Int.right) nextPlayerDir = Vector2Int.left;  }
    public void OnTombolKanan() { if (playerDir != Vector2Int.left)  nextPlayerDir = Vector2Int.right; }

    /// <summary>
    /// Update rotasi kepala ular sesuai arah pergerakan.
    /// </summary>
    void UpdateSnakeHeadRotation(GameObject head, Vector2Int direction)
    {
        if (head == null) return;

        float angle = GetAngleForDirection(direction);
        head.transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    /// <summary>
    /// Hitung angle rotasi berdasarkan arah dan orientasi default sprite.
    /// </summary>
    float GetAngleForDirection(Vector2Int direction)
    {
        // Base angle untuk setiap arah jika sprite default menghadap kanan (0)
        float baseAngle = 0f;

        if (direction == Vector2Int.right)
            baseAngle = 0f;
        else if (direction == Vector2Int.left)
            baseAngle = 180f;
        else if (direction == Vector2Int.up)
            baseAngle = 90f;
        else if (direction == Vector2Int.down)
            baseAngle = -90f;

        // Adjust berdasarkan orientasi default sprite
        // spriteDefaultDirection: 0=Right, 1=Up, 2=Left, 3=Down
        float offset = spriteDefaultDirection * -90f;

        return baseAngle + offset;
    }

    void InitEnemySnake()
    {
        // BUG FIX #7: Validasi bahwa spawn position ditemukan, bukan diam-diam pakai (0,0)
        Vector2Int spawnPos = Vector2Int.zero;
        bool foundSpawn = false;
        HashSet<Vector2Int> occ = GetOccupiedCells();
        for (int i = 0; i < 50; i++)
        {
            var c = new Vector2Int(Random.Range(0, gridWidth), Random.Range(0, gridHeight));
            if (!occ.Contains(c) && ManhattanDist(c, playerBody[0]) >= jarakSpawnMusuh)
            { spawnPos = c; foundSpawn = true; break; }
        }

        if (!foundSpawn)
        {
            Debug.LogWarning("[SnakeGameManager] Tidak dapat menemukan posisi spawn musuh yang valid. " +
                             "Ular musuh tidak akan dimunculkan.");
            aktifkanUlarMusuh = false;
            return;
        }

        enemyBody.Add(spawnPos);
        enemyBodySet.Add(spawnPos);
        GameObject head = Instantiate(enemyHeadPrefab, gridContainer);
        SetUIPosition(head.GetComponent<RectTransform>(), spawnPos);
        enemyObjects.Add(head);
    }

    IEnumerator EnemyLoop()
    {
        while (isPlaying)
        {
            yield return new WaitForSeconds(enemyMoveInterval);
            if (isPlaying) MoveEnemy();
        }
    }

    void MoveEnemy()
    {
        if (playerBody.Count == 0) return;

        HashSet<Vector2Int> obstacles = new HashSet<Vector2Int>(playerBodySet);
        obstacles.UnionWith(enemyBodySet);

        List<Vector2Int> path = AStarFindPath(enemyBody[0], playerBody[0], obstacles);
        Vector2Int nextPos = (path != null && path.Count >= 2)
            ? path[1]
            : GetRandomValidMove(enemyBody[0], obstacles);

        // Hitung arah pergerakan ular musuh untuk rotasi
        Vector2Int enemyDir = nextPos - enemyBody[0];

        if (nextPos == playerBody[0]) { GameOver(false, "Ketangkap ular musuh!"); return; }

        bool musuhTumbuh = (enemyBody.Count < maxEnemyLength) && (Random.value < 0.2f);

        GerakkanSnakePool(enemyBody, enemyBodySet, enemyObjects, enemyBodyPrefab, nextPos, musuhTumbuh);

        // Update rotasi kepala ular musuh (body sudah di-update di GerakkanSnakePool)
        if (enemyObjects.Count > 0)
            UpdateSnakeHeadRotation(enemyObjects[0], enemyDir);
    }

    class AStarNode
    {
        public Vector2Int pos;
        public AStarNode  parent;
        public float g, f;
        public AStarNode(Vector2Int p, AStarNode par, float g, float h)
        { pos = p; parent = par; this.g = g; f = g + h; }
    }

    class SimplePriorityQueue<T>
    {
        private List<KeyValuePair<float, T>> elements = new List<KeyValuePair<float, T>>();
        public int Count => elements.Count;

        public void Enqueue(T item, float priority)
        {
            elements.Add(new KeyValuePair<float, T>(priority, item));
            int i = elements.Count - 1;
            while (i > 0 && elements[i - 1].Key > elements[i].Key)
            {
                var temp = elements[i];
                elements[i] = elements[i - 1];
                elements[i - 1] = temp;
                i--;
            }
        }
        public T Dequeue()
        {
            T bestItem = elements[0].Value;
            elements.RemoveAt(0);
            return bestItem;
        }
    }

    List<Vector2Int> AStarFindPath(Vector2Int start, Vector2Int goal, HashSet<Vector2Int> obstacles)
    {
        var openSet   = new SimplePriorityQueue<AStarNode>();
        var closedSet = new HashSet<Vector2Int>();
        var nodeMap   = new Dictionary<Vector2Int, AStarNode>();

        AStarNode startNode = new AStarNode(start, null, 0, ManhattanDist(start, goal));
        openSet.Enqueue(startNode, startNode.f);
        nodeMap[start] = startNode;

        while (openSet.Count > 0)
        {
            AStarNode cur = openSet.Dequeue();
            nodeMap.Remove(cur.pos);

            if (cur.pos == goal) return ReconstructPath(cur);

            closedSet.Add(cur.pos);

            foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int nb = cur.pos + dir;
                if (!InBounds(nb) || closedSet.Contains(nb) || obstacles.Contains(nb)) continue;

                float g = cur.g + 1f;

                if (nodeMap.TryGetValue(nb, out AStarNode existingNode))
                {
                    if (g >= existingNode.g) continue;
                }

                AStarNode neighborNode = new AStarNode(nb, cur, g, ManhattanDist(nb, goal));
                openSet.Enqueue(neighborNode, neighborNode.f);
                nodeMap[nb] = neighborNode;
            }
            if (closedSet.Count > gridWidth * gridHeight) break;
        }
        return null;
    }

    List<Vector2Int> ReconstructPath(AStarNode node)
    {
        var path = new List<Vector2Int>();
        while (node != null) { path.Add(node.pos); node = node.parent; }
        path.Reverse();
        return path;
    }

    Vector2Int GetRandomValidMove(Vector2Int from, HashSet<Vector2Int> obstacles)
    {
        foreach (var d in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
        {
            Vector2Int n = from + d;
            if (InBounds(n) && !obstacles.Contains(n)) return n;
        }
        return from;
    }

    void GerakkanSnakePool(List<Vector2Int> body, HashSet<Vector2Int> bodySet, List<GameObject> objects, GameObject bodyPrefab, Vector2Int newHeadPos, bool tumbuh)
    {
        Vector2Int oldHeadPos = body[0];

        SetUIPosition(objects[0].GetComponent<RectTransform>(), newHeadPos);

        if (tumbuh)
        {
            GameObject newBodyObj = Instantiate(bodyPrefab, gridContainer);
            SetUIPosition(newBodyObj.GetComponent<RectTransform>(), oldHeadPos);

            objects.Insert(1, newBodyObj);
            body.Insert(1, oldHeadPos);
            bodySet.Add(oldHeadPos);
        }
        else
        {
            if (objects.Count > 1)
            {
                int lastIdx = objects.Count - 1;
                GameObject tailObj = objects[lastIdx];
                Vector2Int tailPos = body[lastIdx];

                bodySet.Remove(tailPos);
                body.RemoveAt(lastIdx);
                objects.RemoveAt(lastIdx);

                SetUIPosition(tailObj.GetComponent<RectTransform>(), oldHeadPos);

                objects.Insert(1, tailObj);
                body.Insert(1, oldHeadPos);
                bodySet.Add(oldHeadPos);
            }
            else
            {
                // Jika panjang ular cuma 1 dan tidak tumbuh, hapus jejak pos lama
                bodySet.Remove(oldHeadPos);
            }
        }

        body[0] = newHeadPos;
        bodySet.Add(newHeadPos);

        // Update rotasi semua body segments setelah movement selesai
        UpdateAllBodyRotations(body, objects);
    }

    /// <summary>
    /// Update rotasi semua body segments berdasarkan arah dari segment ke segment berikutnya.
    /// </summary>
    void UpdateAllBodyRotations(List<Vector2Int> body, List<GameObject> objects)
    {
        // Skip index 0 (kepala sudah di-handle di MovePlayer)
        for (int i = 1; i < body.Count && i < objects.Count; i++)
        {
            // Hitung arah dari segment ini ke segment sebelumnya (di depannya)
            Vector2Int currentPos = body[i];
            Vector2Int prevPos = body[i - 1];
            Vector2Int direction = prevPos - currentPos;

            if (direction.sqrMagnitude > 0)
            {
                float angle = GetAngleForDirection(direction);
                objects[i].transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }
    }

    void TambahSkor(int nilai)
    {
        int prevSkor = skor;
        skor = Mathf.Max(0, skor + nilai);

        if (skor / skorPerSpeedUp > prevSkor / skorPerSpeedUp)
        {
            currentMoveInterval = Mathf.Max(minMoveInterval, currentMoveInterval - speedUpRate);
        }

        UpdateScoreUI();

        // Cek apakah target skor tercapai
        if (skor >= targetSkor)
        {
            Debug.Log($"[Snake] Target skor tercapai! {skor}/{targetSkor}");
            GameOver(true, "Target Tercapai!");
        }
    }

    void UpdateScoreUI()
    {
        if (teksSkor != null)
            teksSkor.text = $"SKOR: {skor} / {targetSkor}";
    }

    IEnumerator TimerCountdown()
    {
        Debug.Log($"[Snake] Timer countdown dimulai: {timeLeft}s");

        while (timeLeft > 0 && isPlaying)
        {
            timeLeft -= Time.deltaTime;
            UpdateTimerUI();
            yield return null;
        }

        // Waktu habis
        if (isPlaying)
        {
            Debug.Log("[Snake] Waktu habis!");
            GameOver(false, "Waktu Habis!");
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

    void IsiPanelHindari()
    {
        if (daftarMakanan == null) return;

        // BUG FIX #6: Sembunyikan semua slot terlebih dulu, lalu isi yang diperlukan
        // Sebelumnya, slot yang tidak terisi menampilkan sprite lama/default
        for (int i = 0; i < iconHindari.Length; i++)
        {
            if (iconHindari[i] != null)
                iconHindari[i].gameObject.SetActive(false);
            if (i < teksHindari.Length && teksHindari[i] != null)
                teksHindari[i].gameObject.SetActive(false);
        }

        int idx = 0;
        foreach (var item in daftarMakanan)
        {
            if (!item.isSegar && idx < iconHindari.Length)
            {
                if (iconHindari[idx] != null)
                {
                    iconHindari[idx].sprite = item.spriteItem;
                    iconHindari[idx].gameObject.SetActive(true);
                }
                if (idx < teksHindari.Length && teksHindari[idx] != null)
                {
                    teksHindari[idx].text = (idx + 1) + ". " + item.namaItem;
                    teksHindari[idx].gameObject.SetActive(true);
                }
                idx++;
            }
        }
    }

    void GameOver(bool menang, string pesan = "")
    {
        Debug.Log($"[Snake] GameOver dipanggil: menang={menang}, pesan={pesan}, isPlaying={isPlaying}");

        if (!isPlaying) return;
        isPlaying = false;

        // BUG FIX #10: Bersihkan orphan floating text sebelum StopAllCoroutines
        // agar objek teks yang sedang animasi tidak tertinggal di scene
        if (gridContainer != null)
        {
            foreach (Transform child in gridContainer.parent)
            {
                // Hapus hanya floating text prefab (bukan gridContainer sendiri)
                if (child != gridContainer && child.GetComponent<TMPro.TextMeshProUGUI>() != null)
                    Destroy(child.gameObject);
            }
        }

        StopAllCoroutines();
        LevelProgressManager.CompleteMiniGame(PlayerPrefs.GetInt("CurrentLevel", 1));
        if (popupHasil != null)
        {
            popupHasil.SetActive(true);
            if (teksHasilSkor != null)
            {
                teksHasilSkor.text = skor.ToString();
            }
        }
    }

    public void OnTombolSelesai() => LevelFlowManager.OnGameSelesai();
    public void OnTombolUlang()
        {
            SceneLoader.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }

    bool InBounds(Vector2Int p)
    {
        return p.x >= 0 && p.x < gridWidth && p.y >= 0 && p.y < gridHeight;
    }

    /// <summary>
    /// Wrap position agar ular yang keluar dari satu sisi muncul di sisi lain.
    /// </summary>
    Vector2Int WrapPosition(Vector2Int pos)
    {
        // Wrap X (horizontal)
        if (pos.x < 0)
            pos.x = gridWidth - 1;  // Keluar kiri → muncul kanan
        else if (pos.x >= gridWidth)
            pos.x = 0;              // Keluar kanan → muncul kiri

        // Wrap Y (vertical)
        if (pos.y < 0)
            pos.y = gridHeight - 1; // Keluar bawah → muncul atas
        else if (pos.y >= gridHeight)
            pos.y = 0;              // Keluar atas → muncul bawah

        return pos;
    }

    float ManhattanDist(Vector2Int a, Vector2Int b) =>
        Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    void SetUIPosition(RectTransform rect, Vector2Int pos)
    {
        // Paksa anchor ke titik tetap (bukan stretch) agar sizeDelta = ukuran aktual.
        // Tanpa ini, prefab dengan anchor stretch akan muncul sangat besar karena
        // sizeDelta pada stretch berarti "offset dari tepi parent", bukan "ukuran".
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.zero;
        rect.pivot     = new Vector2(0.5f, 0.5f);

        // Gunakan actualCellSize (hasil kalkulasi auto atau manual)
        rect.sizeDelta        = new Vector2(actualCellSize, actualCellSize);
        rect.anchoredPosition = new Vector2(
            pos.x * actualCellSize + actualCellSize * 0.5f,
            pos.y * actualCellSize + actualCellSize * 0.5f
        );
    }
}

// Custom Poisson Disk ditaruh di bawah agar tidak mengganggu Unity Editor serialization.
public static class CustomPoissonDiskSampler
{
    public static List<Vector2Int> Generate(
        int gridWidth, int gridHeight,
        float minRadius,
        int jumlah,
        HashSet<Vector2Int> excluded,
        Vector2Int playerHead,
        float minJarakPlayer = 2f,
        int k = 30,
        System.Random rand = null)
    {
        if (rand == null) rand = new System.Random();

        List<Vector2Int> samples    = new List<Vector2Int>();
        List<Vector2Int> activeList = new List<Vector2Int>();

        Vector2Int startPoint = FindStartPoint(
            gridWidth, gridHeight, excluded, playerHead, minJarakPlayer, rand);

        if (startPoint == -Vector2Int.one) return samples;

        samples.Add(startPoint);
        activeList.Add(startPoint);

        while (activeList.Count > 0 && samples.Count < jumlah)
        {
            int idx          = rand.Next(0, activeList.Count);
            Vector2Int pivot = activeList[idx];
            bool foundCandidate = false;

            for (int attempt = 0; attempt < k; attempt++)
            {
                Vector2Int candidate = SampleAnnulus(pivot, minRadius, rand);

                if (!InBounds(candidate, gridWidth, gridHeight)) continue;
                if (excluded.Contains(candidate))                continue;
                if (samples.Contains(candidate))                 continue;
                if (EuclideanDist(candidate, playerHead) < minJarakPlayer) continue;

                bool terlalu_dekat = false;
                foreach (var s in samples)
                {
                    if (EuclideanDist(candidate, s) < minRadius)
                    { terlalu_dekat = true; break; }
                }
                if (terlalu_dekat) continue;

                samples.Add(candidate);
                activeList.Add(candidate);
                foundCandidate = true;

                if (samples.Count >= jumlah) break;
            }

            if (!foundCandidate)
                activeList.RemoveAt(idx);
        }

        return samples;
    }

    static Vector2Int SampleAnnulus(Vector2Int pivot, float r, System.Random rand)
    {
        float angle    = (float)(rand.NextDouble() * Mathf.PI * 2f);
        float distance = (float)(r + rand.NextDouble() * r);
        int x = pivot.x + Mathf.RoundToInt(Mathf.Cos(angle) * distance);
        int y = pivot.y + Mathf.RoundToInt(Mathf.Sin(angle) * distance);
        return new Vector2Int(x, y);
    }

    static Vector2Int FindStartPoint(
        int w, int h,
        HashSet<Vector2Int> excluded,
        Vector2Int playerHead,
        float minJarakPlayer,
        System.Random rand)
    {
        for (int attempt = 0; attempt < 100; attempt++)
        {
            var c = new Vector2Int(rand.Next(0, w), rand.Next(0, h));
            if (!excluded.Contains(c) &&
                EuclideanDist(c, playerHead) >= minJarakPlayer)
                return c;
        }
        return -Vector2Int.one;
    }

    static bool InBounds(Vector2Int p, int w, int h) =>
        p.x >= 0 && p.x < w && p.y >= 0 && p.y < h;

    static float EuclideanDist(Vector2Int a, Vector2Int b)
    {
        float dx = a.x - b.x, dy = a.y - b.y;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }
}
