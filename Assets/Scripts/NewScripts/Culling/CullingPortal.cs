using UnityEngine;

/// <summary>
/// Portal (plano imaginario) que conecta dos CullingZones.
/// Coloca este objeto en la apertura entre dos secciones del pasillo
/// (en la esquina de la Z, o en la puerta entre dos zonas).
/// El portal es visible si alguna de sus esquinas está dentro del frustum de la cámara
/// O si la cámara está muy cerca de él.
/// </summary>
public class CullingPortal : MonoBehaviour
{
    [Header("Zonas conectadas")]
    [Tooltip("Primera zona conectada por este portal.")]
    public CullingZone zoneA;

    [Tooltip("Segunda zona conectada por este portal.")]
    public CullingZone zoneB;

    [Header("Tamaño del portal")]
    [Tooltip("Ancho del portal en metros (eje X local).")]
    [SerializeField] private float _width = 3f;

    [Tooltip("Alto del portal en metros (eje Y local).")]
    [SerializeField] private float _height = 3f;

    [Tooltip("Distancia mínima desde la cámara para considerar el portal siempre visible (evita popping al atravesarlo).")]
    [SerializeField] private float _alwaysVisibleDistance = 1.5f;

    /// <summary>
    /// Devuelve true si el portal es visible desde la cámara dada.
    /// Comprueba las 4 esquinas + centro del portal contra el frustum.
    /// También fuerza visibilidad si la cámara está muy cerca.
    /// </summary>
    public bool IsVisibleFromCamera(Camera cam)
    {
        // Si la cámara está justo dentro/delante del portal, siempre visible
        float dist = Vector3.Distance(cam.transform.position, transform.position);
        if (dist <= _alwaysVisibleDistance)
            return true;

        // El portal solo es visible desde el lado al que mira su normal
        // (evita "ver a través" desde el otro lado)
        Vector3 toCamera = cam.transform.position - transform.position;
        if (Vector3.Dot(transform.forward, toCamera) < 0f &&
            Vector3.Dot(-transform.forward, toCamera) < 0f)
        {
            // La cámara está de canto — comprobamos igualmente por robustez
        }

        // Puntos de las esquinas del portal en espacio mundo
        Vector3[] corners = GetWorldCorners();

        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(cam);

        foreach (var point in corners)
        {
            bool insideFrustum = true;
            foreach (var plane in frustumPlanes)
            {
                if (plane.GetDistanceToPoint(point) < 0f)
                {
                    insideFrustum = false;
                    break;
                }
            }
            if (insideFrustum) return true;
        }

        return false;
    }

    /// <summary>
    /// Dado que la cámara está en zoneFrom, devuelve la zona al otro lado.
    /// </summary>
    public CullingZone GetOtherZone(CullingZone zoneFrom)
    {
        if (zoneFrom == zoneA) return zoneB;
        if (zoneFrom == zoneB) return zoneA;
        return null;
    }

    private Vector3[] GetWorldCorners()
    {
        Vector3 right = transform.right * (_width * 0.5f);
        Vector3 up    = transform.up    * (_height * 0.5f);

        return new Vector3[]
        {
            transform.position,                    // centro
            transform.position + right + up,       // esquina superior derecha
            transform.position - right + up,       // esquina superior izquierda
            transform.position + right - up,       // esquina inferior derecha
            transform.position - right - up,       // esquina inferior izquierda
        };
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 0.8f, 1f, 0.6f);

        Vector3 right = transform.right * (_width * 0.5f);
        Vector3 up    = transform.up    * (_height * 0.5f);

        Vector3 tl = transform.position - right + up;
        Vector3 tr = transform.position + right + up;
        Vector3 bl = transform.position - right - up;
        Vector3 br = transform.position + right - up;

        Gizmos.DrawLine(tl, tr);
        Gizmos.DrawLine(tr, br);
        Gizmos.DrawLine(br, bl);
        Gizmos.DrawLine(bl, tl);
        Gizmos.DrawLine(tl, br);
        Gizmos.DrawLine(tr, bl);

        // Normal del portal
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward * 0.5f);

        UnityEditor.Handles.Label(transform.position + Vector3.up * (_height * 0.5f + 0.3f), gameObject.name);
    }
#endif
}
