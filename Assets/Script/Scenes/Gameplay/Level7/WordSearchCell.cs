using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Komponen untuk setiap sel huruf di grid word search.
/// Attach ke prefab sel (Image + TextMeshProUGUI).
/// </summary>
public class WordSearchCell : MonoBehaviour
{
    [HideInInspector] public int row;
    [HideInInspector] public int col;
    [HideInInspector] public char letter;

    [Header("References")]
    [Tooltip("TextMeshProUGUI untuk menampilkan huruf")]
    public TextMeshProUGUI letterText;

    public void Initialize(int r, int c, char l)
    {
        row = r;
        col = c;
        letter = char.ToUpper(l);

        if (letterText != null)
            letterText.text = letter.ToString();
    }
}
