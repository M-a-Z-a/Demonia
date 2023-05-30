using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    ScreenFader mainFader;
    [SerializeField] AudioClip musicClip;
    [SerializeField] float musicVolume = 1f;
    Button[] buttons;
    GameObject b_continue, b_delete;
    private void Start()
    {
        buttons = GetComponentsInChildren<Button>();
        foreach (Button b in buttons)
        {
            if (b.gameObject.name == "Button(Continue)")
            { b_continue = b.gameObject; continue; }
            if (b.gameObject.name == "Button(Delete)")
            { b_delete = b.gameObject; continue; }
        }
        ScreenFader.GetScreenFader("main fader", out mainFader);
        UpdateSave();
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
    public void Action_Continue(int index)
    {
        SaveManager.LoadDataFromFile();
        Action_Start(index);
    }
    public void Action_Delete()
    {
        SaveManager.DeleteSave();
        UpdateSave();
    }
    public void Action_Exit()
    { GameManager.Exit_Game(); }

    public IEnumerator IStart(int index)
    {
        yield return mainFader.FadeTo(Color.white, 0.1f);
        yield return mainFader.FadeTo(Color.black, 0.5f);
        yield return GameManager.LoadSceneAsync(index);
        yield return mainFader.FadeTo(Color.clear, 4f);
    }

    float save_check_timer = 0;
    private void Update()
    {
        save_check_timer += Time.deltaTime;
        if (save_check_timer >= 1)
        { UpdateSave(); }
    }

    void UpdateSave()
    { SaveExists(SaveManager.SaveExists()); save_check_timer = 0; }

    void SaveExists(bool state)
    {
        b_continue.SetActive(state);
        b_delete.SetActive(state);
    }


}
