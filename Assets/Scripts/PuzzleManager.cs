using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    [Header("Shapes to Manage")]
    [SerializeField] private DraggableShape[] allShapes;

    [Header("Sockets")]
    [SerializeField] private MetaSnapFeedback[] sockets;

    [Header("Feedback & Door")]
    [SerializeField] private Transform doorPivot;
    [SerializeField] private float doorOpenAngle = -90f;
    [SerializeField] private float doorOpenDuration = 1.5f;
    [SerializeField] private TextMeshProUGUI errorTextDisplay;
    [SerializeField] private GameObject taskCompletedBanner;

    [Header("Dashboard UI")]
    [SerializeField] private TextMeshProUGUI objectsRemainingText;
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Timer Rules")]
    [SerializeField] private float totalTimeAllowed = 60f;
    [Tooltip("If true, reloads the scene on timeout. If false, resets in-place.")]
    [SerializeField] private bool reloadSceneOnTimeout = false;

    private float timeRemaining;
    private bool isCompleted = false;
    private bool isTimeOutProcessing = false;

    private void Awake()
    {
        Instance = this;

        // Cap framerate for laptop cooling & stability in Editor / Simulator
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 72;
    }

    private void OnEnable()
    {
        MetaSnapFeedback.OnCorrectPlacement += HandleCorrectPlacement;
        MetaSnapFeedback.OnWrongPlacement += HandleWrongPlacement;
    }

    private void OnDisable()
    {
        MetaSnapFeedback.OnCorrectPlacement -= HandleCorrectPlacement;
        MetaSnapFeedback.OnWrongPlacement -= HandleWrongPlacement;
    }

    private void Start()
    {
        RestartFullTask();
    }

    private void Update()
    {
        if (!isCompleted && !isTimeOutProcessing)
        {
            timeRemaining -= Time.deltaTime;

            if (timeRemaining <= 0f)
            {
                timeRemaining = 0f;
                UpdateTimerUI();
                StartCoroutine(HandleTimeOutRestart());
                return;
            }

            UpdateTimerUI();
        }
    }

    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60F);
        int seconds = Mathf.FloorToInt(timeRemaining % 60F);
        if (timerText != null)
        {
            timerText.text = $"{minutes:00}:{seconds:00}";
            timerText.color = timeRemaining <= 10f ? Color.red : Color.white;
        }
    }

    private IEnumerator HandleTimeOutRestart()
    {
        isTimeOutProcessing = true;

        if (errorTextDisplay != null)
        {
            errorTextDisplay.text = "TIME EXPIRED! RESTARTING TASK...";
        }

        yield return new WaitForSeconds(2.0f);

        if (reloadSceneOnTimeout)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        else
        {
            RestartFullTask();
            isTimeOutProcessing = false;
        }
    }

    private void HandleCorrectPlacement()
    {
        UpdateRemainingUI();
        CheckAllSockets();
    }

    private void HandleWrongPlacement(string message)
    {
        if (errorTextDisplay != null)
        {
            StopCoroutine(nameof(ClearErrorAfterSeconds));
            errorTextDisplay.text = message;
            StartCoroutine(ClearErrorAfterSeconds(2.5f));
        }
    }

    private IEnumerator ClearErrorAfterSeconds(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (errorTextDisplay != null) errorTextDisplay.text = "";
    }

    private void UpdateRemainingUI()
    {
        int remaining = 0;
        foreach (var socket in sockets)
        {
            if (socket != null && !socket.IsSolved) remaining++;
        }

        if (objectsRemainingText != null)
        {
            objectsRemainingText.text = remaining.ToString();
        }
    }

    private void CheckAllSockets()
    {
        foreach (var socket in sockets)
        {
            if (socket == null || !socket.IsSolved) return;
        }

        CompleteTask();
    }

    private void CompleteTask()
    {
        isCompleted = true;
        if (taskCompletedBanner != null) taskCompletedBanner.SetActive(true);
        if (doorPivot != null) StartCoroutine(AnimateDoorOpen());
    }

    private IEnumerator AnimateDoorOpen()
    {
        Quaternion initialRotation = doorPivot.localRotation;
        Quaternion targetRotation = Quaternion.Euler(0f, doorOpenAngle, 0f);
        float elapsed = 0f;

        while (elapsed < doorOpenDuration)
        {
            elapsed += Time.deltaTime;
            doorPivot.localRotation = Quaternion.Slerp(initialRotation, targetRotation, elapsed / doorOpenDuration);
            yield return null;
        }
        doorPivot.localRotation = targetRotation;
    }

    /// <summary>
    /// Triggered by the "RESET OBJECTS" button.
    /// Resets all unplaced objects back to their table spawn points without clearing solved sockets or timer.
    /// </summary>
    public void ResetUnplacedObjects()
    {
        if (errorTextDisplay != null) errorTextDisplay.text = "";

        if (allShapes == null || allShapes.Length == 0)
        {
            allShapes = FindObjectsByType<DraggableShape>(FindObjectsSortMode.None);
        }

        foreach (var shape in allShapes)
        {
            if (shape != null) shape.ResetToSpawn();
        }
    }

    /// <summary>
    /// Triggered by the "RESTART TASK" button or 60-Second timeout.
    /// Full wipe: resets sockets, shapes, timer, error message, and closes the door.
    /// </summary>
    public void RestartFullTask()
    {
        isCompleted = false;
        isTimeOutProcessing = false;
        timeRemaining = totalTimeAllowed;

        if (taskCompletedBanner != null) taskCompletedBanner.SetActive(false);
        if (errorTextDisplay != null) errorTextDisplay.text = "";

        if (doorPivot != null) doorPivot.localRotation = Quaternion.identity;

        foreach (var socket in sockets)
        {
            if (socket != null) socket.ResetSocket();
        }

        if (allShapes == null || allShapes.Length == 0)
        {
            allShapes = FindObjectsByType<DraggableShape>(FindObjectsSortMode.None);
        }

        foreach (var shape in allShapes)
        {
            if (shape != null) shape.UnlockAndReset();
        }

        UpdateRemainingUI();
        UpdateTimerUI();
    }
}