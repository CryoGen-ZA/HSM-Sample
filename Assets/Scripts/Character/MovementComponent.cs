using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class MovementComponent
{
    private readonly CharacterController _characterController;
    private Vector3 _currentVelocity = Vector3.zero;
    private Vector3 _newVelocity = Vector3.zero;
    
    public MovementComponent(CharacterController characterController)
    {
        _characterController = characterController;
    }

    public Vector3 GetPreviousVelocity()
    {
        return _currentVelocity;
    }

    public Vector3 GetCurrentVelocity()
    {
        return _newVelocity;
    }

    public void AddVelocity(Vector3 velocity)
    {
        _newVelocity += velocity;
    }

    public void ReplaceVelocity(Vector3 velocity)
    {
        _newVelocity = velocity;
    }

    public void DoUpdate()
    { 
        ApplyVelocity();
    }

    private void ApplyVelocity()
    {
        _characterController.Move(_newVelocity * Time.deltaTime);
        _currentVelocity = _newVelocity;
    }

    public Vector3 GetCurrentVelocityWithoutGravity()
    {
        var tempVelocity = _newVelocity;
        tempVelocity.y = 0;
        return tempVelocity;
    }
}
