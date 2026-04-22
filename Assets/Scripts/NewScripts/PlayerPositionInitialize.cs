using System.Collections;
using UnityEngine;
using Autohand;

public class PlayerPositionInitialize : MonoBehaviour
{
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private AutoHandPlayer _autoHandPlayer;

    [Header("Rotación inicial")]
    [Tooltip("Si está activo, usa el ángulo Y de abajo en lugar de la rotación del SpawnPoint.")]
    [SerializeField] private bool _overrideYRotation = false;

    [Tooltip("Ángulo en grados (eje Y/horizontal) hacia donde mirará el jugador al entrar. Solo se aplica si Override Y Rotation está activo.")]
    [Range(0f, 360f)]
    [SerializeField] private float _facingAngleY = 0f;

    [Header("Configuración")]
    [Tooltip("Tiempo máximo de espera (segundos) para que el tracking se active antes de forzar la posición igualmente.")]
    [SerializeField] private float _trackingWaitTimeout = 5f;

    private void Start()
    {
        if (_autoHandPlayer == null)
            _autoHandPlayer = FindFirstObjectByType<AutoHandPlayer>();

        if (_spawnPoint == null || _autoHandPlayer == null)
        {
            Debug.LogWarning("PlayerPositionInitialize: asigna SpawnPoint y AutoHandPlayer en el Inspector.");
            return;
        }

        StartCoroutine(ForcePositionAfterTracking());
    }

    /// <summary>
    /// Espera a que el headset empiece a reportar tracking real y entonces
    /// llama a SetPosition(), que actualiza el rig, el trackingContainer,
    /// el headPhysicsFollower y los rigidbodies de las manos de forma coherente.
    /// </summary>
    private IEnumerator ForcePositionAfterTracking()
    {
        Camera headCam = _autoHandPlayer.headCamera;

        // Guardamos la posición inicial de la cámara (antes de tracking activo)
        Vector3 initialHeadPos = headCam.transform.position;
        float elapsed = 0f;

        // Esperamos hasta que la cámara empiece a moverse (tracking activo)
        // o hasta que se cumpla el timeout de seguridad
        while (headCam.transform.position == initialHeadPos && elapsed < _trackingWaitTimeout)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Un frame adicional para que Autohand procese su WaitFlagForTrackingStart
        yield return new WaitForEndOfFrame();

        // Usamos el método oficial de AutoHand para teleportar al jugador,
        // que actualiza el trackingContainer, headPhysicsFollower, body y manos.
        Quaternion targetRotation = _overrideYRotation
            ? Quaternion.Euler(0f, _facingAngleY, 0f)
            : _spawnPoint.rotation;

        _autoHandPlayer.SetPosition(_spawnPoint.position, targetRotation);

        Debug.Log($"PlayerPositionInitialize: jugador posicionado en {_spawnPoint.position}, rotación Y={targetRotation.eulerAngles.y:F1}° tras {elapsed:F2}s de espera.");
    }
}
