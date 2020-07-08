using System;
using System.Collections.Generic;
using UnityEngine;

public class Dummy : MonoBehaviour, IDamageable
{
    public Action<float> Damaged { get; set; }
    public Action Destroyed { get; set; }

    public float Health { get; private set; }
    public float MaxHealth = 100;

    private Color _startColor;
    private Renderer _renderer;

    private Timer _beforeRegenTimer;
    private float _beforeRegenGoal = 5;

    private Timer _regenTimer;
    private float _regenGoal = 10;

    private bool _regen;
    private float _healthBeforeRegen;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _startColor = _renderer.material.color;
        Health = MaxHealth;
        
        _beforeRegenTimer = new Timer(_beforeRegenGoal, () => 
        { 
            _healthBeforeRegen = Health; _regen = true; _regenTimer.Start(); 
        });


        _regenTimer = new Timer(_regenGoal, () => { _regenTimer.Stop(); } );
    }

    private void Update()
    {
        _beforeRegenTimer.Update(Time.deltaTime);
        _regenTimer.Update(Time.deltaTime);

        if (_regen == true)
        {
            if (Health < MaxHealth)
            {
                Health = Mathf.Lerp(_healthBeforeRegen,MaxHealth,_regenTimer.Elapsed / _regenGoal);
            }
        }

        _renderer.material.color = Color.Lerp(Color.red,_startColor, Health/MaxHealth);

    }

    public void TakeDamage(float amount)
    {
        Health -= amount;

        _regen = false;

        //Restart the regen timer
        _beforeRegenTimer.Stop();
        _regenTimer.Stop();
        _beforeRegenTimer.Start();


        Damaged?.Invoke(amount);

        if (Health < 0)
        {
            Health = 0;
            Destroyed?.Invoke();
            return;
        }
    }

}
