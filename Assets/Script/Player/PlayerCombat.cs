using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static InputManagerClasses;
using static Utility;

public partial class Player : PlayerController
{

    Damage meleeDamage = new Damage(25f);

    GameObject SpawnProjectile(GameObject prefab, Vector3 position, Vector2 direction, bool flipX)
    {
        GameObject go = Instantiate(prefab, position, Quaternion.identity);
        Projectile proj = go.GetComponent<Projectile>();
        proj.onHitActions.Add(OnProjectileHit);
        proj.flip = flipX;
        Vector3 eul = go.transform.eulerAngles;
        eul.z = Vector2.SignedAngle(Vector2.right, direction);
        go.transform.eulerAngles = eul;
        go.SetActive(true);
        return go;
    }

    public void OnProjectileHit(Projectile p, Collider2D coll)
    {
        //Debug.Log($"proj_hit {p.name} {coll.name}");
        if (coll.tag != "Player")
        {
            if (coll.TryGetComponent(out Entity ent))
            {
                Debug.Log($"{ent.name}");
                if (p.projectileTag == "ukick1")
                { ent.velocity = Vector2.zero; }
                float kbmult = 0.5f + p.timeToLive / p.lifeTime * 0.6f;
                ent.AddForce(p.knockback * kbmult);

                ent.ApplyDamage(meleeDamage, this);
                stat_hp.value += meleeDamage.damage * 0.2f;

                if (p.timeToLive > p.lifeTime - 0.1f)
                { TimeControl.SetTimeScaleFadeForTime(0.1f, 0.2f, 0f, 0f, 1f); }
            }
        }
    }

    void Attack_Charge_Up()
    {
        animator.SetState(meleeUpChargeAnim);
    }
    void Attack_Charge_Down()
    {
        animator.SetState(meleeDownChargeAnim);
    }
    void Attack_Charge_Neutral()
    {
        animator.SetState(meleeChargeAnim);
    }

    public void OnAttack()
    {
        asource.pitch = 1f;
        Vector2 pdir, knockback; bool flip;
        GameObject go;
        Projectile proj;
        Vector3 ppos = transform.position;
        facing = !animator.flipX ? 1 : -1;
        curAttack++;
        string canim = animator.currentAnimation.ID;

        if (canim == meleeUpAir)
        {
            if (curAttack == 1)
            { last_maj = meleeAirJump; }
            if (facing > 0)
            { pdir = AngleToVector2(80f); flip = false; }
            else
            { pdir = AngleToVector2(-80f); flip = true; }
            go = SpawnProjectile(projectiles.chargeMeleeUp, ppos.Add(x: 0f * facing, y: 0f), pdir, flip);//.transform.parent = transform;
            knockback = AngleToVector2(75f) * (curAttack == 1 ? 10f : 20f);
            knockback.x *= facing;
            proj = go.GetComponent<Projectile>();
            proj.knockback = knockback;
            proj.projectileTag = $"ukick{curAttack}";
            asource.clip = soundSlash;
            asource.Play();
            _velocity = new Vector2(facing * 2f + dir.x * 1f, 8f);
        }
        else if (canim == meleeDownAirRelease)
        {
            if (curAttack == 1)
            { last_maj = meleeAirJump; }
            if (facing > 0)
            { pdir = AngleToVector2(-80f); flip = false; }
            else
            { pdir = AngleToVector2(80f); flip = true; }
            go = SpawnProjectile(projectiles.chargeMeleeUp, ppos.Add(x: 0f * facing, y: 0f), pdir, flip);//.transform.parent = transform;
            knockback = AngleToVector2(-75f) * (curAttack == 1 ? 10f : 20f);
            knockback.x *= facing;
            proj = go.GetComponent<Projectile>();
            proj.knockback = knockback;
            proj.projectileTag = $"adkick{curAttack}";
            asource.clip = soundSlash;
            asource.Play();
            _velocity = new Vector2(facing * 2f + dir.x * 1f, -8f);
        }
        else if (canim == meleeDownAirGround)
        {
            if (facing > 0)
            { pdir = AngleToVector2(80f); flip = false; }
            else
            { pdir = AngleToVector2(-80f); flip = true; }
            go = SpawnProjectile(projectiles.chargeMeleeDown, ppos.Add(x: 0.4f * facing, y: -0.15f), Vector2.right, flip);//.transform.parent = transform;
            knockback = AngleToVector2(45f) * 20f;
            knockback.x *= facing;
            proj = go.GetComponent<Projectile>();
            proj.knockback = knockback;
            proj.projectileTag = $"adkick{curAttack}";
            asourceFeet.PlayOneShot(soundAxeExpl, 0.1f);
        }
        else if (canim == meleeUpChargeAnim)
        {
            if (facing > 0)
            { pdir = AngleToVector2(80f); flip = false; }
            else
            { pdir = AngleToVector2(-80f); flip = true; }
            go = SpawnProjectile(projectiles.chargeMeleeUp, ppos.Add(x: 0f * facing, y: 0f), pdir, flip);//.transform.parent = transform;
            knockback = AngleToVector2(75f) * (curAttack == 1 ? 10f : 20f);
            knockback.x *= facing;
            proj = go.GetComponent<Projectile>();
            proj.knockback = knockback;
            proj.projectileTag = $"ukick{curAttack}";
            asource.clip = soundSlash;
            asource.Play();
            _velocity = new Vector2(facing * 2f + dir.x * 1f, 8f);
        }
        else if (canim == meleeChargeAnim)
        {
            if (curAttack == 2)
            {
                _velocity = new Vector2(6f * -facing, 2f);
                return;
            }
            if (facing > 0)
            { pdir = Vector2.right; flip = false; }
            else
            { pdir = Vector2.left; flip = true; }
            go = SpawnProjectile(projectiles.chargeMeleeForward, ppos.Add(x: 0.4f * facing, y: 0.1f), Vector2.right, flip);//.transform.parent = transform;
            knockback = AngleToVector2(30f) * 20f;
            knockback.x *= facing;
            proj = go.GetComponent<Projectile>();
            proj.knockback = knockback;
            proj.projectileTag = $"nkick{curAttack}";
            asource.clip = soundSlash;
            asource.Play();
            if (dir.x != 0)
            { _velocity = new Vector2(6f * dir.x, 2f); }
            else
            { _velocity = new Vector2(4f * facing, 2f); }
        }
        else if (canim == meleeDownChargeAnim)
        {
            if (facing > 0)
            { pdir = AngleToVector2(80f); flip = false; }
            else
            { pdir = AngleToVector2(-80f); flip = true; }
            go = SpawnProjectile(projectiles.chargeMeleeDown, ppos.Add(x: 0.4f * facing, y: -0.15f), Vector2.right, flip);//.transform.parent = transform;
            knockback = AngleToVector2(45f) * 20f;
            knockback.x *= facing;
            proj = go.GetComponent<Projectile>();
            proj.knockback = knockback;
            proj.projectileTag = $"dkick{curAttack}";
            asourceFeet.PlayOneShot(soundAxeExpl, 0.1f);
            //asource.clip = soundAxeExpl;
            //asource.Play();
            //_velocity = new Vector2(facing * 2f + dir.x * 1f, 8f);
        }
    }

    bool AttackCharge()
    {
        if (isCharging > 0) return true;
        float stime = 0.1f;
        string anim;
        Vector2 att_dir;
        if (up.hold && up.holdTime < stime)
        {
            anim = "melee.g.u.c";
            //animator.SetState("melee.g.u.c");
            att_dir = new Vector2(dir.x != 0 ? dir.x : facing, 1);
        }
        else if (down.hold && down.holdTime < stime)
        {
            anim = "melee.g.d.c";
            //animator.SetState("melee.g.d.c");
            att_dir = new Vector2(dir.x != 0 ? dir.x : facing, -1);
        }
        else if (left.hold && left.holdTime < stime)
        {
            anim = "melee.g.n.c";
            //animator.SetState("melee.g.n.c");
            facing = -1; animator.FlipX(true);
            att_dir = new Vector2(-1, 0);
        }
        else if (right.hold && right.holdTime < stime)
        {
            anim = "melee.g.n.c";
            //animator.SetState("melee.g.n.c");
            facing = 1; animator.FlipX(false);
            att_dir = new Vector2(1, 0);
        }
        else { return false; }

        isCharging = Mathf.Max(1, isCharging);

        //facing = animator.flipX ? -1 : 1;
        curAttack = 0;
        Debug.Log($"charge init");
        //if (chargeCoroutine == null) chargeCoroutine = StartCoroutine(IChargeAttack());
        StartCoroutine(IChargeAttack(anim, att_dir));
        return true;


    }
    void AttackChargeRelease(Vector2 vec)
    {
        if (isCharging < 1) { chargeAmount = 0; return; }
        //isCharging = 1;
        if (vec.y > 0)
        { Attack_Charge_Up(); }
        else if (vec.y < 0)
        { Attack_Charge_Down(); }
        else
        { Attack_Charge_Neutral(); }
        chargeAmount = 0;
    }

    IEnumerator IComboTimer(float time)
    {
        cComboTimer = time;
        while (cComboTimer > 0)
        {
            cComboTimer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        cComboStage = 0;
        cComboTimer = 0;
        Debug.Log($"!Combo End");
    }


    //Coroutine chargeCoroutine;
    IEnumerator IChargeAttack(string anim, Vector2 att_dir)
    {

        isCharging = Mathf.Max(1, isCharging);
        animator.SetState(anim);
        chargeAmount = 0f;
        //int l_int = 0;
        //animator.SetState("charge.n");
        float stimer = 9f, prate, cdelta;
        while ((attackA.hold && isCharging > 1) || isCharging == 1)
        {
            stimer += Time.deltaTime;
            cdelta = chargeAmount / 3f;
            prate = 0.25f + ((1f - cdelta) * 0.75f);
            if (stimer >= prate)
            {
                Instantiate(projectiles.chargeShockwave, transform.position, Quaternion.identity);
                asource.pitch = 1f + cdelta * 1f;
                asource.PlayOneShot(soundChargeup, 0.5f);
                stimer = 0;
            }
            chargeAmount = Mathf.Min(chargeAmount + Time.deltaTime, 3f);
            yield return new WaitForEndOfFrame();
        }
        AttackChargeRelease(att_dir);
        //yield return new WaitForSeconds(1f);
        //isCharging = 0;
        //chargeCoroutine = null;
    }

    IEnumerator IHoldAirAttack(Vector2 att_dir)
    {
        _velocity.y = 10f;
        animator.SetState(meleeDownAirCharge);
        attackState = 1;
        string att_name = meleeDownAirRelease;
        while ((attackState > 0 && attackA.hold) || attackState == 1)
        {
            if (isGrounded)
            {
                if (attackState == 1)
                { attackState = 0; yield break; }
                att_name = meleeDownAirGround; break;
            }
            yield return new WaitForEndOfFrame();
        }
        attackState = 2;
        animator.SetState(att_name);
    }


    public void OnCharge()
    {
        //if (isCharging == 1) isCharging = 2;
    }
}
