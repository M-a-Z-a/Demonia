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
        down = InputManager.SetInputKey("dowb", KeyCode.DownArrow);

        directionX = InputManager.SetInputDirection("x", right, left);
        directionY = InputManager.SetInputDirection("y", up, down);

        inputVector = InputManager.SetInputVector2("direction", directionX, directionY);
    }
    void Start()
    {
        ScreenFader.GetScreenFader("main fader", out mainFader);
        SceneManager.LoadScene(sceneIndexes["MainMenu"]);

        TimeControl.SetTimeScaleFadeForTime(0.25f, 1f, 1f, 4f);
    }


    // Update is called once per frame
    void Update()
    {
        InputManager.UpdateInputs();
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


}



