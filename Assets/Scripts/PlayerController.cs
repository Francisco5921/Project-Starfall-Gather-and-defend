using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //Player move speed
    [SerializeField] private float speed = 5f;

    // Player's resources
    public int wood = 200;
    public int metal = 200;
    public int fuel = 200;
    public int electronics = 200;

    // Tower selection and placement
    [SerializeField] private List <GameObject> towerPrefabs;
    private int selectedTowerIndex = 0;
    private float interactRange = 2f;

    // Player's direction
    private Vector2 movement;
    private Vector2 lastDirection = Vector2.down;


    // Update is called once per frame
    void Update()
    {
        PlayerMovement();
        TowerSelection();
        PlayerActions();
    }

    void PlayerActions()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // Build tower
        {
            BuildTower();
        }
        else if (Input.GetKeyDown(KeyCode.F)) // Repair or Gather
        {
            Interact();
        }
    }

    // Interacts with towers
    void Interact()
    {
        // Raycast in front of player
        RaycastHit2D hit = Physics2D.Raycast(transform.position, GetDirectionFacing(), interactRange);

        if (hit.collider != null)
        {
            if (hit.collider.CompareTag("Tower"))
            {
                RepairTower(hit.collider.gameObject); // Repairs if object is tower
            } 
            else if (hit.collider.CompareTag("Resource"))
            {
                GatherResource(hit.collider.gameObject); // Gathers if object is resource
            }
        }
    }

    void GatherResource(GameObject resource)
    {
        // Gets resource stats
        ResourceStats resourceStats = resource.GetComponent<ResourceStats>();

        if (resourceStats != null)
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

    // Scroll though object containing tower prefabs with Q and E
    void TowerSelection()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            selectedTowerIndex = (selectedTowerIndex - 1 + towerPrefabs.Count) % towerPrefabs.Count;
            Debug.Log("Selected Tower: " + towerPrefabs[selectedTowerIndex].name);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            selectedTowerIndex = (selectedTowerIndex + 1) % towerPrefabs.Count;
            Debug.Log("Selected Tower: " + towerPrefabs[selectedTowerIndex].name);
        }

    }


    // Builds a tower in front of the player if they have enough resources
    void BuildTower()
    {
        GameObject selectedTowerPrefab = towerPrefabs[selectedTowerIndex];
        TowerStats towerStats = selectedTowerPrefab.GetComponent<TowerStats>();

        // Check if the player has enough resources to build a tower
        if (wood >= towerStats.woodCost && metal >= towerStats.metalCost && fuel >= towerStats.fuelCost && electronics >= towerStats.electronicsCost )
        {
            Vector2 buildPosition = (Vector2)transform.position + GetDirectionFacing() * interactRange;

            // Check for overlapping with existing towers
            if (Physics2D.OverlapCircle(buildPosition, 0.5f, LayerMask.GetMask("Tower")))
            {
                Debug.Log("Cannot place tower here! There's already a tower in this position.");
                return;
            }

            // Instantiate tower and deduct resources
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


    void RepairTower(GameObject tower)
    {
        TowerStats towerHealth = tower.GetComponent<TowerStats>();

        if (towerHealth != null) // Check if tower is null
        {
            if (towerHealth.GetCurrentHealth() < towerHealth.maxHealth) // Check if current health is below max health
            {
                int requiredWood = towerHealth.RepairCostWood();
                int requiredMetal = towerHealth.RepairCostMetal();
                int requiredFuel = towerHealth.RepairCostFuel();
                int requiredElectronics = towerHealth.RepairCostElectronics();

                if (wood >= requiredWood && metal >= requiredMetal && fuel >= requiredFuel && electronics >= requiredElectronics)
                {
                    // Repair the tower
                    towerHealth.Repair();

                    // Deduct resources used for repair
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
        switch (true)
        {
            case var _ when Input.GetKey(KeyCode.W):
                lastDirection = Vector2.up;
                break;
            case var _ when Input.GetKey(KeyCode.S):
                lastDirection = Vector2.down;
                break;
            case var _ when Input.GetKey(KeyCode.A):
                lastDirection = Vector2.left;
                break;
            case var _ when Input.GetKey(KeyCode.D):
                lastDirection = Vector2.right;
                break;
        }
            return lastDirection;
    }

    void PlayerMovement()
    {
        movement = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;

        if (movement != Vector2.zero)
        {
            lastDirection = movement;
            transform.position += (Vector3)movement * speed * Time.deltaTime;
            FaceDirection(lastDirection);
        }
        else
        {
            FaceDirection(lastDirection);
        }
    }

    void FaceDirection(Vector2 direction)
    {
        if (direction != Vector2.zero)
        {
            // Calculate angle for rotation 
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
    }

}
