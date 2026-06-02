using TMPro;
using UnityEngine;
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
    [SerializeField] private Button choiceAButton;
    [SerializeField] private Button choiceBButton;

    private UiStep currentUiStep = UiStep.EventAndChoices;

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
            + " / 衛生: " + FormatSigned(gameManager.LastHygieneDelta)
            + " / 信頼: " + FormatSigned(gameManager.LastTrustDelta)
            + " / 共助: " + FormatSigned(gameManager.LastCoopDelta);
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

        if (eventText == null)
        {
            eventText = FindTmpUnder("joukyou");
        }

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

    private void OnDestroy()
    {
        if (choiceAButton != null)
        {
            choiceAButton.onClick.RemoveListener(OnClickChoiceA);
        }

        if (choiceBButton != null)
        {
            choiceBButton.onClick.RemoveListener(OnClickChoiceB);
        }
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
}
