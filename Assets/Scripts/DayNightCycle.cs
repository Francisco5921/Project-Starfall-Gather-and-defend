using UnityEngine;
using System.Collections;
using TMPro;  // Add this to use TextMeshPro

public class DayNightCycle : MonoBehaviour
{
    public GameObject nightOverlay;
    public TextMeshProUGUI clockText;   // Reference to the TextMeshPro clock text UI
    public TextMeshProUGUI daysText;
    public float dayLength = 24f; // How long a day lasts in seconds
    private float time;           // Tracks the in-game time
    public int daySurvived = 1;

    public GameManager gameManager;     // Reference to GameManager
    [SerializeField] private GameObject[] resourcePrefabs;   // Resource prefab to spawn
    private readonly float resourceSpawnInterval = 3f;


    public Transform player;   // Reference to the player’s transform
    public Vector3 offset;     // Offset position of the camera relative to the player
    private bool nightStarted = false;

    private void Start()
    {
        time = 0;
        offset = transform.position - player.position; // Set the initial offset
        StartCoroutine(SpawnResources());
    }

    private void Update()
{
    // Update the in-game time
    time += Time.deltaTime * (24 / dayLength);
        if (time >= 24)
        {
            time = 0;
            daySurvived++;
        }

    // Display the time in a 12-hour format
    daysText.text = "Day: " + daySurvived;
    int hours = Mathf.FloorToInt(time);
    int minutes = Mathf.FloorToInt((time - hours) * 60);
    string amPm = hours >= 12 ? "PM" : "AM";
    hours %= 12;
    if (hours == 0) hours = 12;

    clockText.text = $"{hours:D2}:{minutes:D2} {amPm}";


    if (hours == 7 && amPm == "PM" && !nightStarted)
        {
            nightStarted = true;
            nightOverlay.SetActive(true);
            gameManager.SendWave();
        }

        // Reset the flag when it becomes AM to allow the wave to start again the next day
    if (hours == 5 && amPm == "AM")
        {
            nightStarted = false;
            nightOverlay.SetActive(false);

        }
    }

    private void LateUpdate()
    {
        // Keep the camera's rotation fixed
        transform.position = player.position + offset;
    }

    private IEnumerator SpawnResources()
    {
        while (true)
        {
            yield return new WaitForSeconds(resourceSpawnInterval);

            if (!nightStarted)
            {
                SpawnResourceAtRandomPosition();
            }
        }
    }

    private void SpawnResourceAtRandomPosition()
    {
        Vector3 spawnPosition = GetRandomPositionBeyondCamera();
        GameObject prefab = resourcePrefabs[Random.Range(0, resourcePrefabs.Length)];
        Instantiate(prefab, spawnPosition, Quaternion.identity);
    }

    private Vector3 GetRandomPositionBeyondCamera()
    {
        Camera mainCamera = Camera.main;
        float cameraHeight = 2f * mainCamera.orthographicSize;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        float xOffset = Random.Range(cameraWidth / 2, cameraWidth / 2 + 5f) * (Random.value > 0.5f ? 1 : -1);
        float yOffset = Random.Range(cameraHeight / 2, cameraHeight / 2 + 5f) * (Random.value > 0.5f ? 1 : -1);

        Vector3 playerPosition = player.position;
        return new Vector3(playerPosition.x + xOffset, playerPosition.y + yOffset, 0);
    }

  /*  public void ShowHUD()
    {
        clockText.gameObject.SetActive(true);
    }

    public void HideHUD()
    {
        clockText.gameObject.SetActive(false);
    }
  */

}


