using System.IO;
using UnityEditor;
using UnityEngine;

public abstract class LazySingletonResourcesSO<T> : ScriptableObject
    where T : ScriptableObject
{
    protected static T _instance { get; private set; }
    public static T Instance
    {
        get
        {
            if (!_instance)
                _instance = Resources.Load<T>(typeof(T).Name);
#if UNITY_EDITOR
            if (!_instance)
                _instance = Create();
#endif // UNITY_EDITOR
            return _instance;
        }
    }


#if UNITY_EDITOR
    protected static T Create()
    {
        var path = "Resources";
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        var instance = CreateInstance<T>();
        AssetDatabase.CreateAsset(instance, path);
        AssetDatabase.SaveAssets();

        return instance;
    }
#endif // UNITY_EDITOR
}