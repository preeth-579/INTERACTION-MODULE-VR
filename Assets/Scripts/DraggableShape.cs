using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using System.Collections;
using UnityEngine;

public class DraggableShape : MonoBehaviour
{
    public enum ShapeType { Cube, Sphere, Cylinder }

    [Header("Configuration")]
    [SerializeField] private ShapeType shapeType;
    public ShapeType Type => shapeType;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Rigidbody rb;
    private Grabbable grabbable;
    private GrabInteractable grabInteractable;
    private HandGrabInteractable handGrabInteractable;

    private bool isLocked = false;
    public bool IsLocked => isLocked;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        grabbable = GetComponent<Grabbable>();
        grabInteractable = GetComponent<GrabInteractable>();
        handGrabInteractable = GetComponent<HandGrabInteractable>();

        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    private void Update()
    {
        // Safety: If shape falls off the table below Y = 0, auto-respawn
        if (!isLocked && transform.position.y < 0.2f)
        {
            ResetToSpawn();
        }
    }

    public void LockToSocket(Transform targetSnapPoint)
    {
        isLocked = true;
        StopAllCoroutines();

        // Sever grab interaction completely
        if (grabbable != null) grabbable.enabled = false;
        if (grabInteractable != null) grabInteractable.enabled = false;
        if (handGrabInteractable != null) handGrabInteractable.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        transform.position = targetSnapPoint.position;
        transform.rotation = targetSnapPoint.rotation;
    }

    public void ResetToSpawn()
    {
        if (isLocked) return;
        StartCoroutine(HardResetRoutine());
    }

    public void UnlockAndReset()
    {
        isLocked = false;
        StopAllCoroutines();
        StartCoroutine(HardResetRoutine());
    }

    private IEnumerator HardResetRoutine()
    {
        // 1. Disable interactors to force Meta SDK to release grab hold
        if (grabbable != null) grabbable.enabled = false;
        if (grabInteractable != null) grabInteractable.enabled = false;
        if (handGrabInteractable != null) handGrabInteractable.enabled = false;

        // 2. Clear physics forces
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 3. Move back to spawn
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        // 4. Wait one frame for SDK input buffers to clear
        yield return new WaitForFixedUpdate();

        // 5. Ensure position sticks
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        // 6. Restore grabbing only if not permanently locked
        if (!isLocked)
        {
            if (grabbable != null) grabbable.enabled = true;
            if (grabInteractable != null) grabInteractable.enabled = true;
            if (handGrabInteractable != null) handGrabInteractable.enabled = true;
        }
    }
}