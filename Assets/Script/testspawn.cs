using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testspawn : MonoBehaviour
{

    public GameObject prefab;
    public bool TEST = false;
    private void OnValidate()
    {
        if (TEST)
        {
            TEST = false;
            SpawnShape();
        }
    }

    void SpawnShape()
    {
        List<int> ilist = new ();
        Color[] colors = new[] { Color.blue, Color.yellow, Color.cyan };
        float xrad = Mathf.PI / 7f, yrad = Mathf.PI / 5f;
        float mult = 1.63f;
        Vector2 vec;
        for (int y = 0; y < 6; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                vec = new Vector2(Mathf.Sin(xrad * x) * mult, Mathf.Sin(yrad * y) * mult);
                int i = Mathf.RoundToInt(vec.x * vec.y);
                GameObject go = Instantiate(prefab, transform.position + new Vector3(x, y, 0), Quaternion.identity);
                go.GetComponent<SpriteRenderer>().color = colors[i];
                ilist.Add(i);
            }
            Debug.Log(string.Join(',', ilist));
            ilist = new();
        }
    }
}
