using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Area tong sampah untuk membuang makanan tidak layak di Level 6.
/// Gunakan dua child GameObject terpisah untuk sprite tertutup dan terbuka.
/// </summary>
public class TrashArea : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Child GameObjects")]
    [Tooltip("Child GameObject dengan sprite tong TERTUTUP (aktif saat normal)")]
    public GameObject objTertutup;
    [Tooltip("Child GameObject dengan sprite tong TERBUKA (aktif saat drag over)")]
    public GameObject objTerbuka;

    void Awake()
    {
        SetTertutup();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.dragging || eventData.pointerDrag != null)
            SetTerbuka();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetTertutup();
    }

    /// <summary>Dipanggil oleh DraggableFood setelah food di-drop.</summary>
    public void ResetVisual()
    {
        SetTertutup();
    }

    private void SetTerbuka()
    {
        if (objTertutup != null) objTertutup.SetActive(false);
        if (objTerbuka  != null) objTerbuka.SetActive(true);
    }

    private void SetTertutup()
    {
        if (objTertutup != null) objTertutup.SetActive(true);
        if (objTerbuka  != null) objTerbuka.SetActive(false);
    }
}
