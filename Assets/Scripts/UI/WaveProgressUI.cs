using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WaveProgressUI : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private Vector2 panelPosition = new Vector2(0, 50);
    [SerializeField] private Vector2 panelSize = new Vector2(300, 80);

    private GameObject progressPanel;
    private TextMeshProUGUI waveInfoText;
    private Image progressFillImage;
    private Canvas canvas;

    private int totalEnemies;
    private int enemiesKilled;

    void Awake()
    {
        CreateUI();
    }

    private void CreateUI()
    {
        // Find or create Canvas
        canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("WaveProgressCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // Create Panel
        progressPanel = new GameObject("WaveProgressPanel");
        progressPanel.transform.SetParent(canvas.transform, false);
        
        Image panelImage = progressPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f);
        
        RectTransform panelRect = progressPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0f);
        panelRect.anchorMax = new Vector2(0.5f, 0f);
        panelRect.pivot = new Vector2(0.5f, 0f);
        panelRect.anchoredPosition = panelPosition;
        panelRect.sizeDelta = panelSize;

        // Create Wave Info Text
        GameObject waveInfoObj = new GameObject("WaveInfoText");
        waveInfoObj.transform.SetParent(progressPanel.transform, false);
        
        waveInfoText = waveInfoObj.AddComponent<TextMeshProUGUI>();
        waveInfoText.text = "Wave 1: 0/10";
        waveInfoText.fontSize = 24;
        waveInfoText.alignment = TextAlignmentOptions.Center;
        waveInfoText.color = Color.white;
        waveInfoText.fontStyle = FontStyles.Bold;
        
        RectTransform waveInfoRect = waveInfoObj.GetComponent<RectTransform>();
        waveInfoRect.anchorMin = new Vector2(0, 1);
        waveInfoRect.anchorMax = new Vector2(1, 1);
        waveInfoRect.pivot = new Vector2(0.5f, 1f);
        waveInfoRect.anchoredPosition = new Vector2(0, -5);
        waveInfoRect.sizeDelta = new Vector2(-10, 30);

        // Create Progress Bar Background
        GameObject bgObj = new GameObject("ProgressBackground");
        bgObj.transform.SetParent(progressPanel.transform, false);
        
        Image bgImage = bgObj.AddComponent<Image>();
        Texture2D bgTex = new Texture2D(1, 1);
        bgTex.SetPixel(0, 0, Color.white);
        bgTex.Apply();
        bgImage.sprite = Sprite.Create(bgTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0);
        bgRect.anchorMax = new Vector2(1, 0);
        bgRect.pivot = new Vector2(0.5f, 0f);
        bgRect.anchoredPosition = new Vector2(0, 8);
        bgRect.sizeDelta = new Vector2(-20, 20);

        // Create Progress Fill Image
        GameObject fillObj = new GameObject("ProgressFillImage");
        fillObj.transform.SetParent(bgObj.transform, false);
        
        progressFillImage = fillObj.AddComponent<Image>();
        
        Texture2D fillTex = new Texture2D(1, 1);
        fillTex.SetPixel(0, 0, Color.white);
        fillTex.Apply();
        progressFillImage.sprite = Sprite.Create(fillTex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        
        progressFillImage.type = Image.Type.Filled;
        progressFillImage.fillMethod = Image.FillMethod.Horizontal;
        progressFillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        progressFillImage.fillAmount = 0f;
        progressFillImage.color = new Color(1f, 0.5f, 0f, 1f); // Orange
        
        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.sizeDelta = Vector2.zero;

        // Start hidden
        progressPanel.SetActive(false);
    }

    public void StartWave(string waveName, int totalEnemyCount)
    {
        totalEnemies = totalEnemyCount;
        enemiesKilled = 0;
        
        progressPanel.SetActive(true);
        UpdateDisplay(waveName);
    }

    public void EnemyKilled()
    {
        enemiesKilled++;
        Debug.Log($"[WaveProgressUI] Enemy killed! Progress: {enemiesKilled}/{totalEnemies}");
        UpdateDisplay(null);
    }

    public void HideProgress()
    {
        progressPanel.SetActive(false);
    }

    private void UpdateDisplay(string waveName)
    {
        if (waveName != null)
        {
            waveInfoText.text = $"{waveName}: {enemiesKilled}/{totalEnemies}";
        }
        else
        {
            // Keep the same wave name, just update numbers
            string currentText = waveInfoText.text;
            int colonIndex = currentText.IndexOf(':');
            if (colonIndex >= 0)
            {
                string nameOnly = currentText.Substring(0, colonIndex + 1);
                waveInfoText.text = $"{nameOnly} {enemiesKilled}/{totalEnemies}";
            }
        }

        float progress = totalEnemies > 0 ? (float)enemiesKilled / totalEnemies : 0f;
        progressFillImage.fillAmount = progress;

        // Change color based on progress
        if (progress < 0.33f)
            progressFillImage.color = new Color(1f, 0.3f, 0f, 1f); // Red-Orange
        else if (progress < 0.66f)
            progressFillImage.color = new Color(1f, 0.7f, 0f, 1f); // Orange-Yellow
        else
            progressFillImage.color = new Color(0f, 1f, 0f, 1f); // Green
    }
}
