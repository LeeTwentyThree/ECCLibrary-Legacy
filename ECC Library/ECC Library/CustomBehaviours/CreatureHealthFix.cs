#if BZ
using UnityEngine;

namespace ECCLibrary.Internal
{
    internal class CreatureHealthFix : MonoBehaviour
    {
        public float maxHealth;

        private LiveMixin _lm;

        private void OnEnable()
        {
            if (_lm == null)
            {
                _lm = GetComponent<LiveMixin>();
            }
            if (_lm && _lm.health == 0f)
            {
                _lm.health = maxHealth;
            }
        }
    }
}

#endif