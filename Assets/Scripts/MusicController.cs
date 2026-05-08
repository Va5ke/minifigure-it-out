using UnityEngine;

public class MusicController : MonoBehaviour
{
    private static MusicController _instance;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        GetComponent<AudioSource>().Play();
    }
}