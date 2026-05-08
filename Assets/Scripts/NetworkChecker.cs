using System.Collections;
using UnityEngine;
using UnityEngine.Android;

public class NetworkChecker : MonoBehaviour
{
    [Header("No Internet UI")]
    [SerializeField] private GameObject noInternetPanel;

    private void Start()
    {
        StartCoroutine(CheckConnectivityLoop());
    }

    [SerializeField] private SceneController sceneController;
    private bool _gameStarted = false;

    [SerializeField] private AudioSource backgroundMusic;

    private IEnumerator CheckConnectivityLoop()
    {
        while (true)
        {
            bool hasInternet = Application.internetReachability != NetworkReachability.NotReachable;
            noInternetPanel.SetActive(!hasInternet);

            if (hasInternet && !_gameStarted)
            {
                _gameStarted = true;
                backgroundMusic.Play();
                sceneController.StartGame();
            }

            yield return new WaitForSeconds(hasInternet ? 5f : 1f);
        }
    }

    // Called by the "Open Wi-Fi Settings" button in the noInternetPanel
    public void OpenWifiSettings()
    {
#if UNITY_ANDROID
        using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
        using (var intent = new AndroidJavaObject("android.content.Intent", "android.settings.WIFI_SETTINGS"))
        {
            activity.Call("startActivity", intent);
        }
#elif UNITY_IOS
        // iOS does not allow apps to open Wi-Fi settings directly since iOS 11.
        // The closest permitted action is opening the general Settings app.
        Application.OpenURL("App-Prefs:root=WIFI");
#endif
    }
}