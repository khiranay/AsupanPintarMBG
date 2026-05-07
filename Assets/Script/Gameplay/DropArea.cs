using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropArea : MonoBehaviour, IDropHandler
{
    [Header("Setting")]
    public bool isAmanArea;     // centang jika ini Area Aman
    public DragDropManager gameManager;

    public void OnDrop(PointerEventData eventData)
    {
        FoodItem food = eventData.pointerDrag.GetComponent<FoodItem>();
        if (food == null) return;

        bool isBenar = (isAmanArea && food.isLayak) || (!isAmanArea && !food.isLayak);

        gameManager.OnFoodDropped(food, isBenar, transform);
    }
}