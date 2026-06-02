using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Applies PDF choice labels/effects to loaded ScriptableObjects (used when assets are minimal stubs).
/// </summary>
public static class PdfScenarioChoiceBootstrap
{
    public static void ApplyAll()
    {
        ApplyEvac();
        ApplyShelter();
        ApplyEnding();
    }

    private static void ApplyEvac()
    {
        EventData e01 = Load("Evac_01");
        SetBoth(e01, "Evac_02");
        Choice(e01, true, "A：水と食料を優先する", hp: -1, hunger: 2, supplies: 1, water: 20);
        Choice(e01, false, "B：救急用品とライトを優先する", san: 1, hygiene: 10, coop: 1);

        EventData e02 = Load("Evac_02");
        SetBoth(e02, "Evac_03");
        Choice(e02, true, "A：一緒に障害物をどかす", hp: -1, san: 1, coop: 1, add: "helpedObstacle");
        Choice(e02, false, "B：別の道を探して先に進む", san: -1);

        EventData e03 = Load("Evac_03");
        SetBoth(e03, "Evac_04");
        Choice(e03, true, "A：短時間だけ物資を探す", hp: -1, supplies: 2, water: 10, hygiene: 5);
        Choice(e03, false, "B：危険なので入らず避難を優先する");

        EventData e04 = Load("Evac_04");
        SetBoth(e04, "Evac_05");
        Choice(e04, true, "A：一緒に避難所まで連れていく", hp: -1, san: 2, water: -5, coop: 2, add: "helpedChild");
        Choice(e04, false, "B：方向だけ教えて先に進む", san: -1);

        EventData e05 = Load("Evac_05");
        SetBoth(e05, "Evac_06");
        Choice(e05, true, "A：水を分ける", san: 1, water: -10, coop: 1, add: "sharedWater");
        Choice(e05, false, "B：自分の分を残す", san: -1);

        EventData e06 = Load("Evac_06");
        SetBoth(e06, "Evac_Result");
        Choice(e06, true, "A：誰かの荷物を少し持つ", hp: -1, san: 1, coop: 1);
        Choice(e06, false, "B：自分のペースで避難所に向かう");

        EventData result = Load("Evac_Result");
        if (result != null)
        {
            result.nextEventAfterChoiceA = Load("Shelter_01");
            Choice(result, true, "避難所へ入る", switchPhase: true, nextPhase: GamePhase.PostEvacuation);
            Choice(result, false, "-", reqNever: true);
        }
    }

    private static void ApplyShelter()
    {
        EventData sh01 = Load("Shelter_01");
        SetBoth(sh01, "Shelter_02");
        Choice(sh01, true, "A：受付の手伝いを申し出る", hp: -1, san: 1, trust: 10, coop: 1);
        Choice(sh01, false, "B：まず自分の場所と物資を確保する", hp: 1, water: 5);

        SetBoth(Load("Shelter_02"), "Shelter_03");
        Choice(Load("Shelter_02"), true, "A：ありがたく受け取る", water: 10, hunger: 1, san: 1, trust: 5);
        Choice(Load("Shelter_02"), false, "B：他に困っている人へ渡してもらう", san: 2, trust: 15, coop: 1);

        SetBoth(Load("Shelter_03"), "Shelter_04");
        Choice(Load("Shelter_03"), true, "A：避難所の仕事を手伝う", hp: -1, san: 1, trust: 10, coop: 1);
        Choice(Load("Shelter_03"), false, "B：明日に備えて休む", hp: 1, san: 1);

        SetBoth(Load("Shelter_04"), "Shelter_05");
        Choice(Load("Shelter_04"), true, "A：リンクを開いて申請する", san: -2, add: GameManager.FlagScamVictim);
        Choice(Load("Shelter_04"), false, "B：避難所スタッフに確認する", san: 1, trust: 10, add: GameManager.FlagScamAvoided);

        SetBoth(Load("Shelter_05"), "Shelter_06");
        Choice(Load("Shelter_05"), true, "A：自分の分として受け取る", water: 20, hp: 1, san: -1);
        Choice(Load("Shelter_05"), false, "B：後ろの親子に一部を譲る", water: 5, san: 2, trust: 15, coop: 1);

        SetBoth(Load("Shelter_06"), "Shelter_07");
        Choice(Load("Shelter_06"), true, "A：自分の衛生用品を温存する", hygiene: 10);
        Choice(Load("Shelter_06"), false, "B：衛生管理のために一部提供する", hygiene: -5, trust: 10, coop: 1);

        SetBoth(Load("Shelter_07"), "Shelter_08");
        Choice(Load("Shelter_07"), true, "A：家が心配なので記入する", san: -2, add: GameManager.FlagScamVictim);
        Choice(Load("Shelter_07"), false, "B：避難所スタッフに確認する", trust: 10, coop: 1, add: GameManager.FlagScamAvoided);

        SetBoth(Load("Shelter_08"), "Shelter_09");
        Choice(Load("Shelter_08"), true, "A：投稿を信じて移動を考える", hp: -1, water: -10, san: -1, add: GameManager.FlagRumorVictim);
        Choice(Load("Shelter_08"), false, "B：公式情報や掲示板で確認する", san: 1, trust: 5, add: GameManager.FlagRumorAvoided);

        SetBoth(Load("Shelter_09"), "Shelter_10");
        Choice(Load("Shelter_09"), true, "A：自分の分を受け取って離れる", hunger: 2, hp: 1, trust: -5, san: -1);
        Choice(Load("Shelter_09"), false, "B：配布の整理を手伝う", hp: -1, hunger: 1, trust: 15, coop: 1, san: 1);

        SetBoth(Load("Shelter_10"), "Shelter_11");
        Choice(Load("Shelter_10"), true, "A：声をかけて職員を呼びに行く", hp: -1, san: 2, trust: 15, coop: 1);
        Choice(Load("Shelter_10"), false, "B：自分も限界なので休む", hp: 1, san: -1);

        EventData sh11 = Load("Shelter_11");
        if (sh11 != null)
        {
            sh11.nextEventAfterChoiceA = Load("Ending");
            sh11.nextEventAfterChoiceB = Load("Ending");
            Choice(sh11, true, "A：最後の見回りを手伝う", hp: -2, trust: 20, san: 2, coop: 1);
            Choice(sh11, false, "B：救助に備えて休む", hp: 2, san: 1);
        }
    }

    private static void ApplyEnding()
    {
        EventData ending = Load("Ending");
        if (ending == null)
        {
            return;
        }

        Choice(ending, true, "リスタート", resetRun: true);
        Choice(ending, false, "-", reqNever: true);
    }

    private static EventData Load(string name)
    {
        return Resources.Load<EventData>("ScenarioPDF/" + name);
    }

    private static void SetBoth(EventData from, string nextName)
    {
        if (from == null)
        {
            return;
        }

        EventData next = Load(nextName);
        from.nextEventAfterChoiceA = next;
        from.nextEventAfterChoiceB = next;
    }

    private static void Choice(
        EventData data,
        bool isA,
        string label,
        int hp = 0,
        int hunger = 0,
        int san = 0,
        int supplies = 0,
        int water = 0,
        int hygiene = 0,
        int trust = 0,
        int coop = 0,
        string add = null,
        bool switchPhase = false,
        GamePhase nextPhase = GamePhase.Evacuation,
        bool resetRun = false,
        bool reqNever = false)
    {
        if (data == null)
        {
            return;
        }

        ChoiceData choice = isA ? data.choiceA : data.choiceB;
        if (choice == null)
        {
            choice = new ChoiceData();
            if (isA) data.choiceA = choice;
            else data.choiceB = choice;
        }

        choice.label = label;
        choice.hpDelta = hp;
        choice.hungerDelta = hunger;
        choice.sanDelta = san;
        choice.suppliesDelta = supplies;
        choice.waterDelta = water;
        choice.hygieneDelta = hygiene;
        choice.trustDelta = trust;
        choice.coopDelta = coop;
        choice.switchPhaseAfterChoice = switchPhase;
        choice.nextPhase = nextPhase;
        choice.resetRunAfterChoice = resetRun;
        choice.requiredFlags = reqNever ? new List<string> { "__never__" } : new List<string>();
        choice.addFlags = string.IsNullOrEmpty(add) ? new List<string>() : new List<string> { add };
    }
}
