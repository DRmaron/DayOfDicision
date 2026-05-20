using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SanityEffect : MonoBehaviour
{
    [Header("画面端のUI Image")]
    [SerializeField] private Image sanityImage;

    [Header("SAN値の設定")]
    [SerializeField] private float maxSanity = 100f;
    private float currentSanity;

    
    void Start()
    {
        currentSanity = maxSanity;

        if (sanityImage == null)
        {
            Debug.LogError("sanityImageがインスペクターで設定されていません！");
        }
        else
        {
            // 最初は完全に透明（演出なし）にしておく
            SetImageAlpha(255f);
        }
    }

    void Update()
    {
        // テスト用：時間の経過でじわじわSAN値が減る（確認できたら消してください）
        if (currentSanity > 0)
        {
            currentSanity -= Time.deltaTime * 5f;
            UpdateSanityEffect();
        }
    }

    // SAN値の割合に応じて、画像の透明度を更新する
    public void UpdateSanityEffect()
    {
        if (sanityImage == null || sanityImage.material == null) return;

        // SAN値の割合（1＝正常、0＝狂気）
        float sanityPercent = currentSanity / maxSanity;
        float insanityPercent = 1f - sanityPercent;

        // SAN値が減るほど、VignetteSizeを大きくして中央まで暗闇を広げる
        // 正常時：0.5（画面外） ➔ 完全に発狂：2.5（中心近くまで真っ黒）
        float vSize = Mathf.Lerp(0.5f, 2.5f, insanityPercent);

        // マテリアルの値をリアルタイムに変更
        sanityImage.material.SetFloat("_VignetteSize", vSize);
    }

    // 画像のアルファ値（透明度）だけを書き換えるヘルパー関数
    private void SetImageAlpha(float alpha)
    {
        Color c = sanityImage.color;
        c.a = alpha;
        sanityImage.color = c;
    }

    // 外部からSAN値を減らすための関数（敵に遭遇した時などに呼ぶ）
    public void DecreaseSanity(float amount)
    {
        currentSanity = Mathf.Clamp(currentSanity - amount, 0f, maxSanity);
        UpdateSanityEffect();
    }
}
