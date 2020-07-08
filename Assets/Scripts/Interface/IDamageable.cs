using System;

public interface IDamageable
{
    float Health { get; }
    
    Action<float> Damaged { get; set; }
    Action Destroyed { get; set; }

    void TakeDamage(float amount);

}
