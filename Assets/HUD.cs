using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static EntityStats;

public class HUD : MonoBehaviour
{
    public static HUD instance { get; protected set; }
    Transform stat, thp;
    EntityStats estats;
    Stat stat_hp;
    Player p;
    [SerializeField] GameObject hp_object;
    [SerializeField] Image hp_bar;
    [SerializeField] TMPro.TextMeshProUGUI hp_text;


    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        //stat = transform.Find("STAT");
        //thp = stat.Find("HUD_HP");
        //hp_text = thp.GetComponent<TMPro.TextMeshPro>();
        //StartCoroutine(IWaitPlayer());
    }

    public void SetStatHP(Stat stat)
    { 
        stat_hp = stat;
        stat_hp.AddListener(PlayerHPChanged);
        PlayerHPChanged(stat_hp.value, 0);
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (p && p.isActiveAndEnabled)
        {
            stat.gameObject.SetActive(true);
            if (hp_text)
            hp_text.text = $"{Mathf.Floor(stat_hp.value+0.1f)} / {Mathf.Floor(stat_hp.max + 0.1f)}";
        }
        else
        {
            stat.gameObject.SetActive(false);
        }
        */
    }

    /*
    IEnumerator IWaitPlayer()
    {
        do {
            p = Player.instance;
            yield return new WaitForEndOfFrame();
        } while (p == null);
        estats = p.entityStats;
        while (!estats.TryGetStat("health", out stat_hp))
        { yield return new WaitForEndOfFrame(); }
        Debug.Log("player health found!");
        stat_hp.AddListener(PlayerHPChanged);
    }
    */

    public void PlayerHPChanged(float new_value, float old_value)
    {
        Debug.Log($"stat_hp changed {new_value} {old_value}");
        hp_bar.fillAmount = new_value / stat_hp.max;
        if (new_value > 0)
        { hp_text.text = $"{Mathf.Max(Mathf.Round(new_value), 1)} / {Mathf.Round(stat_hp.max)}"; }
        else
        { hp_text.text = $"{0} / {Mathf.Round(stat_hp.max)}"; }
    }
}
