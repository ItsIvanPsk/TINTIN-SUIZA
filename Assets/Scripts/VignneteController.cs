using UnityEngine;
using UnityEngine.Rendering.PostProcessing; // Asegúrate de tener esta librería si usas post-processing

public class VignetteController : MonoBehaviour
{
    public PostProcessVolume volume;
    private Vignette vignette;

    void Start()
    {
        if (volume.profile.TryGetSettings(out Vignette vignette))
        {
            this.vignette = vignette;
        }
    }

    public void ActivateVignette(bool activate)
    {
        if (vignette != null)
        {
            vignette.active = activate;
        }
    }
}
