using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMoverClasses : MonoBehaviour
{


    public class RaycastGroup
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

        public RaycastGroup(params Vector2[] points)
        {
            _points = points;
            _rcasts = new RaycastHit2D[_points.Length];
            _rhits = new();
        }

        public RaycastGroup(int count, Vector2 start, Vector2 end, LayerMask layerMask)
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
            for (int i = index; i < index+count; i++)
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




    public bool ChainRaycast(LayerMask layerMask, out RaycastHit2D rhit, params Ray2D[] rays)
    {
        rhit = default;
        for (int i = 0; i < rays.Length; i++)
        {
            rhit = Physics2D.Raycast(rays[i].origin, rays[i].direction, 1f, layerMask);
            
            if (rhit.collider)
            {
                if (i == rays.Length-1)
                { Debug.DrawRay(rays[i].origin, rays[i].direction, Color.green); return true; }
                Debug.DrawRay(rays[i].origin, rays[i].direction, Color.red);
                return false; 
            }
            Debug.DrawRay(rays[i].origin, rays[i].direction, Color.white);
        }
        return rhit.collider != null;
    }







}


