using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Area : MonoBehaviour
{
    static Area _activeArea;
    public static Area activeArea { get => _activeArea; }

    private void Start()
    {
        if (ScreenFader.GetScreenFader("main fader", out ScreenFader sfade))
        {
            sfade.SetColor(Color.black);
            sfade.Pause(1f);
            sfade.FadeTo(Color.white, 0.5f);
            sfade.FadeTo(new Color(1, 1, 1, 0f), 1f);
        }
    }

}
