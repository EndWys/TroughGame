using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    [RequireComponent(typeof(Camera))]
    public sealed class LocalCameraComponent : MonoBehaviour, ILocalCamera
    {
        [SerializeField]
        private Camera _camera;
        [SerializeField]
        private SpriteRenderer _levelBoundsRenderer;
        [SerializeField]
        private Collider2D[] _levelBoundaryColliders;
        [SerializeField, Min(0f)]
        private float _followSmoothTime = 0.08f;
        [SerializeField, Range(0.2f, 0.4f)]
        private float _maxZoomChange = 0.4f;
        [SerializeField, Range(-1f, 1f)]
        private float _zoomFactor;

        private NetworkEntityRegistry _networkEntityRegistry;
        private Transform _target;
        private Vector3 _smoothVelocity;
        private float _initialOrthographicSize;

        [Inject]
        private void Construct(NetworkEntityRegistry networkEntityRegistry)
        {
            _networkEntityRegistry = networkEntityRegistry;
        }

        public float ZoomFactor => _zoomFactor;

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = GetComponent<Camera>();
            }

            if (_camera != null)
            {
                _initialOrthographicSize = _camera.orthographicSize;
            }
        }

        private void LateUpdate()
        {
            if (_camera == null || !_camera.orthographic)
            {
                return;
            }

            UpdateZoom();
            FitViewportToLevelBounds();
            TryGetLocalPlayerTarget();

            if (_target == null)
            {
                return;
            }

            Vector3 desiredPosition = transform.position;
            desiredPosition.x = _target.position.x;
            desiredPosition.y = _target.position.y;
            desiredPosition = ClampToLevelBounds(desiredPosition);

            Vector3 nextPosition = _followSmoothTime <= 0f
                ? desiredPosition
                : Vector3.SmoothDamp(
                    transform.position,
                    desiredPosition,
                    ref _smoothVelocity,
                    _followSmoothTime);

            nextPosition = ClampToLevelBounds(nextPosition);
            transform.position = new Vector3(nextPosition.x, nextPosition.y, transform.position.z);
        }

        public void SetZoomFactor(float zoomFactor)
        {
            _zoomFactor = Mathf.Clamp(zoomFactor, -1f, 1f);
        }

        private void TryGetLocalPlayerTarget()
        {
            if (_target != null && _target.gameObject.activeInHierarchy)
            {
                return;
            }

            _target = null;

            if (_networkEntityRegistry == null)
            {
                return;
            }

            foreach (BaseNetworkEntityRoot entity in _networkEntityRegistry.EntitiesById.Values)
            {
                if (entity != null && entity.HasInputAuthority)
                {
                    _target = entity.transform;
                    return;
                }
            }
        }

        private void UpdateZoom()
        {
            if (_initialOrthographicSize <= 0f)
            {
                return;
            }

            float maxZoomChange = Mathf.Clamp(_maxZoomChange, 0.2f, 0.4f);
            float zoomScale = 1f - (_zoomFactor * maxZoomChange);
            _camera.orthographicSize = _initialOrthographicSize * zoomScale;
        }

        private Vector3 ClampToLevelBounds(Vector3 position)
        {
            if (!TryGetLevelBounds(out Bounds levelBounds))
            {
                return position;
            }

            float halfHeight = _camera.orthographicSize;
            float halfWidth = halfHeight * _camera.aspect;

            position.x = ClampViewportAxis(
                position.x,
                levelBounds.min.x + halfWidth,
                levelBounds.max.x - halfWidth,
                levelBounds.center.x);
            position.y = ClampViewportAxis(
                position.y,
                levelBounds.min.y + halfHeight,
                levelBounds.max.y - halfHeight,
                levelBounds.center.y);

            return position;
        }

        private void FitViewportToLevelBounds()
        {
            if (!TryGetLevelBounds(out Bounds levelBounds))
            {
                return;
            }

            float maximumOrthographicSize = Mathf.Min(
                levelBounds.size.y * 0.5f,
                levelBounds.size.x / (2f * _camera.aspect));

            if (maximumOrthographicSize > 0f &&
                _camera.orthographicSize > maximumOrthographicSize)
            {
                _camera.orthographicSize = maximumOrthographicSize;
            }
        }

        private bool TryGetLevelBounds(out Bounds levelBounds)
        {
            if (_levelBoundsRenderer == null)
            {
                levelBounds = default;
                return false;
            }

            levelBounds = _levelBoundsRenderer.bounds;

            if (_levelBoundaryColliders == null)
            {
                return true;
            }

            float minimumX = levelBounds.min.x;
            float maximumX = levelBounds.max.x;
            float minimumY = levelBounds.min.y;
            float maximumY = levelBounds.max.y;

            foreach (Collider2D boundaryCollider in _levelBoundaryColliders)
            {
                if (boundaryCollider == null || !boundaryCollider.enabled)
                {
                    continue;
                }

                Bounds colliderBounds = boundaryCollider.bounds;
                bool isHorizontalBoundary = colliderBounds.size.x >= colliderBounds.size.y;

                if (isHorizontalBoundary)
                {
                    if (colliderBounds.center.y >= levelBounds.center.y)
                    {
                        maximumY = Mathf.Min(maximumY, colliderBounds.min.y);
                    }
                    else
                    {
                        minimumY = Mathf.Max(minimumY, colliderBounds.max.y);
                    }
                }
                else if (colliderBounds.center.x >= levelBounds.center.x)
                {
                    maximumX = Mathf.Min(maximumX, colliderBounds.min.x);
                }
                else
                {
                    minimumX = Mathf.Max(minimumX, colliderBounds.max.x);
                }
            }

            levelBounds.SetMinMax(
                new Vector3(minimumX, minimumY, levelBounds.min.z),
                new Vector3(maximumX, maximumY, levelBounds.max.z));
            return true;
        }

        private static float ClampViewportAxis(
            float value,
            float minimum,
            float maximum,
            float fallback)
        {
            return minimum > maximum
                ? fallback
                : Mathf.Clamp(value, minimum, maximum);
        }
    }
}
