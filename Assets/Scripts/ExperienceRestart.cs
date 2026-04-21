using Autohand;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExperienceRestart : MonoBehaviour
{
    [SerializeField] private ChangeSceneManager _changeSceneManager;
    [SerializeField] private int _timesPressed;

    public void LoadExperience()
    {

        _timesPressed++;
        Debug.Log($"[ExperienceStartManager] - Button pressed {_timesPressed} times");

        if (_timesPressed == 5)
        {
            _changeSceneManager.ButtonScene = 0;
            _changeSceneManager.OnButtonPressed();
        }
    }
}
