using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class Entity_Window : EditorWindow
{

    int tselect = 0;
    string[] toptions = { "Entity", "Stats", "Attributes" };

    Entity entity;
    EntityStats estats;

    [MenuItem("Window/Entity/Entity Editor")]
    public static void ShowWindow()
    {
        GetWindow<Entity_Window>("Entity Editor");
    }


    private void OnGUI()
    {
        if (entity)
        {
            tselect = GUILayout.Toolbar(tselect, toptions);
            switch (tselect)
            {
                case 0:
                    GUIEntity();
                    break;
                case 1:
                    GUIEntityStats();
                    break;
                case 2:
                    GUIEntityAttributes();
                    break;
            }
        }
        else
        {
            GUILayout.Label("Select object with Entity component");
        }
    }


    private void OnSelectionChange()
    {
        GameObject obj = Selection.activeGameObject;
        if (obj != null && obj.TryGetComponent<Entity>(out Entity ent))
        { entity = ent; }
        Repaint();
    }

    void GUIEntity()
    {

    }
    void GUIEntityStats()
    {
        if (!estats)
        {
            entity.FetchEntityComponents();
            estats = entity.entityStats;
            if (estats == null)
            {
                if (!entity.TryGetComponent<EntityStats>(out estats))
                {
                    if (GUILayout.Button("Add EntityStats Component"))
                    {
                        entity.gameObject.AddComponent<EntityStats>();
                        entity.FetchEntityComponents();
                    }
                }
            }
            return;
        }

        GUIStatsDict();

    }
    void GUIEntityAttributes()
    {

    }

    void GUIStatsDict(int nameWidth = 64)
    {
        List<string> keys = new();

        if (GUILayout.Button("Add Stat", GUILayout.Width(nameWidth)))
        { estats.stats.Add("new_stat", new EntityStats.Stat("new_stat")); Repaint(); }

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        foreach (string k in estats.stats.Keys)
        { 
            keys.Add(k);
            GUILayout.Label(k, GUILayout.Width(nameWidth));
        }
        GUILayout.EndVertical();
        GUILayout.BeginVertical();

        for (int i = 0; i < keys.Count; i++)
        {
            GUIStatNoLabel(estats.stats[keys[i]]);
        }

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();

    }

    void GUIStat(EntityStats.Stat stat, int labelWidth = -1)
    {
        GUILayout.BeginHorizontal();

        if (labelWidth < 0) GUILayout.Label($"{stat.name}:");
        else GUILayout.Label($"{stat.name}:", GUILayout.Width(labelWidth));
        stat.value = EditorGUILayout.FloatField(stat.value, GUILayout.Width(32));
        GUILayout.Label("/", GUILayout.Width(8));
        stat.value = EditorGUILayout.FloatField(stat.max, GUILayout.Width(32));

        GUILayout.EndHorizontal();
    }
    void GUIStatNoLabel(EntityStats.Stat stat)
    {
        GUILayout.BeginHorizontal();

        stat.value = EditorGUILayout.FloatField(stat.value, GUILayout.Width(32));
        GUILayout.Label("/", GUILayout.Width(8));
        stat.value = EditorGUILayout.FloatField(stat.max, GUILayout.Width(32));

        GUILayout.EndHorizontal();
    }

    void GUIAttribute(EntityStats.Attribute attr, int labelWidth = -1)
    {
        GUILayout.BeginHorizontal();

        if (labelWidth < 0) GUILayout.Label($"{attr.name}:");
        else GUILayout.Label($"{attr.name}:", GUILayout.Width(labelWidth));
        attr.value = EditorGUILayout.FloatField(attr.value, GUILayout.Width(32));

        GUILayout.EndHorizontal();
    }


    Entity GetSelection()
    {
        GameObject go = Selection.activeGameObject;
        if (go && go.TryGetComponent(out Entity ent))
        { return ent; }
        return null;
    }
    bool TryGetSelection(out Entity ent)
    {
        ent = GetSelection();
        return ent != null;
    }
}
