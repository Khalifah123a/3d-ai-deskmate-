using UnityEngine;
using System.Collections;
using System.IO;
using UniGLTF;
using VRM;

public class VRMLoader : MonoBehaviour
{
    public string vrmPath = "";
    public Vector3 spawnPosition = Vector3.zero;
    public Vector3 spawnRotation = new Vector3(0, 180, 0);
    public Vector3 spawnScale = new Vector3(0.8f, 0.8f, 0.8f);

    private VRMImporterContext _context;
    private RuntimeGltfInstance _loadedInstance;
    private bool _isLoading = false;
    private PlaceholderAvatar _placeholder;

    public bool IsLoaded => _loadedInstance != null;
    public GameObject LoadedCharacter => _loadedInstance != null ? _loadedInstance.Root : null;

    public System.Action<GameObject> OnVRMLoaded;

    void Start()
    {
        _placeholder = GetComponent<PlaceholderAvatar>();

        if (!string.IsNullOrEmpty(vrmPath) && File.Exists(vrmPath))
            StartCoroutine(LoadVRM());
        else
            Debug.LogWarning("[VRM] No VRM path. Using placeholder.");
    }

    private IEnumerator LoadVRM()
    {
        if (_isLoading) yield break;
        _isLoading = true;

        Debug.Log("[VRM] Loading: " + vrmPath);

        RuntimeGltfInstance loaded = null;
        bool loadError = false;

        var data = new GlbFileParser(vrmPath).Parse();
        _context = new VRMImporterContext(new VRMData(data));

        var loadTask = _context.LoadAsync(new RuntimeOnlyAwaitCaller());
        while (!loadTask.IsCompleted) yield return null;

        if (loadTask.IsFaulted)
        {
            Debug.LogError("[VRM] Load failed: " + loadTask.Exception);
            loadError = true;
        }
        else
        {
            loaded = loadTask.Result;
            _loadedInstance = loaded;
        }

        if (loadError || loaded == null)
        {
            Cleanup();
            _isLoading = false;
            yield break;
        }

        loaded.EnableUpdateWhenOffscreen();
        loaded.ShowMeshes();

        GameObject root = loaded.Root;
        root.transform.position = spawnPosition;
        root.transform.localScale = spawnScale;
        root.transform.rotation = Quaternion.Euler(spawnRotation);
        root.name = "VRMCharacter";

        if (_placeholder != null)
            _placeholder.HidePlaceholder();

        SetupVRM(root);

        Debug.Log("[VRM] Loaded! Euler: " + root.transform.eulerAngles);

        // Disable VRMLookAtBoneApplyer permanently (causes arrow glitch)
        foreach (var lb in root.GetComponentsInChildren<VRMLookAtBoneApplyer>(true))
            lb.enabled = false;

        // Disable VRMFirstPerson (not needed)
        var firstPerson = root.GetComponent<VRMFirstPerson>();
        if (firstPerson != null) firstPerson.enabled = false;

        // Re-enable LookAtHead + SpringBone after 2 seconds
        StartCoroutine(EnablePhysicsDelayed(root));

        _isLoading = false;
        OnVRMLoaded?.Invoke(root);
    }

    private IEnumerator EnablePhysicsDelayed(GameObject root)
    {
        yield return new WaitForSeconds(2f);

        // Enable SpringBone physics (hair/clothes)
        var springs = root.GetComponentsInChildren<VRMSpringBone>(true);
        foreach (var s in springs)
        {
            s.enabled = true;
            s.m_gravityDir = new Vector3(0, -1, 0);
        }
        Debug.Log("[VRM] Enabled " + springs.Length + " SpringBone components");

        yield return new WaitForSeconds(0.5f);

        // Enable VRMLookAtHead with camera target
        var lookAt = root.GetComponent<VRMLookAtHead>();
        if (lookAt != null && Camera.main != null)
        {
            lookAt.Target = Camera.main.transform;
            lookAt.enabled = true;
            Debug.Log("[VRM] Enabled VRMLookAtHead (target: camera)");
        }

        // Keep VRMLookAtBoneApplyer disabled (causes arrow glitch)
        foreach (var lb in root.GetComponentsInChildren<VRMLookAtBoneApplyer>(true))
            lb.enabled = false;
    }

    private void SetupVRM(GameObject root)
    {
        // Disable everything initially
        var springs = root.GetComponentsInChildren<VRMSpringBone>(true);
        foreach (var s in springs) s.enabled = false;

        var lookAt = root.GetComponent<VRMLookAtHead>();
        if (lookAt != null) lookAt.enabled = false;

        var boneApplyer = root.GetComponent<VRMLookAtBoneApplyer>();
        if (boneApplyer != null) boneApplyer.enabled = false;

        foreach (var lb in root.GetComponentsInChildren<VRMLookAtBoneApplyer>(true))
            lb.enabled = false;
        foreach (var lh in root.GetComponentsInChildren<VRMLookAtHead>(true))
            lh.enabled = false;

        // Add our scripts
        var idle = root.AddComponent<VRMIdleAnimator>();
        idle.Init(root);

        var expr = root.AddComponent<ExpressionController>();
        var animBridge = root.AddComponent<VRMAnimationBridge>();
        animBridge.Init(root);
        expr.InitBridge();

        var lipSync = root.AddComponent<LipSyncManager>();
        lipSync.Init(expr, idle);

        Debug.Log("[VRM] Setup: " + springs.Length + " springs (disabled), LookAt disabled, idle+expression+lip ready");
    }

    private void Cleanup()
    {
        if (_context != null)
        {
            try { _context.Dispose(); } catch { }
            _context = null;
        }
    }

    public void Unload() { Cleanup(); _loadedInstance = null; _isLoading = false; }
    void OnDestroy() { Unload(); }
}
