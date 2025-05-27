using UnityEngine;

namespace HSM.Core
{
    public static class StateHelpers
    {
        public static Vector3 CreateAirControllerVelocity(Transform cameraTransform, Vector2 playerInput, Vector3 initialVelocity, Vector3 currentVelocity, float normalSpeed, float airControl)
        {
                var movementDirection = cameraTransform.right * playerInput.x + cameraTransform.forward * playerInput.y;
                movementDirection.y = 0;
            
                var newVelocity = currentVelocity;
                var cachedVelocityY = newVelocity.y;
                newVelocity.y = 0;
                newVelocity += movementDirection.normalized * (normalSpeed * airControl * Time.deltaTime);

                if (newVelocity.magnitude > initialVelocity.magnitude)
                {
                    newVelocity = newVelocity.normalized * initialVelocity.magnitude;
                }
            
                newVelocity.y = cachedVelocityY;
                return newVelocity;
        }
        
        public static float Remap (float value, float from1, float to1, float from2, float to2) {
            var remappedValue = (value - from1) / (to1 - from1) * (to2 - from2) + from2;
            return Mathf.Clamp(remappedValue, from2, to2);
        }
    }
}