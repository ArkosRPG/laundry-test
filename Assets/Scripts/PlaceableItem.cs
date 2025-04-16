using System;
using UnityEngine;
using UnityEngine.Rendering;

public class PlaceableItem : MonoBehaviour
{
    private enum State
    {
        Undefined = 1,
        Placed    = 2,
        Picked    = 3,
    }


    [Header(nameof(Collider))]
    [SerializeField] private Collider _collider;

    [Header(nameof(Material))]
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Material _commonMaterial;
    [SerializeField] private Material _pickedMaterial;

    [Header(nameof(StackView))]
    [SerializeField] private StackView _stack;

    [Header("Model")]
    [SerializeField] private Transform _model;


    private State _state = State.Undefined;
    private Vector3 _positionCache;
    private Quaternion _rotationCache;


    public Vector3 ModelScale => _model.lossyScale;


    public bool TryPick()
    {
        switch (_state)
        {
            case State.Undefined:

            case State.Placed:
                _renderer.material = _pickedMaterial;
                _renderer.shadowCastingMode = ShadowCastingMode.Off;
                _collider.enabled = false;

                var tf = transform;
                _positionCache = tf.position;
                _rotationCache = tf.rotation;

                _state = State.Picked;
                return true;

            case State.Picked:
                return false;

            default:
                throw new ArgumentOutOfRangeException();
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
                throw new NotSupportedException(nameof(State.Undefined));

            case State.Picked:
                Place(position ?? _positionCache, rotation ?? _rotationCache);
                return true;

            case State.Placed:
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
                throw new NotSupportedException(nameof(State.Undefined));

            case State.Picked:
                Place(_positionCache, _rotationCache);
                return true;

            case State.Placed:
                return false;

            default:
                throw new ArgumentOutOfRangeException();
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