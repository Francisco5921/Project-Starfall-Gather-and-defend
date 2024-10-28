using UnityEngine;
using TMPro;  // Add this to use TextMeshPro

public class DayNightCycle : MonoBehaviour
{
    public Light sun;       // Reference to the Sun light
    public Light moon;      // Reference to the Moon light
    public TextMeshProUGUI clockText;   // Reference to the TextMeshPro clock text UI
    public float dayLength = 24f; // How long a day lasts in minutes
    private float time;           // Tracks the in-game time

    private void Start()
    {
        time = 0;
    }

    private void Update()
{
    // Update the in-game time
    time += Time.deltaTime * (24 / dayLength);
    if (time >= 24) time = 0;

    // Calculate the sun and moon rotation with a 180-degree offset
    float sunRotation = (time / 24) * 360 - 90;
    sun.transform.rotation = Quaternion.Euler(sunRotation, 0, 0);
    moon.transform.rotation = Quaternion.Euler(sunRotation + 180, 0, 0);

    // Display the time in a 12-hour format
    int hours = Mathf.FloorToInt(time);
    int minutes = Mathf.FloorToInt((time - hours) * 60);
    string amPm = hours >= 12 ? "PM" : "AM";
    hours = hours % 12;
    if (hours == 0) hours = 12;

    clockText.text = $"{hours:D2}:{minutes:D2} {amPm}";
}
}


