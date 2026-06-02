using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class PdfScenarioGenerator
{
    private const string RootFolder = "Assets/Resources/ScenarioPDF";

    [MenuItem("Tools/DisaVenture/PDF Scenario/Generate or Update Assets")]
    public static void GenerateOrUpdateAssets()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder(RootFolder);

        // Evacuation chain
        EventData evac01 = CreateOrUpdateEvent("Evac_01", "EVAC_01", GamePhase.Evacuation, BuildEvac01(), out ChoiceData evac01A, out ChoiceData evac01B);
        EventData evac02 = CreateOrUpdateEvent("Evac_02", "EVAC_02", GamePhase.Evacuation, BuildEvac02(), out ChoiceData evac02A, out ChoiceData evac02B);
        EventData evac03 = CreateOrUpdateEvent("Evac_03", "EVAC_03", GamePhase.Evacuation, BuildEvac03(), out ChoiceData evac03A, out ChoiceData evac03B);
        EventData evac04 = CreateOrUpdateEvent("Evac_04", "EVAC_04", GamePhase.Evacuation, BuildEvac04(), out ChoiceData evac04A, out ChoiceData evac04B);
        EventData evac05 = CreateOrUpdateEvent("Evac_05", "EVAC_05", GamePhase.Evacuation, BuildEvac05(), out ChoiceData evac05A, out ChoiceData evac05B);
        EventData evac06 = CreateOrUpdateEvent("Evac_06", "EVAC_06", GamePhase.Evacuation, BuildEvac06(), out ChoiceData evac06A, out ChoiceData evac06B);
        EventData evacResult = CreateOrUpdateEvent("Evac_Result", "EVAC_RESULT", GamePhase.Evacuation, "避難所に到着した。", out ChoiceData evacResA, out ChoiceData evacResB);

        // Shelter chain
        EventData sh01 = CreateOrUpdateEvent("Shelter_01", "SH_01", GamePhase.PostEvacuation, BuildSh01(), out ChoiceData sh01A, out ChoiceData sh01B);
        EventData sh02 = CreateOrUpdateEvent("Shelter_02", "SH_02", GamePhase.PostEvacuation, BuildSh02(), out ChoiceData sh02A, out ChoiceData sh02B);
        EventData sh03 = CreateOrUpdateEvent("Shelter_03", "SH_03", GamePhase.PostEvacuation, BuildSh03(), out ChoiceData sh03A, out ChoiceData sh03B);
        EventData sh04 = CreateOrUpdateEvent("Shelter_04", "SH_04", GamePhase.PostEvacuation, BuildSh04(), out ChoiceData sh04A, out ChoiceData sh04B);
        EventData sh05 = CreateOrUpdateEvent("Shelter_05", "SH_05", GamePhase.PostEvacuation, BuildSh05(), out ChoiceData sh05A, out ChoiceData sh05B);
        EventData sh06 = CreateOrUpdateEvent("Shelter_06", "SH_06", GamePhase.PostEvacuation, BuildSh06(), out ChoiceData sh06A, out ChoiceData sh06B);
        EventData sh07 = CreateOrUpdateEvent("Shelter_07", "SH_07", GamePhase.PostEvacuation, BuildSh07(), out ChoiceData sh07A, out ChoiceData sh07B);
        EventData sh08 = CreateOrUpdateEvent("Shelter_08", "SH_08", GamePhase.PostEvacuation, BuildSh08(), out ChoiceData sh08A, out ChoiceData sh08B);
        EventData sh09 = CreateOrUpdateEvent("Shelter_09", "SH_09", GamePhase.PostEvacuation, BuildSh09(), out ChoiceData sh09A, out ChoiceData sh09B);
        EventData sh10 = CreateOrUpdateEvent("Shelter_10", "SH_10", GamePhase.PostEvacuation, BuildSh10(), out ChoiceData sh10A, out ChoiceData sh10B);
        EventData sh11 = CreateOrUpdateEvent("Shelter_11", "SH_11", GamePhase.PostEvacuation, BuildSh11(), out ChoiceData sh11A, out ChoiceData sh11B);
        EventData ending = CreateOrUpdateEvent("Ending", "ENDING", GamePhase.PostEvacuation, "救助が到着した。", out ChoiceData endingA, out ChoiceData endingB);

        // --- Choice definitions (effects) ---
        // Evac 01
        evac01A.label = "A：水と食料を優先する";
        evac01A.waterDelta = +20;
        evac01A.hungerDelta = +2;
        evac01A.hpDelta = -1;
        evac01A.suppliesDelta = +1;
        evac01B.label = "B：救急用品とライトを優先する";
        evac01B.hygieneDelta = +10;
        evac01B.sanDelta = +1;
        evac01B.coopDelta = +1;

        // Evac 02
        evac02A.label = "A：一緒に障害物をどかす";
        evac02A.hpDelta = -1;
        evac02A.sanDelta = +1;
        evac02A.coopDelta = +1;
        evac02A.addFlags = new List<string> { "helpedObstacle" };
        evac02B.label = "B：別の道を探して先に進む";
        evac02B.sanDelta = -1;

        // Evac 03
        evac03A.label = "A：短時間だけ物資を探す";
        evac03A.hpDelta = -1;
        evac03A.suppliesDelta = +2;
        evac03A.waterDelta = +10;
        evac03A.hygieneDelta = +5;
        evac03B.label = "B：危険なので入らず避難を優先する";
        evac03B.sanDelta = +0;

        // Evac 04
        evac04A.label = "A：一緒に避難所まで連れていく";
        evac04A.hpDelta = -1;
        evac04A.waterDelta = -5;
        evac04A.sanDelta = +2;
        evac04A.coopDelta = +2;
        evac04A.addFlags = new List<string> { "helpedChild" };
        evac04B.label = "B：方向だけ教えて先に進む";
        evac04B.sanDelta = -1;

        // Evac 05
        evac05A.label = "A：水を分ける";
        evac05A.waterDelta = -10;
        evac05A.sanDelta = +1;
        evac05A.coopDelta = +1;
        evac05A.addFlags = new List<string> { "sharedWater" };
        evac05B.label = "B：自分の分を残す";
        evac05B.sanDelta = -1;

        // Evac 06
        evac06A.label = "A：誰かの荷物を少し持つ";
        evac06A.hpDelta = -1;
        evac06A.sanDelta = +1;
        evac06A.coopDelta = +1;
        evac06B.label = "B：自分のペースで避難所に向かう";
        evac06B.sanDelta = 0;

        // Shelter 01
        sh01A.label = "A：受付の手伝いを申し出る";
        sh01A.hpDelta = -1;
        sh01A.sanDelta = +1;
        sh01A.trustDelta = +10;
        sh01A.coopDelta = +1;
        sh01B.label = "B：まず自分の場所と物資を確保する";
        sh01B.hpDelta = +1;
        sh01B.waterDelta = +5;

        // Shelter 02 (conditional flavor via flags, effects are generic)
        sh02A.label = "A：ありがたく受け取る";
        sh02A.waterDelta = +10;
        sh02A.hungerDelta = +1;
        sh02A.sanDelta = +1;
        sh02A.trustDelta = +5;
        sh02B.label = "B：他に困っている人へ渡してもらう";
        sh02B.sanDelta = +2;
        sh02B.trustDelta = +15;
        sh02B.coopDelta = +1;

        // Shelter 03
        sh03A.label = "A：避難所の仕事を手伝う";
        sh03A.hpDelta = -1;
        sh03A.sanDelta = +1;
        sh03A.trustDelta = +10;
        sh03A.coopDelta = +1;
        sh03B.label = "B：明日に備えて休む";
        sh03B.hpDelta = +1;
        sh03B.sanDelta = +1;

        // Shelter 04 (scam mail)
        sh04A.label = "A：リンクを開いて申請する";
        sh04A.sanDelta = -2;
        sh04A.addFlags = new List<string> { GameManager.FlagScamVictim };
        sh04B.label = "B：避難所スタッフに確認する";
        sh04B.sanDelta = +1;
        sh04B.trustDelta = +10;
        sh04B.addFlags = new List<string> { GameManager.FlagScamAvoided };

        // Shelter 05 (water distribution)
        sh05A.label = "A：自分の分として受け取る";
        sh05A.waterDelta = +20;
        sh05A.hpDelta = +1;
        sh05A.sanDelta = -1;
        sh05B.label = "B：後ろの親子に一部を譲る";
        sh05B.waterDelta = +5;
        sh05B.sanDelta = +2;
        sh05B.trustDelta = +15;
        sh05B.coopDelta = +1;

        // Shelter 06 (toilet hygiene)
        sh06A.label = "A：自分の衛生用品を温存する";
        sh06A.hygieneDelta = +10;
        sh06B.label = "B：衛生管理のために一部提供する";
        sh06B.hygieneDelta = -5;
        sh06B.trustDelta = +10;
        sh06B.coopDelta = +1;

        // Shelter 07 (sales scam)
        sh07A.label = "A：家が心配なので記入する";
        sh07A.sanDelta = -2;
        sh07A.addFlags = new List<string> { GameManager.FlagScamVictim };
        sh07B.label = "B：避難所スタッフに確認する";
        sh07B.trustDelta = +10;
        sh07B.coopDelta = +1;
        sh07B.addFlags = new List<string> { GameManager.FlagScamAvoided };

        // Shelter 08 (SNS rumor)
        sh08A.label = "A：投稿を信じて移動を考える";
        sh08A.hpDelta = -1;
        sh08A.waterDelta = -10;
        sh08A.sanDelta = -1;
        sh08A.addFlags = new List<string> { GameManager.FlagRumorVictim };
        sh08B.label = "B：公式情報や掲示板で確認する";
        sh08B.sanDelta = +1;
        sh08B.trustDelta = +5;
        sh08B.addFlags = new List<string> { GameManager.FlagRumorAvoided };

        // Shelter 09 (conflict)
        sh09A.label = "A：自分の分を受け取って離れる";
        sh09A.hungerDelta = +2;
        sh09A.hpDelta = +1;
        sh09A.trustDelta = -5;
        sh09A.sanDelta = -1;
        sh09B.label = "B：配布の整理を手伝う";
        sh09B.hpDelta = -1;
        sh09B.hungerDelta = +1;
        sh09B.trustDelta = +15;
        sh09B.coopDelta = +1;
        sh09B.sanDelta = +1;

        // Shelter 10 (ill person)
        sh10A.label = "A：声をかけて職員を呼びに行く";
        sh10A.hpDelta = -1;
        sh10A.sanDelta = +2;
        sh10A.trustDelta = +15;
        sh10A.coopDelta = +1;
        sh10B.label = "B：自分も限界なので休む";
        sh10B.hpDelta = +1;
        sh10B.sanDelta = -1;

        // Shelter 11 (patrol)
        sh11A.label = "A：最後の見回りを手伝う";
        sh11A.hpDelta = -2;
        sh11A.trustDelta = +20;
        sh11A.sanDelta = +2;
        sh11A.coopDelta = +1;
        sh11B.label = "B：救助に備えて休む";
        sh11B.hpDelta = +2;
        sh11B.sanDelta = +1;

        // Link sequential flow
        evac01.nextEventAfterChoiceA = evac02;
        evac01.nextEventAfterChoiceB = evac02;
        evac02.nextEventAfterChoiceA = evac03;
        evac02.nextEventAfterChoiceB = evac03;
        evac03.nextEventAfterChoiceA = evac04;
        evac03.nextEventAfterChoiceB = evac04;
        evac04.nextEventAfterChoiceA = evac05;
        evac04.nextEventAfterChoiceB = evac05;
        evac05.nextEventAfterChoiceA = evac06;
        evac05.nextEventAfterChoiceB = evac06;
        evac06.nextEventAfterChoiceA = evacResult;
        evac06.nextEventAfterChoiceB = evacResult;

        evacResA.label = "避難所へ入る";
        evacResA.switchPhaseAfterChoice = true;
        evacResA.nextPhase = GamePhase.PostEvacuation;
        evacResult.nextEventAfterChoiceA = sh01;
        evacResB.label = "-";
        evacResB.requiredFlags = new List<string> { "__never__" };

        // shelter flow
        sh01.nextEventAfterChoiceA = sh02;
        sh01.nextEventAfterChoiceB = sh02;
        sh02.nextEventAfterChoiceA = sh03;
        sh02.nextEventAfterChoiceB = sh03;
        sh03.nextEventAfterChoiceA = sh04;
        sh03.nextEventAfterChoiceB = sh04;
        sh04.nextEventAfterChoiceA = sh05;
        sh04.nextEventAfterChoiceB = sh05;
        sh05.nextEventAfterChoiceA = sh06;
        sh05.nextEventAfterChoiceB = sh06;
        sh06.nextEventAfterChoiceA = sh07;
        sh06.nextEventAfterChoiceB = sh07;
        sh07.nextEventAfterChoiceA = sh08;
        sh07.nextEventAfterChoiceB = sh08;
        sh08.nextEventAfterChoiceA = sh09;
        sh08.nextEventAfterChoiceB = sh09;
        sh09.nextEventAfterChoiceA = sh10;
        sh09.nextEventAfterChoiceB = sh10;
        sh10.nextEventAfterChoiceA = sh11;
        sh10.nextEventAfterChoiceB = sh11;
        sh11.nextEventAfterChoiceA = ending;
        sh11.nextEventAfterChoiceB = ending;

        endingA.label = "リスタート";
        endingA.resetRunAfterChoice = true;
        endingB.label = "-";
        endingB.requiredFlags = new List<string> { "__never__" };

        EditorUtility.SetDirty(evac01);
        EditorUtility.SetDirty(evac02);
        EditorUtility.SetDirty(evac03);
        EditorUtility.SetDirty(evac04);
        EditorUtility.SetDirty(evac05);
        EditorUtility.SetDirty(evac06);
        EditorUtility.SetDirty(evacResult);
        EditorUtility.SetDirty(sh01);
        EditorUtility.SetDirty(sh02);
        EditorUtility.SetDirty(sh03);
        EditorUtility.SetDirty(sh04);
        EditorUtility.SetDirty(sh05);
        EditorUtility.SetDirty(sh06);
        EditorUtility.SetDirty(sh07);
        EditorUtility.SetDirty(sh08);
        EditorUtility.SetDirty(sh09);
        EditorUtility.SetDirty(sh10);
        EditorUtility.SetDirty(sh11);
        EditorUtility.SetDirty(ending);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("PDF Scenario assets generated/updated under Resources/ScenarioPDF. Start path: ScenarioPDF/Evac_01");
    }

    private static EventData CreateOrUpdateEvent(string fileName, string eventId, GamePhase phase, string text, out ChoiceData choiceA, out ChoiceData choiceB)
    {
        string path = $"{RootFolder}/{fileName}.asset";
        EventData asset = AssetDatabase.LoadAssetAtPath<EventData>(path);
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<EventData>();
            AssetDatabase.CreateAsset(asset, path);
        }

        asset.eventId = eventId;
        asset.phase = phase;
        asset.priority = 0;
        asset.consumeOnce = false;
        asset.requiredFlags = asset.requiredFlags ?? new List<string>();
        asset.eventText = text;

        asset.choiceA ??= new ChoiceData();
        asset.choiceB ??= new ChoiceData();
        choiceA = asset.choiceA;
        choiceB = asset.choiceB;

        NormalizeChoice(choiceA);
        NormalizeChoice(choiceB);
        EditorUtility.SetDirty(asset);
        return asset;
    }

    private static void NormalizeChoice(ChoiceData choice)
    {
        choice.label ??= "選ぶ";
        choice.resultText ??= string.Empty;
        choice.requiredFlags ??= new List<string>();
        choice.addFlags ??= new List<string>();
        choice.removeFlags ??= new List<string>();
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

    // --- Text builders (from PDF) ---

    private static string BuildEvac01() =>
        "イベント1「持ち出すものを選ぶ」\n\n"
        + "避難する前に、手元にあるものを少しだけ持っていける。\n"
        + "ただし、持ちすぎると移動が遅くなり、体力を消費しやすくなる。";

    private static string BuildEvac02() =>
        "イベント2「倒れた自転車と道を塞ぐ荷物」\n\n"
        + "細い道で自転車や棚が倒れて道を塞いでいる。\n"
        + "近くに高齢者と小学生がいて、先に進めず困っている。";

    private static string BuildEvac03() =>
        "イベント3「半壊したコンビニ」\n\n"
        + "シャッターが半分開いたコンビニがある。\n"
        + "中には水や食料、乾電池、衛生用品が残っている。\n"
        + "しかし店内は少し危険そうだ。";

    private static string BuildEvac04() =>
        "イベント4「泣いている子ども」\n\n"
        + "道路脇で子どもが泣いている。\n"
        + "親とはぐれたらしく、避難所の場所も分からない様子。";

    private static string BuildEvac05() =>
        "イベント5「水を分けるか、残すか」\n\n"
        + "避難所まであと少し。\n"
        + "途中で、喉が渇いて動けない人に出会う。";

    private static string BuildEvac06() =>
        "イベント6「避難所前の最後の坂」\n\n"
        + "避難所の小学校が見えてくる。\n"
        + "最後の坂道で、荷物を持った人たちが苦しそうに歩いている。";

    private static string BuildSh01() =>
        "イベント01「避難所の受付」\n\n"
        + "体育館には、すでに多くの人が集まっていた。\n"
        + "受付には長い列ができ、人手が足りていないようだ。";

    private static string BuildSh02() =>
        "イベント02「助けた人との再会」\n\n"
        + "避難中に声をかけた人と再会するかもしれない。\n"
        + "助けていない場合は別の展開になる。";

    private static string BuildSh03() =>
        "イベント03「最初の夜の仕事」\n\n"
        + "夜になり、避難所では人手が足りなくなってきた。";

    private static string BuildSh04() =>
        "イベント04「支援金申請メール」\n\n"
        + "スマホに一通のメールが届いた。\n"
        + "公式サイトなのか判断できないリンクがある。";

    private static string BuildSh05() =>
        "イベント05「水の配布」\n\n"
        + "水の配布が始まった。\n"
        + "全員に十分な量はない。";

    private static string BuildSh06() =>
        "イベント06「トイレの問題」\n\n"
        + "避難所生活が続き、トイレの状態が悪くなってきた。";

    private static string BuildSh07() =>
        "イベント07「無料点検の営業」\n\n"
        + "避難所の入口付近に、作業着姿の男性が現れた。\n"
        + "書類への記入を求められる。";

    private static string BuildSh08() =>
        "イベント08「SNS の物資情報」\n\n"
        + "SNSで『隣町の避難所は物資が余っている』という投稿が広がっていた。";

    private static string BuildSh09() =>
        "イベント09「避難所内の対立」\n\n"
        + "食料配布の量をめぐって避難者同士が揉めている。";

    private static string BuildSh10() =>
        "イベント10「体調不良者」\n\n"
        + "近くの人が苦しそうにしている。\n"
        + "職員もすぐには来られそうにない。";

    private static string BuildSh11() =>
        "イベント11「救助到着前夜」\n\n"
        + "救助が近いという話が入った。\n"
        + "職員が夜間の見回りを手伝える人を探している。";
}

