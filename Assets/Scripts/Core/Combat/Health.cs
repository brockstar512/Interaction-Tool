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
            var before = _currentHealth;
            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            _iFramesEnd = Time.time + _iFramesDuration;
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
