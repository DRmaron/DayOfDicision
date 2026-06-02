using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public const string FlagScamVictim = "scamVictim";
    public const string FlagScamAvoided = "scamAvoided";
    public const string FlagRumorVictim = "rumorVictim";
    public const string FlagRumorAvoided = "rumorAvoided";

    public enum RunState
    {
        Playing = 0,
        GameOver = 1,
        Cleared = 2
    }

    [Header("Mode")]
    [SerializeField] private bool usePdfScenario = true;
    [SerializeField] private string startEventResourcesPath = "ScenarioPDF/Evac_01";
    [SerializeField] private bool autoResetOnStart = false;

    [Header("Initial Status")]
    [SerializeField] private int maxHp = 10;
    [SerializeField] private int maxHunger = 10;
    [SerializeField] private int maxSan = 10;
    [SerializeField] private int maxWater = 100;
    [SerializeField] private int maxHygiene = 100;
    [SerializeField] private int maxTrust = 100;
    [SerializeField] private int maxCoop = 100;
    [SerializeField] private int startHp = 8;
    [SerializeField] private int startHunger = 7;
    [SerializeField] private int startSan = 7;
    [SerializeField] private int startSupplies = 0;
    [SerializeField] private int startWater = 50;
    [SerializeField] private int startHygiene = 50;
    [SerializeField] private int startTrust = 50;
    [SerializeField] private int startCoop = 0;
    [SerializeField] private GamePhase startPhase = GamePhase.Evacuation;

    [Header("Event Master (prototype fallback)")]
    [SerializeField] private List<EventData> allEvents = new List<EventData>();
    [SerializeField] private int lifePhaseGoalEvents = 6;

    public EventData CurrentEvent { get; private set; }
    public GamePhase CurrentPhase { get; private set; }
    public RunState CurrentRunState { get; private set; }
    public bool PendingShelterSceneLoad { get; private set; }
    public bool PendingEvacSceneLoad { get; private set; }

    public int MaxHp => maxHp;
    public int MaxHunger => maxHunger;
    public int MaxSan => maxSan;
    public int MaxWater => maxWater;
    public int MaxHygiene => maxHygiene;
    public int MaxTrust => maxTrust;
    public int MaxCoop => maxCoop;

    public int Hp { get; private set; }
    public int Hunger { get; private set; }
    public int San { get; private set; }
    public int Supplies { get; private set; }
    public int Water { get; private set; }
    public int Hygiene { get; private set; }
    public int Trust { get; private set; }
    public int Coop { get; private set; }

    public string LastChoiceResultText { get; private set; }
    public int LastHpDelta { get; private set; }
    public int LastHungerDelta { get; private set; }
    public int LastSanDelta { get; private set; }
    public int LastSuppliesDelta { get; private set; }
    public int LastWaterDelta { get; private set; }
    public int LastHygieneDelta { get; private set; }
    public int LastTrustDelta { get; private set; }
    public int LastCoopDelta { get; private set; }

    public event Action<GamePhase> OnPhaseChanged;

    private readonly HashSet<string> flags = new HashSet<string>();
    private readonly HashSet<string> consumedEventIds = new HashSet<string>();
    private readonly List<string> choiceResultLogs = new List<string>();
    private int lifePhaseCompletedEvents;
    private bool lastChoiceWasA;
    private const int MaxChoiceResultLogs = 12;

    private void Start()
    {
        if (autoResetOnStart)
        {
            ResetRun();
        }
    }

    public void ClearPendingSceneFlags()
    {
        PendingShelterSceneLoad = false;
        PendingEvacSceneLoad = false;
    }

    public void ResetRun()
    {
        Hp = Mathf.Clamp(startHp, 0, maxHp);
        Hunger = Mathf.Clamp(startHunger, 0, maxHunger);
        San = Mathf.Clamp(startSan, 0, maxSan);
        Supplies = Mathf.Max(0, startSupplies);
        Water = Mathf.Clamp(startWater, 0, maxWater);
        Hygiene = Mathf.Clamp(startHygiene, 0, maxHygiene);
        Trust = Mathf.Clamp(startTrust, 0, maxTrust);
        Coop = Mathf.Clamp(startCoop, 0, maxCoop);
        CurrentPhase = startPhase;

        flags.Clear();
        consumedEventIds.Clear();
        choiceResultLogs.Clear();
        lifePhaseCompletedEvents = 0;
        CurrentRunState = RunState.Playing;
        PendingShelterSceneLoad = false;
        PendingEvacSceneLoad = false;
        ClearLastDeltas();
        LastChoiceResultText = string.Empty;

        CurrentEvent = PickStartEvent();
    }

    public void SelectChoiceA()
    {
        if (CurrentEvent == null || CurrentEvent.choiceA == null)
        {
            return;
        }

        lastChoiceWasA = true;
        ApplyChoice(CurrentEvent.choiceA);
    }

    public void SelectChoiceB()
    {
        if (CurrentEvent == null || CurrentEvent.choiceB == null)
        {
            return;
        }

        lastChoiceWasA = false;
        ApplyChoice(CurrentEvent.choiceB);
    }

    public string GetEventText(EventData eventData)
    {
        if (eventData == null)
        {
            return string.Empty;
        }

        if (eventData.eventId == "EVAC_RESULT")
        {
            return BuildEvacResultText();
        }

        if (eventData.eventId == "ENDING")
        {
            return BuildEndingText();
        }

        if (eventData.eventId == "SH_02")
        {
            return BuildShelter02Text(eventData.eventText);
        }

        string pdfText = PdfScenarioTextProvider.GetText(eventData.eventId);
        if (!string.IsNullOrWhiteSpace(pdfText))
        {
            return pdfText;
        }

        return eventData.eventText;
    }

    public string GetStatusText()
    {
        return "HP:" + Hp + "/" + maxHp
            + " 腹:" + Hunger + "/" + maxHunger
            + " SAN:" + San + "/" + maxSan
            + " 水:" + Water
            + " 衛生:" + Hygiene
            + " 信頼:" + Trust
            + " 共助:" + Coop
            + " 物資:" + Supplies;
    }

    public string GetChoiceALockReason()
    {
        return GetChoiceLockReason(CurrentEvent != null ? CurrentEvent.choiceA : null);
    }

    public string GetChoiceBLockReason()
    {
        return GetChoiceLockReason(CurrentEvent != null ? CurrentEvent.choiceB : null);
    }

    public string GetChoiceResultLogText(int maxLines)
    {
        if (choiceResultLogs.Count == 0 || maxLines <= 0)
        {
            return string.Empty;
        }

        int startIndex = Mathf.Max(0, choiceResultLogs.Count - maxLines);
        return string.Join("\n", choiceResultLogs.GetRange(startIndex, choiceResultLogs.Count - startIndex));
    }

    public bool HasFlag(string flag)
    {
        return !string.IsNullOrWhiteSpace(flag) && flags.Contains(flag);
    }

    private void ApplyChoice(ChoiceData choice)
    {
        if (CurrentRunState != RunState.Playing)
        {
            return;
        }

        string lockReason = GetChoiceLockReason(choice);
        if (!string.IsNullOrEmpty(lockReason))
        {
            Debug.Log("Choice blocked: " + lockReason);
            return;
        }

        EventData previousEvent = CurrentEvent;
        Supplies = Mathf.Max(0, Supplies - choice.suppliesCost);

        int beforeHp = Hp;
        int beforeHunger = Hunger;
        int beforeSan = San;
        int beforeSupplies = Supplies;
        int beforeWater = Water;
        int beforeHygiene = Hygiene;
        int beforeTrust = Trust;
        int beforeCoop = Coop;

        Hp = Mathf.Clamp(Hp + choice.hpDelta, 0, maxHp);
        Hunger = Mathf.Clamp(Hunger + choice.hungerDelta, 0, maxHunger);
        San = Mathf.Clamp(San + choice.sanDelta, 0, maxSan);
        Supplies = Mathf.Max(0, Supplies + choice.suppliesDelta);
        Water = Mathf.Clamp(Water + choice.waterDelta, 0, maxWater);
        Hygiene = Mathf.Clamp(Hygiene + choice.hygieneDelta, 0, maxHygiene);
        Trust = Mathf.Clamp(Trust + choice.trustDelta, 0, maxTrust);
        Coop = Mathf.Clamp(Coop + choice.coopDelta, 0, maxCoop);

        LastHpDelta = Hp - beforeHp;
        LastHungerDelta = Hunger - beforeHunger;
        LastSanDelta = San - beforeSan;
        LastSuppliesDelta = Supplies - beforeSupplies;
        LastWaterDelta = Water - beforeWater;
        LastHygieneDelta = Hygiene - beforeHygiene;
        LastTrustDelta = Trust - beforeTrust;
        LastCoopDelta = Coop - beforeCoop;

        ApplyFlagChanges(choice.addFlags, choice.removeFlags);

        GamePhase phaseBefore = CurrentPhase;
        if (choice.switchPhaseAfterChoice)
        {
            CurrentPhase = choice.nextPhase;
            if (phaseBefore != CurrentPhase)
            {
                OnPhaseChanged?.Invoke(CurrentPhase);
                if (CurrentPhase == GamePhase.PostEvacuation && previousEvent != null && previousEvent.eventId == "EVAC_RESULT")
                {
                    PendingShelterSceneLoad = true;
                }
            }
        }

        if (previousEvent != null && previousEvent.consumeOnce && !string.IsNullOrWhiteSpace(previousEvent.eventId))
        {
            consumedEventIds.Add(previousEvent.eventId);
        }

        if (CurrentPhase == GamePhase.PostEvacuation && !usePdfScenario)
        {
            lifePhaseCompletedEvents++;
        }

        LastChoiceResultText = BuildChoiceResultText(choice);
        PushChoiceResultLog(LastChoiceResultText);

        EvaluateRunState();

        if (choice.resetRunAfterChoice)
        {
            ResetRun();
            PendingEvacSceneLoad = true;
            return;
        }

        CurrentEvent = ResolveNextEvent(previousEvent, choice);
    }

    private EventData ResolveNextEvent(EventData previousEvent, ChoiceData choice)
    {
        if (CurrentRunState != RunState.Playing)
        {
            return null;
        }

        if (usePdfScenario && previousEvent != null)
        {
            EventData linked = lastChoiceWasA ? previousEvent.nextEventAfterChoiceA : previousEvent.nextEventAfterChoiceB;
            if (linked != null)
            {
                return linked;
            }
        }

        return PickNextEvent();
    }

    private EventData PickStartEvent()
    {
        if (usePdfScenario && !string.IsNullOrWhiteSpace(startEventResourcesPath))
        {
            PdfScenarioRuntimeLinker.EnsureLinked(this);
            PdfScenarioChoiceBootstrap.ApplyAll();
            EventData start = Resources.Load<EventData>(startEventResourcesPath);
            if (start != null)
            {
                return start;
            }

            Debug.LogError("GameManager: Could not load start event at Resources/" + startEventResourcesPath);
        }

        return PickNextEvent();
    }

    private string BuildChoiceResultText(ChoiceData choice)
    {
        if (!string.IsNullOrWhiteSpace(choice.resultText))
        {
            return choice.resultText.Trim();
        }

        return "「" + choice.label + "」を選んだ。";
    }

    private void PushChoiceResultLog(string logText)
    {
        if (string.IsNullOrWhiteSpace(logText))
        {
            return;
        }

        choiceResultLogs.Add(logText);
        if (choiceResultLogs.Count > MaxChoiceResultLogs)
        {
            choiceResultLogs.RemoveAt(0);
        }
    }

    private EventData PickNextEvent()
    {
        if (CurrentRunState != RunState.Playing)
        {
            return null;
        }

        List<EventData> candidates = new List<EventData>();

        foreach (EventData eventData in allEvents)
        {
            if (eventData == null)
            {
                continue;
            }

            if (eventData.phase != CurrentPhase)
            {
                continue;
            }

            if (!CanShowEvent(eventData))
            {
                continue;
            }

            candidates.Add(eventData);
        }

        if (candidates.Count == 0)
        {
            if (!usePdfScenario)
            {
                CurrentRunState = RunState.Cleared;
            }

            return null;
        }

        int highestPriority = int.MinValue;
        for (int i = 0; i < candidates.Count; i++)
        {
            if (candidates[i].priority > highestPriority)
            {
                highestPriority = candidates[i].priority;
            }
        }

        List<EventData> topPriority = new List<EventData>();
        for (int i = 0; i < candidates.Count; i++)
        {
            if (candidates[i].priority == highestPriority)
            {
                topPriority.Add(candidates[i]);
            }
        }

        return topPriority[UnityEngine.Random.Range(0, topPriority.Count)];
    }

    private bool CanShowEvent(EventData eventData)
    {
        if (eventData.consumeOnce && !string.IsNullOrWhiteSpace(eventData.eventId))
        {
            if (consumedEventIds.Contains(eventData.eventId))
            {
                return false;
            }
        }

        return HasAllFlags(eventData.requiredFlags);
    }

    private string GetChoiceLockReason(ChoiceData choice)
    {
        if (CurrentRunState != RunState.Playing)
        {
            return "ゲームは終了しています。";
        }

        if (choice == null)
        {
            return "選択肢データが未設定です。";
        }

        if (Supplies < choice.suppliesCost)
        {
            return "物資が足りません。必要: " + choice.suppliesCost;
        }

        if (!HasAllFlags(choice.requiredFlags))
        {
            return "条件フラグを満たしていません。";
        }

        return string.Empty;
    }

    private bool HasAllFlags(List<string> required)
    {
        if (required == null || required.Count == 0)
        {
            return true;
        }

        for (int i = 0; i < required.Count; i++)
        {
            string flag = required[i];
            if (flag == "__never__")
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(flag))
            {
                continue;
            }

            if (!flags.Contains(flag))
            {
                return false;
            }
        }

        return true;
    }

    private void ApplyFlagChanges(List<string> addList, List<string> removeList)
    {
        if (addList != null)
        {
            for (int i = 0; i < addList.Count; i++)
            {
                string flag = addList[i];
                if (!string.IsNullOrWhiteSpace(flag))
                {
                    flags.Add(flag);
                }
            }
        }

        if (removeList == null)
        {
            return;
        }

        for (int i = 0; i < removeList.Count; i++)
        {
            string flag = removeList[i];
            if (!string.IsNullOrWhiteSpace(flag))
            {
                flags.Remove(flag);
            }
        }
    }

    private void EvaluateRunState()
    {
        if (Hp <= 0 || Hunger <= 0 || San <= 0 || Water <= 0 || Hygiene <= 0)
        {
            CurrentRunState = RunState.GameOver;
            return;
        }

        if (!usePdfScenario && CurrentPhase == GamePhase.PostEvacuation && lifePhaseCompletedEvents >= lifePhaseGoalEvents)
        {
            CurrentRunState = RunState.Cleared;
        }
    }

    private void ClearLastDeltas()
    {
        LastHpDelta = 0;
        LastHungerDelta = 0;
        LastSanDelta = 0;
        LastSuppliesDelta = 0;
        LastWaterDelta = 0;
        LastHygieneDelta = 0;
        LastTrustDelta = 0;
        LastCoopDelta = 0;
    }

    private string BuildEvacResultText()
    {
        char grade = GetArrivalGrade();
        switch (grade)
        {
            case 'A':
                return "到着結果 A\n\n共助のおかげで、多くの人と協力しながら避難所にたどり着いた。\n体力は残っているが、少し疲れている。";
            case 'B':
                return "到着結果 B\n\nなんとか避難所に到着した。\n物資は少ないが、これから避難所生活が始まる。";
            default:
                return "到着結果 C\n\nぎりぎり避難所にたどり着いた。\n疲労と物資不足で、これからが不安だ。";
        }
    }

    private char GetArrivalGrade()
    {
        if (Coop >= 3 && Supplies >= 1 && Hp >= 5)
        {
            return 'A';
        }

        if (Coop >= 1 || Supplies >= 1)
        {
            return 'B';
        }

        return 'C';
    }

    private string BuildShelter02Text(string fallback)
    {
        bool helped = HasFlag("helpedObstacle") || HasFlag("helpedChild");
        if (helped)
        {
            return "イベント02「助けた人との再会」\n\n"
                + "避難中に助けた人が、感謝の言葉とともに水と食料を分けてくれた。";
        }

        return "イベント02「見知らぬ避難者」\n\n"
            + "知らない避難者が、少し距離を置いて様子を見ている。\n"
            + "まだ避難所内の人間関係は築けていない。";
    }

    private string BuildEndingText()
    {
        char grade = GetEndingGrade();
        switch (grade)
        {
            case 'A':
                return "エンディング A\n\n信頼と共助のおかげで、避難所は協力して乗り越えた。\n救助が到着し、地域は回復へ向かう。";
            case 'B':
                return "エンディング B\n\n大きな混乱は避けられたが、いくつかの課題は残った。\nそれでも人々は助け合い、救助を待つ。";
            case 'C':
                return "エンディング C\n\n避難所内の不信感が残った。\n救助は来るが、回復には時間がかかるだろう。";
            default:
                return "エンディング D\n\n詐欺やデマに振り回され、避難所は混乱した。\n教訓を胸に、次の支援を待つしかない。";
        }
    }

    private char GetEndingGrade()
    {
        if (HasFlag(FlagScamVictim) || HasFlag(FlagRumorVictim))
        {
            return 'D';
        }

        if (Trust >= 60 && Coop >= 4 && !HasFlag(FlagScamVictim))
        {
            return 'A';
        }

        if (Trust >= 40 || Coop >= 2)
        {
            return 'B';
        }

        return 'C';
    }
}
