using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestor central del sistema de Portal Culling.
/// Colócalo en un GameObject vacío en la escena.
///
/// CÓMO CONFIGURARLO:
/// 1. Divide el pasillo en secciones (ZonaA, ZonaB, ZonaC...).
///    Cada sección tiene un GameObject con CullingZone que agrupa sus Renderers.
/// 2. En cada esquina o apertura entre secciones, crea un GameObject con CullingPortal.
///    Asigna las dos zonas que conecta y ajusta su tamaño para que cubra la abertura.
/// 3. En cada CullingZone, arrastra los portales que la conectan con otras zonas.
/// 4. Arrastra aquí todas las zonas y asigna la cámara del jugador.
/// </summary>
public class ZoneCullingManager : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Cámara principal del jugador (la del headset en VR).")]
    [SerializeField] private Camera _playerCamera;

    [Tooltip("Todas las CullingZones de la escena.")]
    [SerializeField] private List<CullingZone> _allZones = new List<CullingZone>();

    [Header("Configuración")]
    [Tooltip("Cuántos portales en cadena puede atravesar la visibilidad. 1 = solo zona adyacente. 2 = dos zonas de profundidad.")]
    [SerializeField, Range(1, 4)] private int _maxPortalDepth = 2;

    [Tooltip("Cada cuántos frames se actualiza el culling. 1 = cada frame. 2 = cada 2 frames (más barato).")]
    [SerializeField, Range(1, 6)] private int _updateEveryNFrames = 2;

    [Tooltip("Collider que define el volumen de cada zona. Necesario para detectar en qué zona está el jugador.")]
    [SerializeField] private bool _detectZoneByCollider = true;

    // Zona donde se encuentra actualmente el jugador
    private CullingZone _currentZone;
    private int _frameCounter;

    // Cache para evitar GC por frame
    private readonly HashSet<CullingZone> _visibleZones  = new HashSet<CullingZone>();
    private readonly HashSet<CullingZone> _processedZones = new HashSet<CullingZone>();

    private void Awake()
    {
        if (_playerCamera == null)
            _playerCamera = Camera.main;
    }

    private void Start()
    {
        // Comenzar con todo visible para evitar pantalla negra inicial
        foreach (var zone in _allZones)
            zone.SetVisible(true);

        UpdateCurrentZone();
        UpdateCulling();
    }

    private void Update()
    {
        _frameCounter++;
        if (_frameCounter < _updateEveryNFrames) return;
        _frameCounter = 0;

        UpdateCurrentZone();
        UpdateCulling();
    }

    /// <summary>
    /// Detecta en qué zona se encuentra el jugador.
    /// Método 1: por Collider solapado (más preciso).
    /// Método 2: por distancia al centro de la zona (fallback).
    /// </summary>
    private void UpdateCurrentZone()
    {
        Vector3 playerPos = _playerCamera.transform.position;
        CullingZone nearest = null;
        float nearestDist = float.MaxValue;

        foreach (var zone in _allZones)
        {
            if (zone == null) continue;

            if (_detectZoneByCollider)
            {
                var col = zone.GetComponent<Collider>();
                if (col != null && col.bounds.Contains(playerPos))
                {
                    _currentZone = zone;
                    return;
                }
            }

            // Fallback: distancia
            float dist = Vector3.Distance(playerPos, zone.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = zone;
            }
        }

        if (nearest != null)
            _currentZone = nearest;
    }

    /// <summary>
    /// Recorre el grafo de portales desde la zona actual y marca las zonas visibles.
    /// Luego oculta las que no aparecen en el resultado.
    /// </summary>
    private void UpdateCulling()
    {
        if (_currentZone == null) return;

        _visibleZones.Clear();
        _processedZones.Clear();

        // La zona donde está el jugador siempre es visible
        _visibleZones.Add(_currentZone);

        // BFS/DFS a través de portales
        TraversePortals(_currentZone, 0);

        // Aplicar visibilidad a todas las zonas
        foreach (var zone in _allZones)
        {
            if (zone == null) continue;
            zone.SetVisible(_visibleZones.Contains(zone));
        }
    }

    /// <summary>
    /// Recursivamente comprueba los portales de una zona y añade las zonas
    /// visibles al set, hasta el límite de profundidad.
    /// </summary>
    private void TraversePortals(CullingZone zone, int depth)
    {
        if (depth >= _maxPortalDepth) return;
        if (_processedZones.Contains(zone)) return;

        _processedZones.Add(zone);

        foreach (var portal in zone.portals)
        {
            if (portal == null) continue;

            if (!portal.IsVisibleFromCamera(_playerCamera)) continue;

            CullingZone otherZone = portal.GetOtherZone(zone);
            if (otherZone == null || _visibleZones.Contains(otherZone)) continue;

            _visibleZones.Add(otherZone);
            TraversePortals(otherZone, depth + 1);
        }
    }

    /// <summary>
    /// Fuerza visibilidad completa (útil al cambiar de escena o en cinemáticas).
    /// </summary>
    public void ShowAll()
    {
        foreach (var zone in _allZones)
            if (zone != null) zone.SetVisible(true);
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        if (!Application.isPlaying) return;
        GUI.Label(new Rect(10, 10, 300, 20),
            $"Zona actual: {(_currentZone != null ? _currentZone.name : "ninguna")}");
        GUI.Label(new Rect(10, 30, 300, 20),
            $"Zonas visibles: {_visibleZones.Count} / {_allZones.Count}");
    }
#endif
}
