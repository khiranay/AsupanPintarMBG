using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class SpotTheDifference : MonoBehaviour
{
    [Header("Gambar")]
    public Image imageBComponent;

    [Header("Area Perbedaan (Manual)")]
    public List<RectTransform> differenceAreas;

    [Header("UI")]
    public GameObject highlightPrefab;
    public Transform highlightParent;

    private List<bool> foundList = new List<bool>();

    void Start()
    {
        foundList.Clear();
        foreach (var area in differenceAreas)
        {
            foundList.Add(false);
        }
    }

    public void OnClickImageB(PointerEventData pointerData)
    {
        Vector2 screenPos = pointerData.position;
        CheckClick(screenPos);
    }

    void CheckClick(Vector2 screenPos)
    {
        for (int i = 0; i < differenceAreas.Count; i++)
        {
            if (foundList[i]) continue;

            RectTransform area = differenceAreas[i];

            if (RectTransformUtility.RectangleContainsScreenPoint(area, screenPos))
            {
                foundList[i] = true;

                // highlight di tengah area
                SpawnHighlight(area.position, true);

                CheckAllFound();
                return;
            }
        }

        // kalau salah
        SpawnHighlight(screenPos, false);
    }

    void SpawnHighlight(Vector2 screenPos, bool isCorrect)
    {
        if (highlightPrefab == null || highlightParent == null)
        {
            Debug.LogError("Highlight Prefab / Parent belum diisi!");
            return;
        }

        GameObject obj = Instantiate(highlightPrefab, highlightParent);

        RectTransform parentRect = highlightParent as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            screenPos,
            null,
            out Vector2 localPos
        );

        obj.GetComponent<RectTransform>().localPosition = localPos;

        Image img = obj.GetComponent<Image>();
        if (img != null)
            img.color = isCorrect ? Color.green : Color.red;

        if (!isCorrect) Destroy(obj, 1f);
    }

    void CheckAllFound()
    {
        foreach (bool found in foundList)
        {
            if (!found) return;
        }

        Debug.Log("Semua perbedaan ditemukan!");

        int level = PlayerPrefs.GetInt("CurrentLevel", 1);
        LevelProgressManager.CompleteMiniGame(level);
    }
}