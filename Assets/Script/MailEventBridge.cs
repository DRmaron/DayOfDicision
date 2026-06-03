using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Future hook for SH_04 mail UI (Male Scene). Currently no-op; choices run in shelter scene.
/// </summary>
public class MailEventBridge : MonoBehaviour
{
    public const string MaleSceneName = "Male Scene";
    public const string MailEventId = "SH_04";

    [SerializeField] private bool enableMaleSceneOverlay;

    public static bool ShouldOpenMailUi(EventData currentEvent, bool enableOverlay)
    {
        return enableOverlay
            && currentEvent != null
            && currentEvent.eventId == MailEventId;
    }

    public void TryOpenMailUi(EventData currentEvent)
    {
        if (!ShouldOpenMailUi(currentEvent, enableMaleSceneOverlay))
        {
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(MaleSceneName))
        {
            Debug.LogWarning("MailEventBridge: Male Scene is not in Build Settings yet.");
            return;
        }

        SceneManager.LoadScene(MaleSceneName, LoadSceneMode.Additive);
    }

    public void CloseMailUi()
    {
        Scene mailScene = SceneManager.GetSceneByName(MaleSceneName);
        if (mailScene.isLoaded)
        {
            SceneManager.UnloadSceneAsync(MaleSceneName);
        }
    }
}
