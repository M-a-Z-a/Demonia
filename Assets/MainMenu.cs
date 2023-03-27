using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    ScreenFader mainFader;
    private void Start()
    {
        ScreenFader.GetScreenFader("main fader", out mainFader);
    }

    public void LoadScene(int index)
    { GameManager.LoadSceneAsync(index); }
    

    public void ActivateObject(GameObject go)
    { go.SetActive(true); }
    public void DeactivateObject(GameObject go)
    { go.SetActive(false); }

    public void ActionStart(int index)
    { GameManager.instance.StartCoroutine(IStart(index)); }

    public IEnumerator IStart(int index)
    {
        yield return mainFader.FadeTo(Color.white, 0.1f);
        yield return mainFader.FadeTo(Color.black, 0.5f);
        yield return GameManager.LoadSceneAsync(index);
        yield return mainFader.FadeTo(Color.clear, 4f);
    }

}
