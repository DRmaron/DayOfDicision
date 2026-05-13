using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum RunState
    {
        Playing = 0,
        GameOver = 1,
        Cleared = 2
    }

    [Header("Initial Status")]
    [SerializeField] private int maxHp = 10;
    [SerializeField] private int maxHunger = 10;
    [SerializeField] private int maxSan = 10;
    [SerializeField] private int startHp = 8;
    [SerializeField] private int startHunger = 7;
    [SerializeField] private int startSan = 7;
    [SerializeField] private int startSupplies = 0;
    [SerializeField] private GamePhase startPhase = GamePhase.Evacuation;

    [Header("Event Master")]
    [SerializeField] private List<EventData> allEvents = new List<EventData>();
    [SerializeField] private int lifePhaseGoalEvents = 6;

    public EventData CurrentEvent { get; private set; }
    public GamePhase CurrentPhase { get; private set; }
    public RunState CurrentRunState { get; private set; }

    public int Hp { get; private set; }
    public int Hunger { get; private set; }
    public int San { get; private set; }
    public int Supplies { get; private set; }
    public string LastChoiceResultText { get; private set; }
    public int LastHpDelta { get; private set; }
    public int LastHungerDelta { get; private set; }
    public int LastSanDelta { get; private set; }
    public int LastSuppliesDelta { get; private set; }

    private readonly HashSet<string> flags = new HashSet<string>();
    private readonly HashSet<string> consumedEventIds = new HashSet<string>();
    private readonly List<string> choiceResultLogs = new List<string>();
    private int lifePhaseCompletedEvents;
    private const int MaxChoiceResultLogs = 12;

    private void Start()
    {
        ResetRun();
    }

    public void ResetRun()
    {
        Hp = Mathf.Clamp(startHp, 0, maxHp);
        Hunger = Mathf.Clamp(startHunger, 0, maxHunger);
        San = Mathf.Clamp(startSan, 0, maxSan);
        Supplies = Mathf.Max(0, startSupplies);
        CurrentPhase = startPhase;

        flags.Clear();
        consumedEventIds.Clear();
        choiceResultLogs.Clear();
        lifePhaseCompletedEvents = 0;
        CurrentRunState = RunState.Playing;
        LastChoiceResultText = string.Empty;
        LastHpDelta = 0;
        LastHungerDelta = 0;
        LastSanDelta = 0;
        LastSuppliesDelta = 0;

        CurrentEvent = PickNextEvent();
    }

    public void SelectChoiceA()
    {
        if (CurrentEvent == null || CurrentEvent.choiceA == null)
        {
            return;
        }

        ApplyChoice(CurrentEvent.choiceA);
    }

    public void SelectChoiceB()
    {
        if (CurrentEvent == null || CurrentEvent.choiceB == null)
        {
            return;
        }

        ApplyChoice(CurrentEvent.choiceB);
    }

    public string GetStatusText()
    {
        return "HP: " + Hp + "/" + maxHp
            + "  腹: " + Hunger + "/" + maxHunger
            + "  SAN: " + San + "/" + maxSan
            + "  物資: " + Supplies
            + "  フェーズ: " + CurrentPhase
            + "  状態: " + CurrentRunState;
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

        Supplies = Mathf.Max(0, Supplies - choice.suppliesCost);

        int beforeHp = Hp;
        int beforeHunger = Hunger;
        int beforeSan = San;
        int beforeSupplies = Supplies;

        Hp = Mathf.Clamp(Hp + choice.hpDelta, 0, maxHp);
        Hunger = Mathf.Clamp(Hunger + choice.hungerDelta, 0, maxHunger);
        San = Mathf.Clamp(San + choice.sanDelta, 0, maxSan);
        Supplies = Mathf.Max(0, Supplies + choice.suppliesDelta);

        LastHpDelta = Hp - beforeHp;
        LastHungerDelta = Hunger - beforeHunger;
        LastSanDelta = San - beforeSan;
        LastSuppliesDelta = Supplies - beforeSupplies;

        ApplyFlagChanges(choice.addFlags, choice.removeFlags);

        if (choice.switchPhaseAfterChoice)
        {
            CurrentPhase = choice.nextPhase;
        }

        if (CurrentEvent.consumeOnce && !string.IsNullOrWhiteSpace(CurrentEvent.eventId))
        {
            consumedEventIds.Add(CurrentEvent.eventId);
        }

        if (CurrentPhase == GamePhase.PostEvacuation)
        {
            lifePhaseCompletedEvents++;
        }

        LastChoiceResultText = BuildChoiceResultText(choice);
        PushChoiceResultLog(LastChoiceResultText);

        EvaluateRunState();
        CurrentEvent = PickNextEvent();
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
            CurrentRunState = RunState.Cleared;
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

        return topPriority[Random.Range(0, topPriority.Count)];
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

    private bool CanUseChoice(ChoiceData choice)
    {
        return string.IsNullOrEmpty(GetChoiceLockReason(choice));
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
        if (Hp <= 0 || Hunger <= 0 || San <= 0)
        {
            CurrentRunState = RunState.GameOver;
            return;
        }

        if (CurrentPhase == GamePhase.PostEvacuation && lifePhaseCompletedEvents >= lifePhaseGoalEvents)
        {
            CurrentRunState = RunState.Cleared;
        }
    }
}
