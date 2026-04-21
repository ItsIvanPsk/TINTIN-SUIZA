using System.Collections;
using UnityEngine;

public class AudioCheckpoint : MonoBehaviour
{
    [SerializeField] private GameObject _audioIcon;   
    [SerializeField] private AudioClip _audio;        
    [SerializeField] private AudioSource _audioSource;

    [SerializeField] private GameObject _rendererToToggle;
    [SerializeField] private GameObject _imageToToggle;
    [SerializeField] private float _speed = 60f;


    private void Awake()
    {
        StartCoroutine(RotateAudio());
    }

    private IEnumerator RotateAudio()
    {
        while (true)
        {
            _audioIcon.transform.Rotate(0f, _speed * Time.deltaTime, 0f);
            yield return null;
        }
    }

    private void LoadAudio()
    {
        if (_audio != null && _audioSource != null)
        {
            if (_audioSource.clip == _audio && _audioSource.isPlaying)
                return;

            _audioSource.clip = _audio;
            _audioSource.Play();
        }
        else
        {
            Debug.LogWarning("AudioCheckpoint: falta AudioClip o AudioSource asignado");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Checkpoint alcanzado: " + other.gameObject.name);
        Debug.Log("Tag del objeto: " + other.gameObject.tag);

        if (other.gameObject.CompareTag("Checkpoint") || other.gameObject.CompareTag("GameController") || other.gameObject.CompareTag("MainCamera")|| other.gameObject.CompareTag("Player"))
        {
            Debug.Log(" Checkpoint activado por: " + other.gameObject.name);
            LoadAudio();
        }
    }
}
