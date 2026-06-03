using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Wires PDF gameplay into UI scenes at runtime without requiring manual scene edits.
/// </summary>
public static class DisaVentureRuntimeBootstrap
{
    private static bool evacRunStarted;
    private static bool sceneLoadHooked;
    private static GameObject persistentBgmManager;
    private static AudioSource persistentBgmSource;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        evacRunStarted = false;
        sceneLoadHooked = false;
        persistentBgmManager = null;
        persistentBgmSource = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (!sceneLoadHooked)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            sceneLoadHooked = true;
        }

        WireActiveScene();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        WireActiveScene();
    }

    private static void WireActiveScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != SceneFlowController.SceneEvac && sceneName != SceneFlowController.SceneShelter)
        {
            return;
        }

        SceneFlowController flow = EnsureSceneFlowController();
        GameManager gameManager = flow != null ? SceneFlowController.GetGameManager() : EnsureStandaloneGameManager();

        CleanupDuplicateManagers(gameManager);
        EnsureBgmPersists();

        if (sceneName == SceneFlowController.SceneEvac && !evacRunStarted)
        {
            gameManager.ResetRun();
            evacRunStarted = true;
        }
        else if (sceneName == SceneFlowController.SceneEvac && gameManager.PendingEvacSceneLoad)
        {
            gameManager.ClearPendingSceneFlags();
        }

        EnsureGameUi(gameManager, sceneName == SceneFlowController.SceneEvac);
    }

    private static void EnsureBgmPersists()
    {
        if (persistentBgmManager != null)
        {
            ResumePersistentBgm();
            return;
        }

        GameObject bgmManager = GameObject.Find("BGMManager");
        if (bgmManager == null)
        {
            return;
        }

        persistentBgmManager = bgmManager;
        persistentBgmSource = bgmManager.GetComponent<AudioSource>();
        bgmManager.transform.SetParent(null, true);
        Object.DontDestroyOnLoad(bgmManager);
        ResumePersistentBgm();
    }

    private static void ResumePersistentBgm()
    {
        if (persistentBgmSource == null)
        {
            return;
        }

        if (persistentBgmSource.clip != null && !persistentBgmSource.isPlaying)
        {
            persistentBgmSource.Play();
        }
    }

    private static SceneFlowController EnsureSceneFlowController()
    {
        SceneFlowController existing = Object.FindObjectOfType<SceneFlowController>();
        if (existing != null)
        {
            return existing;
        }

        GameObject systems = new GameObject("GameSystems");
        return systems.AddComponent<SceneFlowController>();
    }

    private static GameManager EnsureStandaloneGameManager()
    {
        GameManager gm = Object.FindObjectOfType<GameManager>();
        if (gm != null)
        {
            Object.DontDestroyOnLoad(gm.gameObject);
            return gm;
        }

        GameObject go = new GameObject("GameManager");
        gm = go.AddComponent<GameManager>();
        Object.DontDestroyOnLoad(go);
        return gm;
    }

    private static void CleanupDuplicateManagers(GameManager keeper)
    {
        GameManager[] all = Object.FindObjectsOfType<GameManager>();
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] == keeper)
            {
                continue;
            }

            if (all[i].gameObject.name == "GameSystems" && keeper.gameObject.name == "GameSystems")
            {
                Object.Destroy(all[i].gameObject);
            }
            else
            {
                Object.Destroy(all[i].gameObject);
            }
        }

        SceneFlowController[] flows = Object.FindObjectsOfType<SceneFlowController>();
        SceneFlowController keeperFlow = keeper.GetComponent<SceneFlowController>()
            ?? Object.FindObjectOfType<SceneFlowController>();
        for (int i = 0; i < flows.Length; i++)
        {
            if (flows[i] != keeperFlow)
            {
                Object.Destroy(flows[i].gameObject);
            }
        }
    }

    private static void EnsureGameUi(GameManager gameManager, bool isEvacScene)
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

        ConfigureStatBars(statBar, isEvacScene);
        presenter.SetGameManager(gameManager);

        DisaVenturePolishController polish = uiRoot.GetComponent<DisaVenturePolishController>();
        if (polish == null)
        {
            polish = uiRoot.AddComponent<DisaVenturePolishController>();
        }

        polish.Configure(presenter, isEvacScene);
    }

    private static void ConfigureStatBars(StatBarView statBar, bool isEvacScene)
    {
        if (isEvacScene)
        {
            statBar.Configure(new[]
            {
                Heart("hitpoint", 0.25f),
                Heart("hitpoint (1)", 0.5f),
                Heart("hitpoint (2)", 0.75f),
                Heart("hitpoint (3)", 1f),
                Bar(StatBarView.StatType.Water, "bottle"),
                Bar(StatBarView.StatType.Supplies, "busshi")
            });
        }
        else
        {
            statBar.Configure(new[]
            {
                Heart("hitpoint (2)", 0.25f),
                Heart("hitpoint (3)", 0.5f),
                Heart("hitpoint (4)", 0.75f),
                Heart("hitpoint (5)", 1f),
                Bar(StatBarView.StatType.Water, "energy"),
                Bar(StatBarView.StatType.Hygiene, "energy (1)"),
                Bar(StatBarView.StatType.Supplies, "bag")
            });
        }
    }

    private static StatBarView.BarBinding Heart(string objectName, float threshold)
    {
        StatBarView.BarBinding binding = Bar(StatBarView.StatType.Hp, objectName);
        binding.visibleThreshold = threshold;
        return binding;
    }

    private static StatBarView.BarBinding Bar(StatBarView.StatType type, string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        Image image = FindFillImage(obj);
        SpriteRenderer spriteRenderer = obj != null ? obj.GetComponent<SpriteRenderer>() : null;
        return new StatBarView.BarBinding { statType = type, fillImage = image, spriteRenderer = spriteRenderer };
    }

    private static Image FindFillImage(GameObject obj)
    {
        if (obj == null)
        {
            return null;
        }

        Image image = obj.GetComponent<Image>();
        return image != null ? image : obj.GetComponentInChildren<Image>(true);
    }
}
