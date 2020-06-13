using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHoleManager : MonoBehaviour
{
    public static BulletHoleManager Instance { get; private set; }

    public ObjectPool BulletPool;


    private void Awake()
    {
        BulletPool.Initialize();
    }

    private void OnEnable()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlaceBulletHole(Transform parent, Vector3 position, Quaternion rotation )
    {
        GameObject go = BulletPool.GetObject();

        go.transform.parent = parent;
        go.transform.position = position;
        go.transform.rotation = rotation;
    }

}
