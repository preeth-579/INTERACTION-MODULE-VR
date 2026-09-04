using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MetaSnapFeedback : MonoBehaviour
{
    [Header("Validation")]
    [SerializeField] private DraggableShape.ShapeType requiredType;

    [Header("Visuals & Materials")]
    [SerializeField] private MeshRenderer socketRenderer;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material successMaterial;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip successClip;
    [SerializeField] private AudioClip errorClip;

    [Header("Snap Point")]
    [SerializeField] private Transform snapPoint;
    [SerializeField] private float snapDistanceThreshold = 0.18f;

    public bool IsSolved { get; private set; } = false;
    public static event Action<string> OnWrongPlacement;
    public static event Action OnCorrectPlacement;

    private AudioSource audioSource;
    private float lastWrongTriggerTime = 0f;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null) audioSource.playOnAwake = false;

        if (socketRenderer == null)
            socketRenderer = GetComponent<MeshRenderer>();

        if (snapPoint == null)
            snapPoint = transform;

        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;

        ResetSocket();
    }

    private void Update()
    {
        if (IsSolved) return;

        // Proximity detection fallback: guarantees detection even if physics triggers skip
        Collider[] hits = Physics.OverlapSphere(transform.position, snapDistanceThreshold);
        foreach (var hit in hits)
        {
            DraggableShape shape = hit.GetComponentInParent<DraggableShape>();
            if (shape != null && !shape.IsLocked)
            {
                EvaluateShape(shape);
                break;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsSolved) return;

        DraggableShape shape = other.GetComponentInParent<DraggableShape>();
        if (shape != null && !shape.IsLocked)
        {
            EvaluateShape(shape);
        }
    }

    private void EvaluateShape(DraggableShape shape)
    {
        if (shape.Type == requiredType)
        {
            PlaceSuccess(shape);
        }
        else
        {
            if (Time.time - lastWrongTriggerTime > 1.2f)
            {
                lastWrongTriggerTime = Time.time;
                PlaySound(errorClip);
                OnWrongPlacement?.Invoke($"Incorrect! {shape.Type} does not fit here.");
                shape.ResetToSpawn();
            }
        }
    }

    private void PlaceSuccess(DraggableShape shape)
    {
        IsSolved = true;
        shape.LockToSocket(snapPoint != null ? snapPoint : transform);

        if (socketRenderer != null && successMaterial != null)
        {
            socketRenderer.material = successMaterial;
        }

        PlaySound(successClip);
        OnCorrectPlacement?.Invoke();
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public void ResetSocket()
    {
        IsSolved = false;
        if (socketRenderer != null && defaultMaterial != null)
        {
            socketRenderer.material = defaultMaterial;
        }
    }
}