using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Textsystem : MonoBehaviour
{
    private enum UiStep
    {
        EventAndChoices = 0,
        ChoiceResult = 1,
        ChoiceStatResult = 2
    }

    private const string DebugLogAbsolutePath = @"C:\Users\shunm\OneDrive\デスクトップ\Lecture\2026_前期\デジタルコンテンツ総合実習\DisaVenture\debug-9a3fa9.log";
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Text eventText;
    [SerializeField] private Text choiceAText;
    [SerializeField] private Text choiceBText;
    [SerializeField] private Text statusText;
    [SerializeField] private Text messageText;
    [SerializeField] private Button choiceAButton;
    [SerializeField] private Button choiceBButton;
    private UiStep currentUiStep = UiStep.EventAndChoices;

    private void Awake()
    {
        if (choiceAButton != null)
        {
            choiceAButton.onClick.AddListener(OnClickChoiceA);
        }

        if (choiceBButton != null)
        {
            choiceBButton.onClick.AddListener(OnClickChoiceB);
        }
    }

    private void Start()
    {
        AutoWireMissingReferences();

        // #region agent log
        WriteDebugLog("{\"sessionId\":\"9a3fa9\",\"runId\":\"pre-fix\",\"hypothesisId\":\"H1\",\"location\":\"Textsystem.cs:Start\",\"message\":\"Textsystem Start called\",\"data\":{\"hasGameManager\":" + (gameManager != null ? "true" : "false") + "},\"timestamp\":" + System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "}\n");
        // #endregion
        // #region agent log
        WriteDebugLog("{\"sessionId\":\"9a3fa9\",\"runId\":\"pre-fix\",\"hypothesisId\":\"H5\",\"location\":\"Textsystem.cs:Start\",\"message\":\"UI references state\",\"data\":{\"eventText\":" + (eventText != null ? "true" : "false") + ",\"choiceAText\":" + (choiceAText != null ? "true" : "false") + ",\"choiceBText\":" + (choiceBText != null ? "true" : "false") + ",\"statusText\":" + (statusText != null ? "true" : "false") + ",\"messageText\":" + (messageText != null ? "true" : "false") + ",\"choiceAButton\":" + (choiceAButton != null ? "true" : "false") + ",\"choiceBButton\":" + (choiceBButton != null ? "true" : "false") + "},\"timestamp\":" + System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "}\n");
        // #endregion

        if (gameManager == null)
        {
            // #region agent log
            WriteDebugLog("{\"sessionId\":\"9a3fa9\",\"runId\":\"pre-fix\",\"hypothesisId\":\"H1\",\"location\":\"Textsystem.cs:Start\",\"message\":\"GameManager missing in Start\",\"data\":{\"eventTextAssigned\":" + (eventText != null ? "true" : "false") + ",\"choiceAButtonAssigned\":" + (choiceAButton != null ? "true" : "false") + "},\"timestamp\":" + System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "}\n");
            // #endregion
            Debug.LogError("Textsystem: GameManager is not assigned.");
            return;
        }

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (gameManager == null)
        {
            // #region agent log
            WriteDebugLog("{\"sessionId\":\"9a3fa9\",\"runId\":\"pre-fix\",\"hypothesisId\":\"H1\",\"location\":\"Textsystem.cs:RefreshUI\",\"message\":\"RefreshUI skipped because GameManager is null\",\"data\":{},\"timestamp\":" + System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "}\n");
            // #endregion
            return;
        }

        if (currentUiStep == UiStep.ChoiceResult)
        {
            ShowChoiceResultStep();
            return;
        }

        if (currentUiStep == UiStep.ChoiceStatResult)
        {
            ShowChoiceStatResultStep();
            return;
        }

        EventData current = gameManager.CurrentEvent;
        if (current == null)
        {
            if (eventText != null) eventText.text = GetRunStateText();
            if (choiceAText != null) choiceAText.text = "-";
            if (choiceBText != null) choiceBText.text = "-";
            if (statusText != null) statusText.text = gameManager.GetStatusText();
            if (messageText != null) messageText.text = BuildMessageText(string.Empty);

            SetChoiceButtonsInteractable(false);
            return;
        }

        if (eventText != null) eventText.text = current.eventText;
        if (choiceAText != null) choiceAText.text = current.choiceA.label;
        if (choiceBText != null) choiceBText.text = current.choiceB.label;
        if (statusText != null) statusText.text = gameManager.GetStatusText();
        if (messageText != null) messageText.text = BuildMessageText(BuildChoiceHint());
        // #region agent log
        WriteDebugLog("{\"sessionId\":\"9a3fa9\",\"runId\":\"pre-fix\",\"hypothesisId\":\"H6\",\"location\":\"Textsystem.cs:RefreshUI\",\"message\":\"RefreshUI applied\",\"data\":{\"hasCurrentEvent\":" + (current != null ? "true" : "false") + ",\"choiceAHasLabel\":" + (current != null && current.choiceA != null && !string.IsNullOrEmpty(current.choiceA.label) ? "true" : "false") + ",\"choiceBHasLabel\":" + (current != null && current.choiceB != null && !string.IsNullOrEmpty(current.choiceB.label) ? "true" : "false") + "},\"timestamp\":" + System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "}\n");
        // #endregion

        bool canUseA = string.IsNullOrEmpty(gameManager.GetChoiceALockReason());
        bool canUseB = string.IsNullOrEmpty(gameManager.GetChoiceBLockReason());
        SetChoiceButtonsInteractable(canUseA, canUseB);
    }

    public void OnClickChoiceA()
    {
        if (gameManager == null)
        {
            return;
        }

        if (currentUiStep == UiStep.ChoiceResult)
        {
            currentUiStep = UiStep.ChoiceStatResult;
            RefreshUI();
            return;
        }

        if (currentUiStep == UiStep.ChoiceStatResult)
        {
            currentUiStep = UiStep.EventAndChoices;
            RefreshUI();
            return;
        }

        gameManager.SelectChoiceA();
        currentUiStep = UiStep.ChoiceResult;
        RefreshUI();
    }

    public void OnClickChoiceB()
    {
        if (gameManager == null)
        {
            return;
        }

        if (currentUiStep != UiStep.EventAndChoices)
        {
            return;
        }

        gameManager.SelectChoiceB();
        currentUiStep = UiStep.ChoiceResult;
        RefreshUI();
    }

    private void ShowChoiceResultStep()
    {
        if (eventText != null) eventText.text = gameManager.LastChoiceResultText;
        if (choiceAText != null) choiceAText.text = "次へ";
        if (choiceBText != null) choiceBText.text = "-";
        if (statusText != null) statusText.text = gameManager.GetStatusText();
        if (messageText != null) messageText.text = "選択の結果が発生しました。";
        SetChoiceButtonsInteractable(true, false);
    }

    private void ShowChoiceStatResultStep()
    {
        if (eventText != null) eventText.text = BuildStatResultText();
        if (choiceAText != null) choiceAText.text = "次へ";
        if (choiceBText != null) choiceBText.text = "-";
        if (statusText != null) statusText.text = gameManager.GetStatusText();
        if (messageText != null) messageText.text = "パラメータ変化を確認しました。";
        SetChoiceButtonsInteractable(true, false);
    }

    private string BuildStatResultText()
    {
        return "結果\n"
            + "HP: " + FormatSigned(gameManager.LastHpDelta)
            + " / 腹: " + FormatSigned(gameManager.LastHungerDelta)
            + " / SAN: " + FormatSigned(gameManager.LastSanDelta)
            + " / 物資: " + FormatSigned(gameManager.LastSuppliesDelta);
    }

    private string FormatSigned(int value)
    {
        return value >= 0 ? "+" + value : value.ToString();
    }

    private void SetChoiceButtonsInteractable(bool canInteract)
    {
        if (choiceAButton != null) choiceAButton.interactable = canInteract;
        if (choiceBButton != null) choiceBButton.interactable = canInteract;
    }

    private void SetChoiceButtonsInteractable(bool canUseA, bool canUseB)
    {
        if (choiceAButton != null) choiceAButton.interactable = canUseA;
        if (choiceBButton != null) choiceBButton.interactable = canUseB;
    }

    private string BuildChoiceHint()
    {
        string lockA = gameManager.GetChoiceALockReason();
        string lockB = gameManager.GetChoiceBLockReason();

        if (string.IsNullOrEmpty(lockA) && string.IsNullOrEmpty(lockB))
        {
            return string.Empty;
        }

        if (!string.IsNullOrEmpty(lockA) && !string.IsNullOrEmpty(lockB))
        {
            return "A/B選択不可: " + lockA;
        }

        if (!string.IsNullOrEmpty(lockA))
        {
            return "A選択不可: " + lockA;
        }

        return "B選択不可: " + lockB;
    }

    private string BuildMessageText(string choiceHint)
    {
        string logText = gameManager.GetChoiceResultLogText(3);

        if (string.IsNullOrEmpty(logText))
        {
            return choiceHint;
        }

        if (string.IsNullOrEmpty(choiceHint))
        {
            return logText;
        }

        return logText + "\n" + choiceHint;
    }

    private string GetRunStateText()
    {
        switch (gameManager.CurrentRunState)
        {
            case GameManager.RunState.GameOver:
                return "ゲームオーバー";
            case GameManager.RunState.Cleared:
                return "クリア: 生活が安定しました。";
            default:
                return "表示できるイベントがありません。";
        }
    }

    private void AutoWireMissingReferences()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        Canvas canvas = EnsureCanvas();
        if (eventText == null) eventText = EnsureText(canvas.transform, "EventText", new Vector2(0f, 90f), new Vector2(900f, 260f), TextAnchor.UpperLeft, 28);
        if (choiceAText == null) choiceAText = EnsureText(canvas.transform, "ChoiceAText", new Vector2(-220f, -150f), new Vector2(380f, 70f), TextAnchor.MiddleCenter, 24);
        if (choiceBText == null) choiceBText = EnsureText(canvas.transform, "ChoiceBText", new Vector2(220f, -150f), new Vector2(380f, 70f), TextAnchor.MiddleCenter, 24);
        if (statusText == null) statusText = EnsureText(canvas.transform, "StatusText", new Vector2(0f, 305f), new Vector2(1100f, 70f), TextAnchor.MiddleLeft, 22);
        if (messageText == null) messageText = EnsureText(canvas.transform, "MessageText", new Vector2(0f, -250f), new Vector2(1100f, 55f), TextAnchor.MiddleCenter, 22);
        if (choiceAButton == null) choiceAButton = EnsureButton(canvas.transform, "ChoiceAButton", new Vector2(-220f, -150f), new Vector2(400f, 90f));
        if (choiceBButton == null) choiceBButton = EnsureButton(canvas.transform, "ChoiceBButton", new Vector2(220f, -150f), new Vector2(400f, 90f));

        // #region agent log
        WriteDebugLog("{\"sessionId\":\"9a3fa9\",\"runId\":\"post-fix\",\"hypothesisId\":\"H7\",\"location\":\"Textsystem.cs:AutoWireMissingReferences\",\"message\":\"Auto-wire result\",\"data\":{\"gameManager\":" + (gameManager != null ? "true" : "false") + ",\"eventText\":" + (eventText != null ? "true" : "false") + ",\"choiceAText\":" + (choiceAText != null ? "true" : "false") + ",\"choiceBText\":" + (choiceBText != null ? "true" : "false") + ",\"statusText\":" + (statusText != null ? "true" : "false") + ",\"messageText\":" + (messageText != null ? "true" : "false") + ",\"choiceAButton\":" + (choiceAButton != null ? "true" : "false") + ",\"choiceBButton\":" + (choiceBButton != null ? "true" : "false") + "},\"timestamp\":" + System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + "}\n");
        // #endregion
    }

    private Text FindText(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        return obj != null ? obj.GetComponent<Text>() : null;
    }

    private Button FindButton(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        return obj != null ? obj.GetComponent<Button>() : null;
    }

    private Canvas EnsureCanvas()
    {
        Canvas canvas = FindObjectOfType<Canvas>();
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

    private Text EnsureText(Transform parent, string objectName, Vector2 pos, Vector2 size, TextAnchor anchor, int fontSize)
    {
        Text existing = FindText(objectName);
        if (existing != null)
        {
            return existing;
        }

        GameObject obj = new GameObject(objectName, typeof(RectTransform), typeof(Text));
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Text text = obj.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.color = Color.black;
        text.alignment = anchor;
        text.fontSize = fontSize;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.text = objectName;
        return text;
    }

    private Button EnsureButton(Transform parent, string objectName, Vector2 pos, Vector2 size)
    {
        Button existing = FindButton(objectName);
        if (existing != null)
        {
            return existing;
        }

        GameObject obj = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image img = obj.GetComponent<Image>();
        img.color = new Color(0.85f, 0.85f, 0.85f, 1f);
        return obj.GetComponent<Button>();
    }

    private void WriteDebugLog(string payload)
    {
        try
        {
            File.AppendAllText(DebugLogAbsolutePath, payload);
        }
        catch
        {
            try
            {
                string fallbackPath = Path.Combine(Directory.GetCurrentDirectory(), "debug-9a3fa9.log");
                File.AppendAllText(fallbackPath, payload);
            }
            catch
            {
                // Keep runtime stable even if logging fails.
            }
        }
    }
}
