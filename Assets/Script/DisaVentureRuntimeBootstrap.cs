using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Wires PDF gameplay into UI scenes at runtime without requiring manual scene edits.
/// </summary>
public static class DisaVentureRuntimeBootstrap
{
    private static bool evacRunStarted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AfterSceneLoad()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName != SceneFlowController.SceneEvac && sceneName != SceneFlowController.SceneShelter)
        {
            return;
        }

        SceneFlowController flow = EnsureSceneFlowController();
        GameManager gameManager = flow != null ? SceneFlowController.GetGameManager() : EnsureStandaloneGameManager();

        CleanupDuplicateManagers(gameManager);

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
    }

    private static void ConfigureStatBars(StatBarView statBar, bool isEvacScene)
    {
        if (isEvacScene)
        {
            statBar.Configure(new[]
            {
                Bar(StatBarView.StatType.Hp, "hitpoint"),
                Bar(StatBarView.StatType.Hunger, "hitpoint (1)"),
                Bar(StatBarView.StatType.San, "hitpoint (2)"),
                Bar(StatBarView.StatType.Water, "bottle"),
                Bar(StatBarView.StatType.Supplies, "busshi")
            });
        }
        else
        {
            statBar.Configure(new[]
            {
                Bar(StatBarView.StatType.Hp, "hitpoint"),
                Bar(StatBarView.StatType.Hunger, "hitpoint (2)"),
                Bar(StatBarView.StatType.San, "hitpoint (3)"),
                Bar(StatBarView.StatType.Trust, "hitpoint (4)"),
                Bar(StatBarView.StatType.Coop, "hitpoint (5)"),
                Bar(StatBarView.StatType.Water, "energy"),
                Bar(StatBarView.StatType.Hygiene, "energy (1)"),
                Bar(StatBarView.StatType.Supplies, "busshi")
            });
        }
    }

    private static StatBarView.BarBinding Bar(StatBarView.StatType type, string objectName)
    {
        Image image = FindFillImage(objectName);
        return new StatBarView.BarBinding { statType = type, fillImage = image };
    }

    private static Image FindFillImage(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj == null)
        {
            return null;
        }

        Image image = obj.GetComponent<Image>();
        return image != null ? image : obj.GetComponentInChildren<Image>(true);
    }
}
