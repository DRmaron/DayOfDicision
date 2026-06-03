using UnityEngine;

[CreateAssetMenu(fileName = "DisaVentureAudioLibrary", menuName = "DisaVenture/Audio Library")]
public class DisaVentureAudioLibrary : ScriptableObject
{
    public AudioClip evacuationBgm;
    public AudioClip shelterBgm;
    public AudioClip choiceClickSe;
    public AudioClip nextClickSe;
    public AudioClip titleStartSe;

    public static DisaVentureAudioLibrary LoadDefault()
    {
        return Resources.Load<DisaVentureAudioLibrary>("DisaVentureAudioLibrary");
    }
}
