using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class RuleTileGenerator_Window : EditorWindow
{


    static Dictionary<Color, int> RTColorRules = new()
    {
        { new Color(1, 0, 0, 1), 2 }, // This
        { new Color(0, 1, 0, 1), 1 }, // Not this
        { new Color(0, 0, 0, 0), 0 }, // Fixed
        { new Color(0, 0, 0, 0), 0 }, // Rotated
        { new Color(0, 0, 0, 0), 0 }, // MirrorX
        { new Color(0, 0, 0, 0), 0 }, // MirrorY
        { new Color(0, 0, 0, 0), 0 }  // MirrorXY
    };


    RuleTile activeRuleTile;

    [MenuItem("Window/2D/Rule Tile Generator")]
    public static void ShowWindow()
    {
        GetWindow<RuleTileGenerator_Window>("Rule Tile Generator");
    }

    private void OnSelectionChange()
    {
        if (Selection.activeObject is RuleTile)
        {
            activeRuleTile = (RuleTile)Selection.activeObject;
        }
        Repaint();
    }

    private void OnGUI()
    {
        
        if (activeRuleTile)
        {
            /*
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save", GUILayout.Width(128)))
            { ShowSaveWindow(); return; }
            if (GUILayout.Button("Clear selection", GUILayout.Width(128)))
            { activeRuleTile = null; return; }

            if (GUILayout.Button("Log tiling rules"))
            {
                List<RuleTile.TilingRule> trules = activeRuleTile.m_TilingRules;
                Debug.Log($"Tiling rules: {trules.Count}");
                for (int i = 0; i < trules.Count; i++)
                {
                    Debug.Log($" TileRule[i]\n  - neighbors: {string.Join(',',trules[i].m_NeighborPositions)}");
                }
            }
            GUILayout.EndHorizontal();
            GUIDrawRuleTileGrid(activeRuleTile);
            */
        }
        else
        {
            GUILayout.Label("Select RuleTile", EditorStyles.boldLabel);
        }
    }





    Sprite GUISpriteField(Sprite sprite)
    { return (Sprite)EditorGUILayout.ObjectField("", sprite, typeof(Sprite), false, GUILayout.Width(60)); }
    bool GUISpriteFieldChanged(Sprite sprite, out Sprite sprite_out)
    {
        Sprite sprin = sprite;
        sprite_out = (Sprite)EditorGUILayout.ObjectField("", sprite, typeof(Sprite), false, GUILayout.Width(60));
        return sprite_out != sprite;
    }

    void GUIDrawRuleTileGrid(RuleTile ruleTile)
    {
        GUILayout.BeginVertical(GUILayout.Width(60 * 4), GUILayout.Height(60*4));
        GUILayout.BeginHorizontal();

        GUIDrawRuleTile9x9();
        GUILayout.Space(4);
        GUIDrawRuleTileVertical();

        GUILayout.EndHorizontal();

        GUILayout.Space(4);

        GUILayout.BeginHorizontal();

        GUIDrawRuleTileHorizontal();
        GUILayout.Space(4);
        GUIDrawRuleTileOne();

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    Sprite[,] GUIDrawRuleTile9x9()
    {
        Sprite[,] sprites = new Sprite[3, 3];

        GUILayout.BeginVertical();
        for (int c = 0; c < 3; c++)
        {
            GUILayout.BeginHorizontal();
            for (int r = 0; r < 3; r++)
            {
                GUISpriteField(default);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();

        return sprites;
    }

    void GUIDrawRuleTileVertical()
    {
        GUILayout.BeginVertical();
        for (int r = 0; r < 3; r++)
        {
            GUISpriteField(default);
        }
        GUILayout.EndVertical();
    }

    void GUIDrawRuleTileHorizontal()
    {
        GUILayout.BeginHorizontal();
        for (int c = 0; c < 3; c++)
        {
            GUISpriteField(default);
        }
        GUILayout.EndHorizontal();
    }

    void GUIDrawRuleTileOne()
    {
        GUISpriteField(default);
    }

    void ShowSaveWindow()
    {
        RuleTileGeneratorSave_Window rtgsw = GetWindow<RuleTileGeneratorSave_Window>("Save RuleTile");
        rtgsw.SetRTGW(this);
    }



    void GUITileSet(Texture2D tset)
    {
        //Texture2D tilemap = (Texture2D)EditorGUILayout.ObjectField("", sprite, typeof(Texture2D), false, GUILayout.Width(60));
    }

    public class RuleTileSingle
    {
        public string name;
        public Sprite sprite;
        public List<RuleVector> rules;

        public RuleTileSingle(string name, params RuleVector[] rules)
        {
            this.name = name;
            this.rules = new(rules);
        }


        public static List<RuleTileSingle> ToRuleTileSingles(string name, Vector2Int point, Vector2Int tileSize, Texture2D tileMap, Texture2D colorMap)
        {
            List<RuleTileSingle> rtiles = new();
            Vector2Int cmap_size = TileSizeMin(Tex2DSize(colorMap), new Vector2Int(3, 3));
            Vector2Int t_count = new Vector2Int(cmap_size.x / 3, cmap_size.y / 3);

            Vector2Int spos = Vector2Int.zero;
            for (int r = 0; r < t_count.y; r++)
            {
                spos.y = r * 3;
                for (int c = 0; c < t_count.x; c++)
                {
                    spos.x = c * 3;
                    rtiles.Add(new RuleTileSingle($"{name}_{c}-{r}", GetRules(spos, colorMap.GetPixels(spos.x, spos.y, 3, 3)).ToArray()));
                }
            }
            return rtiles;

        }

        static List<RuleVector> GetRules(Vector2Int start_pos, Color[] colors)
        {
            List<RuleVector> rvec = new ();
            int i = 0;
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 3; x++)
                {
                    if (RTColorRules.TryGetValue(colors[i], out int rule))
                    { rvec.Add(new RuleVector(rule, new Vector2Int(x - 1, y - 1))); }
                    i++;
                }
            }
            return rvec;
        }

        

        static Vector2Int TileSizeMin(Vector2Int size, Vector2Int tile_size)
        { return new Vector2Int(size.x - (size.x % tile_size.x), size.y - (size.y % tile_size.y)); }
        static Vector2Int Tex2DSize(Texture2D tex2d)
        { return new Vector2Int(tex2d.width, tex2d.height); }

    }


    public struct RuleVector
    {
        public int rule;
        public Vector2Int vector;
        public RuleVector(int rule, Vector2Int vector) 
        { this.rule = rule; this.vector = vector; }
    }



    /*
    public static class RTColorMatrix
    {
        static List<RTColor> rcol = new()
        {
            new RTColor("this", 1, Color.green)
        };

        static Dictionary<Color, int> colorIndex;
        static Dictionary<string, int> nameIndex;
        static Dictionary<int, int> ruleIndex;

        static RTColorMatrix()
        {
            rcol = new();
            colorIndex = new();
            nameIndex = new();
            ruleIndex = new();

            RTColor[] rcol_arr = rcol.ToArray();
            rcol = new();

            for (int i = 0; i < rcol_arr.Length; i++)
            { AddRTColor(rcol_arr[i]); }
        }

        static bool AddRTColor(RTColor rt_color)
        {
            if (colorIndex.ContainsKey(rt_color.color) || !nameIndex.ContainsKey(rt_color.ruleName) || ruleIndex.ContainsKey(rt_color.rule))
            { return false; }
            rcol.Add(rt_color);
            int rindex = rcol.Count - 1;
            colorIndex.Add(rt_color.color, rindex);
            nameIndex.Add(rt_color.ruleName, rindex);
            ruleIndex.Add(rt_color.rule, rindex);
            return true;
        }


        public static RTColor GetByColor(Color color)
        { return rcol[colorIndex[color]]; }
        public static RTColor GetByName(string name)
        { return rcol[nameIndex[name]]; }
        public static RTColor GetByRule(int rule)
        { return rcol[ruleIndex[rule]]; }

        public class RTColor
        {
            string _ruleName;
            int _rule;
            Color _color;
            public string ruleName { get => _ruleName; }
            public int rule { get => _rule; }
            public Color color { get => _color; }
            public RTColor(string rulename, int rule, Color color, bool addToMatrix = false)
            { _ruleName = rulename; _rule = rule; _color = color; if (addToMatrix) { AddRTColor(this); } }
        }

    }
    */

}
