using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Komponen yang ditempel di prefab Food di grid.
/// Prefab struktur: GameObject > Image (food icon)
/// </summary>
public class FoodItemLv5 : MonoBehaviour
{
    public Image    itemImage;   // Assign di prefab
    public FoodItemDataLv5 data; // Di-set saat spawn

    public void Setup(FoodItemDataLv5 itemData)
    {
        data = itemData;
        if (itemImage != null)
            itemImage.sprite = itemData.spriteItem;

        // Efek visual: makanan meragukan agak transparan / beda warna
        if (!itemData.isSegar && itemImage != null)
        {
            itemImage.color = new Color(1f, 1f, 1f, 0.85f);
        }
    }
}