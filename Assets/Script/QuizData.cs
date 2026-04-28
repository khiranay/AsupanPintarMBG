using UnityEngine;

[System.Serializable]
public class QuizData
{
    [Header("Identitas")]
    public int levelId;
    public string pertanyaan;

    [Header("Pilihan Jawaban (isi 4)")]
    public string[] pilihanJawaban;

    [Header("Jawaban Benar")]
    public int indexJawabanBenar;

    [Header("Penjelasan (tampil di panel)")]
    [TextArea(3, 5)]
    public string penjelasanBenar;

    [TextArea(3, 5)]
    public string penjelasanSalah;

    [Header("Ikon (opsional)")]
    public Sprite ikonBenar;
    public Sprite ikonSalah;
}