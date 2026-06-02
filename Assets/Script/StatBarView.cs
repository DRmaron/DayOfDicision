using UnityEngine;
using UnityEngine.UI;

public class StatBarView : MonoBehaviour
{
    [System.Serializable]
    public class BarBinding
    {
        public Image fillImage;
        public StatType statType;
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
            if (bar.fillImage == null)
            {
                continue;
            }

            float ratio = GetRatio(bar.statType);
            bar.fillImage.fillAmount = ratio;
        }
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
