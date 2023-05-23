using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    ScreenFader mainFader;
    [SerializeField] AudioClip musicClip;
    [SerializeField] float musicVolume = 1f;
    private void Start()
    {
        ScreenFader.GetScreenFader("main fader", out mainFader);
    }

    private void OnEnable()
    {
        if (musicClip == null) AudioListenerControl.Music_Stop();
        else
        { 
            AudioListenerControl.Music_Set(musicClip);
            AudioListenerControl.Music_Play(musicVolume, 0f);
        }
    }

    public void LoadScene(int index)
    { GameManager.LoadSceneAsync(index); }

    public void ActivateObject(GameObject go)
    { go.SetActive(true); }
    public void DeactivateObject(GameObject go)
    { go.SetActive(false); }

    public void Action_Start(int index)
    { GameManager.instance.StartCoroutine(IStart(index)); }
    public void Action_Exit()
    { GameManager.Exit_Game(); }

    public IEnumerator IStart(int index)
    {
        yield return mainFader.FadeTo(Color.white, 0.1f);
        yield return mainFader.FadeTo(Color.black, 0.5f);
        yield return GameManager.LoadSceneAsync(index);
        yield return mainFader.FadeTo(Color.clear, 4f);
    }


}
