using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Autohand;

/// <summary>
/// Corrige en runtime los problemas más comunes del rig OpenXR de AutoHand:
///   1. Drag excesivo en el Rigidbody del player (m_Drag alto en el prefab).
///   2. Input Actions de rotación/teleport no habilitadas.
///   3. Asegura que el AutoHandPlayer tiene useMovement activo.
///
/// Coloca este script en el mismo GameObject que AutoHandPlayer
/// o en cualquier GameObject de la escena.
/// </summary>
public class AutoHandPlayerFixer : MonoBehaviour
{
    [Header("Referencias (auto-detect si están vacías)")]
    [SerializeField] private AutoHandPlayer _player;

    [Header("Rigidbody – Drag")]
    [Tooltip("Drag lineal del Rigidbody del player. AutoHand gestiona el freno por código (groundedDrag).\n" +
             "Ponlo a 0 para movimiento físico correcto.")]
    [SerializeField] private float _playerRigidbodyDrag = 0f;

    [Tooltip("Drag angular del Rigidbody del player. Normalmente 0.")]
    [SerializeField] private float _playerRigidbodyAngularDrag = 0.05f;

    [Header("Movimiento")]
    [Tooltip("Fuerza activar useMovement en AutoHandPlayer.")]
    [SerializeField] private bool _forceEnableMovement = true;

    [Tooltip("Tipo de rotación: Snap (giro por pasos) o Smooth (giro continuo).")]
    [SerializeField] private RotationType _rotationType = RotationType.snap;

    [Tooltip("Ángulo por paso en Snap Turn.")]
    [SerializeField] private float _snapTurnAngle = 30f;

    [Tooltip("Velocidad de Smooth Turn (grados/segundo).")]
    [SerializeField] private float _smoothTurnSpeed = 120f;

    [Header("Input Actions – habilitar en Start")]
    [Tooltip("Input Actions del Asset que deben estar habilitadas. " +
             "Arrastra aquí el InputActionAsset del proyecto si las acciones no se habilitan solas.")]
    [SerializeField] private InputActionAsset _inputActionAsset;

    [Tooltip("Nombres de los Action Maps que deben estar habilitados (ej: 'XRI LeftHand', 'XRI RightHand').")]
    [SerializeField] private string[] _actionMapsToEnable = new string[]
    {
        "XRI LeftHand",
        "XRI RightHand",
        "XRI LeftHand Locomotion",
        "XRI RightHand Locomotion",
    };

    // ───────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (_player == null)
            _player = GetComponent<AutoHandPlayer>() ?? FindFirstObjectByType<AutoHandPlayer>();

        if (_player == null)
        {
            Debug.LogWarning("[AutoHandPlayerFixer] No se encontró AutoHandPlayer. Asígnalo en el Inspector.");
            return;
        }

        ApplyRigidbodyFix();
        ApplyMovementSettings();
    }

    private IEnumerator Start()
    {
        // Esperamos un frame para que todos los componentes se inicialicen
        yield return null;
        EnableInputActions();
    }

    // ── Fixes ───────────────────────────────────────────────────────────────

    private void ApplyRigidbodyFix()
    {
        // El AutoHandPlayer tiene un Rigidbody en su transform raíz
        var rb = _player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            if (rb.linearDamping != _playerRigidbodyDrag)
            {
                Debug.Log($"[AutoHandPlayerFixer] Rigidbody drag: {rb.linearDamping} → {_playerRigidbodyDrag}");
                rb.linearDamping = _playerRigidbodyDrag;
            }
            if (rb.angularDamping != _playerRigidbodyAngularDrag)
                rb.angularDamping = _playerRigidbodyAngularDrag;
        }
        else
        {
            Debug.LogWarning("[AutoHandPlayerFixer] No se encontró Rigidbody en AutoHandPlayer.");
        }
    }

    private void ApplyMovementSettings()
    {
        if (_forceEnableMovement && !_player.useMovement)
        {
            Debug.Log("[AutoHandPlayerFixer] useMovement estaba desactivado → activando.");
            _player.useMovement = true;
        }

        _player.rotationType   = _rotationType;
        _player.snapTurnAngle  = _snapTurnAngle;
        _player.smoothTurnSpeed = _smoothTurnSpeed;

        Debug.Log($"[AutoHandPlayerFixer] Movimiento configurado: rotationType={_rotationType}, " +
                  $"snapAngle={_snapTurnAngle}°, smoothSpeed={_smoothTurnSpeed}°/s");
    }

    private void EnableInputActions()
    {
        // Opción A: habilitar desde el InputActionAsset asignado
        if (_inputActionAsset != null)
        {
            int enabled = 0;
            foreach (var mapName in _actionMapsToEnable)
            {
                var map = _inputActionAsset.FindActionMap(mapName, throwIfNotFound: false);
                if (map != null && !map.enabled)
                {
                    map.Enable();
                    enabled++;
                    Debug.Log($"[AutoHandPlayerFixer] Action Map habilitado: '{mapName}'");
                }
            }

            if (enabled == 0)
                Debug.Log("[AutoHandPlayerFixer] Todos los Action Maps ya estaban habilitados o no se encontraron los nombres indicados.");

            return;
        }

        // Opción B: habilitar desde los InputActionProperty de los componentes del rig
        EnableActionsOnComponent<Autohand.Demo.OpenXRHandPlayerControllerLink>(_player.gameObject);
        EnableActionsOnComponent<Autohand.Demo.OpenXRTeleporterLink>(_player.gameObject);
    }

    /// <summary>
    /// Busca componentes del tipo T en el rig del jugador y habilita sus InputActionProperties
    /// mediante reflexión, sin depender de la API privada de cada script.
    /// </summary>
    private void EnableActionsOnComponent<T>(GameObject root) where T : MonoBehaviour
    {
        var components = root.GetComponentsInChildren<T>(true);
        foreach (var comp in components)
        {
            var fields = comp.GetType().GetFields(
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (field.FieldType == typeof(InputActionProperty))
                {
                    var prop = (InputActionProperty)field.GetValue(comp);
                    if (prop.action != null && !prop.action.enabled)
                    {
                        prop.action.Enable();
                        Debug.Log($"[AutoHandPlayerFixer] Habilitada InputAction '{prop.action.name}' en {comp.GetType().Name}");
                    }
                }
            }
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Diagnosticar estado actual")]
    private void DiagnoseCurrentState()
    {
        if (_player == null)
            _player = FindFirstObjectByType<AutoHandPlayer>();

        if (_player == null) { Debug.LogError("No se encontró AutoHandPlayer."); return; }

        var rb = _player.GetComponent<Rigidbody>();

        Debug.Log(
            $"[AutoHandPlayerFixer] ══ DIAGNÓSTICO ══\n" +
            $"  useMovement       : {_player.useMovement}\n" +
            $"  rotationType      : {_player.rotationType}\n" +
            $"  snapTurnAngle     : {_player.snapTurnAngle}\n" +
            $"  smoothTurnSpeed   : {_player.smoothTurnSpeed}\n" +
            $"  maxMoveSpeed      : {_player.maxMoveSpeed}\n" +
            $"  groundedDrag      : {_player.groundedDrag}\n" +
            $"  Rigidbody drag    : {(rb != null ? rb.linearDamping.ToString() : "sin Rigidbody")}\n" +
            $"  Rigidbody angDrag : {(rb != null ? rb.angularDamping.ToString() : "sin Rigidbody")}\n" +
            $"  InputActionAsset  : {(_inputActionAsset != null ? _inputActionAsset.name : "no asignado (usará reflexión)")}"
        );

        // Comprobar si las acciones relevantes están habilitadas
        var links = _player.GetComponentsInChildren<Autohand.Demo.OpenXRHandPlayerControllerLink>(true);
        foreach (var l in links)
        {
            Debug.Log($"  OpenXRHandPlayerControllerLink → " +
                      $"moveAxis enabled: {l.moveAxis.action?.enabled}, " +
                      $"turnAxis enabled: {l.turnAxis.action?.enabled}");
        }

        var tps = _player.GetComponentsInChildren<Autohand.Demo.OpenXRTeleporterLink>(true);
        foreach (var t in tps)
        {
            Debug.Log($"  OpenXRTeleporterLink → " +
                      $"startTeleport enabled: {t.startTeleportAction.action?.enabled}, " +
                      $"finishTeleport enabled: {t.finishTeleportAction.action?.enabled}");
        }
    }
#endif
}
