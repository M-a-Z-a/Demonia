using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoopPattern : MonoBehaviour
{
    [SerializeField] float defaultAngle = 0, angleAdd = 10f;
    [SerializeField] int count = 1;
    [SerializeField] bool alternateSide = false;
    [SerializeField] List<float> angles;

#if UNITY_EDITOR
    public bool draw_gizmos = true;
    public float angle_offset = 90f, ray_length = 5f;

    private void OnDrawGizmosSelected()
    {
        Vector2 a;
        foreach (float v in angles)
        {
            Gizmos.color = Color.green;
            a = Utility.AngleToVector2(angle_offset + v) * ray_length;
            Gizmos.DrawRay(transform.position, new Vector3(a.x, a.y, 0));
        }
    }

#endif

    public int Count { get => angles.Count; }
    private void OnValidate()
    {
        GenerateAngles();
    }


    public void GenerateAngles(int count = 1, float default_angle = 0, float add_angle = 0, bool alternate_side = false)
    {
        this.count = count;
        defaultAngle = 0;
        angleAdd = add_angle;
        alternateSide = alternate_side;

        GenerateAngles();
    }
    public void GenerateAngles()
    {
        angles = new();
        float l_angle = 0;
        angles.Add(defaultAngle);
        if (alternateSide)
        {
            bool alt = false; 
            for (int i = 1; i < count; i++)
            {
                if (alt)
                { angles.Add(defaultAngle - l_angle); }
                else
                {
                    l_angle += angleAdd;
                    angles.Add(defaultAngle + l_angle);
                }
                alt = !alt;
            }
        }
        else
        {
            for (int i = 1; i < count; i++)
            {
                l_angle += angleAdd;
                angles.Add(defaultAngle + l_angle);
            }
        }
    }

    int enumIndex = 0;
    public void ResetEnumerator()
    { enumIndex = 0; }
    public float GetCurrent()
    { return angles[enumIndex]; }
    public float GetNext()
    { return angles[enumIndex = (enumIndex+1) % Count]; }
    public float GetAt(int index)
    { return angles[0]; }
    
    public IEnumerator<float> GetEnumerator()
    {
        for (int i = 0; i < angles.Count; i++)
        { yield return angles[i]; }
    }

    public float this[int index]
    { get => angles[index]; }
}
