using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; protected set; }
    public static UnityEvent<Scene> onSceneLoadInit = new(), onSceneLoadFinished = new();
    public enum GameRunState { None, Menu, Loading, Running }
    public static GameRunState gameRunState = 0;
    public enum ResetGameMode { Total, Menu, ToCheckpoint, ToSavepoint }
    public static bool gameIsRunning { get => (int)gameRunState >= 2; }

    public InputManager.InputKeyCode left, right, up, down;
    InputManager.InputDirection directionX, directionY;
    public InputManager.InputVector2 inputVector;
    Transform startpoint, checkpoint, savepoint;
    public Vector2 activeCheckpoint { get => checkpoint.position; set => SetActiveCheckpoint(value); }
    Vector3 orig_spPos, orig_cpPos;
    public static Transform Checkpoint { get => instance.checkpoint; } 
    public static Transform Savepoint { get => instance.savepoint; }

    static ScreenFader mainFader;
    public static float loadingProgress { get; protected set; }

    static Dictionary<string, int> sceneIndexes = new()
    {
        { "MasterScene", 0 },
        { "LoadingScreen", 1 },
        { "MainMenu", 2 }
    };


    public static void SetActiveCheckpoint(Vector3? point = null)
    {
        if (point == null)
        { instance.checkpoint.position = instance.savepoint.position; return; }
        instance.checkpoint.position = (Vector3)point;
    }
    public static void SetActiveSavepoint(Vector3? point = null)
    {
        if (point != null)
        { instance.savepoint.position = (Vector3)point; }
        SaveManager.SetVector("savepoint", instance.savepoint.position);
    }
    public static void UpdateSavepoint()
    {
        if (SaveManager.GetVector("savepoint", out Vector3 pos))
        { instance.savepoint.position = pos; }
        else
        { instance.savepoint.position = instance.startpoint.position; }
    }

    

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        instance = this;

        left = InputManager.SetInputKey("left", KeyCode.LeftArrow);
        right = InputManager.SetInputKey("right", KeyCode.RightArrow);
        up = InputManager.SetInputKey("up", KeyCode.UpArrow);
        down = InputManager.SetInputKey("down", KeyCode.DownArrow);

        directionX = InputManager.SetInputDirection("x", right, left);
        directionY = InputManager.SetInputDirection("y", up, down);

        inputVector = InputManager.SetInputVector2("direction", directionX, directionY);

    }
    void Start()
    {
        checkpoint = transform.Find("Checkpoint");
        startpoint = transform.Find("Startpoint");
        savepoint = transform.Find("Savepoint");
        orig_cpPos = checkpoint.position;
        orig_spPos = startpoint.position;
        activeCheckpoint = startpoint.position;
        ScreenFader.GetScreenFader("main fader", out mainFader);
        //SceneManager.LoadScene(sceneIndexes["MainMenu"]);
        LoadScene(sceneIndexes["MainMenu"]);
        //TimeControl.SetTimeScaleFadeForTime(0f, 2f, 0f, 1f);
    }

    public static bool LoadScene(int index)
    {
        if (index < 3) HUD.instance.gameObject.SetActive(false);
        Scene scene = SceneManager.GetSceneByBuildIndex(index);
        if (scene == null) return false;
        onSceneLoadInit.Invoke(scene);
        SetRunStateFromSceneIndex(index);
        SceneManager.LoadScene(index);
        onSceneLoadFinished.Invoke(scene);
        if (index > 2) HUD.instance.gameObject.SetActive(true);
        return true;
    }

    // Update is called once per frame
    void Update()
    {
        InputManager.UpdateInputs();
        if (Input.GetKeyDown(KeyCode.Escape))
        { Reset_Menu(); }
        if (Input.GetKeyDown(KeyCode.F4))
        { Reset_Checkpoint(); Reset_Game_Fade(ResetGameMode.ToSavepoint); }
        if (Input.GetKeyDown(KeyCode.F5))
        { Reset_Game_Fade(ResetGameMode.ToCheckpoint); }
    }
    public static void Reset_Checkpoint()
    {
        instance.checkpoint.position = instance.orig_cpPos;
    }
    public static void Reset_Menu()
    {
        LoadScene(sceneIndexes["MainMenu"]);
    }
    public static void Reset_Game(ResetGameMode resetMode)
    {
        Debug.Log($"Reset_Game_Fade({resetMode})");
        switch (resetMode) 
        {
            case ResetGameMode.ToCheckpoint: LoadScene(3); break;
            case ResetGameMode.ToSavepoint:
                SaveManager.LoadDataFromFile();
                SetActiveCheckpoint();
                LoadScene(3);
                break;
            case ResetGameMode.Menu:
            default: LoadScene(sceneIndexes["MainMenu"]); break;
        }
    }
    public static void Reset_Game_Fade(ResetGameMode resetMode)
    {
        Debug.Log($"Reset_Game_Fade({resetMode})");
        instance.StartCoroutine(instance.IResetFade(resetMode));
    }
    public static void Exit_Game()
    { Application.Quit(); }
    
    public static Coroutine LoadSceneAsync(int index)
    { return instance.StartCoroutine(ILoadSceneAsync(index)); }


    static IEnumerator ILoadSceneAsync(int index)
    {
        if (index < 3) HUD.instance.gameObject.SetActive(false);
        Scene scene = SceneManager.GetSceneByBuildIndex(index);
        onSceneLoadInit.Invoke(scene);
        gameRunState = GameRunState.Loading;
        AsyncOperation operation = SceneManager.LoadSceneAsync(index);
        operation.allowSceneActivation = true;

        while (!operation.isDone)
        {
            loadingProgress = operation.progress;
            if (operation.progress >= 0.9)
            {
                SetRunStateFromSceneIndex(index);
                operation.allowSceneActivation = true; 
            }
            yield return new WaitForEndOfFrame();
        }
        onSceneLoadFinished.Invoke(scene);
        if (index > 2) HUD.instance.gameObject.SetActive(true);
    }


    IEnumerator IResetFade(ResetGameMode resetMode)
    {
        TimeControl.SetTimeScaleFade(0.1f, 0.1f);
        ScreenFader.GetScreenFader("main fader", out mainFader);
        yield return mainFader.FadeTo(Color.white, 0.15f);
        Reset_Game(resetMode);
        TimeControl.SetTimeScaleFade(1f, 0.5f);
        yield return mainFader.FadeTo(new Color(1f, 1f, 1f, 0f), 0.5f);
    }

    static void SetRunStateFromSceneIndex(int index)
    { gameRunState = GetRunStateFromSceneIndex(index); }
    static GameRunState GetRunStateFromSceneIndex(int index)
    {
        if (index < 0) return 0;
        switch(index)
        {
            case 0: return GameRunState.None;
            case 1: return GameRunState.Menu;
            case 2: return GameRunState.Loading;
            default: return GameRunState.Running;
        }
    }


}



