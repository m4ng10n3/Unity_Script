using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Health : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 3;
    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;

    public event Action OnDeath;

    //  la firma corretta: due interi
    public event Action<int, int> OnHealthChanged;

    private void Awake()
    {
        CurrentHealth = maxHealth;
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
    }

    //  firma a 3 parametri (usata da SwordAttack/PlayerController2D)
    public void TakeDamage(int amount, Vector2 hitPoint, Vector2 hitNormal)
    {
        if (CurrentHealth <= 0) return;

        CurrentHealth -= amount;
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
        OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        Destroy(gameObject);
    }
}
