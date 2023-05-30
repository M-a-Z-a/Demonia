using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public static SceneSwitcher instance { get; protected set; }
    private void Awake()
    { instance = this; }

    public float loadDelay = 0;
    public string nextSceneName = "";
    public int nextSceneBuildIndex = 0;
    public void LoadNextSceneBuildIndex()
    { ChangeScene(nextSceneBuildIndex); }
    public void LoadNextSceneName()
    { ChangeScene(nextSceneName); }
    public void ChangeScene(int sceneBuildIndex)
    {
        if (loadDelay > 0)
        { StartCoroutine(IChangeScene(sceneBuildIndex, loadDelay)); return; }
        SceneManager.LoadScene(sceneBuildIndex);
    }
    public bool ChangeScene(string sceneName)
    {
        int scount = SceneManager.sceneCountInBuildSettings;
        Scene scene;
        for (int i = 0; i < scount; i++)
        {
            scene = SceneManager.GetSceneByBuildIndex(i);
            if (scene.name == sceneName)
            {
                ChangeScene(i);
                return true;
            }
        }
        return false;
    }

    IEnumerator IChangeScene(int sceneBuildindex, float load_delay)
    {
        yield return new WaitForSecondsRealtime(load_delay);
        SceneManager.LoadScene(sceneBuildindex);
        //ChangeScene(sceneBuildindex);
    }

}
