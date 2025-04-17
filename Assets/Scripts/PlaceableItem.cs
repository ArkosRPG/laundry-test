using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

public class PlaceableItem : MonoBehaviour
{
    private enum State
    {
        Undefined = 1,
        Placed    = 2,
        Targeted  = 3,
        Picked    = 4,
        Placeable = 5,
    }


    [SerializeField] private PlacementType _type;

    [SerializeField] private Collider _collider;

    [Header(nameof(Material))]
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Material _commonMaterial;
    [SerializeField] private Material _targetedMaterial;
    [SerializeField] private Material _pickedMaterial;
    [SerializeField] private Material _placeableMaterial;

    [Header(nameof(StackView))]
    [SerializeField, CanBeNull] private StackView _stack;

    [Header("Model")]
    [SerializeField] private Transform _model;


    private State _state = State.Undefined;
    private Vector3 _positionCache;
    private Quaternion _rotationCache;


    public PlacementType Type => _type;
    public Vector3 ModelScale => _model.lossyScale;


    public bool Target()
    {
        switch (_state)
        {
            case State.Undefined:

            case State.Placed:
                _renderer.material = _targetedMaterial;
                _state = State.Targeted;
                return true;

            case State.Targeted:
                return false;

            default:
                throw new ArgumentOutOfRangeException($"{_state}");
        }
    }

    public bool Untarget()
    {
        switch (_state)
        {
            case State.Targeted:
                _renderer.material = _commonMaterial;
                _state = State.Placed;
                return true;

            case State.Placed:
            case State.Picked:
            case State.Placeable:
                return false;

            default:
                throw new ArgumentOutOfRangeException($"{_state}");
        }
    }

    public bool TryPick()
    {
        switch (_state)
        {
            case State.Targeted:
                _renderer.material = _pickedMaterial;
                _renderer.shadowCastingMode = ShadowCastingMode.Off;
                _collider.enabled = false;

                var tf = transform;
                _positionCache = tf.position;
                _rotationCache = tf.rotation;

                _state = State.Picked;

                StackView.Show(_type);
                if (_stack)
                    _stack.Hide();
                return true;

            case State.Placed:
                return false;

            default:
                throw new ArgumentOutOfRangeException($"{_state}");
        }
    }

    public void MoveTo(Vector3 position, Quaternion rotation)
    {
        var tf = transform;
        tf.position = position;
        tf.rotation = rotation;
    }

    public bool SetPlaceable(bool placeable)
    {
        if (placeable)
        {
            switch (_state)
            {
                case State.Picked:
                    _renderer.material = _placeableMaterial;
                    _state = State.Placeable;
                    return true;

                case State.Placeable:
                    return false;

                default:
                    throw new ArgumentOutOfRangeException($"{_state}");
            }
        }
        else
        {
            switch (_state)
            {
                case State.Placeable:
                    _renderer.material = _pickedMaterial;
                    _state = State.Picked;
                    return true;

                case State.Picked:
                    return false;

                default:
                    throw new ArgumentOutOfRangeException($"{_state}");
            }
        }
    }

    public bool TryPlace(Vector3 position, Quaternion rotation)
    {
        switch (_state)
        {
            case State.Picked:
                return false;

            case State.Placeable:
                Place(position, rotation);
                return true;

            default:
                throw new ArgumentOutOfRangeException($"{_state}");
        }
    }

    public bool Unpick()
    {
        switch (_state)
        {
            case State.Picked:
            case State.Placeable:
                Place(_positionCache, _rotationCache);
                return true;

            default:
                throw new ArgumentOutOfRangeException($"{_state}");
        }
    }


    private void Place(Vector3 position, Quaternion rotation)
    {
        _renderer.material = _commonMaterial;
        _renderer.shadowCastingMode = ShadowCastingMode.Off;
        _collider.enabled = true;

        MoveTo(position, rotation);

        _state = State.Placed;

        StackView.HideAll();
    }
}