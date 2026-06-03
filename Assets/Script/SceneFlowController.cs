using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlowController : MonoBehaviour
{
    public const string SceneEvac = "UI_Rekuto";
    public const string SceneShelter = "UI_Rikuto_Shelter";

    private static SceneFlowController instance;

    [SerializeField] private GameManager gameManagerPrefab;
    [SerializeField] private bool resetRunOnEvacScene = true;

    private GameManager gameManager;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureGameManager();
    }

    private void Start()
    {
        BindPresenterInActiveScene();
    }

    private void Update()
    {
        if (gameManager == null)
        {
            return;
        }

        if (gameManager.PendingShelterSceneLoad)
        {
            gameManager.ClearPendingSceneFlags();
            SceneManager.LoadScene(SceneShelter);
            return;
        }

        if (gameManager.PendingEvacSceneLoad)
        {
            gameManager.ClearPendingSceneFlags();
            SceneManager.LoadScene(SceneEvac);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindPresenterInActiveScene();
    }

    private void EnsureGameManager()
    {
        if (gameManager != null)
        {
            return;
        }

        gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            DontDestroyOnLoad(gameManager.gameObject);
            return;
        }

        if (gameManagerPrefab != null)
        {
            gameManager = Instantiate(gameManagerPrefab);
        }
        else
        {
            GameObject gmObject = new GameObject("GameManager");
            gameManager = gmObject.AddComponent<GameManager>();
        }

        DontDestroyOnLoad(gameManager.gameObject);
    }

    private void BindPresenterInActiveScene()
    {
        if (gameManager == null)
        {
            return;
        }

        GameUiPresenter presenter = FindObjectOfType<GameUiPresenter>();
        if (presenter != null)
        {
            presenter.ResetUiStep();
            presenter.SetGameManager(gameManager);
        }
    }

    public static GameManager GetGameManager()
    {
        if (instance != null && instance.gameManager != null)
        {
            return instance.gameManager;
        }

        return FindObjectOfType<GameManager>();
    }
}
