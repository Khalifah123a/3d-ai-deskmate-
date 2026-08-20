using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class SceneSetup
{
    [MenuItem("Tools/3D AI Assistant/Setup Scene")]
    public static void SetupScene()
    {
        var roots = Object.FindObjectsByType<Transform>(FindObjectsSortMode.None);
        var toDestroy = new System.Collections.Generic.List<GameObject>();
        foreach (var obj in roots)
            if (obj.parent == null) toDestroy.Add(obj.gameObject);
        foreach (var go in toDestroy)
            Object.DestroyImmediate(go);

        GameObject camObj = new GameObject("Main Camera");
        Camera cam = camObj.AddComponent<Camera>();
        camObj.tag = "MainCamera";
        camObj.AddComponent<AudioListener>();
        cam.transform.position = new Vector3(0, 1.2f, -2.5f);
        cam.transform.LookAt(new Vector3(0, 1f, 0));
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.08f, 0.12f);
        cam.nearClipPlane = 0.1f;

        GameObject lightObj = new GameObject("Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

        new GameObject("WebSocket").AddComponent<WebSocketClient>();
        new GameObject("AudioManager").AddComponent<AudioManager>();
        GameObject vrmLoader = new GameObject("VRMLoader");
        vrmLoader.AddComponent<VRMLoader>();
        vrmLoader.AddComponent<PlaceholderAvatar>();
        GameObject uiObj = new GameObject("UIManager");
        UIManager uiMgr = uiObj.AddComponent<UIManager>();

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        GameObject canvasObj = new GameObject("ChatCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.anchorMin = Vector2.zero;
        canvasRect.anchorMax = Vector2.one;
        canvasRect.offsetMin = Vector2.zero;
        canvasRect.offsetMax = Vector2.zero;
        canvasRect.localScale = Vector3.one;

        var existingEventSystem = UnityEngine.Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>();
        if (existingEventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        GameObject statusBar = CreatePanel(canvasObj.transform, "StatusBar",
            new Vector2(0, 0), new Vector2(1, 0.04f), new Color(0.05f, 0.05f, 0.08f, 1f));
        Text statusText = CreateText(statusBar.transform, "StatusText", font,
            "Backend: Connecting...", 14, Color.yellow, TextAnchor.MiddleLeft);
        RectTransform stRect = statusText.GetComponent<RectTransform>();
        stRect.offsetMin = new Vector2(15, 0);
        stRect.offsetMax = new Vector2(-15, 0);

        GameObject inputBar = CreatePanel(canvasObj.transform, "InputBar",
            new Vector2(0, 0.04f), new Vector2(1, 0.12f), new Color(0.1f, 0.1f, 0.15f, 0.95f));

        GameObject inputObj = CreatePanel(inputBar.transform, "InputField",
            new Vector2(0, 0), new Vector2(1, 1), new Color(0.2f, 0.2f, 0.28f, 1f));
        RectTransform inputRect = inputObj.GetComponent<RectTransform>();
        inputRect.offsetMin = new Vector2(10, 8);
        inputRect.offsetMax = new Vector2(-80, -8);

        InputField inputField = inputObj.AddComponent<InputField>();

        GameObject inputTextObj = CreatePanel(inputObj.transform, "Text",
            Vector2.zero, Vector2.one, Color.clear);
        Text inputText = CreateText(inputTextObj.transform, "", font,
            "", 22, Color.white, TextAnchor.MiddleLeft);
        RectTransform itr = inputTextObj.GetComponent<RectTransform>();
        itr.offsetMin = new Vector2(10, 0);
        itr.offsetMax = Vector2.zero;

        GameObject phObj = CreatePanel(inputObj.transform, "Placeholder",
            Vector2.zero, Vector2.one, Color.clear);
        Text phText = CreateText(phObj.transform, "", font,
            "Ketik pesan...", 22, new Color(0.5f, 0.5f, 0.55f), TextAnchor.MiddleLeft);
        phText.fontStyle = FontStyle.Italic;
        RectTransform phr = phObj.GetComponent<RectTransform>();
        phr.offsetMin = new Vector2(10, 0);
        phr.offsetMax = Vector2.zero;

        inputField.textComponent = inputText;
        inputField.placeholder = phText;

        GameObject btnObj = CreatePanel(inputBar.transform, "SendButton",
            new Vector2(1, 0), new Vector2(1, 1), new Color(0.2f, 0.55f, 0.9f, 1f));
        RectTransform btnRect = btnObj.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 0);
        btnRect.anchorMax = new Vector2(1, 1);
        btnRect.pivot = new Vector2(1, 0.5f);
        btnRect.offsetMin = new Vector2(-70, 8);
        btnRect.offsetMax = new Vector2(-8, -8);
        Button btn = btnObj.AddComponent<Button>();

        Text btnLabel = CreateText(btnObj.transform, "Label", font,
            "Kirim", 18, Color.white, TextAnchor.MiddleCenter);

        GameObject respArea = CreatePanel(canvasObj.transform, "ResponseArea",
            new Vector2(0.02f, 0.12f), new Vector2(0.98f, 0.96f), new Color(0, 0, 0, 0.7f));

        GameObject respTextObj = CreatePanel(respArea.transform, "ResponseText",
            Vector2.zero, Vector2.one, Color.clear);
        RectTransform respTextRect = respTextObj.GetComponent<RectTransform>();
        respTextRect.offsetMin = new Vector2(15, 15);
        respTextRect.offsetMax = new Vector2(-15, -15);
        Text respText = CreateText(respTextObj.transform, "", font,
            "AI: Halo! Ketik pesan di bawah.\n", 18, Color.white, TextAnchor.UpperLeft);
        respText.supportRichText = true;

        ScrollRect scroll = respArea.AddComponent<ScrollRect>();
        scroll.content = respTextRect;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.vertical = true;
        scroll.horizontal = false;

        uiMgr.legacyInputField = inputField;
        uiMgr.legacySendButton = btn;
        uiMgr.legacyResponseText = respText;
        uiMgr.legacyStatusText = statusText;
        uiMgr.chatScroll = scroll;

        btn.onClick.AddListener(() => uiMgr.SendChatMessage());

        EditorUtility.DisplayDialog("Setup Complete",
            "1. Restart backend: python server.py\n2. Set VRMLoader Vrm Path\n3. Play",
            "OK");
    }

    private static GameObject CreatePanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        if (color.a > 0.01f)
        {
            Image img = obj.AddComponent<Image>();
            img.color = color;
        }
        return obj;
    }

    private static Text CreateText(Transform parent, string name, Font font,
        string text, int fontSize, Color color, TextAnchor alignment)
    {
        GameObject obj = new GameObject(string.IsNullOrEmpty(name) ? "Text" : name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        Text t = obj.AddComponent<Text>();
        if (font != null) t.font = font;
        t.text = text;
        t.fontSize = fontSize;
        t.color = color;
        t.alignment = alignment;
        return t;
    }
}
