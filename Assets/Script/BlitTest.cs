using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlitTest : MonoBehaviour
{

    [SerializeField] List<Material> mats;
    RenderTexture tempSrc, tempDst;
    bool switcheroo;
    [SerializeField] RenderTexture m_rt;
    RenderTexture rtBuffer;

    [ExecuteInEditMode]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {

        for (int i = 0; i < mats.Count; i++)
        {
            Graphics.Blit(source, rtBuffer);
            Graphics.Blit(rtBuffer, destination, mats[i]);
        }
        /*
        tempSrc = RenderTexture.GetTemporary(source.width, source.height, source.depth, source.format);
        tempDst = RenderTexture.GetTemporary(source.width, source.height, source.depth, source.format);
        switcheroo = false;

        Debug.Log("renderrr");

        for (int i = 0; i < mats.Count; i++)
        {
            switcheroo = !switcheroo;
            if (switcheroo)
            { Graphics.Blit(tempSrc, tempDst, mats[i]); continue; }
            Graphics.Blit(tempDst, tempSrc, mats[i]);
        }

        if (switcheroo)
        { Graphics.Blit(tempSrc, destination); }
        else
        { Graphics.Blit(tempDst, destination); }

        RenderTexture.ReleaseTemporary(tempSrc);
        RenderTexture.ReleaseTemporary(tempDst);
        */
    }



}
