using UnityEngine;
using UnityEngine.UI;

public class SanityEffect : MonoBehaviour
{
    [Header("画面端のUI Image")]
    [SerializeField] private Image sanityImage;

    [SerializeField] private float maxSanity = 10f;
    private float currentSanity;

    private void Awake()
    {
        if (sanityImage == null)
        {
            GameObject effectImage = GameObject.Find("SanityEffectImage");
            if (effectImage != null)
            {
                sanityImage = effectImage.GetComponent<Image>();
            }
        }
    }

    private void Start()
    {
        currentSanity = maxSanity;

        if (sanityImage != null)
        {
            SetImageAlpha(255f);
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
        Color c = sanityImage.color;
        c.a = alpha / 255f;
        sanityImage.color = c;
    }

    public void DecreaseSanity(float amount)
    {
        currentSanity = Mathf.Clamp(currentSanity - amount, 0f, maxSanity);
        UpdateSanityEffect();
    }
}
