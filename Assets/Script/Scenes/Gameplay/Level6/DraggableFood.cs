using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Component untuk food item yang bisa di-drag dan drop ke customer.
/// Attach script ini ke prefab food.
/// </summary>
[RequireComponent(typeof(Image))]
[RequireComponent(typeof(CanvasGroup))]
public class DraggableFood : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    public Image foodImage;
    public Canvas canvas;

    [Header("Drag Settings")]
    [Tooltip("Alpha saat di-drag")]
    public float dragAlpha = 0.6f;

    // Private
    private FoodItemDataLv5 foodData;
    private RestaurantManager manager;
    private int spawnIndex;
    private Vector3 originalPosition;
    private Transform originalParent;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private bool isDragging = false;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (foodImage == null)
            foodImage = GetComponent<Image>();

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
    }

    /// <summary>
    /// Initialize food dengan data dan manager reference.
    /// </summary>
    public void Initialize(FoodItemDataLv5 data, RestaurantManager mgr, int index)
    {
        foodData = data;
        manager = mgr;
        spawnIndex = index;

        // Set sprite
        if (foodImage != null && data != null)
        {
            // Gunakan sprite dari data
            foodImage.sprite = data.spriteItem;

            // Reset warna ke normal (opaque penuh)
            foodImage.color = Color.white;
        }

        originalPosition = transform.position;
        originalParent = transform.parent;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"[DraggableFood] Begin drag: {foodData.namaItem}");

        isDragging = true;
        originalPosition = transform.position;
        originalParent = transform.parent;

        // Set canvas group alpha
        if (canvasGroup != null)
        {
            canvasGroup.alpha = dragAlpha;
            canvasGroup.blocksRaycasts = false; // Agar tidak block raycast ke drop target
        }

        // Move ke top layer
        transform.SetParent(canvas.transform, true);
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvas == null) return;

        // Follow mouse/touch position
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            eventData.position,
            canvas.worldCamera,
            out position
        );

        transform.position = canvas.transform.TransformPoint(position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"[DraggableFood] End drag: {foodData.namaItem}");

        isDragging = false;

        // Restore canvas group
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
        }

        // Check if dropped on valid target
        GameObject hitObject = eventData.pointerCurrentRaycast.gameObject;

        if (hitObject != null)
        {
            // --- Cek CustomerCharacter ---
            CustomerCharacter customer = hitObject.GetComponent<CustomerCharacter>();
            if (customer == null)
                customer = hitObject.GetComponentInParent<CustomerCharacter>();

            if (customer != null)
            {
                Debug.Log("[DraggableFood] Dropped on customer!");
                if (manager != null)
                    manager.OnFoodServed(foodData, gameObject, spawnIndex);
                return;
            }

            // --- Cek TrashArea ---
            TrashArea trash = hitObject.GetComponent<TrashArea>();
            if (trash == null)
                trash = hitObject.GetComponentInParent<TrashArea>();

            if (trash != null)
            {
                Debug.Log("[DraggableFood] Dropped on trash!");
                trash.ResetVisual();
                if (manager != null)
                    manager.OnFoodDiscarded(foodData, gameObject, spawnIndex);
                return;
            }
        }

        // Not dropped on valid target, return to original position
        Debug.Log("[DraggableFood] Not dropped on valid target, returning to original position");
        ReturnToOriginalPosition();
    }

    void ReturnToOriginalPosition()
    {
        transform.SetParent(originalParent, true);
        transform.position = originalPosition;
    }

    /// <summary>
    /// Get food data untuk debugging/info.
    /// </summary>
    public FoodItemDataLv5 GetFoodData()
    {
        return foodData;
    }
}
