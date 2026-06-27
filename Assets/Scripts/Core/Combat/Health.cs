using System;
using UnityEngine;
using IT.Core;

namespace IT.Core.Combat
{
    public class Health : MonoBehaviour, IHealthSource, IDamageable
    {
        [SerializeField] int _maxHealth = 10;
        [SerializeField] float _iFramesDuration = 0.5f;

        int _currentHealth;
        float _iFramesEnd;

        public int Current => _currentHealth;
        public int Max => _maxHealth;
        public float IFramesDuration => _iFramesDuration;   // read-only: damage VFX mirrors the i-frame window

        public event Action<int, int> HealthChanged;
        public event Action HealthDepleted;

        void Start()
        {
            _currentHealth = _maxHealth;
            HealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        public void Damage(int amount)
        {
            if (Time.time < _iFramesEnd) return;
            _iFramesEnd = Time.time + _iFramesDuration;
            ApplyDamageInternal(amount);
        }

        // Damage-over-time entry (Story 4.4 Poison): NO i-frame read or write. I-frames debounce
        // discrete hits; DoT is independent — it must neither be eaten by an active i-frame window
        // (e.g. right after a bomb) nor grant invulnerability itself.
        public void DamageOverTime(int amount) => ApplyDamageInternal(amount);

        // Shared core: clamp, fire totals event, fire one-shot depletion. Used by both the
        // i-frame-gated Damage path and the DoT path.
        void ApplyDamageInternal(int amount)
        {
            var before = _currentHealth;
            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            HealthChanged?.Invoke(_currentHealth, _maxHealth);
            if (_currentHealth == 0 && before > 0)
                HealthDepleted?.Invoke();
        }

        public void Heal(int amount)
        {
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            HealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        public void ResetToMax()
        {
            _currentHealth = _maxHealth;
            _iFramesEnd = 0f;
            HealthChanged?.Invoke(_currentHealth, _maxHealth);
        }

        void IDamageable.ApplyDamage(int amount, Vector2 sourcePosition) => Damage(amount);
    }
}
