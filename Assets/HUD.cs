using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static EntityStats;

public class HUD : MonoBehaviour
{

    TMPro.TextMeshPro hp_text;
    Transform stat, thp;
    EntityStats estats;
    Stat stat_hp;
    Player p;

    private void Start()
    {
        stat = transform.Find("STAT");
        thp = stat.Find("HUD_HP");
        hp_text = thp.GetComponent<TMPro.TextMeshPro>();
        StartCoroutine(IWaitPlayer());
    }


    // Update is called once per frame
    void Update()
    {
        if (p && p.isActiveAndEnabled)
        {
            stat.gameObject.SetActive(true);
            hp_text.text = $"{Mathf.Floor(stat_hp.value+0.1f)} / {Mathf.Floor(stat_hp.max + 0.1f)}";
        }
        else
        {
            stat.gameObject.SetActive(false);
        }
    }

    IEnumerator IWaitPlayer()
    {
        while (Player.instance == null)
        { yield return null; }
        p = Player.instance;
        estats = p.entityStats;
        stat_hp = estats.GetStat("health");
    }
}
