using UnityEngine;

[CreateAssetMenu(fileName = "FoodItemLv4", menuName = "Game/Food Item")]
public class FoodItemData : ScriptableObject
{
    public string namaItem;
    public Sprite spriteNormal;     // tampilan luar
    public Sprite spriteXRay;       // tampilan dalam (x-ray)
    public bool isAman;             // aman dimakan atau tidak
    public string keterangan;       // "Aman dimakan" / "Mengandung jamur"
}