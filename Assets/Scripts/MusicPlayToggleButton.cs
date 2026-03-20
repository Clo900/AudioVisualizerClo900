using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class MusicPlayToggleButton : MonoBehaviour
{
    public MusicPlay MusicPlayer;
    public AudioSource AudioSourceRef;
    public Button ToggleButton;
    public TMP_Text Label;
    public string PlayText = "播放";
    public string PauseText = "暂停";
    public UnityEvent OnPlayUI;
    public UnityEvent OnPauseUI;

    bool lastIsPlaying;
    float lastPausedTime;
    bool hasPausedTime;

    void Start()
    {
        if (MusicPlayer == null) MusicPlayer = FindObjectOfType<MusicPlay>();
        if (ToggleButton == null) ToggleButton = GetComponent<Button>();
        if (Label == null) Label = GetComponentInChildren<TMP_Text>();
        if (ToggleButton != null) ToggleButton.onClick.AddListener(OnToggleClick);
        UpdateLabel();
        lastIsPlaying = MusicPlayer != null && MusicPlayer.IsPlaying();
    }

    void Update()
    {
        UpdateLabel();
        bool isPlaying = MusicPlayer != null && MusicPlayer.IsPlaying();
        if (isPlaying != lastIsPlaying)
        {
            if (isPlaying)
            {
                OnPlayUI.Invoke();
            }
            else
            {
                OnPauseUI.Invoke();
            }
            lastIsPlaying = isPlaying;
        }
    }

    void OnToggleClick()
    {
        if (MusicPlayer == null) return;
        if (MusicPlayer.IsPlaying())
        {
            if (AudioSourceRef != null && AudioSourceRef.clip != null)
            {
                lastPausedTime = AudioSourceRef.time;
                hasPausedTime = true;
            }
            MusicPlayer.Pause();
        }
        else
        {
            MusicPlayer.Play();
            if (AudioSourceRef != null && AudioSourceRef.clip != null && hasPausedTime)
            {
                float len = AudioSourceRef.clip.length;
                AudioSourceRef.time = Mathf.Clamp(lastPausedTime, 0f, Mathf.Max(0f, len - 0.01f));
            }
            hasPausedTime = false;
        }
    }

    void UpdateLabel()
    {
        if (MusicPlayer == null) return;
        if (Label != null) Label.text = MusicPlayer.IsPlaying() ? PauseText : PlayText;
    }
}
