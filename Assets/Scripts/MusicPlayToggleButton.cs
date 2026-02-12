using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class MusicPlayToggleButton : MonoBehaviour
{
    public MusicPlay MusicPlayer;
    public Button ToggleButton;
    public TMP_Text Label;
    public string PlayText = "播放";
    public string PauseText = "暂停";
    public UnityEvent OnPlayUI;
    public UnityEvent OnPauseUI;

    bool lastIsPlaying;

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
            MusicPlayer.Pause();
            if (Label != null) Label.text = PlayText;
            OnPauseUI.Invoke();
        }
        else
        {
            MusicPlayer.Play();
            if (Label != null) Label.text = PauseText;
            OnPlayUI.Invoke();
        }
    }

    void UpdateLabel()
    {
        if (MusicPlayer == null) return;
        if (Label != null) Label.text = MusicPlayer.IsPlaying() ? PauseText : PlayText;
    }
}
