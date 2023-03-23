using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMoverClasses : MonoBehaviour
{

    public abstract class RaycastGroup2D
    {

        public RaycastHit2D[] rays;
        protected RaycastHit2D rMin, rMax, rhit;
        public Vector2 _origin, _offset;
        public int rayCount;
        public int hits;
        public int shortIndex;
        public RaycastHit2D shortHit { get => rays[shortIndex]; }

        protected float hitDistance, hitBaseDistance;

        public abstract int Cast(Vector2 offset, Rect rect, Vector2 direction, float distance, float base_distance, LayerMask layerMask);
    }

    public class RaycastGroup2DVertical : RaycastGroup2D
    {


        public RaycastGroup2DVertical(int rayCount)
        {
            rays = new RaycastHit2D[rayCount];
            this.rayCount = rayCount;
        }


        public override int Cast(Vector2 offset, Rect rect, Vector2 direction, float distance, float base_distance, LayerMask layerMask)
        {

            Vector2 pos = offset + _origin;

            float wlen = CastSideRays(pos, rect.width, out Vector2 vecLeft, out Vector2 vecRight, layerMask);
            float jump = wlen / (rays.Length - 1);
            float dist = base_distance + distance;

            shortIndex = 0;
            hits = 0;
            hitDistance = dist;
            hitBaseDistance = hitDistance - base_distance;

            Vector2 rpos;
            for (int i = 0; i < rays.Length; i++)
            {
                rpos = new Vector2(pos.x + vecLeft.x + jump * i, pos.y + vecLeft.y);
                rhit = Physics2D.Raycast(rpos, direction, dist, layerMask);
                if (rhit.collider)
                {
                    shortIndex = i;
                    hits++;
                    hitDistance = rhit.distance;
                    hitBaseDistance = hitDistance - base_distance;
                }
                rays[i] = rhit;
            }

            return hits;
        }


        float CastSideRays(Vector2 pos, float width, out Vector2 vecLeft, out Vector2 vecRight, LayerMask layerMask)
        {
            vecLeft = Vector2.zero; vecRight = Vector2.zero;
            float w = width / 2;
            rMin = Physics2D.Raycast(pos, Vector2.left, w, layerMask);
            rMax = Physics2D.Raycast(pos, Vector2.right, w, layerMask);

            if (rMin.collider)
            { vecLeft = rMin.point; }
            else
            { vecLeft = new Vector2(pos.x - w, pos.y); vecLeft.x += 0.02f; }

            if (rMax.collider)
            { vecRight = rMax.point; vecRight.x -= 0.02f; }
            else
            { vecLeft = new Vector2(pos.x + w, pos.y); }

            return vecRight.x - vecLeft.x;
        }

    }






}




























/*
public class RaycastGroup2
{
    public LayerMask layerMask;
    Vector2[] _points;
    RaycastHit2D[] _rcasts;
    List<RaycastHit2D> _rhits;

    float _sdist, _sbdist; int _sindex, _shits;
    public int hitCount { get => _shits; }
    public float shortDistance { get => _sdist; }
    public float shortBaseDistance { get => _sbdist; }
    public int shortIndex { get => _sindex; }
    public RaycastHit2D shortHit { get => _rcasts[_sindex]; }
    public RaycastHit2D[] casts { get => _rcasts; }
    public List<RaycastHit2D> hits { get => _rhits; }

    public RaycastGroup2(params Vector2[] points)
    {
        _points = points;
        _rcasts = new RaycastHit2D[_points.Length];
        _rhits = new();
    }

    public RaycastGroup2(int count, Vector2 start, Vector2 end, LayerMask layerMask)
    {
        this.layerMask = layerMask;
        count = count < 2 ? 2 : count;
        _points = new Vector2[count];
        _rcasts = new RaycastHit2D[count];
        _rhits = new();
        Vector2 vdiff = end - start;
        Vector2 vjump = vdiff / (count - 1);
        for (int i = 0; i < count; i++)
        { _points[i] = start + vjump * i; }
    }

    public int Cast(Vector2 offset, Vector2 direction, float distance, float base_distance = 0f)
    { return CastRange(0, _points.Length, offset, direction, distance, base_distance); }
    public int CastRange(int index, int count, Vector2 offset, Vector2 direction, float distance, float base_distance = 0f)
    {
        _shits = 0; _sindex = 0;
        _sdist = distance + base_distance;
        _sbdist = _sdist - base_distance;
        _rhits = new();
        for (int i = index; i < index + count; i++)
        {
            float d = distance + base_distance;
            RaycastHit2D rhit = Physics2D.Raycast(offset + _points[i], direction, d, layerMask);
            Color d_color = Color.green;
            if (rhit.collider)
            {
                _shits++;
                _rhits.Add(_rcasts[i]);
                d_color = rhit.distance < base_distance ? Color.red : Color.yellow;
                d = rhit.distance;
                if (d < _sdist)
                {
                    _sdist = rhit.distance; _sindex = i;
                    _sbdist = rhit.distance - base_distance;
                }
            }
            _rcasts[i] = rhit;
            Debug.DrawRay(offset + _points[i], direction * d, d_color);
        }
        return _shits;
    }

    public IEnumerator<RaycastHit2D> GetEnumerator()
    {
        for (int i = 0; i < _rhits.Count; i++)
        {
            yield return _rhits[i];
        }
    }


}

*/