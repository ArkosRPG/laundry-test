using System.Collections.Generic;
using UnityEngine;

public abstract class MultitonMB<T> : MonoBehaviour
    where T : MultitonMB<T>
{
    private static readonly HashSet<T> _instances = new();
    public static IReadOnlyCollection<T> Instances => _instances;


    protected virtual void Awake()
    {
        _instances.Add(this as T);
    }

    protected void OnDestroy()
    {
        _instances.Remove(this as T);
    }
}