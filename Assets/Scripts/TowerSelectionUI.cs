using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TowerSelectionUI : MonoBehaviour
{
    [SerializeField] private Image selectedTowerImage; // UI image element to display the selected tower
    [SerializeField] private TextMeshProUGUI towerNameText; // UI text for tower name
    [SerializeField] private TextMeshProUGUI towerStatsText; // UI text for tower stats
    [SerializeField] private PlayerController playerController; // Reference to the PlayerController script

    private void Update()
    {
        UpdateSelectedTowerUI();
    }

    void UpdateSelectedTowerUI()
    {
        // Get the currently selected tower prefab
        GameObject selectedTowerPrefab = playerController.GetSelectedTowerPrefab();
        if (selectedTowerPrefab != null)
        {
            // Get the tower stats and sprite
            TowerStats stats = selectedTowerPrefab.GetComponent<TowerStats>();
            Sprite towerSprite = selectedTowerPrefab.GetComponent<SpriteRenderer>().sprite;

            // Update the tower image
            selectedTowerImage.sprite = towerSprite;
            selectedTowerImage.enabled = true; // Ensure the image is visible

            // Update the tower details
            towerNameText.text = selectedTowerPrefab.name;
            towerStatsText.text =
                $"Cost: {stats.woodCost} Wood, {stats.metalCost} Metal, {stats.fuelCost} Fuel, {stats.electronicsCost} Electronics\n" /* +
                $"Health: {stats.maxHealth}\n" +
                $"Damage: {stats.bulletDamage}\n" +
                $"Fire Rate: {stats.fireRate}" */ ;
        }
        else
        {
            // Hide the image and clear text if no tower is selected
            selectedTowerImage.enabled = false;
            towerNameText.text = string.Empty;
            towerStatsText.text = string.Empty;
        }
    }
}
