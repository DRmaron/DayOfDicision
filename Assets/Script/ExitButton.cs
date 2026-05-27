using UnityEngine;

//ボタンから呼び出す用のゲーム終了処理
//ゲーム終了ボタンに付けて使用

public class ExitButton : MonoBehaviour
{
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
