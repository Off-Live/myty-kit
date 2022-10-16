using System.Collections.Generic;
using UnityEngine;

namespace MYTYKit.MotionAdapters.Reduce
{
    public abstract class ReduceOperator: MonoBehaviour
    {
        public abstract Vector3 Reduce(List<Vector3> items);
    }
}