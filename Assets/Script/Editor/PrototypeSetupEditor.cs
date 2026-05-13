using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class PrototypeSetupEditor
{
    private const string EventFolder = "Assets/GameData/Events";

    [MenuItem("Tools/DisaVenture/Setup Prototype Scene")]
    public static void SetupPrototypeScene()
    {
        // #region agent log
        File.AppendAllText("debug-9a3fa9.log", "{\"sessionId\":\"9a3fa9\",\"runId\":\"pre-fix\",\"hypothesisId\":\"H2\",\"location\":\"PrototypeSetupEditor.cs:SetupPrototypeScene\",\"message\":\"SetupPrototypeScene started\",\"data\":{},\"timestamp\":" + System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "}\n");
        // #endregion

        Canvas canvas = FindOrCreateCanvas();
        EnsureEventSystem();

        GameManager gameManager = FindOrCreateComponent<GameManager>("GameManager");
        Textsystem textsystem = FindOrCreateComponent<Textsystem>("Textsystem");

        Text eventText = CreateOrFindText(canvas.transform, "EventText", new Vector2(0f, 90f), new Vector2(900f, 260f), TextAnchor.UpperLeft, 28);
        Text choiceAText = CreateOrFindText(canvas.transform, "ChoiceAText", new Vector2(-220f, -150f), new Vector2(380f, 70f), TextAnchor.MiddleCenter, 24);
        Text choiceBText = CreateOrFindText(canvas.transform, "ChoiceBText", new Vector2(220f, -150f), new Vector2(380f, 70f), TextAnchor.MiddleCenter, 24);
        Text statusText = CreateOrFindText(canvas.transform, "StatusText", new Vector2(0f, 305f), new Vector2(1100f, 70f), TextAnchor.MiddleLeft, 22);
        Text messageText = CreateOrFindText(canvas.transform, "MessageText", new Vector2(0f, -250f), new Vector2(1100f, 55f), TextAnchor.MiddleCenter, 22);

        Button choiceAButton = CreateOrFindButton(canvas.transform, "ChoiceAButton", new Vector2(-220f, -150f), new Vector2(400f, 90f));
        Button choiceBButton = CreateOrFindButton(canvas.transform, "ChoiceBButton", new Vector2(220f, -150f), new Vector2(400f, 90f));

        AssignTextsystemReferences(textsystem, gameManager, eventText, choiceAText, choiceBText, statusText, messageText, choiceAButton, choiceBButton);

        // #region agent log
        File.AppendAllText("debug-9a3fa9.log", "{\"sessionId\":\"9a3fa9\",\"runId\":\"pre-fix\",\"hypothesisId\":\"H2\",\"location\":\"PrototypeSetupEditor.cs:SetupPrototypeScene\",\"message\":\"References assigned via serialized object\",\"data\":{\"textsystemFound\":" + (textsystem != null ? "true" : "false") + ",\"gameManagerFound\":" + (gameManager != null ? "true" : "false") + "},\"timestamp\":" + System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "}\n");
        // #endregion

        EditorUtility.SetDirty(textsystem);
        EditorUtility.SetDirty(gameManager);
        AssetDatabase.SaveAssets();

        // #region agent log
        File.AppendAllText("debug-9a3fa9.log", "{\"sessionId\":\"9a3fa9\",\"runId\":\"pre-fix\",\"hypothesisId\":\"H3\",\"location\":\"PrototypeSetupEditor.cs:SetupPrototypeScene\",\"message\":\"SetupPrototypeScene completed\",\"data\":{},\"timestamp\":" + System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "}\n");
        // #endregion
        Debug.Log("DisaVenture prototype scene setup completed.");
    }

    [MenuItem("Tools/DisaVenture/Create Sample Events")]
    public static void CreateSampleEvents()
    {
        EnsureFolder("Assets/GameData");
        EnsureFolder(EventFolder);

        CreateEventAsset(
            "E001_StreetBlock",
            GamePhase.Evacuation,
            10,
            "倒壊した道路の先で、住民が助けを求めている。",
            CreateChoice("助ける (HP-1, SAN+1, フラグ付与)", -1, 0, 1, 0, 0, null, new List<string> { "helpedResident1" }, null, false, GamePhase.Evacuation),
            CreateChoice("物資を探す (HP-1, 物資+2)", -1, 0, 0, 2, 0, null, null, null, false, GamePhase.Evacuation)
        );

        CreateEventAsset(
            "E002_FamilyCall",
            GamePhase.Evacuation,
            10,
            "近所の家族が避難経路を探せず立ち往生している。",
            CreateChoice("誘導して助ける (HP-1, SAN+1, フラグ付与)", -1, 0, 1, 0, 0, null, new List<string> { "helpedFamily" }, null, false, GamePhase.Evacuation),
            CreateChoice("商店に向かう (SAN-1, 物資+2)", 0, 0, -1, 2, 0, null, null, null, false, GamePhase.Evacuation)
        );

        CreateEventAsset(
            "E003_ElderlySupport",
            GamePhase.Evacuation,
            10,
            "高齢者が転倒し、避難所までの移動支援が必要だ。",
            CreateChoice("介助する (HP-1, SAN+1, フラグ付与)", -1, 0, 1, 0, 0, null, new List<string> { "helpedElderly" }, null, false, GamePhase.Evacuation),
            CreateChoice("荷物を回収する (HP-1, 物資+3, SAN-1)", -1, 0, -1, 3, 0, null, null, null, false, GamePhase.Evacuation)
        );

        CreateEventAsset(
            "E004_ToShelter",
            GamePhase.Evacuation,
            9,
            "避難所の受け入れが始まった。今のうちに合流するか？",
            CreateChoice("避難所に向かう", 0, 0, 0, 0, 0, null, null, null, true, GamePhase.PostEvacuation),
            CreateChoice("最後に物資を回収して向かう (HP-1, 物資+1)", -1, 0, 0, 1, 0, null, null, null, true, GamePhase.PostEvacuation)
        );

        CreateEventAsset(
            "L001_DailyMeal",
            GamePhase.PostEvacuation,
            10,
            "配給が不足している。今日の食事をどう確保する？",
            CreateChoice("物資を使って食事を確保 (物資-1, 腹+2)", 0, 2, 0, 0, 1, null, null, null, false, GamePhase.PostEvacuation),
            CreateChoice("我慢する (腹-2, SAN-1)", 0, -2, -1, 0, 0, null, null, null, false, GamePhase.PostEvacuation)
        );

        CreateEventAsset(
            "L002_InfoMail",
            GamePhase.PostEvacuation,
            10,
            "救援物資を装ったメールが届いた。返信を求められている。",
            CreateChoice("無視して公式情報を確認 (SAN+1)", 0, 0, 1, 0, 0, null, null, null, false, GamePhase.PostEvacuation),
            CreateChoice("返信してしまう (物資-2, SAN-1)", 0, 0, -1, -2, 0, null, null, null, false, GamePhase.PostEvacuation)
        );

        CreateEventAsset(
            "L003_ThanksSupply",
            GamePhase.PostEvacuation,
            11,
            "以前助けた住民からお礼として食料を受け取った。",
            CreateChoice("受け取り、共有する (物資+1, SAN+1)", 0, 0, 1, 1, 0, null, null, null, false, GamePhase.PostEvacuation),
            CreateChoice("自分の分だけ受け取る (物資+2, SAN-1)", 0, 0, -1, 2, 0, null, null, null, false, GamePhase.PostEvacuation),
            new List<string> { "helpedResident1" }
        );

        CreateEventAsset(
            "L004_FamilyNetwork",
            GamePhase.PostEvacuation,
            11,
            "助けた家族経由で、信頼できる支援情報が回ってきた。",
            CreateChoice("情報を活用する (SAN+2)", 0, 0, 2, 0, 0, null, null, null, false, GamePhase.PostEvacuation),
            CreateChoice("物資交換に使う (物資+1, SAN+1)", 0, 0, 1, 1, 0, null, null, null, false, GamePhase.PostEvacuation),
            new List<string> { "helpedFamily" }
        );

        CreateEventAsset(
            "L005_ElderlyAdvice",
            GamePhase.PostEvacuation,
            11,
            "介助した高齢者から詐欺の手口を教えてもらった。",
            CreateChoice("周囲にも共有する (SAN+2)", 0, 0, 2, 0, 0, null, null, null, false, GamePhase.PostEvacuation),
            CreateChoice("自分だけ覚える (SAN+1, 物資+1)", 0, 0, 1, 1, 0, null, null, null, false, GamePhase.PostEvacuation),
            new List<string> { "helpedElderly" }
        );

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        AssignAllEventAssetsToGameManager();
        Debug.Log("DisaVenture sample events created and assigned.");
    }

    private static void AssignAllEventAssetsToGameManager()
    {
        GameManager gameManager = Object.FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogWarning("GameManager not found. Events were created, but assignment was skipped.");
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:EventData", new[] { EventFolder });
        List<EventData> events = new List<EventData>();
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            EventData eventData = AssetDatabase.LoadAssetAtPath<EventData>(path);
            if (eventData != null)
            {
                events.Add(eventData);
            }
        }

        SerializedObject so = new SerializedObject(gameManager);
        SerializedProperty prop = so.FindProperty("allEvents");
        prop.ClearArray();
        for (int i = 0; i < events.Count; i++)
        {
            prop.InsertArrayElementAtIndex(i);
            prop.GetArrayElementAtIndex(i).objectReferenceValue = events[i];
        }
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(gameManager);
    }

    private static void AssignTextsystemReferences(
        Textsystem textsystem,
        GameManager gameManager,
        Text eventText,
        Text choiceAText,
        Text choiceBText,
        Text statusText,
        Text messageText,
        Button choiceAButton,
        Button choiceBButton)
    {
        SerializedObject so = new SerializedObject(textsystem);
        so.FindProperty("gameManager").objectReferenceValue = gameManager;
        so.FindProperty("eventText").objectReferenceValue = eventText;
        so.FindProperty("choiceAText").objectReferenceValue = choiceAText;
        so.FindProperty("choiceBText").objectReferenceValue = choiceBText;
        so.FindProperty("statusText").objectReferenceValue = statusText;
        so.FindProperty("messageText").objectReferenceValue = messageText;
        so.FindProperty("choiceAButton").objectReferenceValue = choiceAButton;
        so.FindProperty("choiceBButton").objectReferenceValue = choiceBButton;
        so.ApplyModifiedProperties();
    }

    private static ChoiceData CreateChoice(
        string label,
        int hpDelta,
        int hungerDelta,
        int sanDelta,
        int suppliesDelta,
        int suppliesCost,
        List<string> requiredFlags,
        List<string> addFlags,
        List<string> removeFlags,
        bool switchPhaseAfterChoice,
        GamePhase nextPhase)
    {
        return new ChoiceData
        {
            label = label,
            hpDelta = hpDelta,
            hungerDelta = hungerDelta,
            sanDelta = sanDelta,
            suppliesDelta = suppliesDelta,
            suppliesCost = suppliesCost,
            requiredFlags = requiredFlags ?? new List<string>(),
            addFlags = addFlags ?? new List<string>(),
            removeFlags = removeFlags ?? new List<string>(),
            switchPhaseAfterChoice = switchPhaseAfterChoice,
            nextPhase = nextPhase
        };
    }

    private static void CreateEventAsset(
        string id,
        GamePhase phase,
        int priority,
        string text,
        ChoiceData choiceA,
        ChoiceData choiceB,
        List<string> requiredFlags = null)
    {
        EventData eventData = ScriptableObject.CreateInstance<EventData>();
        eventData.eventId = id;
        eventData.phase = phase;
        eventData.priority = priority;
        eventData.eventText = text;
        eventData.requiredFlags = requiredFlags ?? new List<string>();
        eventData.consumeOnce = true;
        eventData.choiceA = choiceA;
        eventData.choiceB = choiceB;

        string path = AssetDatabase.GenerateUniqueAssetPath(EventFolder + "/" + id + ".asset");
        AssetDatabase.CreateAsset(eventData, path);
    }

    private static Canvas FindOrCreateCanvas()
    {
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            return canvas;
        }

        GameObject canvasObj = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        return canvas;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<EventSystem>() != null)
        {
            return;
        }

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    private static T FindOrCreateComponent<T>(string objectName) where T : Component
    {
        T existing = Object.FindObjectOfType<T>();
        if (existing != null)
        {
            return existing;
        }

        GameObject obj = new GameObject(objectName);
        return obj.AddComponent<T>();
    }

    private static Text CreateOrFindText(
        Transform parent,
        string name,
        Vector2 position,
        Vector2 size,
        TextAnchor anchor,
        int fontSize)
    {
        GameObject obj = FindOrCreateUIObject(parent, name);
        Text text = obj.GetComponent<Text>();
        if (text == null)
        {
            text = obj.AddComponent<Text>();
        }

        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.color = Color.black;
        text.alignment = anchor;
        text.fontSize = fontSize;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.text = name;

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = position;
        return text;
    }

    private static Button CreateOrFindButton(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject obj = FindOrCreateUIObject(parent, name);
        Image image = obj.GetComponent<Image>();
        if (image == null)
        {
            image = obj.AddComponent<Image>();
        }
        image.color = new Color(0.85f, 0.85f, 0.85f, 1f);

        Button button = obj.GetComponent<Button>();
        if (button == null)
        {
            button = obj.AddComponent<Button>();
        }

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.sizeDelta = size;
        rt.anchoredPosition = position;
        return button;
    }

    private static GameObject FindOrCreateUIObject(Transform parent, string name)
    {
        Transform child = parent.Find(name);
        if (child != null)
        {
            return child.gameObject;
        }

        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        return obj;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            return;
        }

        int splitIndex = path.LastIndexOf('/');
        string parent = path.Substring(0, splitIndex);
        string folderName = path.Substring(splitIndex + 1);
        if (!AssetDatabase.IsValidFolder(parent))
        {
            EnsureFolder(parent);
        }
        AssetDatabase.CreateFolder(parent, folderName);
    }
}
