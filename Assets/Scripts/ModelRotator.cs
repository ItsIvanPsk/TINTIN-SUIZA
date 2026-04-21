using UnityEngine;

public class ModelRotator : MonoBehaviour
{
    [SerializeField]
    private float rotateSpeed;

    [SerializeField]
    private GameObject Model;

    [SerializeField]
    private Vector3 rotationDirection;

    private void Update()
    {
        gameObject.transform.Rotate(rotateSpeed * Time.deltaTime * rotationDirection);
    }
}
