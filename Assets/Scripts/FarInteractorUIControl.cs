using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class FarInteractorUIControl : MonoBehaviour
{
    [Header("XR Interaction References")]
    [SerializeField] XRRayInteractor interactorRay;    // Drag komponen “XR Ray Interactor (Action-based)”
    [SerializeField] XRInteractionGroup interactionGroup;  // Drag komponen XR Interaction Group

    [Header("UI Settings")]
    [SerializeField] LayerMask uiLayerMask = 1 << 5;   // Default UI layer (5)
    [SerializeField] float maxDistance = 10f;

    void Update()
    {
        if (interactorRay == null) return;

        // Raycast manual ke arah ray interactor
        Ray ray = new Ray(interactorRay.rayOriginTransform.position, interactorRay.rayOriginTransform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, uiLayerMask))
        {
            // Kalau kena UI, aktifkan interactor
            interactorRay.enabled = true;
            if (interactionGroup != null)
                interactionGroup.enabled = true;
        }
        else
        {
            // Kalau tidak kena UI, matikan
            interactorRay.enabled = false;
            if (interactionGroup != null)
                interactionGroup.enabled = false;
        }
    }
}
