using UnityEngine;

public class SpaceActivateAndPlay : MonoBehaviour
{
    public GameObject target;
    public AudioSource audioSource;
    public MusicPlay musicPlay;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (target != null && !target.activeSelf) target.SetActive(true);

            var mp = musicPlay;
            if (mp == null && target != null) mp = target.GetComponent<MusicPlay>();

            var src = audioSource;
            if (src == null)
            {
                if (target != null) src = target.GetComponent<AudioSource>();
                if (src == null) src = FindObjectOfType<AudioSource>();
            }

            if (mp != null)
            {
                mp.Play();
            }
            else if (src != null)
            {
                if (!src.gameObject.activeSelf) src.gameObject.SetActive(true);
                if (src.clip != null) src.Play();
            }
        }
    }
}
