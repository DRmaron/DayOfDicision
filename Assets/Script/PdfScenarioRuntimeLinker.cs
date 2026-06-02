using UnityEngine;

/// <summary>
/// Ensures PDF event chain references exist when loading from Resources.
/// </summary>
public static class PdfScenarioRuntimeLinker
{
    public static void EnsureLinked(GameManager gameManager = null)
    {
        EventData start = Resources.Load<EventData>("ScenarioPDF/Evac_01");
        if (start == null)
        {
            Debug.LogError("PdfScenarioRuntimeLinker: Missing Resources/ScenarioPDF/Evac_01");
            return;
        }

        if (start.nextEventAfterChoiceA != null)
        {
            return;
        }

        EventData evac02 = Load("Evac_02");
        EventData evac03 = Load("Evac_03");
        EventData evac04 = Load("Evac_04");
        EventData evac05 = Load("Evac_05");
        EventData evac06 = Load("Evac_06");
        EventData evacResult = Load("Evac_Result");
        EventData sh01 = Load("Shelter_01");
        EventData sh02 = Load("Shelter_02");
        EventData sh03 = Load("Shelter_03");
        EventData sh04 = Load("Shelter_04");
        EventData sh05 = Load("Shelter_05");
        EventData sh06 = Load("Shelter_06");
        EventData sh07 = Load("Shelter_07");
        EventData sh08 = Load("Shelter_08");
        EventData sh09 = Load("Shelter_09");
        EventData sh10 = Load("Shelter_10");
        EventData sh11 = Load("Shelter_11");
        EventData ending = Load("Ending");

        LinkBoth(start, evac02);
        LinkBoth(evac02, evac03);
        LinkBoth(evac03, evac04);
        LinkBoth(evac04, evac05);
        LinkBoth(evac05, evac06);
        LinkBoth(evac06, evacResult);
        if (evacResult != null)
        {
            evacResult.nextEventAfterChoiceA = sh01;
        }

        LinkBoth(sh01, sh02);
        LinkBoth(sh02, sh03);
        LinkBoth(sh03, sh04);
        LinkBoth(sh04, sh05);
        LinkBoth(sh05, sh06);
        LinkBoth(sh06, sh07);
        LinkBoth(sh07, sh08);
        LinkBoth(sh08, sh09);
        LinkBoth(sh09, sh10);
        LinkBoth(sh10, sh11);
        if (sh11 != null)
        {
            sh11.nextEventAfterChoiceA = ending;
            sh11.nextEventAfterChoiceB = ending;
        }
    }

    private static EventData Load(string name)
    {
        return Resources.Load<EventData>("ScenarioPDF/" + name);
    }

    private static void LinkBoth(EventData from, EventData to)
    {
        if (from == null || to == null)
        {
            return;
        }

        from.nextEventAfterChoiceA = to;
        from.nextEventAfterChoiceB = to;
    }
}
