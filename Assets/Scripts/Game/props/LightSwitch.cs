using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightSwitch : MonoBehaviour
{
    private List<ReflectionProbe> lights = new List<ReflectionProbe>();
    private SoundsPlayer soundsPlayer;
    // Start is called before the first frame update
    void Start()
    {
        GameObject[] gos = GameObject.FindGameObjectsWithTag("ReflectProbes");
        for (int i = 0; i < gos.Length; i++)
        {
            lights.Add(gos[i].GetComponent<ReflectionProbe>());
        }
        soundsPlayer = transform.Find("Sounds").GetComponent<SoundsPlayer>();
    }

public void SetLights(bool value)
    {
        soundsPlayer.Play(value ? 1 : 0);
        for (int i = 0; i < lights.Count; i++)
        {
            lights[i].intensity = value ? 2 : 0;
        }
    }
    public bool GetLights()
    {
        return (lights[0].intensity == 0 ? false : true);
        
    }
}
