using UnityEngine;
using Unity.XR.CoreUtils;

public class HeightCalibrationByTrigger : MonoBehaviour
{
    public XROrigin xrOrigin; // Arrastra tu XR Origin aquí
    public float baseHeight = 1.7f; // Altura en metros para la cual la vista es correcta



    void CalibrateHeight()
    {
        float playerHeight = MeasurePlayerHeight();
        AdjustXROriginHeight(playerHeight);
        Debug.Log($"Adjusted XROrigin height based on player height: {playerHeight}");
    }

    void AdjustXROriginHeight(float playerHeight)
    {
        // Calcular la diferencia de altura que necesita ser ajustada
        float heightAdjustment = playerHeight - baseHeight;
        Vector3 currentPosition = xrOrigin.transform.position;
        xrOrigin.transform.position = new Vector3(currentPosition.x, currentPosition.y + heightAdjustment, currentPosition.z);
    }

    float MeasurePlayerHeight()
    {
        // Esta función debe implementar la medición de la altura del jugador
        // Aquí se retorna un valor simulado para pruebas
        return 1.8f; // Simula que el jugador tiene una altura de 1.8 metros
    }
}
