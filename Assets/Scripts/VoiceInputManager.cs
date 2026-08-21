using UnityEngine;
using System.Collections;

public class VoiceInputManager : MonoBehaviour
{
    private UIManager _uiManager;
    private WebSocketClient _webSocket;
    private AudioClip _recordedClip;
    private bool _isRecording;
    private float _recordingStartTime;
    private const float MAX_RECORDING_TIME = 30f;
    private const float SILENCE_THRESHOLD = 0.01f;
    private const float SILENCE_TIMEOUT = 1.5f;
    
    private AudioSource _audioSource;
    
    void Awake()
    {
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }
    
    void Start()
    {
        _uiManager = FindAnyObjectByType<UIManager>();
        _webSocket = FindAnyObjectByType<WebSocketClient>();
    }
    
    public void StartRecording()
    {
        if (_isRecording) return;
        
        // Check microphone permission
        if (!Microphone.devices.Length.Equals(0))
        {
            Debug.Log("[Voice] No microphone found");
            return;
        }
        
        _recordedClip = Microphone.Start(null, false, 10, 16000);
        _isRecording = true;
        _recordingStartTime = Time.time;
        
        Debug.Log("[Voice] Recording started");
        
        if (_uiManager != null)
            _uiManager.UpdateStatus("Recording...");
    }
    
    public void StopRecording()
    {
        if (!_isRecording) return;
        
        Microphone.End(null);
        _isRecording = false;
        
        if (_recordedClip == null || _recordedClip.length < 0.5f)
        {
            Debug.Log("[Voice] Recording too short, discarded");
            if (_uiManager != null)
                _uiManager.UpdateStatus("Ready");
            return;
        }
        
        Debug.Log("[Voice] Recording stopped, length: " + _recordedClip.length + "s");
        ProcessRecording();
    }
    
    void Update()
    {
        if (!_isRecording) return;
        
        // Auto-stop after max time
        if (Time.time - _recordingStartTime >= MAX_RECORDING_TIME)
        {
            StopRecording();
            return;
        }
        
        // Check for silence
        if (_recordedClip != null && Time.time - _recordingStartTime > 1f)
        {
            float[] samples = new float[256];
            _recordedClip.GetData(samples, 0);
            
            float sum = 0f;
            for (int i = 0; i < samples.Length; i++)
                sum += Mathf.Abs(samples[i]);
            
            float average = sum / samples.Length;
            
            if (average < SILENCE_THRESHOLD && (Time.time - _recordingStartTime) > 3f)
            {
                Debug.Log("[Voice] Silence detected, stopping");
                StopRecording();
            }
        }
    }
    
    private void ProcessRecording()
    {
        // Convert to base64 for sending to backend
        float[] samples = new float[_recordedClip.samples];
        _recordedClip.GetData(samples, 0);
        
        // Simple amplitude check
        float sum = 0f;
        for (int i = 0; i < samples.Length; i++)
            sum += samples[i] * samples[i];
        
        float rms = Mathf.Sqrt(sum / samples.Length);
        
        if (rms < SILENCE_THRESHOLD)
        {
            Debug.Log("[Voice] Audio too quiet, ignoring");
            if (_uiManager != null)
                _uiManager.UpdateStatus("Ready");
            return;
        }
        
        // For now, just log that we would send to backend
        // In production, you'd send the audio data to a speech-to-text endpoint
        Debug.Log("[Voice] Audio processed, RMS: " + rms);
        
        if (_uiManager != null)
        {
            _uiManager.UpdateStatus("Processing voice...");
            // You would send to backend here:
            // _webSocket.SendAudio(samples, _recordedClip.frequency, _recordedClip.channels);
        }
    }
    
    public bool IsRecording => _isRecording;
    
    public float GetRecordingLevel()
    {
        if (!_isRecording || _recordedClip == null) return 0f;
        
        float[] samples = new float[64];
        _recordedClip.GetData(samples, _recordedClip.samples - 64);
        
        float sum = 0f;
        for (int i = 0; i < samples.Length; i++)
            sum += Mathf.Abs(samples[i]);
        
        return sum / samples.Length;
    }
}
