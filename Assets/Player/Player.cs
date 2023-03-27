using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static InputManagerClasses;
using static Utility;

public class Player : PlayerController
{

    public static Player instance { get; protected set; }
    public static Transform pTransform { get; protected set; }


    Transform cameraTarget;
    InputVector2 inputVector;
    InputKey left, right, jump;


    protected override void OnValidate()
    {
        base.OnValidate();
    }

    protected override void Awake()
    {
        base.Awake();
        instance = this;

        pTransform = transform;
        cameraTarget = transform.Find("CameraTarget");
    }



    protected override void Start()
    {
        base.Start();
        inputVector = InputManager.GetInputVector<InputVector2>("direction");
        left = inputVector.inputX.negative;
        right = inputVector.inputX.positive;
        jump = InputManager.SetInputKey("jump", KeyCode.X);
    }


    protected override void Update()
    {
        base.Update();

        Vector2 dir = (Vector2)inputVector;


    }




}
