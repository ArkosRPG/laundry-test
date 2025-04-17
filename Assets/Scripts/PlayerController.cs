using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header(nameof(CameraControls))]
    [SerializeField] private Transform _camera;
    [Range(0, 90)]
    [SerializeField] private int _cameraXMaxUp = 60;
    [Range(0, 90)]
    [SerializeField] private int _cameraXMaxDown = 60;

    [Header(nameof(Movement))]
    [Min(1)]
    [SerializeField] private int _speed = 5;
    [Min(1)]
    [SerializeField] private int _sprint = 12;

    [Header("Items")]
    [Min(1)]
    [SerializeField] private float _pickDistance = 5f;
    [SerializeField] private LayerMask _itemsMask;
    [SerializeField] private float _placeDistance = 5f;
    [SerializeField] private LayerMask _floorMask;
    [SerializeField] private LayerMask _wallsMask;
    [SerializeField] private LayerMask _allLayersMask;
    [SerializeField] private int _rotationAngle = 45;


    private PlaceableItem _targetedItem;
    private PlaceableItem _pickedItem;
    private int _rotationLimit;
    private int _rotation;


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        _rotationLimit = 180 / _rotationAngle + (180 % _rotationAngle > 0 ? 1 : 0);
    }

    private void Update()
    {
        // TODO: remove debug crutch
        if (Time.time < 0.1f)
            return;

        var tf = transform;
        CameraControls(tf);
        Movement(tf);
        HandleItems(tf);
    }


    private void CameraControls(Transform tf)
    {
        // TODO: use new input system or Rewired etc
        var mousePosition = Input.mousePositionDelta;

        var playerEuler = tf.localEulerAngles;
        var cameraEuler = _camera.localEulerAngles;
        if (cameraEuler.x > 180f)
            cameraEuler.x -= 360f;

        playerEuler.y += mousePosition.x;
        cameraEuler.x = Mathf.Clamp(cameraEuler.x - mousePosition.y, -_cameraXMaxUp, _cameraXMaxDown);

        tf.localEulerAngles = playerEuler;
        _camera.localEulerAngles = cameraEuler;
    }

    private void Movement(Transform tf)
    {
        // TODO: use new input system or Rewired etc
        var input = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
            input.z++;
        if (Input.GetKey(KeyCode.S))
            input.z--;
        if (Input.GetKey(KeyCode.A))
            input.x--;
        if (Input.GetKey(KeyCode.D))
            input.x++;
        var speed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ? _sprint : _speed;

        var desiredDistance = Time.deltaTime * speed;
        var desiredLocalMove = input * desiredDistance;
        var desiredMove = tf.forward * desiredLocalMove.z + tf.right * desiredLocalMove.x;
        tf.position += desiredMove;
    }

    private void HandleItems(Transform tf)
    {
        // RMB
        if (Input.GetMouseButtonDown(1))
        {
            if (_pickedItem)
            {
                _pickedItem.Unpick();
                _pickedItem = null;
            }
            return;
        }

        // LMB
        if (Input.GetMouseButtonDown(0))
        {
            if (_pickedItem)
            {
                RaycastHit placeHit;
                var placeable = _pickedItem.Type switch
                {
                    PlacementType.Floor => RaycastFromCamera(out placeHit, _floorMask, _placeDistance),
                    PlacementType.Walls => RaycastFromCamera(out placeHit, _wallsMask, _placeDistance),
                    _ => throw new ArgumentOutOfRangeException($"{_pickedItem.Type}")
                };
                _pickedItem.SetPlaceable(placeable);
                if (placeable)
                {
                    switch (_pickedItem.Type)
                    {
                        case PlacementType.Floor:
                        {
                            var eulers = tf.eulerAngles;
                            _pickedItem.TryPlace(placeHit.point, Quaternion.Euler(eulers.x, eulers.y + _rotation * 45f, eulers.z));
                            break;
                        }
                        case PlacementType.Walls: _pickedItem.TryPlace(placeHit.point, Quaternion.LookRotation(placeHit.normal)); break;
                        default: throw new ArgumentOutOfRangeException($"{_pickedItem.Type}");
                    }
                    _targetedItem = _pickedItem;
                    _targetedItem.Target();
                    _pickedItem = null;
                }
                return;
            }

            var item = DetectItem();
            if (!item)
                return;

            if (_targetedItem && _targetedItem != item)
            {
                _targetedItem.Untarget();
                _targetedItem = null;
            }

            if (item.TryPick())
            {
                _rotation = 0;
                _pickedItem = item;
            }
            return;
        }

        // no mouse buttons pressed
        if (_pickedItem)
        {
            _rotation += (int)Input.mouseScrollDelta.y;
            if (_rotation <= -_rotationLimit)
                _rotation += _rotationLimit;
            if (_rotation > _rotationLimit)
                _rotation -= _rotationLimit;

            RaycastHit hit;
            var placeable = _pickedItem.Type switch
            {
                PlacementType.Floor => RaycastFromCamera(out hit, _floorMask, _placeDistance),
                PlacementType.Walls => RaycastFromCamera(out hit, _wallsMask, _placeDistance),
                _ => throw new ArgumentOutOfRangeException($"{_pickedItem.Type}")
            };
            _pickedItem.SetPlaceable(placeable);
            if (placeable)
            {
                switch (_pickedItem.Type)
                {
                    case PlacementType.Floor:
                    {
                        var eulers = tf.eulerAngles;
                        _pickedItem.MoveTo(hit.point, Quaternion.Euler(eulers.x,eulers.y +_rotation * 45f,eulers.z));
                        break;
                    }
                    case PlacementType.Walls: _pickedItem.MoveTo(hit.point, Quaternion.LookRotation(hit.normal)); break;
                    default: throw new ArgumentOutOfRangeException($"{_pickedItem.Type}");
                }
            }
            else
            {
                var itemScale = _pickedItem.ModelScale;
                var position = _pickedItem.Type switch
                {
                    PlacementType.Walls => _camera.position,
                    PlacementType.Floor => tf.position,
                                      _ => tf.position
                };
                _pickedItem.MoveTo(position + tf.forward * (2 + itemScale.z / 2f), tf.rotation);
            }
        }
        else
        {
            var item = DetectItem();
            if (_targetedItem != item)
            {
                if (_targetedItem)
                    _targetedItem.Untarget();
                _targetedItem = item;
                if (_targetedItem)
                    _targetedItem.Target();
            }
        }
    }

    private PlaceableItem DetectItem()
    {
        return RaycastFromCamera(out var hit, _itemsMask, _pickDistance)
            ? hit.transform.GetComponentInParent<PlaceableItem>()
            : null;
    }

    private bool RaycastFromCamera(out RaycastHit hit, LayerMask targetLayer, float maxDistance)
    {
        // TODO: non-alloc
        return Physics.Raycast(_camera.position, _camera.forward, out hit, maxDistance, _allLayersMask)
               && 1 << hit.transform.gameObject.layer == targetLayer;
    }
}