using UnityEngine;

/// <summary>
/// ScriptableObject data makanan untuk Level 5 (Snake Game)
/// Cara buat: klik kanan di Project → Create → Game/Food Item Level 5
/// </summary>
[CreateAssetMenu(fileName = "FoodLv5_NamaMakanan", menuName = "Game/Food Item Level 5")]
public class FoodItemDataLv5 : ScriptableObject
{
    public string   namaItem;    // Contoh: "Apel Segar", "Apel Memar"
    public Sprite   spriteItem;  // Icon makanan
    public bool     isSegar;     // true = makanan segar (boleh dimakan), false = meragukan
    public string   keterangan;  // Contoh: "Segar dan bergizi!" / "Sudah memar, hindari!"
}