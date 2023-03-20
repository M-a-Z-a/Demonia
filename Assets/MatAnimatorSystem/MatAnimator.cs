using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MatAnimator : MonoBehaviour
{

    Dictionary<string,MatAnimation> anims;
    Material mat;
    public MatAnimation currentAnimation { get => anims[curAnim]; }
    string curAnim;
    float currentAnimTime = 0;

    public void Start()
    {
        SetMaterial();
    }

    public void SetAnimation(string name)
    {

    }

    void UpdateAnimation()
    {
        
    }

    public void SetMaterial()
    { mat = new Material(Shader.Find("Specular")); }
    
}
