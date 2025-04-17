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
    [SerializeField] private float _placeDistance = 10f;
    [SerializeField] private LayerMask _floorMask;
    [SerializeField] private LayerMask _wallsMask;


    private PlaceableItem _targetedItem;
    private PlaceableItem _pickedItem;


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
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
        var item = DetectItem(_itemsMask);
        if (_targetedItem && _targetedItem != item)
        {
            _targetedItem.Untarget();
            _targetedItem = null;
        }

        RaycastHit placeHit = default;
        var placeable = false;
        if (_pickedItem)
        {
            placeable = _pickedItem.Type switch
            {
                PlacementType.Floor => RaycastFromCamera(out placeHit, _floorMask, _placeDistance),
                PlacementType.Wall  => RaycastFromCamera(out placeHit, _wallsMask, _placeDistance),
                _ => throw new ArgumentOutOfRangeException($"{_pickedItem.Type}")
            };
            _pickedItem.SetPlaceable(placeable);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (item)
            {
                if (_pickedItem)
                {
                    _pickedItem.Unpick();
                    _pickedItem = null;
                }

                _pickedItem = item;
                if (_pickedItem.TryPick())
                {
                    if (_targetedItem)
                        _targetedItem = null;
                }
            }
            else
            {
                if (_pickedItem)
                {
                    switch (_pickedItem.Type)
                    {
                        case PlacementType.Floor:
                            if (placeable)
                            {
                                _pickedItem.TryPlace(placeHit.point, tf.rotation);
                                _pickedItem = null;
                            }
                            break;

                        case PlacementType.Wall:
                            if (placeable)
                            {
                                _pickedItem.TryPlace(placeHit.point, Quaternion.LookRotation(placeHit.normal));
                                _pickedItem = null;
                            }
                            break;

                        default:
                            throw new ArgumentOutOfRangeException($"{_pickedItem.Type}");
                    }
                }
            }
        }
        else
        if (Input.GetMouseButtonDown(1))
        {
            if (_pickedItem)
            {
                _pickedItem.Unpick();
                _pickedItem = null;
            }
        }
        else
        {
            if (_pickedItem)
            {
                var itemScale = _pickedItem.ModelScale;
                var position = _pickedItem.Type switch
                {
                    PlacementType.Wall  => _camera.position,
                    PlacementType.Floor => tf.position,
                                          _ => tf.position
                };
                _pickedItem.MoveTo(position + tf.forward * (1 + itemScale.z / 2f), tf.rotation);
            }
            else
            {
                if (item)
                {
                    _targetedItem = item;
                    _targetedItem.Target();
                }
            }
        }
    }

    private PlaceableItem DetectItem(LayerMask mask)
    {
        // TODO: non-alloc
        if (!RaycastFromCamera(out var hit, mask, _pickDistance))
            return null;

        return hit.transform.GetComponentInParent<PlaceableItem>();
    }

    private bool RaycastFromCamera(out RaycastHit hit, LayerMask mask, float maxDistance)
    {
        // TODO: non-alloc
        return Physics.Raycast(_camera.position, _camera.forward, out hit, maxDistance, mask);
    }
}