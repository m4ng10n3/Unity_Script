using UnityEngine;
using UnityEngine.UI;

// HealthBarUI updates a UI Image to reflect the health of a target entity.
// It listens for health change and death events from the Health component and updates the bar accordingly.
public class HealthBarUI : MonoBehaviour
{
    [Tooltip("Health component to monitor")] 
    [SerializeField] private Health targetHealth;

    [Tooltip("Image component used to display the fill amount of the health bar")] 
    [SerializeField] private Image healthFillImage;

    private void OnEnable()
    {
        if (targetHealth != null)
        {
            // Subscribe to health change and death events
            targetHealth.OnHealthChanged += HandleHealthChanged;
            targetHealth.OnDeath += HandleDeath;
        }
    }

    private void Start()
    {
        // Initialize bar with current health at start
        if (targetHealth != null)
        {
            UpdateBar(targetHealth.CurrentHealth, targetHealth.MaxHealth);
        }
    }

    private void OnDisable()
    {
        if (targetHealth != null)
        {
            // Unsubscribe from events to avoid memory leaks
            targetHealth.OnHealthChanged -= HandleHealthChanged;
            targetHealth.OnDeath -= HandleDeath;
        }
    }

    // Event handler invoked when health changes
    private void HandleHealthChanged(int current, int max)
    {
        UpdateBar(current, max);
    }

    // Event handler invoked when the target dies
    private void HandleDeath()
    {
        // Optionally hide the health bar upon death
        gameObject.SetActive(false);
    }

    // Updates the fill amount of the UI image
    private void UpdateBar(int current, int max)
    {
        if (healthFillImage != null)
        {
            float fill = max > 0 ? (float)current / max : 0f;
            healthFillImage.fillAmount = fill;
        }
    }
}
