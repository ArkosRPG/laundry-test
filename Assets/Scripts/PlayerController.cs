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
    [SerializeField] private float _pickDistance = 2f;
    [SerializeField] private LayerMask _itemsMask;


    private PlaceableItem _currentItem;


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
        if (Input.GetMouseButtonDown(0))
        {
            var item = DetectItem(tf);
            if (item)
            {
                if (_currentItem)
                {
                    _currentItem.Unpick();
                    _currentItem = null;
                }

                _currentItem = item;
                _currentItem.TryPick();
            }
            else
            {
                if (_currentItem)
                {
                    _currentItem.TryPlace();
                    _currentItem = null;
                }
            }
        }
        else
        if (Input.GetMouseButtonDown(1))
        {
            if (_currentItem)
            {
                _currentItem.Unpick();
                _currentItem = null;
            }
        }
        else
        {
            if (_currentItem)
            {
                var itemScale = _currentItem.ModelScale;
                var position = _currentItem.Type switch
                {
                    PlaceableItemType.Wall  => _camera.position,
                    PlaceableItemType.Floor => tf.position,
                                          _ => tf.position
                };
                _currentItem.MoveTo(position + tf.forward * (1 + itemScale.z / 2f), tf.rotation);
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