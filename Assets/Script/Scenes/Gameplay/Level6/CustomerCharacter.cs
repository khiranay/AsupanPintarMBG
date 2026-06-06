using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Component untuk customer/NPC character yang bereaksi terhadap makanan.
/// Attach ke GameObject karakter pelanggan.
/// </summary>
public class CustomerCharacter : MonoBehaviour
{
    [Header("Character Sprites - Drag Sprite Asset dari Project")]
    [Tooltip("Sprite karakter saat idle/normal (drag dari Project window)")]
    public Sprite spriteIdle;
    [Tooltip("Sprite karakter saat senang (drag dari Project window)")]
    public Sprite spriteHappy;
    [Tooltip("Sprite karakter saat muntah (drag dari Project window)")]
    public Sprite spriteSick;

    [Header("Reaction Settings")]
    [Tooltip("Durasi menampilkan reaksi (detik)")]
    public float reactionDuration = 1.5f;

    [Header("Audio (Opsional)")]
    public AudioClip soundHappy;
    public AudioClip soundSick;

    // Private - otomatis di-set
    private Image characterImage;
    private AudioSource audioSource;
    private Coroutine currentReaction;
    private Vector2 originalSizeDelta;
    private Vector3 originalLocalPosition;

    void Awake()
    {
        characterImage = GetComponent<Image>();

        if (characterImage == null)
        {
            Debug.LogError("[CustomerCharacter] GameObject tidak punya Image component!");
            return;
        }

        // Simpan ukuran & posisi asli supaya sprite swap tidak menggeser
        originalSizeDelta = characterImage.rectTransform.sizeDelta;
        originalLocalPosition = characterImage.rectTransform.localPosition;

        audioSource = GetComponent<AudioSource>();

        if (spriteIdle != null)
            SetSprite(spriteIdle);
        else
            Debug.LogWarning("[CustomerCharacter] Sprite Idle belum di-assign!");
    }

    /// <summary>
    /// Ganti sprite tanpa mengubah ukuran/posisi RectTransform.
    /// </summary>
    private void SetSprite(Sprite sprite)
    {
        if (sprite == null || characterImage == null) return;

        characterImage.sprite = sprite;

        // Paksa kembalikan ukuran & posisi agar tidak bergeser
        characterImage.rectTransform.sizeDelta = originalSizeDelta;
        characterImage.rectTransform.localPosition = originalLocalPosition;
    }

    /// <summary>
    /// Trigger reaksi karakter berdasarkan jenis makanan.
    /// </summary>
    public void ReactToFood(bool isFreshFood)
    {
        if (currentReaction != null)
            StopCoroutine(currentReaction);

        currentReaction = StartCoroutine(ShowReaction(isFreshFood));
    }

    IEnumerator ShowReaction(bool isFreshFood)
    {
        if (characterImage == null)
        {
            Debug.LogWarning("[CustomerCharacter] Character Image tidak di-assign!");
            yield break;
        }

        if (isFreshFood)
        {
            SetSprite(spriteHappy);

            if (audioSource != null && soundHappy != null)
                audioSource.PlayOneShot(soundHappy);

            Debug.Log("[CustomerCharacter] Reaksi: SENANG!");
        }
        else
        {
            SetSprite(spriteSick);

            if (audioSource != null && soundSick != null)
                audioSource.PlayOneShot(soundSick);

            Debug.Log("[CustomerCharacter] Reaksi: MUNTAH!");
        }

        yield return new WaitForSeconds(reactionDuration);

        SetSprite(spriteIdle);
        currentReaction = null;
    }

    /// <summary>
    /// Force reset ke idle sprite.
    /// </summary>
    public void ResetToIdle()
    {
        if (currentReaction != null)
        {
            StopCoroutine(currentReaction);
            currentReaction = null;
        }

        SetSprite(spriteIdle);
    }
}
