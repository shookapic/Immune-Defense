using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildMenuController : MonoBehaviour, IPointerExitHandler
{
    [Header("References")]
    [SerializeField] private RectTransform inventoryPanel; // your panel to slide

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.25f;
    [SerializeField] private Vector2 shownAnchoredPos = Vector2.zero; 
    // This is where the panel should be when it's visible (often 0,0)

    [Header("Auto-Close Settings")]
    [SerializeField] private bool autoCloseOnExit = true;

    private Vector2 hiddenAnchoredPos;
    private bool isShown = false;
    private bool isAnimating = false;

    private void Awake()
    {
        // Cache the starting (hidden) position from the current layout
        hiddenAnchoredPos = inventoryPanel.anchoredPosition;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (autoCloseOnExit && isShown && !isAnimating)
        {
            StartCoroutine(Slide(inventoryPanel, shownAnchoredPos, hiddenAnchoredPos));
            isShown = false;
        }
    }

    // Hook this up to your Button's OnClick
    public void ToggleInventory()
    {
        if (isAnimating) return;
        if (isShown)
            StartCoroutine(Slide(inventoryPanel, shownAnchoredPos, hiddenAnchoredPos));
        else
            StartCoroutine(Slide(inventoryPanel, hiddenAnchoredPos, shownAnchoredPos));

        isShown = !isShown;
    }

    private IEnumerator Slide(RectTransform panel, Vector2 from, Vector2 to)
    {
        isAnimating = true;
        float t = 0f;

        while (t < animationDuration)
        {
            t += Time.unscaledDeltaTime; // unscaled so it works even if game is paused
            float lerp = Mathf.Clamp01(t / animationDuration);
            panel.anchoredPosition = Vector2.Lerp(from, to, lerp);
            yield return null;
        }

        panel.anchoredPosition = to;
        isAnimating = false;
    }
}
