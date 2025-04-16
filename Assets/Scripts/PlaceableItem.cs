using System;
using UnityEngine;

public class PlaceableItem : MonoBehaviour
{
    private enum State
    {
        Undefined = 1,
        Placed    = 2,
        Picked    = 3,
    }


    [Header(nameof(Material))]
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Material _commonMaterial;
    [SerializeField] private Material _pickedMaterial;

    [Header(nameof(StackView))]
    [SerializeField] private StackView _stack;


    private State _state = State.Undefined;


    public bool Pick()
    {
        switch (_state)
        {
            case State.Undefined:

            case State.Placed:
                _renderer.material = _pickedMaterial;
                _state = State.Picked;
                return true;

            case State.Picked:
                return false;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public bool Unpick()
    {
        switch (_state)
        {
            case State.Undefined:

            case State.Picked:
                _renderer.material = _commonMaterial;
                _state = State.Placed;
                return true;

            case State.Placed:
                return false;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}