using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance {get; private set;}

    public AudioSource globalAudio;
    private AudioSource playerAudio;

    [Header("SFX")]
    [SerializeField]
    private AudioClip levelUpSFX;
    [SerializeField]
    private AudioClip emotionSuccessSFX;
    [SerializeField]
    private AudioClip emotionFailSFX;

    private void Awake()
    {
        // Singleton recipe so only one instance is active at a time.
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        globalAudio = GetComponent<AudioSource>();
    }

    public void PlayGatherResultSFX(bool isSuccess)
    {
        if (isSuccess)
        {
            playerAudio.PlayOneShot(emotionSuccessSFX);
        }
        else
        {
            playerAudio.PlayOneShot(emotionFailSFX);
        }
    }

    public void SetPlayer(Player player)
    {
        playerAudio = player.gameObject.GetComponent<AudioSource>();
    }

    public void PlayLevelUpSFX()
    {
        playerAudio.PlayOneShot(levelUpSFX);
    }
}
