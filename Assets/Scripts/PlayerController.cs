using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header(nameof(CameraControls))]
    [SerializeField] private Transform _camera;
    [Range(0, 90)]
    [SerializeField] private int _cameraXMaxUp = 30;
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

        CameraControls();
        Movement();
        HandleItems();
    }


    private void CameraControls()
    {
        // TODO: use new input system or Rewired etc
        var mousePosition = Input.mousePositionDelta;

        var playerEuler = transform.localEulerAngles;
        var cameraEuler = _camera.localEulerAngles;
        if (cameraEuler.x > 180f)
            cameraEuler.x -= 360f;

        playerEuler.y += mousePosition.x;
        cameraEuler.x = Mathf.Clamp(cameraEuler.x - mousePosition.y, -_cameraXMaxUp, _cameraXMaxDown);

        transform.localEulerAngles = playerEuler;
        _camera.localEulerAngles = cameraEuler;
    }

    private void Movement()
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

        var tf = transform;
        var desiredDistance = Time.deltaTime * speed;
        var desiredLocalMove = input * desiredDistance;
        var desiredMove = tf.forward * desiredLocalMove.z + tf.right * desiredLocalMove.x;
        tf.position += desiredMove;
    }

    private void HandleItems()
    {
        if (Input.GetMouseButton(0))
        {
            if (_currentItem)
            {
                _currentItem.Unpick();
                _currentItem = null;
            }

            var item = DetectItem();
            if (!item)
                return;

            _currentItem = item;
            _currentItem.Pick();
        }
        else
        if (Input.GetMouseButton(1))
        {
            if (_currentItem)
            {
                _currentItem.Unpick();
                _currentItem = null;
            }
        }
    }

    private PlaceableItem DetectItem()
    {
        var tf = _camera.transform;

        // TODO: non-alloc
        if (!Physics.Raycast(tf.position, tf.forward, out var hit, _pickDistance, _itemsMask))
            return null;

        return hit.transform.parent.GetComponent<PlaceableItem>();
    }
}