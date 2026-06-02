using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameUiPresenter : MonoBehaviour
{
    private enum UiStep
    {
        EventAndChoices = 0,
        ChoiceResult = 1,
        ChoiceStatResult = 2
    }

    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private StatBarView statBarView;
    [SerializeField] private SanityEffect sanityEffect;
    [SerializeField] private TMP_Text eventText;
    [SerializeField] private TMP_Text choiceAText;
    [SerializeField] private TMP_Text choiceBText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text waterText;
    [SerializeField] private TMP_Text suppliesText;
    [SerializeField] private Button choiceAButton;
    [SerializeField] private Button choiceBButton;
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip choiceClickClip;
    [SerializeField] private float choiceClickVolume = 1f;

    private UiStep currentUiStep = UiStep.EventAndChoices;
    private int lastHandledClickFrame = -1;
    private float lastHandledClickTime = -999f;
    private const float ClickDebounceSeconds = 0.08f;

    private void Awake()
    {
        ResolveReferences();
        BindChoiceButtons();
    }

    private void Start()
    {
        ResolveReferences();
        RefreshUI();
    }

    private void OnEnable()
    {
        if (gameManager != null)
        {
            RefreshUI();
        }
    }

    private void Update()
    {
        HandlePointerFallbackInput();
    }

    public void SetGameManager(GameManager manager)
    {
        gameManager = manager;
        if (statBarView != null)
        {
            statBarView.SetGameManager(manager);
        }

        RefreshUI();
    }

    public void RefreshUI()
    {
        ResolveReferences();

        if (gameManager == null)
        {
            return;
        }

        if (sanityEffect != null)
        {
            sanityEffect.SyncFromGameManager(gameManager);
        }

        if (statBarView != null)
        {
            statBarView.Refresh();
        }

        RefreshResourceTexts();

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
            if (eventText != null)
            {
                eventText.text = GetRunStateText();
            }

            if (choiceAText != null) choiceAText.text = "-";
            if (choiceBText != null) choiceBText.text = "-";
            if (statusText != null) statusText.text = gameManager.GetStatusText();
            if (messageText != null) messageText.text = string.Empty;
            SetChoiceButtonsInteractable(false);
            return;
        }

        if (eventText != null)
        {
            eventText.text = gameManager.GetEventText(current);
        }

        if (choiceAText != null && current.choiceA != null)
        {
            choiceAText.text = current.choiceA.label;
        }

        if (choiceBText != null && current.choiceB != null)
        {
            choiceBText.text = current.choiceB.label;
        }

        if (statusText != null)
        {
            statusText.text = gameManager.GetStatusText();
        }

        if (messageText != null)
        {
            messageText.text = BuildMessageText(BuildChoiceHint());
        }

        bool canUseA = string.IsNullOrEmpty(gameManager.GetChoiceALockReason());
        bool canUseB = string.IsNullOrEmpty(gameManager.GetChoiceBLockReason());
        SetChoiceButtonsInteractable(canUseA, canUseB);
    }

    public void OnClickChoiceA()
    {
        HandleChoiceClick(true);
    }

    public void OnClickChoiceB()
    {
        HandleChoiceClick(false);
    }

    public void HandleChoiceClick(bool isChoiceA)
    {
        if (lastHandledClickFrame == Time.frameCount)
        {
            return;
        }

        if (Time.unscaledTime - lastHandledClickTime < ClickDebounceSeconds)
        {
            return;
        }

        lastHandledClickFrame = Time.frameCount;
        lastHandledClickTime = Time.unscaledTime;

        if (gameManager == null)
        {
            return;
        }

        PlayChoiceClickSe();

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

        if (isChoiceA)
        {
            gameManager.SelectChoiceA();
        }
        else
        {
            gameManager.SelectChoiceB();
        }

        currentUiStep = UiStep.ChoiceResult;
        RefreshUI();
    }

    public void ResetUiStep()
    {
        currentUiStep = UiStep.EventAndChoices;
    }

    private void ShowChoiceResultStep()
    {
        if (eventText != null)
        {
            eventText.text = gameManager.LastChoiceResultText;
        }

        if (choiceAText != null) choiceAText.text = "次へ";
        if (choiceBText != null) choiceBText.text = "-";
        if (statusText != null) statusText.text = gameManager.GetStatusText();
        if (messageText != null) messageText.text = "選択の結果が発生しました。";
        SetChoiceButtonsInteractable(true, false);
    }

    private void ShowChoiceStatResultStep()
    {
        if (eventText != null)
        {
            eventText.text = BuildStatResultText();
        }

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
            + " / 物資: " + FormatSigned(gameManager.LastSuppliesDelta)
            + "\n水: " + FormatSigned(gameManager.LastWaterDelta)
            + " / 衛生: " + FormatSigned(gameManager.LastHygieneDelta);
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
                return "ゲームオーバー\nHP・腹減り・SAN・水分・衛生のいずれかが0になりました。";
            case GameManager.RunState.Cleared:
                return "クリア: 生活が安定しました。";
            default:
                return "表示できるイベントがありません。";
        }
    }

    private void ResolveReferences()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        if (statBarView == null)
        {
            statBarView = GetComponent<StatBarView>();
        }

        if (sanityEffect == null)
        {
            sanityEffect = FindObjectOfType<SanityEffect>();
        }

        if (uiAudioSource == null)
        {
            GameObject seManager = GameObject.Find("SEManager");
            if (seManager != null)
            {
                uiAudioSource = seManager.GetComponent<AudioSource>();
            }
        }

        if (eventText == null)
        {
            eventText = FindScenarioText();
        }

        ResolveResourceTexts();

        if (choiceAButton == null)
        {
            GameObject buttonObj = GameObject.Find("senntakushi_A");
            if (buttonObj != null)
            {
                choiceAButton = buttonObj.GetComponent<Button>();
            }
        }

        if (choiceBButton == null)
        {
            GameObject buttonObj = GameObject.Find("senntakushi_B");
            if (buttonObj != null)
            {
                choiceBButton = buttonObj.GetComponent<Button>();
            }
        }

        if (choiceAText == null && choiceAButton != null)
        {
            choiceAText = choiceAButton.GetComponentInChildren<TMP_Text>(true);
        }

        if (choiceBText == null && choiceBButton != null)
        {
            choiceBText = choiceBButton.GetComponentInChildren<TMP_Text>(true);
        }

        BindChoiceButtons();
        BindChoiceForwarders();
        HidePrototypeLabels();
        HideUnusedPrototypeObjects();
    }

    private void BindChoiceButtons()
    {
        if (choiceAButton != null)
        {
            choiceAButton.onClick.RemoveListener(OnClickChoiceA);
            choiceAButton.onClick.AddListener(OnClickChoiceA);
        }

        if (choiceBButton != null)
        {
            choiceBButton.onClick.RemoveListener(OnClickChoiceB);
            choiceBButton.onClick.AddListener(OnClickChoiceB);
        }
    }

    private void BindChoiceForwarders()
    {
        BindChoiceForwarder(choiceAButton, true);
        BindChoiceForwarder(choiceBButton, false);
    }

    private void BindChoiceForwarder(Button button, bool isChoiceA)
    {
        if (button == null)
        {
            return;
        }

        ChoiceButtonForwarder forwarder = button.GetComponent<ChoiceButtonForwarder>();
        if (forwarder == null)
        {
            forwarder = button.gameObject.AddComponent<ChoiceButtonForwarder>();
        }

        forwarder.Configure(this, isChoiceA);
    }

    private void HidePrototypeLabels()
    {
        TMP_Text[] texts = FindObjectsOfType<TMP_Text>(true);
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text text = texts[i];
            if (text == null || IsControlledText(text))
            {
                continue;
            }

            if (IsPrototypeLabel(text.text))
            {
                text.gameObject.SetActive(false);
            }
        }
    }

    private bool IsControlledText(TMP_Text text)
    {
        if (text == eventText || text == choiceAText || text == choiceBText || text == statusText || text == messageText)
        {
            return true;
        }

        return IsChildOf(text.transform, choiceAButton != null ? choiceAButton.transform : null)
            || IsChildOf(text.transform, choiceBButton != null ? choiceBButton.transform : null);
    }

    private static bool IsChildOf(Transform child, Transform parent)
    {
        return child != null && parent != null && child.IsChildOf(parent);
    }

    private static bool IsPrototypeLabel(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        string normalized = text.Replace("\r", "\n").Replace("\n", " ").Trim().Trim('\'').Trim();
        return normalized.Contains("ta-nn")
            || normalized.Contains("seigenzikan")
            || normalized.Contains("seigenjikan")
            || normalized.Contains("seigennjikann")
            || normalized.Contains("seigennzikann")
            || normalized.Contains("senntakusi")
            || normalized.Contains("sentakushi")
            || normalized.Contains("senntakushi");
    }

    private void PlayChoiceClickSe()
    {
        if (uiAudioSource == null || choiceClickClip == null)
        {
            return;
        }

        uiAudioSource.PlayOneShot(choiceClickClip, Mathf.Clamp01(choiceClickVolume));
    }

    private void HandlePointerFallbackInput()
    {
        if (choiceAButton == null && choiceBButton == null)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandlePointerFallbackAt(Input.mousePosition);
        }

        if (Input.touchCount <= 0)
        {
            return;
        }

        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            HandlePointerFallbackAt(touch.position);
        }
    }

    private void HandlePointerFallbackAt(Vector2 screenPosition)
    {
        if (lastHandledClickFrame == Time.frameCount)
        {
            return;
        }

        if (CanUseButton(choiceAButton) && IsScreenPointInside(choiceAButton, screenPosition))
        {
            HandleChoiceClick(true);
            return;
        }

        if (CanUseButton(choiceBButton) && IsScreenPointInside(choiceBButton, screenPosition))
        {
            HandleChoiceClick(false);
        }
    }

    private static bool CanUseButton(Button button)
    {
        return button != null && button.gameObject.activeInHierarchy && button.interactable;
    }

    private static bool IsScreenPointInside(Button button, Vector2 screenPosition)
    {
        RectTransform rectTransform = button != null ? button.transform as RectTransform : null;
        if (rectTransform == null)
        {
            return false;
        }

        Canvas canvas = button.GetComponentInParent<Canvas>();
        Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        return RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPosition, uiCamera);
    }

    private void ResolveResourceTexts()
    {
        if (waterText != null || suppliesText != null)
        {
            return;
        }

        TMP_Text[] labels = FindTextsContaining("zannsuu");
        if (labels.Length == 0)
        {
            return;
        }

        SortByAnchoredX(labels);
        suppliesText = labels[0];
        waterText = labels.Length > 1 ? labels[1] : labels[0];
    }

    private void RefreshResourceTexts()
    {
        if (gameManager == null)
        {
            return;
        }

        if (waterText != null && waterText == suppliesText)
        {
            waterText.text = "水:" + gameManager.Water + " 物資:" + gameManager.Supplies;
            return;
        }

        if (waterText != null)
        {
            waterText.text = "水:" + gameManager.Water;
        }

        if (suppliesText != null)
        {
            suppliesText.text = "物資:" + gameManager.Supplies;
        }
    }

    private static void HideUnusedPrototypeObjects()
    {
        HideObjectByName("limit");
    }

    private static void HideObjectByName(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj != null)
        {
            obj.SetActive(false);
        }
    }

    private void OnDestroy()
    {
    }

    private static TMP_Text FindTmpUnder(string rootName)
    {
        GameObject root = GameObject.Find(rootName);
        if (root == null)
        {
            return null;
        }

        return root.GetComponentInChildren<TMP_Text>(true);
    }

    private static TMP_Text FindScenarioText()
    {
        TMP_Text[] texts = FindObjectsOfType<TMP_Text>(true);
        TMP_Text best = null;
        float bestArea = 0f;

        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text text = texts[i];
            if (text == null)
            {
                continue;
            }

            string value = text.text ?? string.Empty;
            if (!value.Contains("joukyou"))
            {
                continue;
            }

            RectTransform rect = text.rectTransform;
            float area = rect != null ? rect.rect.width * rect.rect.height : 0f;
            if (best == null || area > bestArea)
            {
                best = text;
                bestArea = area;
            }
        }

        return best != null ? best : FindTmpUnder("joukyou");
    }

    private static TMP_Text[] FindTextsContaining(string value)
    {
        TMP_Text[] texts = FindObjectsOfType<TMP_Text>(true);
        System.Collections.Generic.List<TMP_Text> matches = new System.Collections.Generic.List<TMP_Text>();
        for (int i = 0; i < texts.Length; i++)
        {
            TMP_Text text = texts[i];
            if (text != null && (text.text ?? string.Empty).Contains(value))
            {
                matches.Add(text);
            }
        }

        return matches.ToArray();
    }

    private static void SortByAnchoredX(TMP_Text[] texts)
    {
        System.Array.Sort(texts, (a, b) =>
        {
            float ax = a != null && a.rectTransform != null ? a.rectTransform.anchoredPosition.x : 0f;
            float bx = b != null && b.rectTransform != null ? b.rectTransform.anchoredPosition.x : 0f;
            return ax.CompareTo(bx);
        });
    }
}

public class ChoiceButtonForwarder : MonoBehaviour, IPointerClickHandler
{
    private GameUiPresenter presenter;
    private bool isChoiceA;

    public void Configure(GameUiPresenter newPresenter, bool newIsChoiceA)
    {
        presenter = newPresenter;
        isChoiceA = newIsChoiceA;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (presenter != null)
        {
            presenter.HandleChoiceClick(isChoiceA);
        }
    }
}
