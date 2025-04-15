using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform _camera;
    [SerializeField] private int _speed = 5;
    [SerializeField] private int _cameraXMin = -30;
    [SerializeField] private int _cameraXMax = +60;


    private void Update()
    {
        if (Time.time < 1f)
            return;

        CameraControls();
        Movement();


        void CameraControls()
        {
            var mouse = Input.mousePositionDelta;

            var player = transform.localEulerAngles;
            var camera = _camera.localEulerAngles;
            if (camera.x > 180f)
                camera.x -= 360f;

            player.y += mouse.x;
            camera.x = Mathf.Clamp(camera.x - mouse.y, _cameraXMin, _cameraXMax);

            transform.localEulerAngles = player;
            _camera.localEulerAngles = camera;
        }

        void Movement()
        {
            var move = Vector3.zero;
            if (Input.GetKey(KeyCode.W))
                move.z++;
            if (Input.GetKey(KeyCode.S))
                move.z--;
            if (Input.GetKey(KeyCode.A))
                move.x--;
            if (Input.GetKey(KeyCode.D))
                move.x++;

            move *= Time.deltaTime * _speed;

            var tf = transform;
            tf.position += tf.forward * move.z + tf.right * move.x;
        }
    }
}