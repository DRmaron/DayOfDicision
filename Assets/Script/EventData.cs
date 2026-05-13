using System.Collections.Generic;
using UnityEngine;

public enum GamePhase
{
    Evacuation = 0,
    PostEvacuation = 1
}

[System.Serializable]
public class ChoiceData
{
    [Header("Display")]
    public string label;
    [TextArea(2, 4)]
    public string resultText;

    [Header("Parameter Delta")]
    public int hpDelta;
    public int hungerDelta;
    public int sanDelta;
    public int suppliesDelta;

    [Header("Resource Cost / Reward")]
    public int suppliesCost;

    [Header("Flag Conditions")]
    public List<string> requiredFlags = new List<string>();
    public List<string> addFlags = new List<string>();
    public List<string> removeFlags = new List<string>();

    [Header("Optional Transition")]
    public bool switchPhaseAfterChoice;
    public GamePhase nextPhase;
}

[CreateAssetMenu(fileName = "EventData", menuName = "DisaVenture/Event Data")]
public class EventData : ScriptableObject
{
    [Header("Identity")]
    public string eventId;
    public GamePhase phase;
    public int priority;

    [Header("Main Text")]
    [TextArea(3, 8)]
    public string eventText;

    [Header("Optional Event Conditions")]
    public List<string> requiredFlags = new List<string>();
    public bool consumeOnce = true;

    [Header("Choices")]
    public ChoiceData choiceA;
    public ChoiceData choiceB;
}
