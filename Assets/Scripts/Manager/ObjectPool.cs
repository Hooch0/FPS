using System;
using System.Collections.Generic;
using UnityEngine;


//Object pool assumes all objects are of GameObject type
[Serializable]
public class ObjectPool
{   
    public Transform PooledParent;

    public GameObject PooledObject;
    public int StartingPools;
    public int MaxPools;

    private int _currentPoolsCount;
    private Queue<GameObject> _poolQueue;
    private List<GameObject> _activeObjs;


    public void Initialize()
    {
        _poolQueue = new Queue<GameObject>();
        _activeObjs = new List<GameObject>();

        for (int i = 0; i < StartingPools; i++)
        {
            _poolQueue.Enqueue(CreateObject());
        }

        _currentPoolsCount = StartingPools;
    }

    public GameObject GetObject()
    {
        GameObject go = null;

        if (_poolQueue.Count == 0 && _currentPoolsCount < MaxPools)
        {
            //We need more and we can still create more.
            _currentPoolsCount++;
            go = CreateObject();
        }
        else if (_poolQueue.Count == 0 && _currentPoolsCount == MaxPools)
        {
            //we can no longer create more so restore the first dequeued object and get it.
            StoreObject(_activeObjs[0]);
            go = _poolQueue.Dequeue();
        }
        else
        {
            go = _poolQueue.Dequeue();
        }

        go.SetActive(true);

        _activeObjs.Add(go);

        return go;
    }

    public void StoreObject(GameObject obj)
    {
        obj.SetActive(false);

        _activeObjs.Remove(obj);

        _poolQueue.Enqueue(obj);
    }


    private GameObject CreateObject()
    {
        GameObject go = GameObject.Instantiate(PooledObject,Vector3.zero,Quaternion.identity);
        go.SetActive(false);
        go.transform.parent = PooledParent;

        return go;
    }

}
