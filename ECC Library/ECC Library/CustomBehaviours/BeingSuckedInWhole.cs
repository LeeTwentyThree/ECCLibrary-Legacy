using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ECCLibrary.Internal
{
    /// <summary>
    /// This class simply 
    /// </summary>
    public class BeingSuckedInWhole : MonoBehaviour
    {
        /// <summary>
        /// The "throat" of the Creature that is swallowing this.
        /// </summary>
        public Transform target;
        /// <summary>
        /// How long it takes to get swallowed.
        /// </summary>
        public float animationLength;
        float timeStarted;
        float timeFinished;
        Vector3 defaultScale;
        Vector3 defaultPosition;

        void Start()
        {
            timeStarted = Time.time;
            timeFinished = Time.time + animationLength;
            defaultScale = transform.localScale;
            defaultPosition = transform.position;
            foreach(Collider collider in GetComponentsInChildren<Collider>())
            {
                Destroy(collider);
            }
#if SN1
            Creature creature = GetComponent<Creature>();
            if(creature != null)
            {
                creature.flinch = 50f;
            }
#endif
#if BZ
            var flinch = GetComponent<CreatureFlinch>();
            if (flinch != null)
            {
                flinch.OnTakeDamage(new DamageInfo() { damage = 100f });
            }
#endif
        }
    }

        void Update()
        {
            if(target != null)
            {
                float animationProgress = Mathf.InverseLerp(timeStarted, timeFinished, Time.time);
                transform.localScale = Vector3.Scale(defaultScale, Vector3.one - (Vector3.one * animationProgress));
                transform.position = Vector3.Lerp(defaultPosition, target.transform.position, animationProgress);
            }
        }
    }
}
