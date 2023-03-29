using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// utility classes
public static partial class Utility
{
    public class Dir4
    {
        public float left, right, up, down;
        public float x { get => right - left; }
        public float y { get => up - down; }

        public Dir4(float left = 0, float right = 0, float up = 0, float down = 0)
        { this.left = left; this.right = right; this.up = up; this.down = down; }

        public void SetAll(float value)
        { left = value; right = value; up = value; down = value; }
    }



    public class CurveSlider
    {
        float _easeIn, _easeOut, _start, _end, _rang, _xMin, _astart, _aend;
        public CurveSlider(float ease_in, float ease_out)
        { UpdateValues(ease_in, ease_out); }

        void UpdateValues(float ease_in, float ease_out)
        {
            _easeIn = Mathf.Clamp01(ease_in);
            _easeOut = Mathf.Clamp01(ease_out);

            float a1 = _rad90 - _rad90 * _easeIn;
            float a2 = _rad90 - _rad90 * _easeOut;


            _astart = Mathf.Sin(a1);
            _aend = Mathf.Sin(a2);
        }

        public float Evaluate(float a, float b, float t)
        { return Mathf.Lerp(a, b, Evaluate01(t)); }
        public float Evaluate01(float t)
        {
            t = Mathf.Clamp01(t);
            float t1 = EaseInOutSine01(t);

            float a1 = _astart * t;
            float a2 = 1f - _aend * (1f - t);

            return Mathf.Lerp(a1, a2, t1);
        }

        public void TESTVAL()
        { Debug.Log($"{_astart} {_aend}"); }

        IEnumerator<float> IGetLine(int count)
        {
            float div = Mathf.Max(count - 1, 1);
            int i; for (i = 0; i < count; i++)
            { yield return Evaluate01((float)i / div); }
        }

        public void DrawGizmo(Vector2 offset, Vector2 scale, int segments)
        {
            float div = Mathf.Max(segments - 1, 1);
            Vector2 lpos = new Vector2(offset.x, offset.y);
            Vector2 npos; float idiv;
            int i; for (i = 1; i < segments; i++)
            {
                idiv = (float)i / div;
                npos = new Vector2(offset.x + scale.x * idiv, offset.y + Evaluate01(idiv) * scale.y);
                Gizmos.DrawLine(lpos, npos);
                lpos = npos;
            }
        }
    }

    public class Curve
    {

        float _attack, _attackTime, _release, _releaseTime, _delta;
        public float attack { get => _attack; set => Mathf.Abs(value); }
        public float release { get => _release; set => Mathf.Abs(value); }
        public bool accelerating = false;
        public Curve(float attack, float release)
        {
            _attack = attack; _release = release;
        }

        public float Evaluate()
        {
            if (accelerating) return EvaluateAttack(_delta);
            return EvaluateRelease(_delta);
        }

        float EvaluateAttack(float d)
        {
            float tconst = -1f / (_attackTime / Mathf.Log(0.01f));
            return Mathf.Exp(_delta / tconst);
        }
        float EvaluateRelease(float d)
        {
            float tconst = -1f / (_releaseTime / Mathf.Log(0.01f));
            return Mathf.Exp(_delta / tconst);
        }

        IEnumerator IAttack()
        {
            float d = 0;
            while (d < 1f)
            {
                d += Time.deltaTime / _attackTime;
                EvaluateAttack(d);
                yield return null;
            }
        }
        IEnumerator IRelease()
        {
            float d = 0;
            while (d > 0)
            {
                d -= Time.deltaTime / _releaseTime;
                EvaluateAttack(d);
                yield return null;
            }
        }

    }
}
