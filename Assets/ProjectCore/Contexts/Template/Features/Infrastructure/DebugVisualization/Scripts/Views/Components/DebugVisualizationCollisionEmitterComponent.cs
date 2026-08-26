using UnityEngine;

namespace ProjectCore.Template
{
    public sealed class DebugVisualizationCollisionEmitterComponent : MonoBehaviour
    {
        [SerializeField] private float _speed = 3f;
        [SerializeField] private float _range = 4f;
        [SerializeField] private float _damage = 18f;

        private Vector3 _startPosition;
        private Rigidbody _rigidbody;

        private void Awake()
        {
            _startPosition = transform.position;
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            var nextPosition = _startPosition + Vector3.right * (Mathf.Sin(Time.time * _speed) * _range);

            if (_rigidbody != null)
                _rigidbody.MovePosition(nextPosition);
            else
                transform.position = nextPosition;
        }

        private void OnCollisionEnter(Collision collision)
        {
            var contact = collision.GetContact(0);
            DebugVisualizationUtility.DrawNumber(
                contact.point + Vector3.up * 0.35f, _damage, "Examples/Collision Damage", duration: 1.25f);
            DebugVisualizationUtility.DrawRadius(
                contact.point, 0.75f, "Examples/Collision Radius", duration: 1.25f);
        }
    }
}
