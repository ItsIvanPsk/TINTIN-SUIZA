using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema de Occlusion Culling automático por raycasts.
///
/// SETUP CORRECTO DE LAYERS:
/// ─────────────────────────────────────────────────────────────────────────
/// Layer "Occluder"      → SOLO paredes, suelos y techos (la geometría que tapa).
///                         Estos objetos DEBEN tener Collider para que los rayos los detecten.
///
/// Layer "CullingTarget" → Muebles, props, decoración (lo que quieres ocultar).
///                         O deja Target Layers en "Everything" y solo excluye los Occluders.
///
/// Layer "CullingIgnore" → Jugador, manos, UI, partículas (nunca se ocultan).
///
/// PROBLEMA MÁS COMÚN: si asignas "Occluder" a TODO el entorno (paredes Y muebles),
/// los muebles quedan excluidos de la lista de targets. Usa capas separadas.
/// Pulsa "Diagnosticar configuración" (rueda dentada del componente) para ver qué pasa.
/// ─────────────────────────────────────────────────────────────────────────
/// </summary>
public class AutoOcclusionCuller : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Cámara principal del jugador (headset en VR).")]
    [SerializeField] private Camera _playerCamera;

    [Header("Layers")]
    [Tooltip("Layer(s) que actúan como occluders (paredes, techos, suelos). Deben tener Collider.")]
    [SerializeField] private LayerMask _occluderLayers;

    [Tooltip("Layer(s) de los objetos que queremos cullear. -1 / Everything = todos excepto Occluders e Ignored.")]
    [SerializeField] private LayerMask _targetLayers = ~0;

    [Tooltip("Layer(s) que NUNCA se ocultarán (jugador, UI, efectos...).")]
    [SerializeField] private LayerMask _ignoreLayers;

    [Tooltip("Actívalo si usas UNA SOLA layer para todo el entorno (paredes Y props).\n" +
             "Los objetos en la layer Occluder también se incluirán como targets de culling.")]
    [SerializeField] private bool _cullOccludersAlso = false;

    [Header("Rendimiento")]
    [Tooltip("Distancia máxima. Objetos más lejos siempre se ocultan.")]
    [SerializeField] private float _maxDistance = 50f;

    [Tooltip("Actualizar cada N frames para reducir coste. 1 = cada frame.")]
    [SerializeField, Range(1, 8)] private int _updateEveryNFrames = 3;

    [Tooltip("Número de rayos por objeto.\nLow=1 (solo centro). Medium=5 (centro+esquinas). High=9.")]
    [SerializeField] private OcclusionQuality _quality = OcclusionQuality.Medium;

    [Tooltip("Margen de los puntos de muestra respecto al bounds (0 a 1).")]
    [SerializeField, Range(0f, 1f)] private float _boundsInset = 0.85f;

    [Tooltip("Si está activo, también aplica frustum culling.")]
    [SerializeField] private bool _applyFrustumCulling = true;

    [Header("Debug")]
    [SerializeField] private bool _showDebugRays = false;
    [SerializeField] private bool _showStats = true;

    public enum OcclusionQuality { Low = 1, Medium = 5, High = 9 }

    private struct RendererEntry { public Renderer renderer; }

    private readonly List<RendererEntry> _entries = new List<RendererEntry>();
    private readonly Plane[] _frustumPlanes = new Plane[6];
    private int _frameCounter;
    private int _visibleCount;

    // ── Ciclo de vida ───────────────────────────────────────────────────────

    private void Awake()
    {
        if (_playerCamera == null)
            _playerCamera = Camera.main;
    }

    private IEnumerator Start()
    {
        yield return null;
        CollectRenderers();
    }

    private void Update()
    {
        if (_entries.Count == 0) return;
        _frameCounter++;
        if (_frameCounter < _updateEveryNFrames) return;
        _frameCounter = 0;
        PerformCulling();
    }

    // ── Recolección ─────────────────────────────────────────────────────────

    public void CollectRenderers()
    {
        _entries.Clear();
        Renderer[] all = FindObjectsByType<Renderer>(FindObjectsSortMode.None);

        int skippedIgnore = 0, skippedOccluder = 0, skippedTarget = 0;

        foreach (var r in all)
        {
            if (r == null) continue;
            int bit = 1 << r.gameObject.layer;

            if ((_ignoreLayers.value & bit) != 0)               { skippedIgnore++;   continue; }
            bool isOccluder = (_occluderLayers.value & bit) != 0;
            if (isOccluder && !_cullOccludersAlso)              { skippedOccluder++; continue; }
            if (_targetLayers.value != -1 && (_targetLayers.value & bit) == 0) { skippedTarget++;  continue; }

            _entries.Add(new RendererEntry { renderer = r });
        }

        Debug.Log($"[OcclusionCuller] Targets: {_entries.Count} | Ignorados: {skippedIgnore} | " +
                  $"Occluders excluidos: {skippedOccluder} | Fuera de target layer: {skippedTarget}");

        if (_entries.Count == 0)
            Debug.LogWarning("[OcclusionCuller] Sin targets. Usa el ContextMenu 'Diagnosticar configuración'.");
    }

    // ── Culling ─────────────────────────────────────────────────────────────

    private void PerformCulling()
    {
        Vector3 camPos = _playerCamera.transform.position;
        _visibleCount = 0;

        if (_applyFrustumCulling)
            GeometryUtility.CalculateFrustumPlanes(_playerCamera, _frustumPlanes);

        for (int i = _entries.Count - 1; i >= 0; i--)
        {
            var e = _entries[i];
            if (e.renderer == null) { _entries.RemoveAt(i); continue; }

            bool vis = IsVisible(e.renderer, camPos);
            e.renderer.enabled = vis;
            if (vis) _visibleCount++;
        }
    }

    private bool IsVisible(Renderer r, Vector3 camPos)
    {
        Bounds b = r.bounds;
        if (Vector3.Distance(camPos, b.center) > _maxDistance) return false;
        if (_applyFrustumCulling && !GeometryUtility.TestPlanesAABB(_frustumPlanes, b)) return false;
        return !IsOccluded(b, camPos);
    }

    private bool IsOccluded(Bounds b, Vector3 camPos)
    {
        foreach (var point in GetSamplePoints(b))
        {
            Vector3 dir = point - camPos;
            float dist = dir.magnitude;
            if (dist < 0.01f) return false;

            bool blocked = Physics.Raycast(camPos, dir / dist, dist - 0.05f, _occluderLayers);
            if (_showDebugRays) Debug.DrawRay(camPos, dir, blocked ? Color.red : Color.green, Time.deltaTime);
            if (!blocked) return false;
        }
        return true;
    }

    private Vector3[] GetSamplePoints(Bounds b)
    {
        Vector3 c = b.center, ex = b.extents * _boundsInset;
        switch (_quality)
        {
            case OcclusionQuality.Low: return new[] { c };
            case OcclusionQuality.Medium: return new[]
            {
                c,
                c + new Vector3( ex.x,  ex.y,  ex.z), c + new Vector3(-ex.x,  ex.y, -ex.z),
                c + new Vector3( ex.x, -ex.y, -ex.z), c + new Vector3(-ex.x, -ex.y,  ex.z),
            };
            default: return new[]
            {
                c,
                c + new Vector3( ex.x,  ex.y,  ex.z), c + new Vector3(-ex.x,  ex.y,  ex.z),
                c + new Vector3( ex.x,  ex.y, -ex.z), c + new Vector3(-ex.x,  ex.y, -ex.z),
                c + new Vector3( ex.x, -ex.y,  ex.z), c + new Vector3(-ex.x, -ex.y,  ex.z),
                c + new Vector3( ex.x, -ex.y, -ex.z), c + new Vector3(-ex.x, -ex.y, -ex.z),
            };
        }
    }

    // ── API pública ─────────────────────────────────────────────────────────

    public void ForceShowAll()
    {
        foreach (var e in _entries)
            if (e.renderer != null) e.renderer.enabled = true;
    }

    public void RefreshRenderers() => CollectRenderers();

    // ── Diagnóstico ─────────────────────────────────────────────────────────

    [ContextMenu("Diagnosticar configuración")]
    private void DiagnoseSetup()
    {
        Renderer[] all = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        int onOccluder = 0, onIgnore = 0, wouldTarget = 0, noCollider = 0;
        var breakdown = new Dictionary<string, int>();

        foreach (var r in all)
        {
            if (r == null) continue;
            int bit = 1 << r.gameObject.layer;
            string ln = LayerMask.LayerToName(r.gameObject.layer);
            if (!breakdown.ContainsKey(ln)) breakdown[ln] = 0;
            breakdown[ln]++;

            bool isOccluder = (_occluderLayers.value & bit) != 0;
            bool isIgnored  = (_ignoreLayers.value   & bit) != 0;
            bool inTarget   = _targetLayers.value == -1 || (_targetLayers.value & bit) != 0;

            if (isOccluder) { onOccluder++; if (r.GetComponent<Collider>() == null) noCollider++; }
            if (isIgnored) onIgnore++;
            if (!isIgnored && (!isOccluder || _cullOccludersAlso) && inTarget) wouldTarget++;
        }

        string bd = "";
        foreach (var kv in breakdown) bd += $"\n    '{kv.Key}': {kv.Value} renderers";

        Debug.Log(
            $"[OcclusionCuller] ══ DIAGNÓSTICO ══\n" +
            $"  Total renderers           : {all.Length}\n" +
            $"  En layer Occluder         : {onOccluder}\n" +
            $"  En layer Ignore           : {onIgnore}\n" +
            $"  Serían targets            : {wouldTarget}\n" +
            $"  Occluder layer mask       : {_occluderLayers.value}  (0 = NO asignado en Inspector)\n" +
            $"  Target layer mask         : {_targetLayers.value}  (-1 = Everything)\n" +
            $"  Occluders sin Collider    : {noCollider}  (los rayos los atravesarán)\n" +
            $"\n  Renderers por layer:{bd}"
        );

        if (_occluderLayers.value == 0)
            Debug.LogError("[OcclusionCuller] ► Occluder Layers VACÍO. Asigna la layer 'Occluder' en el Inspector.");

        if (wouldTarget == 0)
            Debug.LogError("[OcclusionCuller] ► Sin targets.\n" +
                           "  Causa: todos los objetos están en 'Occluder' y quedan excluidos.\n" +
                           "  SOLUCIÓN A: activa 'Cull Occluders Also'.\n" +
                           "  SOLUCIÓN B: pon paredes en 'Occluder' y props/muebles en 'Default'.");

        if (noCollider > 0)
            Debug.LogWarning($"[OcclusionCuller] ► {noCollider} Occluders sin Collider. Los rayos los atravesarán.");
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (!_showStats || !Application.isPlaying) return;
        GUI.Label(new Rect(10, 10, 400, 20),
            $"[OcclusionCuller] Renderizando: {_visibleCount} / {_entries.Count}");
    }
#endif
}
