using UnityEngine;
using Unity.XR.CoreUtils;

public class HeightCalibration : MonoBehaviour
{
    public XROrigin xrOrigin;
    public float baseHeight;
    public float maxHeight;
    public float minHeight;
    public float cameraYOffset;
    public bool Subido = false;

    void Start() {
        var adjustment = PlayerPrefs.GetFloat("PlayerAdjustation", 10);
        if (adjustment == 10) { return; }
        if (adjustment == 0.15f) {
            AdjustXROriginHeight(1);
        } else if (adjustment == -0.15f) {
            AdjustXROriginHeight(0);
        }
    }

    void Update()
    {
        var primaryBtn = Input.GetButtonDown("XRI_Right_PrimaryButton");
        var secondaryBtn = Input.GetButtonDown("XRI_Right_SecondaryButton");

        if (secondaryBtn && !Subido) {
            Subido = true;
            AdjustXROriginHeight(0);
        }
        if (primaryBtn && Subido) {
            Subido = false;
            AdjustXROriginHeight(1);
        }
    }

    void AdjustXROriginHeight(int direction)
    {
        Vector3 currentPosition = xrOrigin.transform.position;
        float newYPosition;

        if (direction == 0) {
            newYPosition = Mathf.Min(maxHeight, currentPosition.y + 0.15f);
            SaveHeightAdjustation(0.15f);
        }
        else {
            newYPosition = Mathf.Min(maxHeight, currentPosition.y - 0.10f);
            SaveHeightAdjustation(-0.10f);
        }
        
        xrOrigin.transform.position = new Vector3(currentPosition.x, newYPosition, currentPosition.z);
    }

    void SaveHeightAdjustation(float heightAdjustation) {
        PlayerPrefs.SetFloat("PlayerAdjustation", heightAdjustation);
        PlayerPrefs.Save();
    }
}
