using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class Aura : MonoBehaviour
{
    public List<string> filterTags = new();
    public UnityEvent<Collider2D> onTriggerEnter, onTriggerExit;
    [SerializeField] List<ParticlePair> particles;


    private void Start()
    {
        foreach (ParticlePair pp in particles)
        {
            ParticleSystem.EmissionModule emod = pp.psystem.emission;
            emod.enabled = pp.enabledOnStart;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (filterTags.Count > 0 && filterTags.Contains(collision.tag))
        {
            onTriggerEnter.Invoke(collision);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (filterTags.Count > 0 && filterTags.Contains(collision.tag))
        {
            onTriggerExit.Invoke(collision);
        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (filterTags.Count > 0 && filterTags.Contains(collision.tag))
        {
            if (collision.TryGetComponent<EntityStats>(out EntityStats estats))
            { estats.GetStat("health").value += 20 * Time.fixedDeltaTime; }
        }
    }

    public void Activate_Particles(string id)
    {
        foreach (ParticlePair pp in particles)
        {
            if (pp.id != id) continue;
            ParticleSystem.EmissionModule emod = pp.psystem.emission;
            emod.enabled = true; 
        }
    }
    public void Activate_Particles(int index)
    { 
        ParticleSystem.EmissionModule emod = particles[index].psystem.emission;
        emod.enabled = true;
    }
    public void Deactivate_Particles(string id)
    {
        foreach (ParticlePair pp in particles)
        {
            if (pp.id != id) continue;
            ParticleSystem.EmissionModule emod = pp.psystem.emission;
            emod.enabled = false;
        }
    }
    public void Deactivate_Particles(int index)
    {
        ParticleSystem.EmissionModule emod = particles[index].psystem.emission;
        emod.enabled = false;
    }

    [System.Serializable]
    public class ParticlePair
    {
        public string id;
        public ParticleSystem psystem;
        public bool enabledOnStart = true;
        public bool active { get => psystem.gameObject.activeSelf; set => psystem.gameObject.SetActive(value); }
        public ParticlePair(string id, ParticleSystem psys = null, bool enabled_on_start = true)
        { this.id = id; psystem = psys; enabledOnStart = enabled_on_start; }
        
    }

}
