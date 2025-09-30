using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 3;
    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    public event Action OnDeath;
    // Event that notifies listeners when health changes; provides current and max values
    public event Action<int, int> OnHealthChanged;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        // Invoke initial health state
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    public void TakeDamage(int amount, Vector2 hitPoint, Vector2 hitNormal)
    {
        if (CurrentHealth <= 0) return;
        CurrentHealth -= amount;
        // Notify listeners of health change
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        if (CurrentHealth <= 0) Die();
    }

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    private void Die()
    {
        OnDeath?.Invoke();
        // Ensure UI reflects zero health
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        Destroy(gameObject);
    }
}
