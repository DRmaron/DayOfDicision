using UnityEngine;
using UnityEngine.UI;

public class StatBarView : MonoBehaviour
{
    [System.Serializable]
    public class BarBinding
    {
        public Image fillImage;
        public SpriteRenderer spriteRenderer;
        public StatType statType;
        public float visibleThreshold;

        [System.NonSerialized] public bool initialized;
        [System.NonSerialized] public Vector3 initialScale;
        [System.NonSerialized] public Color initialColor;
    }

    public enum StatType
    {
        Hp,
        Hunger,
        San,
        Water,
        Hygiene,
        Trust,
        Coop,
        Supplies
    }

    [SerializeField] private GameManager gameManager;
    [SerializeField] private BarBinding[] bars = new BarBinding[0];

    public void Configure(BarBinding[] newBars)
    {
        bars = newBars ?? new BarBinding[0];
        Refresh();
    }

    private void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        Refresh();
    }

    public void SetGameManager(GameManager manager)
    {
        gameManager = manager;
        Refresh();
    }

    public void Refresh()
    {
        if (gameManager == null)
        {
            return;
        }

        for (int i = 0; i < bars.Length; i++)
        {
            BarBinding bar = bars[i];
            if (bar.fillImage == null && bar.spriteRenderer == null)
            {
                continue;
            }

            float ratio = GetRatio(bar.statType);
            ApplyRatio(bar, ratio);
        }
    }

    private static void ApplyRatio(BarBinding bar, float ratio)
    {
        if (!bar.initialized)
        {
            Transform targetTransform = bar.fillImage != null ? bar.fillImage.transform : bar.spriteRenderer.transform;
            bar.initialScale = targetTransform.localScale;
            bar.initialColor = bar.fillImage != null ? bar.fillImage.color : bar.spriteRenderer.color;
            bar.initialized = true;
        }

        if (bar.fillImage != null)
        {
            bar.fillImage.fillAmount = ratio;
            Color color = bar.fillImage.color;
            color.a = GetDisplayAlpha(bar, ratio);
            bar.fillImage.color = color;
            return;
        }

        if (bar.spriteRenderer == null)
        {
            return;
        }

        Color spriteColor = bar.spriteRenderer.color;
        spriteColor.a = GetDisplayAlpha(bar, ratio);
        bar.spriteRenderer.color = spriteColor;

        Transform spriteTransform = bar.spriteRenderer.transform;
        float scaleRatio = bar.visibleThreshold > 0f && ratio < bar.visibleThreshold ? 0.85f : 1f;
        spriteTransform.localScale = new Vector3(
            bar.initialScale.x * scaleRatio,
            bar.initialScale.y * scaleRatio,
            bar.initialScale.z);
    }

    private static float GetDisplayAlpha(BarBinding bar, float ratio)
    {
        if (bar.visibleThreshold > 0f)
        {
            return ratio >= bar.visibleThreshold ? bar.initialColor.a : 0.15f;
        }

        return Mathf.Lerp(0.25f, bar.initialColor.a, ratio);
    }

    private float GetRatio(StatType statType)
    {
        switch (statType)
        {
            case StatType.Hp:
                return SafeRatio(gameManager.Hp, gameManager.MaxHp);
            case StatType.Hunger:
                return SafeRatio(gameManager.Hunger, gameManager.MaxHunger);
            case StatType.San:
                return SafeRatio(gameManager.San, gameManager.MaxSan);
            case StatType.Water:
                return SafeRatio(gameManager.Water, gameManager.MaxWater);
            case StatType.Hygiene:
                return SafeRatio(gameManager.Hygiene, gameManager.MaxHygiene);
            case StatType.Trust:
                return SafeRatio(gameManager.Trust, gameManager.MaxTrust);
            case StatType.Coop:
                return SafeRatio(gameManager.Coop, gameManager.MaxCoop);
            case StatType.Supplies:
                return SafeRatio(gameManager.Supplies, Mathf.Max(1, 10));
            default:
                return 0f;
        }
    }

    private static float SafeRatio(int value, int max)
    {
        if (max <= 0)
        {
            return 0f;
        }

        return Mathf.Clamp01((float)value / max);
    }
}
