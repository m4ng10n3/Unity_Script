using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 3;
    public int CurrentHealth { get; private set; }
    public event Action OnDeath;

    private void Awake()
    {
        CurrentHealth = maxHealth;
    }

    public void TakeDamage(int amount, Vector2 hitPoint, Vector2 hitNormal)
    {
        if (CurrentHealth <= 0) return;
        CurrentHealth -= amount;
        if (CurrentHealth <= 0) Die();
    }

    public void Heal(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, maxHealth);
    }

    private void Die()
    {
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}
