using System.Collections;
using UnityEngine;

public class ButtonTextManager : MonoBehaviour
{
    public AudioClip AudioClip;
    
    public IEnumerable AudioStart()
    {
        Debug.Log("Pressed!");
        AudioSource audio = GetComponent<AudioSource>();
        audio.clip = AudioClip;
        audio.Play();
        yield return new WaitForSeconds(audio.clip.length);
    }

    public void StartAudio()
    {
        AudioStart();
    }
}
