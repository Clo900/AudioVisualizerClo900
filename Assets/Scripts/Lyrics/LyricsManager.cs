using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text.RegularExpressions;

[System.Serializable]
public class LyricLine
{
    public float time;   //歌词时间（秒）
    public string text;  //歌词文本
}

public class LyricsManager : MonoBehaviour
{
    [Header("组件设置")]
    public AudioSource audioSource;        //播放歌曲
    public ScrollRect scrollRect;          //滚动区域
    public RectTransform content;          //内容区域
    public GameObject linePrefab;          //歌词行预制体
    public TextAsset lrcAsset;             //直接拖拽 LRC 文本
    public string lrcFileName = "song";    
    public float lineHeight = 30f;         //每行高度
    public float fadeInTime = 0.6f;

    [Header("动画设置")]
    public float smoothTime = 0.2f;        //滚动平滑时间
    public float highlightScale = 1.2f;    //高亮行放大倍数
    public float fadeOutTime = 3f;         //歌词结尾淡出时间

    private List<LyricLine> lyrics = new List<LyricLine>();
    private List<TextMeshProUGUI> lineTexts = new List<TextMeshProUGUI>();
    private int currentIndex = 0;
    private Vector2 scrollVelocity = Vector2.zero;
    private bool fadingOut = false;
    private float fadeTimer = 0f;
    private float fadeInTimer = 0f;
    private bool hasFadedIn = false;

    void Start()
    {
        if (!string.IsNullOrEmpty(lrcFileName))
        {
            lrcFileName = lrcFileName.Trim();
            if (lrcFileName.EndsWith(".lrc", System.StringComparison.OrdinalIgnoreCase))
            {
                lrcFileName = lrcFileName.Substring(0, lrcFileName.Length - 4);
            }
        }
        if (linePrefab != null)
        {
            var rt = linePrefab.GetComponent<RectTransform>();
            if (rt != null && rt.sizeDelta.y > 0f) lineHeight = rt.sizeDelta.y;
        }
        LoadLRC();            //加载 LRC 文件
        CreateLyricLines();   //创建歌词行
    }

    void Update()
    {
        bool playing = audioSource != null && audioSource.isPlaying;
        if (!playing)
        {
            hasFadedIn = false;
            fadeInTimer = 0f;
            for (int i = 0; i < lineTexts.Count; i++)
            {
                var c0 = lineTexts[i].color;
                c0.a = 0f;
                lineTexts[i].color = c0;
            }
            return;
        }
        if (lyrics.Count == 0) return;

        float time = audioSource.time;

        //找到当前播放时间对应的歌词行
        for (int i = 0; i < lyrics.Count; i++)
        {
            if (time < lyrics[i].time)
            {
                currentIndex = Mathf.Max(0, i - 1);
                break;
            }
            if (i == lyrics.Count - 1)
                currentIndex = i;
        }

        //高亮当前行并放大
        for (int i = 0; i < lineTexts.Count; i++)
        {
            if (i == currentIndex)
            {
                lineTexts[i].color = Color.yellow;
                lineTexts[i].transform.localScale = Vector3.Lerp(lineTexts[i].transform.localScale,
                    Vector3.one * highlightScale, 0.1f);
            }
            else
            {
                lineTexts[i].color = Color.white;
                lineTexts[i].transform.localScale = Vector3.Lerp(lineTexts[i].transform.localScale,
                    Vector3.one, 0.1f);
            }
        }

        //平滑滚动：使当前行居中
        float viewportHeight = scrollRect.viewport.rect.height;
        float targetY = currentIndex * lineHeight - viewportHeight / 2 + lineHeight / 2;
        targetY = Mathf.Clamp(targetY, 0, Mathf.Max(0, content.sizeDelta.y - viewportHeight)); //避免超出顶部或底部

        Vector2 targetPos = new Vector2(content.anchoredPosition.x, targetY);
        content.anchoredPosition = Vector2.SmoothDamp(content.anchoredPosition, targetPos, ref scrollVelocity, smoothTime);

        float uiAlpha = 1f;
        if (!hasFadedIn)
        {
            fadeInTimer += Time.deltaTime;
            uiAlpha = Mathf.Clamp01(fadeInTimer / Mathf.Max(0.0001f, fadeInTime));
            if (uiAlpha >= 1f) hasFadedIn = true;
        }

        if (currentIndex >= lyrics.Count - 1)
        {
            fadingOut = true;
        }

        if (fadingOut)
        {
            fadeTimer += Time.deltaTime;
            float outAlpha = Mathf.Lerp(1f, 0f, fadeTimer / Mathf.Max(0.0001f, fadeOutTime));
            uiAlpha = Mathf.Min(uiAlpha, outAlpha);
        }
        else
        {
            fadeTimer = 0f;
        }

        for (int i = 0; i < lineTexts.Count; i++)
        {
            Color c = lineTexts[i].color;
            c.a = uiAlpha;
            lineTexts[i].color = c;
        }
    }
 
/// <summary>
/// 解析 LRC 文件
/// </summary>
void LoadLRC()
{
    lyrics.Clear();
    TextAsset source = lrcAsset;
    if (source == null)
    {
        string key = (lrcFileName ?? string.Empty).Trim();
        if (key.EndsWith(".lrc", System.StringComparison.OrdinalIgnoreCase))
            key = key.Substring(0, key.Length - 4);
        source = Resources.Load<TextAsset>(key);
        if (source == null)
        {
            Debug.LogError("LRC file not found in Resources: " + key);
            return;
        }
    }

    string rawText = source.text ?? string.Empty;
    if (rawText.Length > 0 && rawText[0] == '\uFEFF') rawText = rawText.Substring(1);
    string[] lines = rawText.Split('\n');
    Regex tagRegex = new Regex(@"\[(\d{1,2}):(\d{2})(?:\.(\d{1,3}))?\]");

    foreach (string raw in lines)
    {
        string line = raw.TrimEnd('\r');
        if (string.IsNullOrWhiteSpace(line)) continue;

        MatchCollection matches = tagRegex.Matches(line);
        if (matches.Count == 0) continue;

        int lastEnd = matches[matches.Count - 1].Index + matches[matches.Count - 1].Length;
        string text = line.Substring(Mathf.Min(line.Length, lastEnd)).Trim();
        if (string.IsNullOrEmpty(text)) continue;

        foreach (Match m in matches)
        {
            int min = int.Parse(m.Groups[1].Value);
            int sec = int.Parse(m.Groups[2].Value);
            int ms = 0;
            if (m.Groups[3].Success)
            {
                string frac = m.Groups[3].Value;
                if (frac.Length == 1) ms = int.Parse(frac) * 100;
                else if (frac.Length == 2) ms = int.Parse(frac) * 10;
                else ms = int.Parse(frac.Substring(0, Mathf.Min(3, frac.Length)));
            }
            float t = min * 60f + sec + ms / 1000f;
            lyrics.Add(new LyricLine { time = t, text = text });
        }
    }

    lyrics.Sort((a, b) => a.time.CompareTo(b.time));
    if (lyrics.Count > 1)
    {
        var dedup = new List<LyricLine>(lyrics.Count);
        LyricLine prev = null;
        for (int i = 0; i < lyrics.Count; i++)
        {
            var cur = lyrics[i];
            if (prev == null || cur.time != prev.time || cur.text != prev.text)
            {
                dedup.Add(cur);
                prev = cur;
            }
        }
        lyrics = dedup;
    }
}

/// <summary>
/// 创建歌词行 UI
/// </summary>
void CreateLyricLines()
{
    if (content == null || linePrefab == null)
    {
        Debug.LogError("LyricsManager: Content or LinePrefab is not assigned.");
        return;
    }
    foreach (Transform child in content)
    {
        Destroy(child.gameObject);
    }
    lineTexts.Clear();

    foreach (var line in lyrics)
    {
        GameObject obj = Instantiate(linePrefab, content);
        TextMeshProUGUI text = obj.GetComponentInChildren<TextMeshProUGUI>(true);
        if (text != null)
        {
            text.text = line.text;
            var c = text.color;
            c.a = (audioSource != null && audioSource.isPlaying) ? 1f : 0f;
            text.color = c;
            lineTexts.Add(text);
        }
        else
        {
            Debug.LogError("LinePrefab is missing TextMeshProUGUI component.");
        }
    }

    content.sizeDelta = new Vector2(content.sizeDelta.x, lineHeight * Mathf.Max(lyrics.Count, 1));

    // 初始化时让当前（首行）居中一次
    if (scrollRect != null && content != null)
    {
        // 若已在播放，可按当前时间定位；否则默认首行
        int initIndex = 0;
        if (audioSource != null && audioSource.isPlaying && lyrics.Count > 0)
        {
            float t = audioSource.time;
            for (int i = 0; i < lyrics.Count; i++)
            {
                if (t < lyrics[i].time)
                {
                    initIndex = Mathf.Max(0, i - 1);
                    break;
                }
                if (i == lyrics.Count - 1) initIndex = i;
            }
        }
        currentIndex = Mathf.Clamp(initIndex, 0, Mathf.Max(0, lyrics.Count - 1));
        float viewportHeight = scrollRect.viewport != null ? scrollRect.viewport.rect.height : 0f;
        float targetY = currentIndex * lineHeight - viewportHeight / 2f + lineHeight / 2f;
        targetY = Mathf.Clamp(targetY, 0f, Mathf.Max(0f, content.sizeDelta.y - viewportHeight));
        content.anchoredPosition = new Vector2(content.anchoredPosition.x, targetY);
    }
}
}
