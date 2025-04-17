using System.Linq;
using UnityEngine;

public class StackView : MultitonMB<StackView>
{
    [SerializeField] private PlacementType _type;
    [SerializeField] private Collider _collider;
    [SerializeField] private GameObject _model;


    public static void Show(PlacementType type)
    {
        var stacks = Instances.Where(sv => sv._type == type).ToArray();
        foreach (var view in stacks)
        {
            view.Show();
        }
    }

    public static void HideAll()
    {
        foreach (var view in Instances)
        {
            view.Hide();
        }
    }


    public void Show()
    {
        _model.SetActive(true);
        _collider.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _model.SetActive(false);
        _collider.gameObject.SetActive(false);
    }
}