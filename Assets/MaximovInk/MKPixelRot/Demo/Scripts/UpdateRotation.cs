using UnityEngine;

namespace MaximovInk
{
    public class UpdateRotation : MonoBehaviour
    {
        [SerializeField] private float _speed;

        private void Update()
        {
            transform.rotation *= Quaternion.Euler(0, 0, Time.deltaTime * _speed);
        }
    }
}