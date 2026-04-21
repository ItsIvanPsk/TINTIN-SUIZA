using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneManager : MonoBehaviour
{
    [SerializeField]
    public int ButtonScene;

    public void OnButtonPressed()
    {
        SceneManager.LoadScene(ButtonScene);
    }
}
