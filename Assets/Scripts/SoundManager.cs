using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public static AudioClip HoverTeleportTile, MakeItBig, MakeItLittle,
        OnSelectTeleportTile, SelectButton;
    static AudioSource AudioSource;

    private void Awake()
    {
        // ================== SINGLETON ===================================
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        // ================================================================
    }

    void Start()
    {
        HoverTeleportTile = Resources.Load<AudioClip>("HoverTeleportTile");
        OnSelectTeleportTile = Resources.Load<AudioClip>("OnSelectTeleportTile");
        MakeItBig = Resources.Load<AudioClip>("MakeItBig");
        MakeItLittle = Resources.Load<AudioClip>("MakeItLittle");
        SelectButton = Resources.Load<AudioClip>("SelectButton");

        AudioSource = GetComponent<AudioSource>();
    }

    public static void PlaySound(string clip)
    {
        switch (clip)
        {
            case "HoverTeleportTile":
                AudioSource.PlayOneShot(HoverTeleportTile);
                break;
            case "OnSelectTeleportTile":
                AudioSource.PlayOneShot(OnSelectTeleportTile);
                break;
            case "MakeItBig":
                AudioSource.PlayOneShot(MakeItBig);
                break;
            case "MakeItLittle":
                AudioSource.PlayOneShot(MakeItLittle);
                break;
            case "SelectButton":
                AudioSource.PlayOneShot(SelectButton);
                break;

        }
    }
}
