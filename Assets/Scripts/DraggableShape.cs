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
    private Transform initialParent;

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
        initialParent = transform.parent;
    }

    private void Update()
    {
        // Auto-respawn if dropped off table
        if (!isLocked && transform.position.y < 0.2f)
        {
            ResetToSpawn();
        }
    }

    public void LockToSocket(Transform targetSnapPoint)
    {
        isLocked = true;
        StopAllCoroutines();

        // 1. Disable grabbable components to sever active hand/controller grip
        if (grabbable != null) grabbable.enabled = false;
        if (grabInteractable != null) grabInteractable.enabled = false;
        if (handGrabInteractable != null) handGrabInteractable.enabled = false;

        // 2. Freeze physics completely
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // 3. Snap and lock transform
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
        if (grabbable != null) grabbable.enabled = false;
        if (grabInteractable != null) grabInteractable.enabled = false;
        if (handGrabInteractable != null) handGrabInteractable.enabled = false;

        transform.SetParent(initialParent);

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        transform.position = initialPosition;
        transform.rotation = initialRotation;

        yield return new WaitForFixedUpdate();

        transform.position = initialPosition;
        transform.rotation = initialRotation;

        if (!isLocked)
        {
            if (grabbable != null) grabbable.enabled = true;
            if (grabInteractable != null) grabInteractable.enabled = true;
            if (handGrabInteractable != null) handGrabInteractable.enabled = true;
        }
    }
}