using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class TintinVideoHandler : MonoBehaviour
{
    public VideoClip clip;
    public ChangeSceneManager _scene;

    public void StartVideo()
    {
        StartCoroutine(VideoLoading());
    }

    public IEnumerator VideoLoading()
    {
        var duration = 5f + clip.length;
        var elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _scene.ButtonScene = 4;
        _scene.OnButtonPressed();
    }
}
