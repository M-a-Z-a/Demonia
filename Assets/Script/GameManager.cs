using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; protected set; }

    public InputManager.InputKeyCode left, right, up, down;
    InputManager.InputDirection directionX, directionY;
    public InputManager.InputVector2 inputVector;

    static ScreenFader mainFader;
    public static float loadingProgress { get; protected set; }

    static List<MonoBehaviour> persistentMonos = new();

    static Dictionary<string, int> sceneIndexes = new()
    {
        { "MasterScene", 0 },
        { "LoadingScreen", 1},
        { "MainMenu", 2}
    };



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
        ScreenFader.GetScreenFader("main fader", out mainFader);
        //SceneManager.LoadScene(sceneIndexes["MainMenu"]);
        SceneManager.LoadScene(3);

        //TimeControl.SetTimeScaleFadeForTime(0.25f, 1f, 1f, 4f);
    }


    // Update is called once per frame
    void Update()
    {
        InputManager.UpdateInputs();
        if (Input.GetKeyDown(KeyCode.F4))
        { Reset_Game_Fade(); }
        if (Input.GetKeyDown(KeyCode.F5))
        { Checkpoint.activeCheckpoint = null; Reset_Game_Fade(); }
    }

    public static void Reset_Game()
    {
        SceneManager.LoadScene(3);
    }
    public static void Reset_Game_Fade()
    {
        instance.StartCoroutine(instance.IResetFade());
    }
    


    public static Coroutine LoadSceneAsync(int index)
    {
        return instance.StartCoroutine(ILoadSceneAsync(index));
    }


    static IEnumerator ILoadSceneAsync(int index)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(index);
        operation.allowSceneActivation = true;

        while (!operation.isDone)
        {
            loadingProgress = operation.progress;
            if (operation.progress >= 0.9)
            { operation.allowSceneActivation = true; }
            yield return null;
        }
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



