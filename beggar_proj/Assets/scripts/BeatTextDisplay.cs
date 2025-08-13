using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BeatTextDisplay : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private TMP_FontAsset font;
    [SerializeField] private TextAsset beatsFile;
    [SerializeField] private float holdDuration = 0.4f;
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private List<int> targetGroup = new();
    [SerializeField] private float fixedDelay = 0f;
    
    [Header("UI Settings")]
    [SerializeField] private float fontSize = 36f;
    [SerializeField] private Color textColor = Color.white;
    
    private Canvas canvas;
    private List<float> timestamps = new List<float>();
    private string[] randomWords = { "apple", "orange", "pumpkin" };
    private int currentTimestampIndex = 0;
    private float startTime;
    private List<ActiveText> activeTexts = new List<ActiveText>();
    
    private class ActiveText
    {
        public GameObject gameObject;
        public TextMeshProUGUI textComponent;
        public float createTime;
        public bool fadingOut;
        public float fadeStartTime;
    }
    
    private void Start()
    {
        CreateCanvas();
        ParseBeatsFile();
        startTime = Time.time;
    }
    
    private void Update()
    {
        float currentTime = Time.time - startTime;
        
        if (currentTimestampIndex < timestamps.Count)
        {
            float adjustedTimestamp = timestamps[currentTimestampIndex] + fixedDelay;
            if (currentTime >= adjustedTimestamp)
            {
                CreateAndDisplayText();
                currentTimestampIndex++;
            }
        }
        
        UpdateActiveTexts();
    }
    
    private void CreateCanvas()
    {
        GameObject canvasObject = new GameObject("BeatTextCanvas");
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
        canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasScaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObject.AddComponent<GraphicRaycaster>();
    }
    
    private void ParseBeatsFile()
    {
        if (beatsFile == null)
        {
            Debug.LogError("Beats file is not assigned!");
            return;
        }
        
        string[] lines = beatsFile.text.Split('\n');
        
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line))
                continue;
                
            string[] parts = line.Split(',');
            if (parts.Length >= 2)
            {
                if (float.TryParse(parts[0], out float time) && int.TryParse(parts[1], out int group))
                {
                    if (targetGroup.Contains(group))
                    {
                        timestamps.Add(time);
                    }
                }
            }
        }
        
        Debug.Log($"Loaded {timestamps.Count} timestamps for group {targetGroup}");
    }
    
    private void CreateAndDisplayText()
    {
        GameObject textObject = new GameObject("BeatText");
        textObject.transform.SetParent(canvas.transform, false);
        
        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        
        float randomX = Random.Range(100f, Screen.width - 100f);
        float randomY = Random.Range(100f, Screen.height - 100f);
        rectTransform.position = new Vector3(randomX, randomY, 0);
        
        TextMeshProUGUI textComponent = textObject.AddComponent<TextMeshProUGUI>();
        textComponent.text = randomWords[Random.Range(0, randomWords.Length)];
        textComponent.font = font;
        textComponent.fontSize = fontSize;
        textComponent.color = textColor;
        textComponent.alignment = TextAlignmentOptions.Center;
        
        rectTransform.sizeDelta = new Vector2(200, 50);
        
        ActiveText activeText = new ActiveText
        {
            gameObject = textObject,
            textComponent = textComponent,
            createTime = Time.time,
            fadingOut = false,
            fadeStartTime = 0f
        };
        
        activeTexts.Add(activeText);
    }
    
    private void UpdateActiveTexts()
    {
        for (int i = activeTexts.Count - 1; i >= 0; i--)
        {
            ActiveText activeText = activeTexts[i];
            float timeSinceCreation = Time.time - activeText.createTime;
            
            if (!activeText.fadingOut && timeSinceCreation >= holdDuration)
            {
                activeText.fadingOut = true;
                activeText.fadeStartTime = Time.time;
            }
            
            if (activeText.fadingOut)
            {
                float fadeElapsed = Time.time - activeText.fadeStartTime;
                float fadeProgress = fadeElapsed / fadeDuration;
                
                if (fadeProgress >= 1f)
                {
                    Destroy(activeText.gameObject);
                    activeTexts.RemoveAt(i);
                }
                else
                {
                    float alpha = Mathf.Lerp(1f, 0f, fadeProgress);
                    Color currentColor = activeText.textComponent.color;
                    activeText.textComponent.color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
                }
            }
        }
    }
}