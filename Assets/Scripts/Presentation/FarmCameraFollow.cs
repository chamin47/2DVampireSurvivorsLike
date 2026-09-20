using UnityEngine;

namespace DawnFarm
{
    public sealed class FarmCameraFollow : MonoBehaviour
    {
        public Transform Target { get; set; }

        private void LateUpdate()
        {
            if (Target == null) return;

            Vector3 desired = new Vector3(Target.position.x, Target.position.y, -10f);
            transform.position = Vector3.Lerp(
                transform.position,
                desired,
                1f - Mathf.Exp(-8f * Time.unscaledDeltaTime));
        }
    }
}
