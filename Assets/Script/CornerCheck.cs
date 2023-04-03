using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CornerCheck : MonoBehaviour
{

    public Vector2 rPos_a = Vector2.zero, rPos_b = Vector2.zero;
    public Vector2 rDir_a = Vector2.one, rDir_b = Vector2.one;
    //public Ray2D aRay, bRay;
    public LayerMask layerMask;

    private void Start()
    {
        
    }

    private void OnDrawGizmos()
    {
        Vector2 tpos = transform.position;
        //Gizmos.DrawRay(tpos + rPos_a, rDir_a);
        //Gizmos.DrawRay(tpos + rPos_b, rDir_b);
    }

    private void Update()
    {
        Vector2 tpos = transform.position;
        RaycastHit2D arhit = Physics2D.Raycast(tpos + rPos_a, rDir_a, 2f, layerMask);
        RaycastHit2D brhit = Physics2D.Raycast(tpos + rPos_b, rDir_b, 2f, layerMask);

        Debug.DrawRay(tpos + rPos_a, rDir_a, Color.yellow);
        Debug.DrawRay(tpos + rPos_b, rDir_b, Color.yellow);

        if (arhit.collider && brhit.collider)
        {

            Vector2 avec = arhit.normal; avec = new Vector2(avec.y, -avec.x);
            Vector2 bvec = brhit.normal; bvec = new Vector2(-avec.y, avec.x);

            float scal = Scalar(avec, bvec);
            /*
            if (scal < 0)
            {
                avec = -avec;
                scal = Scalar(avec, bvec);
                if (scal < 0)
                { bvec = -bvec; }
            }
            */
            Debug.DrawRay(arhit.point, avec, Color.red);
            Debug.DrawRay(brhit.point, bvec, Color.green);


            Vector2 diff = arhit.point - brhit.point;
            float mag = diff.magnitude;
            
            float a = Vector2.Angle(avec, diff);
            float b = Vector2.Angle(bvec, diff);
            float c = 180 - (a + b);

            //float ad = a /
        }
    }

    float Scalar(Vector2 a, Vector2 b)
    { return a.x * b.y + a.y * b.x; }

    public float Vector2Angle(Vector2 a, Vector2 b)
    {
        float add = a.x * b.x + a.y * b.y;
        float _a = Mathf.Sqrt(a.x * a.x + a.y * a.y);
        float _b = Mathf.Sqrt(b.x * b.x + b.y * b.y);
        return add / (_a * _b);
    }
}
