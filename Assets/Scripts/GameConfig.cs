using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

[CreateAssetMenu]
public class GameConfig : LazySingletonResourcesSO<GameConfig>
{
    [SerializeField] public PlaceableItem[] Items;


#if UNITY_EDITOR
    [ContextMenu("Fill items")]
    private void FillItems()
    {
        Items = AssetDatabase
            .FindAssets("t:Prefab")
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<PlaceableItem>)
            .Where(item => item && !item.name.EndsWith("Base"))
            .ToArray();
    }
#endif // UNITY_EDITOR
}