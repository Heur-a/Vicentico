using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class BackgroundVideo : MonoBehaviour
{
    public RawImage rawImage;
    public VideoPlayer videoPlayer;

    void Start()
    {
        videoPlayer.targetTexture = new RenderTexture(1920, 1080, 0);
        rawImage.texture = videoPlayer.targetTexture;
        videoPlayer.Play();
    }
}
