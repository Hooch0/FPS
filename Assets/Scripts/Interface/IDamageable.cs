using System;

public interface IDamageable
{
    float Health { get; set; }
    
    Action<float> Damaged { get; set; }
    Action<float> Destroyed { get; set; }

}
