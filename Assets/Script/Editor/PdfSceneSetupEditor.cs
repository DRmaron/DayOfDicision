using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public static class PdfSceneSetupEditor
{
    private const string EvacScenePath = "Assets/Scenes/UI_Rekuto.unity";
    private const string ShelterScenePath = "Assets/Scenes/UI_Rikuto_Shelter.unity";
    private const string MailScenePath = "Assets/Scenes/Male Scene.unity";
    private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";

    [MenuItem("Tools/DisaVenture/PDF Scenario/Setup All (Assets + Scenes + Build)")]
    public static void SetupAll()
    {
        PdfScenarioGenerator.GenerateOrUpdateAssets();
        SetupEvacScene();
        SetupShelterScene();
        UpdateBuildSettings();
        AssetDatabase.SaveAssets();
        Debug.Log("DisaVenture PDF setup completed.");
    }

    [MenuItem("Tools/DisaVenture/PDF Scenario/Wire UI_Rekuto Scene")]
    public static void SetupEvacScene()
    {
        Scene scene = EditorSceneManager.OpenScene(EvacScenePath, OpenSceneMode.Single);
        EnsureGameSystems();
        EnsureGameUiEvac();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    [MenuItem("Tools/DisaVenture/PDF Scenario/Wire UI_Rikuto_Shelter Scene")]
    public static void SetupShelterScene()
    {
        Scene scene = EditorSceneManager.OpenScene(ShelterScenePath, OpenSceneMode.Single);
        RemoveSceneGameManagerIfPresent();
        EnsureGameUiShelter();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    [MenuItem("Tools/DisaVenture/PDF Scenario/Update Build Settings")]
    public static void UpdateBuildSettings()
    {
        EditorBuildSettingsScene[] scenes =
        {
            new EditorBuildSettingsScene(EvacScenePath, true),
            new EditorBuildSettingsScene(ShelterScenePath, true),
            new EditorBuildSettingsScene(SampleScenePath, false),
            new EditorBuildSettingsScene(MailScenePath, true)
        };
        EditorBuildSettings.scenes = scenes;
    }

    private static void EnsureGameSystems()
    {
        GameObject systems = GameObject.Find("GameSystems");
        if (systems == null)
        {
            systems = new GameObject("GameSystems");
        }

        GameManager gm = systems.GetComponent<GameManager>();
        if (gm == null)
        {
            gm = systems.AddComponent<GameManager>();
        }

        SceneFlowController flow = systems.GetComponent<SceneFlowController>();
        if (flow == null)
        {
            flow = systems.AddComponent<SceneFlowController>();
        }

        MailEventBridge mail = systems.GetComponent<MailEventBridge>();
        if (mail == null)
        {
            systems.AddComponent<MailEventBridge>();
        }

        SerializedObject gmSo = new SerializedObject(gm);
        gmSo.FindProperty("usePdfScenario").boolValue = true;
        gmSo.FindProperty("autoResetOnStart").boolValue = false;
        gmSo.FindProperty("startEventResourcesPath").stringValue = "ScenarioPDF/Evac_01";
        gmSo.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void RemoveSceneGameManagerIfPresent()
    {
        GameManager[] managers = Object.FindObjectsOfType<GameManager>();
        for (int i = 0; i < managers.Length; i++)
        {
            if (managers[i].gameObject.name == "GameSystems")
            {
                Object.DestroyImmediate(managers[i]);
            }
        }
    }

    private static void EnsureGameUiEvac()
    {
        GameObject uiRoot = GameObject.Find("GameUi");
        if (uiRoot == null)
        {
            uiRoot = new GameObject("GameUi");
        }

        GameUiPresenter presenter = uiRoot.GetComponent<GameUiPresenter>();
        if (presenter == null)
        {
            presenter = uiRoot.AddComponent<GameUiPresenter>();
        }

        StatBarView statBar = uiRoot.GetComponent<StatBarView>();
        if (statBar == null)
        {
            statBar = uiRoot.AddComponent<StatBarView>();
        }

        SanityEffect sanity = Object.FindObjectOfType<SanityEffect>();
        Image hp = FindFillImage("hitpoint");
        Image hunger = FindFillImage("hitpoint (1)");
        Image san = FindFillImage("hitpoint (2)");
        Image water = FindFillImage("bottle");
        Image supplies = FindFillImage("busshi");

        StatBarView.BarBinding[] evacBars =
        {
            Bind(hp, StatBarView.StatType.Hp),
            Bind(hunger, StatBarView.StatType.Hunger),
            Bind(san, StatBarView.StatType.San),
            Bind(water, StatBarView.StatType.Water),
            Bind(supplies, StatBarView.StatType.Supplies)
        };

        SerializedObject statSo = new SerializedObject(statBar);
        statSo.FindProperty("bars").arraySize = evacBars.Length;
        for (int i = 0; i < evacBars.Length; i++)
        {
            SerializedProperty element = statSo.FindProperty("bars").GetArrayElementAtIndex(i);
            element.FindPropertyRelative("fillImage").objectReferenceValue = evacBars[i].fillImage;
            element.FindPropertyRelative("statType").enumValueIndex = (int)evacBars[i].statType;
        }
        statSo.ApplyModifiedPropertiesWithoutUndo();

        WirePresenter(presenter, sanity, FindTmp("joukyou"), FindButton("senntakushi_A"), FindButton("senntakushi_B"));
    }

    private static void EnsureGameUiShelter()
    {
        GameObject uiRoot = GameObject.Find("GameUi");
        if (uiRoot == null)
        {
            uiRoot = new GameObject("GameUi");
        }

        GameUiPresenter presenter = uiRoot.GetComponent<GameUiPresenter>();
        if (presenter == null)
        {
            presenter = uiRoot.AddComponent<GameUiPresenter>();
        }

        StatBarView statBar = uiRoot.GetComponent<StatBarView>();
        if (statBar == null)
        {
            statBar = uiRoot.AddComponent<StatBarView>();
        }

        SanityEffect sanity = Object.FindObjectOfType<SanityEffect>();

        List<StatBarView.BarBinding> bindings = new List<StatBarView.BarBinding>
        {
            Bind(FindFillImage("hitpoint"), StatBarView.StatType.Hp),
            Bind(FindFillImage("hitpoint (2)"), StatBarView.StatType.Hunger),
            Bind(FindFillImage("hitpoint (3)"), StatBarView.StatType.San),
            Bind(FindFillImage("hitpoint (4)"), StatBarView.StatType.Trust),
            Bind(FindFillImage("hitpoint (5)"), StatBarView.StatType.Coop),
            Bind(FindFillImage("energy"), StatBarView.StatType.Water),
            Bind(FindFillImage("energy (1)"), StatBarView.StatType.Hygiene),
            Bind(FindFillImage("busshi"), StatBarView.StatType.Supplies)
        };

        SerializedObject statSo = new SerializedObject(statBar);
        statSo.FindProperty("bars").arraySize = bindings.Count;
        for (int i = 0; i < bindings.Count; i++)
        {
            SerializedProperty element = statSo.FindProperty("bars").GetArrayElementAtIndex(i);
            element.FindPropertyRelative("fillImage").objectReferenceValue = bindings[i].fillImage;
            element.FindPropertyRelative("statType").enumValueIndex = (int)bindings[i].statType;
        }
        statSo.ApplyModifiedPropertiesWithoutUndo();

        WirePresenter(presenter, sanity, FindTmp("joukyou"), FindButton("senntakushi_A"), FindButton("senntakushi_B"));
    }

    private static void WirePresenter(GameUiPresenter presenter, SanityEffect sanity, TMP_Text eventTmp, Button buttonA, Button buttonB)
    {
        SerializedObject so = new SerializedObject(presenter);
        so.FindProperty("sanityEffect").objectReferenceValue = sanity;
        so.FindProperty("eventText").objectReferenceValue = eventTmp;
        so.FindProperty("choiceAButton").objectReferenceValue = buttonA;
        so.FindProperty("choiceBButton").objectReferenceValue = buttonB;

        if (buttonA != null)
        {
            so.FindProperty("choiceAText").objectReferenceValue = buttonA.GetComponentInChildren<TMP_Text>(true);
        }

        if (buttonB != null)
        {
            so.FindProperty("choiceBText").objectReferenceValue = buttonB.GetComponentInChildren<TMP_Text>(true);
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static StatBarView.BarBinding Bind(Image image, StatBarView.StatType type)
    {
        return new StatBarView.BarBinding { fillImage = image, statType = type };
    }

    private static Image FindFillImage(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj == null)
        {
            return null;
        }

        Image image = obj.GetComponent<Image>();
        if (image != null)
        {
            return image;
        }

        return obj.GetComponentInChildren<Image>(true);
    }

    private static TMP_Text FindTmp(string rootName)
    {
        GameObject root = GameObject.Find(rootName);
        return root != null ? root.GetComponentInChildren<TMP_Text>(true) : null;
    }

    private static Button FindButton(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        return obj != null ? obj.GetComponent<Button>() : null;
    }
}
