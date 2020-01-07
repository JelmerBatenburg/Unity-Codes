using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetAudioData : MonoBehaviour
{
    public AudioSource clip;
    public float visualSize;
    public int samples;
    public float hertzASample;
    public AudioType[] ranges;
    public GameObject preview;
    public float detectLevel;

    public void Start()
    {
        GetHertz();
    }

    public void Update()
    {
        GetAudioTypes();
    }

    public void GetAudioTypes()
    {
        float[] spectrum = new float[samples];
        AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);
        for (int i = 0; i < spectrum.Length; i++)
        {
            float currentHertz = i * hertzASample;
            foreach(AudioType type in ranges)
                if (currentHertz <= type.hertzMax && currentHertz >= type.hertzMin && spectrum[i]  >= detectLevel)
                {
                    GameObject g = Instantiate(preview, new Vector3(i * 1.4f, 0, 1), Quaternion.identity);
                    g.GetComponent<MeshRenderer>().material.color = type.color;
                    Destroy(g, 7);
                    break;
                }
        }
    }

    public void GetHertz()
    {
        hertzASample = (AudioSettings.outputSampleRate / 2) / samples;
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        float[] spectrum = new float[samples];
        AudioListener.GetSpectrumData(spectrum, 0, FFTWindow.Rectangular);
        for (int i = 0; i < (spectrum.Length - 1f) / 2f; i++)
            Gizmos.DrawCube(new Vector3(i * 1.4f, spectrum[i] * visualSize / 2, 0), new Vector3(1, spectrum[i] * visualSize, 1));
    }

    [System.Serializable]
    public class AudioType
    {
        public string name;
        public float hertzMin;
        public float hertzMax;
        public Color color;
    }
}
