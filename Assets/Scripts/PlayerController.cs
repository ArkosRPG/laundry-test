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
        var item = DetectItem(tf);
        if (_targetedItem && _targetedItem != item)
            _targetedItem.Untarget();

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
                _pickedItem.TryPick();
            }
            else
            {
                if (_pickedItem)
                {
                    _pickedItem.TryPlace();
                    _pickedItem = null;
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
                    PlaceableItemType.Wall  => _camera.position,
                    PlaceableItemType.Floor => tf.position,
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

    private PlaceableItem DetectItem(Transform tf)
    {
        // TODO: non-alloc
        if (!Physics.Raycast(_camera.position, _camera.forward, out var hit, _pickDistance, _itemsMask))
            return null;

        return hit.transform.GetComponentInParent<PlaceableItem>();
    }
}