using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaveCountdownUI : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private Vector2 panelPosition = new Vector2(0, -100);
    [SerializeField] private Vector2 panelSize = new Vector2(400, 150);
    
    [Header("Animation Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float fadeOutDuration = 0.3f;

    private GameObject countdownPanel;
    private TextMeshProUGUI waveNameText;
    private TextMeshProUGUI countdownText;
    private Image countdownFillImage;
    private CanvasGroup canvasGroup;
    private Coroutine countdownCoroutine;
    private Canvas canvas;

    void Awake()
    {
        Debug.Log("[WaveCountdownUI] Creating UI...");
        CreateUI();
        Debug.Log("[WaveCountdownUI] UI created successfully!");
    }

    private void CreateUI()
    {
        // Find or create Canvas
        canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("WaveCountdownCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100; // Make sure it's on top
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create Panel
        countdownPanel = new GameObject("WaveCountdownPanel");
        countdownPanel.transform.SetParent(canvas.transform, false);
        
        Image panelImage = countdownPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.85f);
        
        RectTransform panelRect = countdownPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0, 0);
        panelRect.sizeDelta = new Vector2(500, 200);

        canvasGroup = countdownPanel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        // Create Wave Name Text
        GameObject waveNameObj = new GameObject("WaveNameText");
        waveNameObj.transform.SetParent(countdownPanel.transform, false);
        
        waveNameText = waveNameObj.AddComponent<TextMeshProUGUI>();
        waveNameText.text = "Wave 1";
        waveNameText.fontSize = 36;
        waveNameText.alignment = TextAlignmentOptions.Center;
        waveNameText.color = Color.white;
        waveNameText.fontStyle = FontStyles.Bold;
        
        RectTransform waveNameRect = waveNameObj.GetComponent<RectTransform>();
        waveNameRect.anchorMin = new Vector2(0, 0.5f);
        waveNameRect.anchorMax = new Vector2(1, 1);
        waveNameRect.pivot = new Vector2(0.5f, 0.5f);
        waveNameRect.anchoredPosition = new Vector2(0, 20);
        waveNameRect.sizeDelta = new Vector2(-40, 0);

        // Create Countdown Text
        GameObject countdownTextObj = new GameObject("CountdownText");
        countdownTextObj.transform.SetParent(countdownPanel.transform, false);
        
        countdownText = countdownTextObj.AddComponent<TextMeshProUGUI>();
        countdownText.text = "";
        countdownText.fontSize = 72;
        countdownText.alignment = TextAlignmentOptions.Center;
        countdownText.color = new Color(1f, 0.8f, 0f); // Yellow/Orange
        countdownText.fontStyle = FontStyles.Bold;
        
        RectTransform countdownTextRect = countdownTextObj.GetComponent<RectTransform>();
        countdownTextRect.anchorMin = new Vector2(0, 0.3f);
        countdownTextRect.anchorMax = new Vector2(1, 0.7f);
        countdownTextRect.pivot = new Vector2(0.5f, 0.5f);
        countdownTextRect.anchoredPosition = new Vector2(0, 0);
        countdownTextRect.sizeDelta = new Vector2(-40, 0);

        // Create Fill Image (Progress Bar)
        GameObject fillObj = new GameObject("CountdownFillImage");
        fillObj.transform.SetParent(countdownPanel.transform, false);
        
        countdownFillImage = fillObj.AddComponent<Image>();
        
        // Create a white sprite for the fill
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        countdownFillImage.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        
        countdownFillImage.type = Image.Type.Filled;
        countdownFillImage.fillMethod = Image.FillMethod.Horizontal;
        countdownFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        countdownFillImage.fillAmount = 0f;
        countdownFillImage.color = new Color(0f, 1f, 0f, 0.8f); // Green
        
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(1, 0);
        fillRect.pivot = new Vector2(0.5f, 0f);
        fillRect.anchoredPosition = new Vector2(0, 20);
        fillRect.sizeDelta = new Vector2(-40, 15);

        // Start hidden
        countdownPanel.SetActive(false);
    }

    /// <summary>
    /// Start the countdown display with wave name
    /// </summary>
    public void StartCountdown(string waveName, float duration)
    {
        Debug.Log($"[WaveCountdownUI] StartCountdown called: {waveName}, {duration}s");
        
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        countdownCoroutine = StartCoroutine(CountdownRoutine(waveName, duration));
    }

    /// <summary>
    /// Hide the countdown immediately
    /// </summary>
    public void Hide()
    {
        if (countdownCoroutine != null)
            StopCoroutine(countdownCoroutine);

        countdownPanel.SetActive(false);
        canvasGroup.alpha = 0f;
    }

    private IEnumerator CountdownRoutine(string waveName, float duration)
    {
        Debug.Log($"[WaveCountdownUI] CountdownRoutine started");
        
        // Show panel
        countdownPanel.SetActive(true);
        waveNameText.text = waveName;
        
        // Reset fill to 0
        if (countdownFillImage != null)
        {
            countdownFillImage.fillAmount = 0f;
            Debug.Log($"[WaveCountdownUI] Fill image reset to 0");
        }

        // Fade in
        yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, 1f, fadeInDuration));

        Debug.Log($"[WaveCountdownUI] Countdown visible, counting down from {duration}s");

        // Countdown
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float remaining = Mathf.Max(0f, duration - elapsed);
            float progress = elapsed / duration; // Changed from 1f - (remaining / duration)

            // Update countdown text
            countdownText.text = Mathf.Ceil(remaining).ToString("F0");

            // Update fill image
            if (countdownFillImage != null)
                countdownFillImage.fillAmount = progress;

            yield return null;
        }

        // Ensure final state
        countdownText.text = "0";
        if (countdownFillImage != null)
            countdownFillImage.fillAmount = 1f;

        // Fade out
        yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 1f, 0f, fadeOutDuration));

        // Hide panel
        countdownPanel.SetActive(false);
        countdownCoroutine = null;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        cg.alpha = to;
    }
}
