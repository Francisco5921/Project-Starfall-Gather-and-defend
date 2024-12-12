using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI; // For UI elements like Image and TextMeshProUGUI

public class PlayerController : MonoBehaviour
{
    // Player move speed
    [SerializeField] private float speed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    // Animator for handling animations
    private Animator animator;
    private Vector2 lastDirection = Vector2.down; // Default facing direction

    // Player's resources
    public int wood = 200;
    public int metal = 200;
    public int fuel = 200;
    public int electronics = 200;

    public TextMeshProUGUI playerResources;

    // Tower selection and placement
    [SerializeField] private List<GameObject> towerPrefabs;
    private int selectedTowerIndex = 0;
    private readonly float interactRange = 2f;

    // Tutorial elements
    [SerializeField] private GameObject hintPanel;      // The panel showing the hint
    [SerializeField] private GameObject controlsPanel;  // The panel showing the controls and rules

    // Track the current state
    private bool isShowingControls = false;

    void Start()
    {
        // Assign Rigidbody2D and Animator components
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); // Ensure the Animator is assigned

        // Ensure the hint panel is visible and controls panel is hidden at the start
        if (hintPanel != null) hintPanel.SetActive(true);
        if (controlsPanel != null) controlsPanel.SetActive(false);
    }

    // Input action callback for toggling the tutorial panels
    public void OnToggleTutorial(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (hintPanel != null && controlsPanel != null)
            {
                // Toggle the state
                isShowingControls = !isShowingControls;

                // Show/hide the appropriate panels
                hintPanel.SetActive(!isShowingControls);
                controlsPanel.SetActive(isShowingControls);
            }
        }
    }

    void Update()
    {
        // Update resource display
        playerResources.text = $"Wood: {wood:D2}<br>" +
                               $"Metal: {metal:D2}<br>" +
                               $"Fuel: {fuel:D2}<br>" +
                               $"Electronics: {electronics:D2}<br>";

        HandleAnimations();
    }

    void FixedUpdate()
    {
        // Apply movement in FixedUpdate for consistent physics updates
        rb.velocity = moveInput * speed;
    }

    // Movement callback from the Input System
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();

        // Update the last direction if the player is moving
        if (moveInput != Vector2.zero)
        {
            lastDirection = moveInput.normalized;
        }
    }

    void HandleAnimations()
    {
        // Check if the animator is assigned to avoid NullReferenceException
        if (animator == null)
        {
            Debug.LogError("Animator is not assigned!");
            return;
        }

        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);
        animator.SetBool("isWalking", moveInput != Vector2.zero);

        if (moveInput == Vector2.zero)
        {
            animator.SetFloat("LastInputX", lastDirection.x);
            animator.SetFloat("LastInputY", lastDirection.y);
        }
    }

    void InteractWithTower(RaycastHit2D hit)
    {
        if (hit.collider.CompareTag("Tower"))
        {
            RepairTower(hit.collider.gameObject); // Repairs if object is a tower
        }
    }

    void InteractWithResource(RaycastHit2D hit)
    {
        if (hit.collider.CompareTag("Resource"))
        {
            GatherResource(hit.collider.gameObject); // Gathers if object is a resource
        }
    }

    void Interact()
    {
        // Raycast in front of the player
        RaycastHit2D hit = Physics2D.Raycast(transform.position, GetDirectionFacing(), interactRange, LayerMask.GetMask("Resource"));

        if (hit.collider != null)
        {
            InteractWithTower(hit);
            InteractWithResource(hit);
        }
        else
        {
            Debug.Log("No interactable object found nearby.");
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Interact();
        }
    }

    void GatherResource(GameObject resource)
    {
        if (resource.TryGetComponent<ResourceStats>(out ResourceStats resourceStats))
        {
            int gatheredAmount = resourceStats.Gather();
            AddResource(resourceStats.resourceType, gatheredAmount);

            Debug.Log($"Gathered {gatheredAmount} {resourceStats.resourceType}. Remaining health: {resourceStats.GetCurrentHealth()}");
        }
    }

    void AddResource(string resourceType, int amount)
    {
        switch (resourceType)
        {
            case "Wood":
                wood += amount;
                break;
            case "Metal":
                metal += amount;
                break;
            case "Fuel":
                fuel += amount;
                break;
            case "Electronics":
                electronics += amount;
                break;
            default:
                Debug.LogWarning("Unknown resource type.");
                break;
        }
    }

    public void OnSelectPreviousTower(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            selectedTowerIndex = (selectedTowerIndex - 1 + towerPrefabs.Count) % towerPrefabs.Count;
            Debug.Log("Selected Tower: " + towerPrefabs[selectedTowerIndex].name);
        }
    }

    public void OnSelectNextTower(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            selectedTowerIndex = (selectedTowerIndex + 1) % towerPrefabs.Count;
            Debug.Log("Selected Tower: " + towerPrefabs[selectedTowerIndex].name);
        }
    }

    void BuildTower()
    {
        GameObject selectedTowerPrefab = towerPrefabs[selectedTowerIndex];
        TowerStats towerStats = selectedTowerPrefab.GetComponent<TowerStats>();

        if (wood >= towerStats.woodCost && metal >= towerStats.metalCost &&
            fuel >= towerStats.fuelCost && electronics >= towerStats.electronicsCost)
        {
            Vector2 buildPosition = (Vector2)transform.position + GetDirectionFacing() * interactRange;

            if (Physics2D.OverlapCircle(buildPosition, 0.5f, LayerMask.GetMask("Tower")))
            {
                Debug.Log("Cannot place tower here! There's already a tower in this position.");
                return;
            }

            Instantiate(selectedTowerPrefab, buildPosition, Quaternion.identity);
            wood -= towerStats.woodCost;
            metal -= towerStats.metalCost;
            fuel -= towerStats.fuelCost;
            electronics -= towerStats.electronicsCost;

            Debug.Log("Tower built in front of the player!");
        }
        else
        {
            Debug.Log("Not enough resources to build a tower!");
        }
    }

    public void OnBuild(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            BuildTower();
        }
    }

    void RepairTower(GameObject tower)
    {
        if (tower.TryGetComponent<TowerStats>(out TowerStats towerHealth))
        {
            if (towerHealth.GetCurrentHealth() < towerHealth.maxHealth)
            {
                int requiredWood = towerHealth.RepairCostWood();
                int requiredMetal = towerHealth.RepairCostMetal();
                int requiredFuel = towerHealth.RepairCostFuel();
                int requiredElectronics = towerHealth.RepairCostElectronics();

                if (wood >= requiredWood && metal >= requiredMetal && fuel >= requiredFuel && electronics >= requiredElectronics)
                {
                    towerHealth.Repair();
                    wood -= requiredWood;
                    metal -= requiredMetal;
                    fuel -= requiredFuel;
                    electronics -= requiredElectronics;

                    Debug.Log("Tower repaired!");
                }
                else
                {
                    Debug.Log("Not enough resources to repair the tower!");
                }
            }
        }
    }

    Vector2 GetDirectionFacing()
    {
        return lastDirection;
    }

    public int GetSelectedTowerIndex()
    {
        return selectedTowerIndex;
    }

    public GameObject GetSelectedTowerPrefab()
    {
        return towerPrefabs[selectedTowerIndex];
    }

    public int GetTotalResources()
    {
        return wood + metal + fuel + electronics;
    }
}
