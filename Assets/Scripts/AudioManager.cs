using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance {get; private set;}
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
    }

    private void Start()
    {
        playerAudio = Player.Instance.gameObject.GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        Player.Instance.result.statusEvent.OnChange += PlayResultSFX;
    }

    private void OnDisable()
    {
        Player.Instance.result.statusEvent.OnChange -= PlayResultSFX;
    }

    private void PlayResultSFX()
    {
        if (Player.Instance.result.isSuccess)
        {
            playerAudio.PlayOneShot(emotionSuccessSFX);
        }
        else
        {
            playerAudio.PlayOneShot(emotionFailSFX);
        }
    }

    public void PlayLevelUpSFX()
    {
        playerAudio.PlayOneShot(levelUpSFX);
    }
}
