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
    }


    [SerializeField] private PlaceableItemType _type;

    [SerializeField] private Collider _collider;

    [Header(nameof(Material))]
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Material _commonMaterial;
    [SerializeField] private Material _targetedMaterial;
    [SerializeField] private Material _pickedMaterial;

    [Header(nameof(StackView))]
    [SerializeField, CanBeNull] private StackView _stack;

    [Header("Model")]
    [SerializeField] private Transform _model;


    private State _state = State.Undefined;
    private Vector3 _positionCache;
    private Quaternion _rotationCache;


    public PlaceableItemType Type => _type;
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
            case State.Picked:
                return false;

            default:
                throw new ArgumentOutOfRangeException($"{_state}");
        }
    }

    public bool Untarget()
    {
        switch (_state)
        {
            case State.Undefined:

            case State.Targeted:
                _renderer.material = _commonMaterial;
                _state = State.Placed;
                return true;

            case State.Placed:
            case State.Picked:
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
                return true;

            case State.Placed:
            case State.Picked:
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

    public bool TryPlace(Vector3? position = null, Quaternion? rotation = null)
    {
        switch (_state)
        {
            case State.Undefined:
                throw new NotSupportedException($"{_state}");

            case State.Picked:
                Place(position ?? _positionCache, rotation ?? _rotationCache);
                return true;

            case State.Placed:
                return false;

            default:
                throw new ArgumentOutOfRangeException($"{_state}");
        }
    }

    public bool Unpick()
    {
        switch (_state)
        {
            case State.Undefined:
                throw new NotSupportedException($"{_state}");

            case State.Picked:
                Place(_positionCache, _rotationCache);
                return true;

            case State.Placed:
                return false;

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
    }
}