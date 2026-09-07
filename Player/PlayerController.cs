using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _speed = 5.5f;
    [SerializeField] private float _gravity = -9.81f;
    private CharacterController _controller;
    private InputAction _moveInput;
    private Vector3 _velocity;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _moveInput = InputSystem.actions.FindActionMap("Player").FindAction("Move");
    }

    void Update()
    {
        Vector3 movement = _moveInput.ReadValue<Vector2>().ToXZ();
        _controller.Move(movement * _speed * Time.deltaTime);

        _velocity.y += _gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }
}
