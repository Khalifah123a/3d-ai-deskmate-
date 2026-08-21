using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public bool enableLipSync = true;

    private AudioSource _audioSource;
    private LipSyncManager _lipSyncManager;
    
    // Object pooling for audio clips
    private readonly Queue<AudioClip> _clipPool = new Queue<AudioClip>();
    private const int POOL_SIZE = 5;

    void Awake()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.loop = false;
    }

    void Start()
    {
        // Pre-warm pool with null entries (will be filled on first use)
        for (int i = 0; i < POOL_SIZE; i++)
            _clipPool.Enqueue(null);
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
                // Return previous clip to pool if exists
                if (_audioSource.clip != null)
                {
                    _clipPool.Enqueue(_audioSource.clip);
                    if (_clipPool.Count > POOL_SIZE)
                        _clipPool.Dequeue();
                }

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
                
                // Return clip to pool for reuse
                _clipPool.Enqueue(clip);
                if (_clipPool.Count > POOL_SIZE)
                    _clipPool.Dequeue();
            }
        }
    }

    public AudioSource GetAudioSource() => _audioSource;
    
    public int PoolSize => _clipPool.Count;
}
