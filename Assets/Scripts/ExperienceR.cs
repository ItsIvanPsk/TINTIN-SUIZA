using Autohand;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExperienceR : MonoBehaviour
{
    [SerializeField] private AutoHandPlayer _player;
    [SerializeField] private GameObject _firstCheckpoint;
    [SerializeField] private int _timesPressed;
    [SerializeField] private ChangeSceneManager _manager;

    public void LoadExperience()
    {

        _timesPressed++;
        Debug.Log($"[ExperienceStartManager] - Button pressed {_timesPressed} times");

        if (_timesPressed == 5)
        {
            _manager.ButtonScene = 0;
            _manager.OnButtonPressed();
        }
    }
}
