using UnityEngine;
using UnityEngine.UI;

public class SanityEffect : MonoBehaviour
{
    [Header("画面端のUI Image")]
    [SerializeField] private Image sanityImage;
    [SerializeField] private CanvasGroup sanityCanvasGroup;

    [SerializeField] private float maxSanity = 10f;
    [SerializeField] private float baseAlpha = 0.5f;
    private float currentSanity;

    private void Awake()
    {
        if (sanityImage == null)
        {
            GameObject effectImage = GameObject.Find("SanityEffectImage");
            if (effectImage != null)
            {
                sanityImage = effectImage.GetComponent<Image>();
                sanityCanvasGroup = effectImage.GetComponent<CanvasGroup>();
                if (sanityCanvasGroup == null)
                {
                    sanityCanvasGroup = effectImage.AddComponent<CanvasGroup>();
                }
            }
        }

        if (sanityImage != null)
        {
            sanityCanvasGroup = sanityImage.GetComponent<CanvasGroup>();
            if (sanityCanvasGroup == null)
            {
                sanityCanvasGroup = sanityImage.gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    private void Start()
    {
        currentSanity = maxSanity;

        if (sanityImage != null)
        {
            ApplyMaterialAlpha(baseAlpha);
            UpdateSanityEffect();
        }
    }

    public void SyncFromGameManager(GameManager gameManager)
    {
        if (gameManager == null)
        {
            return;
        }

        maxSanity = Mathf.Max(1, gameManager.MaxSan);
        currentSanity = gameManager.San;
        UpdateSanityEffect();
    }

    public void UpdateSanityEffect()
    {
        if (sanityImage == null || sanityImage.material == null)
        {
            return;
        }

        float sanityPercent = currentSanity / maxSanity;
        float insanityPercent = 1f - sanityPercent;
        float vSize = Mathf.Lerp(0.5f, 2.5f, insanityPercent);
        sanityImage.material.SetFloat("_VignetteSize", vSize);
    }

    private void SetImageAlpha(float alpha)
    {
        ApplyMaterialAlpha(alpha);
    }

    private void ApplyMaterialAlpha(float alpha)
    {
        float clamped = Mathf.Clamp01(alpha);

        if (sanityImage != null)
        {
            Material material = sanityImage.material;
            if (material != null && material.HasProperty("_Color"))
            {
                Color tint = material.GetColor("_Color");
                tint.a = clamped;
                material.SetColor("_Color", tint);
            }

            Color c = sanityImage.color;
            c.a = clamped;
            sanityImage.color = c;
        }

        if (sanityCanvasGroup != null)
        {
            sanityCanvasGroup.alpha = clamped;
        }
    }

    public void DecreaseSanity(float amount)
    {
        currentSanity = Mathf.Clamp(currentSanity - amount, 0f, maxSanity);
        UpdateSanityEffect();
    }
}
