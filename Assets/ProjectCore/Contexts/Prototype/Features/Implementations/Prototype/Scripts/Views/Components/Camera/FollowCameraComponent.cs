using UnityEngine;

namespace ProjectCore.Prototype
{
    [RequireComponent(typeof(Camera))]
    public class FollowCameraComponent : MonoBehaviour
    {
        [Header("FOV SETTINGS")]
        [SerializeField] private float _fovSmoothTime = 0.2f;
        
        [Header("SETTINGS")]
        [SerializeField] private float _distance = 4.0f;
        [SerializeField] private float _heightOffset = 1.5f;
        [SerializeField] private float _followDamping = 0.1f;
        [SerializeField] private float _rotationDamping = 0.1f;
        
        [Header("COLLISION")]
        [SerializeField] private LayerMask _collisionLayers;
        [SerializeField] private float _collisionRadius = 0.2f;
        [SerializeField] private float _collisionBuffer = 0.2f;

        private Camera _cameraComponent;
        
        private Transform _targetTransform;
        private Transform _lookTarget;
    
        private Vector3 _currentVelocity;
        
        private float _targetFOV;
        private float _currentFovVelocity;
        
        private void Start()
        {
            if (_cameraComponent == null)
            {
                _cameraComponent = GetComponent<Camera>();
            };
            
            if (_cameraComponent !=null)
            {
                _targetFOV = _cameraComponent.fieldOfView;
            }
        }
        
        public void SetTarget(Transform playerTransform, Transform lookAtTarget)
        {
            _targetTransform = playerTransform;
            _lookTarget = lookAtTarget;
            
            if (_targetTransform != null)
            {
                UpdateCameraPosition(true);
            }
        }

        public void ChangeFov(float newFovValue)
        {
            _targetFOV = newFovValue;
        }

        private void LateUpdate()
        {
            if (_targetTransform == null)
            {
                return;
            }

            UpdateCameraPosition(false);
            UpdateFOV();
        }

        private void UpdateCameraPosition(bool immediate)
        {
            Vector3 targetPos = _targetTransform.position - (_targetTransform.forward * _distance);
            targetPos.y += _heightOffset;
            
            Vector3 vectorToCamera = targetPos - _lookTarget.position;
            float currentDist = vectorToCamera.magnitude;
            Vector3 dirToCamera = vectorToCamera.normalized;
            
            if (Physics.SphereCast(_lookTarget.position, _collisionRadius, dirToCamera, out RaycastHit hit, currentDist, _collisionLayers))
            {
                targetPos = hit.point + (hit.normal * _collisionBuffer);
            }
            
            if (immediate)
            {
                transform.position = targetPos;
                transform.rotation = Quaternion.LookRotation(_lookTarget.position - transform.position);
            }
            else
            {
                transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _currentVelocity, _followDamping);
                
                Quaternion lookRotation = Quaternion.LookRotation(_lookTarget.position - transform.position);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime / _rotationDamping);
            }
        }
        
        private void UpdateFOV()
        {
            if (_cameraComponent == null)
            {
                return;
            }
            
            _cameraComponent.fieldOfView = Mathf.SmoothDamp(_cameraComponent.fieldOfView, _targetFOV, ref _currentFovVelocity, _fovSmoothTime);
        }
    }
}
