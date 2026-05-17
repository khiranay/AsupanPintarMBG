using UnityEngine;
using UnityEngine.UI;

public class FoodItem4 : MonoBehaviour
{
    public Image itemImage;
    public FoodItemData data;

    public void Setup(FoodItemData itemData)
    {
        data = itemData;
        itemImage.sprite = itemData.spriteNormal;
    }

    public void TampilkanXRay()
    {
        itemImage.sprite = data.spriteXRay;
    }

    public void TampilkanNormal()
    {
        itemImage.sprite = data.spriteNormal;
    }
}