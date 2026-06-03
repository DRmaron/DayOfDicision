using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DisaVenturePolishController : MonoBehaviour
{
    private const float PanelAlpha = 0.8f;
    private const string EvacuationBackgroundResource = "Backgrounds/EvacuationBackground";
    private const string ShelterBackgroundResource = "Backgrounds/ShelterBackground";

    private static bool titleAccepted;
    private static AudioSource persistentSeSource;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        titleAccepted = false;
        persistentSeSource = null;
    }

    private DisaVentureAudioLibrary audioLibrary;
    private GameUiPresenter presenter;
    private Canvas canvas;

    public void Configure(GameUiPresenter uiPresenter, bool isEvacScene)
    {
        presenter = uiPresenter;
        audioLibrary = DisaVentureAudioLibrary.LoadDefault();
        canvas = FindUiCanvas();

        EnsureBackgroundSurface(isEvacScene);
        ApplyUiPanelAlpha();
        DisableLegacyChoiceEventTriggers();
        ConfigureAudio(isEvacScene);

        if (isEvacScene && !titleAccepted)
        {
            ShowTitleOverlay();
        }
    }

    private void EnsureBackgroundSurface(bool isEvacScene)
    {
        if (canvas == null)
        {
            return;
        }

        GameObject surface = GameObject.Find("main_background");
        bool createdSurface = false;
        if (surface == null)
        {
            surface = GameObject.Find("BackgroundSlot");
        }

        if (surface == null)
        {
            surface = new GameObject("BackgroundSlot", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            surface.transform.SetParent(canvas.transform, false);
            surface.transform.SetAsFirstSibling();
            createdSurface = true;
        }

        Image image = surface.GetComponent<Image>();
        SpriteRenderer spriteRenderer = surface.GetComponent<SpriteRenderer>();
        Sprite fallbackSprite = LoadBackgroundSprite(isEvacScene ? EvacuationBackgroundResource : ShelterBackgroundResource);

        if (image != null)
        {
            RectTransform rect = surface.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                rect.localScale = Vector3.one;
            }

            image.raycastTarget = false;
            image.color = isEvacScene ? new Color(0.07f, 0.12f, 0.18f, 1f) : new Color(0.10f, 0.11f, 0.10f, 1f);
            if (createdSurface && image.sprite == null && fallbackSprite != null)
            {
                image.sprite = fallbackSprite;
            }
            image.preserveAspect = false;
            return;
        }

        if (spriteRenderer != null)
        {
            if (createdSurface && spriteRenderer.sprite == null && fallbackSprite != null)
            {
                spriteRenderer.sprite = fallbackSprite;
            }

            if (spriteRenderer.sortingOrder > -10)
            {
                spriteRenderer.sortingOrder = -10;
            }
        }
    }

    private static Canvas FindUiCanvas()
    {
        GameObject namedCanvas = GameObject.Find("Canvas");
        if (namedCanvas != null)
        {
            Canvas canvas = namedCanvas.GetComponent<Canvas>();
            if (canvas != null)
            {
                return canvas;
            }
        }

        Canvas[] canvases = Object.FindObjectsOfType<Canvas>(true);
        Canvas best = null;
        int bestOrder = int.MinValue;
        for (int i = 0; i < canvases.Length; i++)
        {
            Canvas candidate = canvases[i];
            if (candidate == null || !candidate.gameObject.activeInHierarchy)
            {
                continue;
            }

            int order = candidate.sortingOrder;
            if (best == null || order >= bestOrder)
            {
                best = candidate;
                bestOrder = order;
            }
        }

        return best;
    }

    private static Sprite LoadBackgroundSprite(string resourcePath)
    {
        Sprite sprite = Resources.Load<Sprite>(resourcePath);
        if (sprite != null)
        {
            return sprite;
        }

        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            return null;
        }

        Rect rect = new Rect(0f, 0f, texture.width, texture.height);
        return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f), 100f);
    }

    private void ApplyUiPanelAlpha()
    {
        Image[] images = FindObjectsOfType<Image>(true);
        for (int i = 0; i < images.Length; i++)
        {
            Image image = images[i];
            if (image == null || image.gameObject.name == "BackgroundSlot")
            {
                continue;
            }

            if (IsGreyPanelColor(image.color))
            {
                Color color = image.color;
                color.a = PanelAlpha;
                image.color = color;
            }
        }

        SpriteRenderer[] sprites = FindObjectsOfType<SpriteRenderer>(true);
        for (int i = 0; i < sprites.Length; i++)
        {
            SpriteRenderer sprite = sprites[i];
            if (sprite == null || sprite.gameObject.name == "main_background")
            {
                continue;
            }

            if (IsGreyPanelColor(sprite.color))
            {
                Color color = sprite.color;
                color.a = PanelAlpha;
                sprite.color = color;
            }
        }
    }

    private static bool IsGreyPanelColor(Color color)
    {
        float max = Mathf.Max(color.r, color.g, color.b);
        float min = Mathf.Min(color.r, color.g, color.b);
        return color.a > 0.45f && color.a <= 1f && max - min < 0.08f && max < 0.75f;
    }

    private void ConfigureAudio(bool isEvacScene)
    {
        AudioSource seSource = EnsureSeSource();
        if (presenter != null && audioLibrary != null)
        {
            presenter.ConfigureAudio(seSource, audioLibrary.choiceClickSe, audioLibrary.nextClickSe);
        }

        AudioSource bgmSource = FindBgmSource();
        if (bgmSource == null || audioLibrary == null)
        {
            return;
        }

        AudioClip targetClip = isEvacScene ? audioLibrary.evacuationBgm : audioLibrary.shelterBgm;
        if (targetClip == null)
        {
            return;
        }

        if (bgmSource.clip != targetClip)
        {
            bgmSource.Stop();
            bgmSource.clip = targetClip;
        }

        bgmSource.loop = true;
        if (!bgmSource.isPlaying)
        {
            bgmSource.Play();
        }
    }

    private static void DisableLegacyChoiceEventTriggers()
    {
        DisableEventTrigger("senntakushi_A");
        DisableEventTrigger("senntakushi_B");
    }

    private static void DisableEventTrigger(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj == null)
        {
            return;
        }

        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        if (trigger != null)
        {
            trigger.enabled = false;
        }
    }

    private static AudioSource EnsureSeSource()
    {
        if (persistentSeSource != null)
        {
            return persistentSeSource;
        }

        GameObject seManager = GameObject.Find("SEManager");
        if (seManager == null)
        {
            seManager = new GameObject("SEManager");
        }

        seManager.transform.SetParent(null, true);
        Object.DontDestroyOnLoad(seManager);

        persistentSeSource = seManager.GetComponent<AudioSource>();
        if (persistentSeSource == null)
        {
            persistentSeSource = seManager.AddComponent<AudioSource>();
        }

        persistentSeSource.playOnAwake = false;
        persistentSeSource.loop = false;
        return persistentSeSource;
    }

    private static AudioSource FindBgmSource()
    {
        GameObject bgmManager = GameObject.Find("BGMManager");
        return bgmManager != null ? bgmManager.GetComponent<AudioSource>() : null;
    }

    private void ShowTitleOverlay()
    {
        if (canvas == null)
        {
            return;
        }

        if (canvas.transform.Find("TitleOverlay") != null)
        {
            return;
        }

        GameObject overlay = new GameObject("TitleOverlay", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        overlay.transform.SetParent(canvas.transform, false);
        overlay.transform.SetAsLastSibling();

        RectTransform overlayRect = overlay.GetComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        Image overlayImage = overlay.GetComponent<Image>();
        overlayImage.color = new Color(0.02f, 0.04f, 0.06f, 0.88f);
        overlayImage.raycastTarget = true;

        TMP_Text title = CreateText(overlay.transform, "TitleText", "DisaVenture", 76, FontStyles.Bold);
        SetRect(title.rectTransform, new Vector2(0.5f, 0.68f), new Vector2(860f, 120f));

        TMP_Text subtitle = CreateText(overlay.transform, "SubtitleText", "災害時の判断で、生き延びるための備えを学ぶ", 28, FontStyles.Normal);
        SetRect(subtitle.rectTransform, new Vector2(0.5f, 0.56f), new Vector2(980f, 72f));

        TMP_Text guide = CreateText(overlay.transform, "GuideText", "選択肢を選び、HP・水・物資の変化を確認しながら避難所生活まで進めます。", 22, FontStyles.Normal);
        SetRect(guide.rectTransform, new Vector2(0.5f, 0.45f), new Vector2(1040f, 72f));

        Button startButton = CreateButton(overlay.transform);
        SetRect(startButton.GetComponent<RectTransform>(), new Vector2(0.5f, 0.30f), new Vector2(360f, 82f));
        startButton.onClick.AddListener(() =>
        {
            PlayTitleStartSe();
            titleAccepted = true;
            overlay.SetActive(false);
        });
    }

    private static TMP_Text CreateText(Transform parent, string name, string value, int fontSize, FontStyles style)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        obj.transform.SetParent(parent, false);
        TMP_Text text = obj.GetComponent<TMP_Text>();
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.white;
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateButton(Transform parent)
    {
        GameObject obj = new GameObject("StartButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);

        Image image = obj.GetComponent<Image>();
        image.color = new Color(0.86f, 0.42f, 0.13f, PanelAlpha);

        Button button = obj.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1f, 0.58f, 0.22f, 0.92f);
        colors.pressedColor = new Color(0.62f, 0.27f, 0.08f, 0.95f);
        button.colors = colors;

        TMP_Text label = CreateText(obj.transform, "Label", "はじめる", 32, FontStyles.Bold);
        SetRect(label.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero);
        label.rectTransform.anchorMin = Vector2.zero;
        label.rectTransform.anchorMax = Vector2.one;
        label.rectTransform.offsetMin = Vector2.zero;
        label.rectTransform.offsetMax = Vector2.zero;
        return button;
    }

    private static void SetRect(RectTransform rect, Vector2 anchor, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = size;
    }

    private void PlayTitleStartSe()
    {
        if (audioLibrary == null || audioLibrary.titleStartSe == null)
        {
            return;
        }

        AudioSource seSource = EnsureSeSource();
        seSource.PlayOneShot(audioLibrary.titleStartSe, 1f);
    }
}
