#if BZ
using UnityEngine;

namespace ECCLibrary.Mono
{
    /// <summary>
    /// BZ-only class that controls basic velocity-based animation for walking creatures. Attach to the root of the prefab. Influences the `on_surface` and `speed` parameters.
    /// </summary>
    public class AnimateWalkingCreature : MonoBehaviour
    {
        /// <summary>
        /// Used to determine the 'speed' parameter in the Animator.
        /// </summary>
        public float animationMaxSpeed = 1f;

        private Animator _animator;
        private SwimWalkCreatureController _swimWalk;
        private Rigidbody _rb;

        private void Start()
        {
            _animator = GetComponentInChildren<Animator>();
            _swimWalk = GetComponent<SwimWalkCreatureController>();
            _rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            var walking = _swimWalk.state == SwimWalkCreatureController.State.Walk;
            _animator.SetBool(AnimatorHashID.on_surface, walking);
            if (walking)
            {
                var vector = transform.InverseTransformVector(_rb.velocity) / animationMaxSpeed;
                _animator.SetFloat(AnimatorHashID.speed, Mathf.Clamp01(Mathf.Sqrt(vector.x * vector.x + vector.z * vector.z)));
            }
        }
    }
}
#endif