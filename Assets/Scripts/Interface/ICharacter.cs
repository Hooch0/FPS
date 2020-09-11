using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacter
{
    bool IsAimingDownSight { get; }

    float MaxVerticalLook { get; }
    float MinVerticalLook { get; }

    Transform GetArmsHolder();
    Inventory GetInventory();
    Quaternion GetRotation();
    Vector3 GetPosition();

    Ray GetHitScanRay();

    void SetRotation(Vector3 euler);
    void ApplyRotation(Vector3 euler);
}
