using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public bool enableLipSync = true;

    private AudioSource _audioSource;
    private LipSyncManager _lipSyncManager;

    void Awake()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    public void PlayAudio(string audioUrl)
    {
        if (string.IsNullOrEmpty(audioUrl)) return;
        StartCoroutine(DownloadAndPlay(audioUrl));
    }

    private IEnumerator DownloadAndPlay(string url)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("[Audio] Download failed: " + www.error);
                yield break;
            }

            AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
            if (clip != null)
            {
                _audioSource.clip = clip;
                _audioSource.Play();

                // Find LipSyncManager on VRM character
                if (_lipSyncManager == null)
                    _lipSyncManager = FindAnyObjectByType<LipSyncManager>();

                if (_lipSyncManager != null && enableLipSync)
                    _lipSyncManager.SetAudioSource(_audioSource);

                yield return new WaitUntil(() => !_audioSource.isPlaying);

                if (_lipSyncManager != null)
                    _lipSyncManager.ClearAudioSource();
            }
        }
    }

    public AudioSource GetAudioSource() => _audioSource;
}
