#if SN1
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace ECCLibrary.Internal
{
    public class LastTarget_New : LastTarget
    {
        public RoarAbility roar;
        float timeNextRoar;
        const float minTimeBetweenRoars = 5f;

        public override void SetTarget(GameObject target)
        {
            if(roar != null && Time.time >= timeNextRoar)
            {
                if (target != null && _target != target)
                {
                    timeNextRoar = Time.time + minTimeBetweenRoars;
                    roar.PlayRoar();
                }
            }
            base.SetTarget(target);
        }
    }
}
#endif