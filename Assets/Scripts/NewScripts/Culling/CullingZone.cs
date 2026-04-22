using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Define una zona del pasillo (sección entre esquinas o habitación).
/// Agrupa los Renderers que deben activarse/desactivarse según visibilidad.
/// </summary>
public class CullingZone : MonoBehaviour
{
    [Header("Contenido de la zona")]
    [Tooltip("Renderers que pertenecen a esta zona. Puedes dejar vacío para auto-recogerlos en los hijos.")]
    [SerializeField] private List<Renderer> _renderers = new List<Renderer>();

    [Tooltip("Si está activo, al iniciar se recogerán todos los Renderers hijos automáticamente.")]
    [SerializeField] private bool _autoCollectChildRenderers = true;

    [Header("Portales conectados")]
    [Tooltip("Portales que conectan esta zona con otras.")]
    [SerializeField] public List<CullingPortal> portals = new List<CullingPortal>();

    // El sistema de culling marca esta zona como visible o no cada frame
    [HideInInspector] public bool isVisible = false;

    private void Awake()
    {
        if (_autoCollectChildRenderers && _renderers.Count == 0)
            _renderers.AddRange(GetComponentsInChildren<Renderer>(true));
    }

    /// <summary>Activa o desactiva todos los renderers de esta zona.</summary>
    public void SetVisible(bool visible)
    {
        if (isVisible == visible) return;
        isVisible = visible;

        foreach (var r in _renderers)
        {
            if (r != null)
                r.enabled = visible;
        }
    }

    /// <summary>Añade un renderer manualmente en runtime si es necesario.</summary>
    public void RegisterRenderer(Renderer r)
    {
        if (!_renderers.Contains(r))
            _renderers.Add(r);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = isVisible ? new Color(0, 1, 0, 0.15f) : new Color(1, 0, 0, 0.15f);
        var bounds = GetComponentInChildren<Collider>();
        if (bounds != null)
            Gizmos.DrawCube(bounds.bounds.center, bounds.bounds.size);
        else
            Gizmos.DrawCube(transform.position, Vector3.one * 2f);

        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, gameObject.name);
    }
#endif
}
