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

    public InputManager.InputKeyCode left, right, up, down;
    InputManager.InputDirection directionX, directionY;
    public InputManager.InputVector2 inputVector;
    Transform startpoint, checkpoint;
    public Vector2 activeCheckpoint { get => checkpoint.position; set => SetActiveCheckpoint(value); }
    Vector3 orig_spPos, orig_cpPos;
    public static Transform Checkpoint { get => instance.checkpoint; } 

    static ScreenFader mainFader;
    public static float loadingProgress { get; protected set; }

    static List<MonoBehaviour> persistentMonos = new();

    static Dictionary<string, int> sceneIndexes = new()
    {
        { "MasterScene", 0 },
        { "LoadingScreen", 1 },
        { "MainMenu", 2 }
    };


    public static void SetActiveCheckpoint(Vector2? point = null)
    {
        if (point == null)
        { instance.checkpoint.position = instance.startpoint.position; return; }
        instance.checkpoint.position = (Vector2)point;
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
        orig_cpPos = checkpoint.position;
        orig_spPos = startpoint.position;
        activeCheckpoint = startpoint.position;
        ScreenFader.GetScreenFader("main fader", out mainFader);
        //SceneManager.LoadScene(sceneIndexes["MainMenu"]);
        //SceneManager.LoadScene(sceneIndexes["MainMenu"]);
        LoadScene(sceneIndexes["MainMenu"]);
        //TimeControl.SetTimeScaleFadeForTime(0f, 2f, 0f, 1f);
    }

    public static bool LoadScene(int index)
    {
        Scene scene = SceneManager.GetSceneByBuildIndex(index);
        if (scene == null) return false;
        onSceneLoadInit.Invoke(scene);
        SceneManager.LoadScene(index);
        onSceneLoadFinished.Invoke(scene);
        return true;
    }
    

    // Update is called once per frame
    void Update()
    {
        InputManager.UpdateInputs();
        if (Input.GetKeyDown(KeyCode.Escape))
        { Exit_Game(); }
        if (Input.GetKeyDown(KeyCode.F4))
        { Reset_Checkpoint(); Reset_Game_Fade(); }
        if (Input.GetKeyDown(KeyCode.F5))
        { Reset_Game_Fade(); }
    }
    public static void Reset_Checkpoint()
    {
        instance.checkpoint.position = instance.orig_cpPos;
    }
    public static void Reset_Game()
    {
        LoadScene(sceneIndexes["MainMenu"]);
        //TimeControl.SetTimeScaleFadeForTime(0f, 2f, 0, 1f);
        //TimeControl.SetTimeScaleFade(0f, 1f);
    }
    public static void Reset_Game_Fade()
    {
        instance.StartCoroutine(instance.IResetFade());
    }
    public static void Exit_Game()
    { Application.Quit(); }
    
    public static Coroutine LoadSceneAsync(int index)
    { return instance.StartCoroutine(ILoadSceneAsync(index)); }


    static IEnumerator ILoadSceneAsync(int index)
    {
        Scene scene = SceneManager.GetSceneByBuildIndex(index);
        onSceneLoadInit.Invoke(scene);
        AsyncOperation operation = SceneManager.LoadSceneAsync(index);
        operation.allowSceneActivation = true;

        while (!operation.isDone)
        {
            loadingProgress = operation.progress;
            if (operation.progress >= 0.9)
            { operation.allowSceneActivation = true; }
            yield return new WaitForEndOfFrame();
        }
        onSceneLoadFinished.Invoke(scene);
    }


    IEnumerator IResetFade()
    {
        TimeControl.SetTimeScaleFade(0.1f, 0.1f);
        ScreenFader.GetScreenFader("main fader", out mainFader);
        yield return mainFader.FadeTo(Color.white, 0.15f);
        Reset_Game();
        TimeControl.SetTimeScaleFade(1f, 0.5f);
        yield return mainFader.FadeTo(new Color(1f, 1f, 1f, 0f), 0.5f);
    }

}



