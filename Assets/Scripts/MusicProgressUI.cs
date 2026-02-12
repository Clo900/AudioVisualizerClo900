using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MusicProgressUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public MusicPlay MusicPlayer;
    public Slider ProgressSlider;
    public Text CurrentTimeText;
    bool isDragging;
    bool wasPlayingBeforeDrag;

    void Start()
    {
        ProgressSlider.minValue = 0f;
        ProgressSlider.maxValue = 1f;
        ProgressSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    void Update()
    {
        float duration = MusicPlayer != null ? MusicPlayer.GetDuration() : 0f;
        if (duration > 0f)
        {
            if (isDragging)
            {
                float previewTime = duration * ProgressSlider.value;
                CurrentTimeText.text = FormatTime(previewTime) + " / " + FormatTime(duration);
            }
            else
            {
                float t = MusicPlayer.GetCurrentTime() / duration;
                ProgressSlider.SetValueWithoutNotify(t);
                CurrentTimeText.text = FormatTime(MusicPlayer.GetCurrentTime()) + " / " + FormatTime(duration);
            }
        }
        else
        {
            ProgressSlider.SetValueWithoutNotify(0f);
            CurrentTimeText.text = "00:00 / 00:00";
        }
    }

    void OnSliderChanged(float value)
    {
        float duration = MusicPlayer != null ? MusicPlayer.GetDuration() : 0f;
        if (duration > 0f)
        {
            if (!isDragging)
            {
                MusicPlayer.Seek(duration * value);
            }
        }
    }

    string FormatTime(float time)
    {
        int totalSeconds = Mathf.FloorToInt(time);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return minutes.ToString("00") + ":" + seconds.ToString("00");
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        wasPlayingBeforeDrag = MusicPlayer != null && MusicPlayer.IsPlaying();
        if (MusicPlayer != null)
        {
            MusicPlayer.Pause();
        }
        isDragging = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        float duration = MusicPlayer != null ? MusicPlayer.GetDuration() : 0f;
        if (MusicPlayer != null && duration > 0f)
        {
            float target = duration * ProgressSlider.value;
            MusicPlayer.Seek(target);
            if (wasPlayingBeforeDrag)
            {
                MusicPlayer.Play();
            }
        }
    }
}
