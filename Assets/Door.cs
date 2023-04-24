using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{

    Animation anim;

    private void Awake()
    {
        anim = GetComponent<Animation>();
    }

    public void Open()
    {
        anim.Play("Door01_Open");
    }
    public void Close()
    {
        anim.Play("Door01_Close");
    }
}
