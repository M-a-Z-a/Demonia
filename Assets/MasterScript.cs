using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MasterScript : MonoBehaviour
{

    public InputManager.InputKeyCode left, right, up, down;
    InputManager.InputDirection directionX, directionY;
    public InputManager.InputVector2 inputVector;

    // Start is called before the first frame update
    private void Awake()
    {
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

    }

    // Update is called once per frame
    void Update()
    {
        InputManager.UpdateInputs();
    }
}
