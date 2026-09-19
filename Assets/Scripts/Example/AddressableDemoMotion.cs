using UnityEngine;

namespace Example
{
    public sealed class AddressableDemoMotion : MonoBehaviour
    {
        [SerializeField] private float rotationSpeed = 45f;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseAmount = 0.12f;

        private Vector3 initialScale;

        private void Awake()
        {
            initialScale = transform.localScale;
        }

        private void Update()
        {
            transform.Rotate(0f, rotationSpeed * Time.deltaTime, rotationSpeed * 0.35f * Time.deltaTime);
            float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = initialScale * scale;
        }
    }
}
