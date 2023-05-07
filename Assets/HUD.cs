using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using static EntityStats;
using Cyan;

public class HUD : MonoBehaviour
{
    public static HUD instance { get; protected set; }
    Transform stat, thp;
    EntityStats estats;
    Stat stat_hp, stat_sp, stat_mp, stat_boss_hp;
    Player p;
    [SerializeField] GameObject hp_object, sp_object, boss_hp_object;
    [SerializeField] Image hp_bar, sp_bar, boss_hp_bar;
    [SerializeField] TMPro.TextMeshProUGUI hp_text, sp_text;

    [SerializeField] ScriptableRendererFeature manaOverlay;
    Material overlayMaterial;

    RectTransform boss_hp_rect;

    private void Awake()
    {
        instance = this;


        overlayMaterial = ((Blit)manaOverlay).blitPass.blitMaterial;
        //Debug.Log(overlayMaterial);
        boss_hp_rect = boss_hp_object.GetComponent<RectTransform>();

        bosshp_pos = boss_hp_rect.anchoredPosition; //new Vector2(0f, 6.4f);
        bosshp_size = boss_hp_rect.sizeDelta; //new Vector2(256f, 8f);

        boss_hp_object.SetActive(false);
    }
    private void Start()
    {

    }


    public void SetBossHP(Stat stat)
    {
        stat_boss_hp = stat;
        stat_boss_hp.onValueChanged.AddListener(BossHPChanged);
    }
    public void EnableBossHpBar(bool state)
    {
        if (state)
        {
            StartCoroutine(IAnimateBossHPBar());
            return;
        }
        boss_hp_object.SetActive(false);
    }



    public void SetStatHP(Stat stat)
    {
        stat_hp = stat;
        stat_hp.onValueChanged.AddListener(PlayerHPChanged);
        PlayerHPChanged(stat_hp.value, 0);
    }
    public void SetStatSP(Stat stat)
    {
        stat_sp = stat;
        stat_sp.onValueChanged.AddListener(PlayerSPChanged);
        PlayerSPChanged(stat_sp.value, 0);
    }
    public void SetStatMP(Stat stat)
    {
        stat_mp = stat;
        stat_mp.onValueChanged.AddListener(PlayerMPChanged);
        PlayerMPChanged(stat_sp.value, 0);
    }

    public void PlayerHPChanged(float new_value, float old_value)
    {
        hp_bar.fillAmount = new_value / stat_hp.max;
        if (new_value > 0)
        { hp_text.text = $"{Mathf.Max(Mathf.Round(new_value), 1)} / {Mathf.Round(stat_hp.max)}"; }
        else
        { hp_text.text = $"{0} / {Mathf.Round(stat_hp.max)}"; }
    }

    public void PlayerSPChanged(float new_value, float old_value)
    {
        sp_bar.fillAmount = new_value / stat_sp.max;
        if (new_value > 0)
        { sp_text.text = $"{Mathf.Max(Mathf.Round(new_value), 1)} / {Mathf.Round(stat_sp.max)}"; }
        else
        { sp_text.text = $"{0} / {Mathf.Round(stat_sp.max)}"; }
    }

    float posdelta = 0f, mpd = 0f;
    Vector2 vecCenter = new Vector2(0.5f, 0.5f);
    public void SetOverlayPosition(Vector2 vec)
    {
        overlayMaterial.SetVector("_Position", Vector2.Lerp(vecCenter, vec, 1f - mpd));
    }

    public void PlayerMPChanged(float new_value, float old_value)
    {
        mpd = new_value / stat_mp.max;
        manaOverlay.SetActive(mpd < 1f);
        overlayMaterial.SetFloat("_Volume", 0.5f - mpd * 0.4f);
    }


    public void BossHPChanged(float new_value, float old_value)
    {
        boss_hp_bar.fillAmount = new_value / stat_boss_hp.max;
    }

    Vector2 bosshp_pos;// = new Vector2(0f, 6.4f);
    Vector2 bosshp_size;// = new Vector2(256f, 8f);
    IEnumerator IAnimateBossHPBar()
    {
        Vector2 spos = bosshp_pos;
        Vector2 ssize = bosshp_size;
        spos.y = -spos.y;
        ssize.x = 4;
        boss_hp_rect.anchoredPosition = bosshp_pos;
        boss_hp_rect.sizeDelta = bosshp_pos;
        boss_hp_bar.fillAmount = 0;
        boss_hp_object.SetActive(true);
        float t = 0, time = 1.2f;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            boss_hp_rect.anchoredPosition = Vector2.Lerp(spos, bosshp_pos, t / time);
            yield return new WaitForEndOfFrame();
        }
        boss_hp_rect.anchoredPosition = bosshp_pos;
        t = 0; time = 1f;
        while (t < time)
        {
            t += Time.unscaledDeltaTime;
            boss_hp_rect.sizeDelta = Vector2.Lerp(ssize, bosshp_size, t / time);
            yield return new WaitForEndOfFrame();
        }
        boss_hp_rect.sizeDelta = bosshp_size;

        t = 0; time = 1f;
        while (t < time)
        {
            boss_hp_bar.fillAmount = t / time;
            t += Time.unscaledDeltaTime;
            yield return new WaitForEndOfFrame();
        }
        boss_hp_bar.fillAmount = 1f;
    }

}
