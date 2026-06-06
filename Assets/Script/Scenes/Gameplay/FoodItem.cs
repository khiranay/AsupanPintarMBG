using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FoodItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Setting")]
    public bool isLayak;            // centang jika makanan layak
    public string foodName;
    public bool isDragging = false;
    [HideInInspector] public bool wasDropped = false; // true saat berhasil di-drop ke area

    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Transform originalParent;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
{
    isDragging = true;
    canvasGroup.blocksRaycasts = false;

    originalParent = transform.parent;
    originalPosition = rectTransform.anchoredPosition;

    // Simpan world position sebelum pindah parent
    Vector3 worldPos = rectTransform.position;

    transform.SetParent(canvas.transform);

    // Restore world position setelah pindah parent agar tidak lompat
    rectTransform.position = worldPos;
}

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        canvasGroup.blocksRaycasts = true;

        // Hanya kembali ke posisi asal jika TIDAK berhasil di-drop ke area
        if (!wasDropped)
            ReturnToOrigin();
    }

    public void ReturnToOrigin()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
    }

}
