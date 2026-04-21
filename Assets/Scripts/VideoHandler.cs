using UnityEngine;
using UnityEngine.Video;

public class VideoEventHandler : MonoBehaviour
{
    public VideoPlayer videoPlayer; 
    void Start()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        Debug.Log("El vídeo ha terminado.");
        vp.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        videoPlayer.loopPointReached -= OnVideoFinished;
    }
}
