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
    [SerializeField] private float amplitudeCutoffBig = 0.95f;
    [SerializeField] private float fixedDelay = 0f;
    
    [Header("Word Sources")]
    [SerializeField] private TextAsset randomWordSource;
    [SerializeField] private TextAsset bigWordSource;
    
    [Header("UI Settings")]
    [SerializeField] private float bigFontSize = 48f;
    [SerializeField] private float smallFontSize = 24f;
    [SerializeField] private Color textColor = Color.white;
    
    private Canvas canvas;
    private List<BeatData> beatDataList = new List<BeatData>();
    private string[] randomWords;
    private string[] bigWords;
    private int currentBeatIndex = 0;
    private int currentBigWordIndex = 0;
    private float startTime;
    private List<ActiveText> activeTexts = new List<ActiveText>();
    
    private class BeatData
    {
        public float time;
        public int group;
        public float amplitude;
    }
    
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
        ParseWordSources();
        ParseBeatsFile();
        startTime = Time.time;
    }
    
    private void Update()
    {
        float currentTime = Time.time - startTime;
        
        if (currentBeatIndex < beatDataList.Count)
        {
            BeatData beat = beatDataList[currentBeatIndex];
            float adjustedTimestamp = beat.time + fixedDelay;
            if (currentTime >= adjustedTimestamp)
            {
                CreateAndDisplayText(beat);
                currentBeatIndex++;
            }
        }
        
        UpdateActiveTexts();
    }
    
    private void ParseWordSources()
    {
        if (randomWordSource != null)
        {
            randomWords = randomWordSource.text.Split('\n');
            for (int i = 0; i < randomWords.Length; i++)
            {
                randomWords[i] = randomWords[i].Trim();
            }
        }
        else
        {
            randomWords = new string[] { "word", "text", "item" };
        }
        
        if (bigWordSource != null)
        {
            bigWords = bigWordSource.text.Split('\n');
            for (int i = 0; i < bigWords.Length; i++)
            {
                bigWords[i] = bigWords[i].Trim();
            }
        }
        else
        {
            bigWords = new string[] { "BIG", "LARGE", "HUGE" };
        }
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
            if (parts.Length >= 3)
            {
                if (float.TryParse(parts[0], out float time) && 
                    int.TryParse(parts[1], out int group) && 
                    float.TryParse(parts[2], out float amplitude))
                {
                    BeatData beat = new BeatData
                    {
                        time = time,
                        group = group,
                        amplitude = amplitude
                    };
                    beatDataList.Add(beat);
                }
            }
        }
        
        Debug.Log($"Loaded {beatDataList.Count} beats from file");
    }
    
    private void CreateAndDisplayText(BeatData beat)
    {
        bool isBigWord = beat.amplitude >= amplitudeCutoffBig && targetGroup.Contains(beat.group);
        
        if (!isBigWord && beat.amplitude >= amplitudeCutoffBig)
        {
            return;
        }
        
        GameObject textObject = new GameObject("BeatText");
        textObject.transform.SetParent(canvas.transform, false);
        
        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        
        float randomX = Random.Range(100f, Screen.width - 100f);
        float randomY = Random.Range(100f, Screen.height - 100f);
        rectTransform.position = new Vector3(randomX, randomY, 0);
        
        TextMeshProUGUI textComponent = textObject.AddComponent<TextMeshProUGUI>();
        
        if (isBigWord)
        {
            if (bigWords != null && bigWords.Length > 0)
            {
                textComponent.text = bigWords[currentBigWordIndex % bigWords.Length];
                currentBigWordIndex++;
            }
            else
            {
                textComponent.text = "BIG";
            }
            textComponent.fontSize = bigFontSize;
        }
        else
        {
            if (randomWords != null && randomWords.Length > 0)
            {
                textComponent.text = randomWords[Random.Range(0, randomWords.Length)];
            }
            else
            {
                textComponent.text = "word";
            }
            textComponent.fontSize = smallFontSize;
        }
        
        textComponent.font = font;
        textComponent.color = textColor;
        textComponent.alignment = TextAlignmentOptions.Center;
        
        rectTransform.sizeDelta = new Vector2(300, 100);
        
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