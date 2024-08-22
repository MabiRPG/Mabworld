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

    private void Start()
    {
        // playerAudio = Player.Instance.gameObject.GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        // Player.Instance.trainingEvent += PlayResultSFX;
    }

    private void OnDisable()
    {
        // Player.Instance.trainingEvent -= PlayResultSFX;
    }

    private void PlayResultSFX<T>(T resultHandler) where T : ResultHandler
    {
        if (resultHandler.isSuccess)
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
