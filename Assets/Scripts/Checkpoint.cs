using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class CollisionEvent : UnityEvent<Collision> { }

public class Checkpoint : MonoBehaviour
{

    public CollisionEvent OnCollisionEnterEvent;
    public CollisionEvent OnCollisionExitEvent;

    private void OnCollisionEnter(Collision other)
    {
        if (gameObject.CompareTag("Checkpoint")) {
            OnCollisionEnterEvent?.Invoke(other);
            DisableCheckpoint(other.gameObject);
        }
    }

    private void DisableCheckpoint(GameObject col)
    {
        gameObject.GetComponent<BoxCollider>().enabled = false;
        gameObject.GetComponent<MeshRenderer>().enabled = false;
    }

}
