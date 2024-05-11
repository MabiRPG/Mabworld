using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private AudioSource playerAudio;
    [Header("SFX")]
    [SerializeField]
    private AudioClip levelUpSFX;
    [SerializeField]
    private AudioClip emotionSuccessSFX;
    [SerializeField]
    private AudioClip emotionFailSFX;

    private void Start()
    {
        playerAudio = Player.Instance.gameObject.GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        Player.Instance.result.statusEvent.OnChange += StartPlayerSFX;
    }

    private void OnDisable()
    {
        Player.Instance.result.statusEvent.OnChange -= StartPlayerSFX;
    }

    private void StartPlayerSFX()
    {
        if (Player.Instance.result.isSuccess)
        {
            playerAudio.clip = emotionSuccessSFX;
        }
        else
        {
            playerAudio.clip = emotionFailSFX;
        }

        playerAudio.Play();
    }
}
