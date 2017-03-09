using System;
using System.Collections.Generic;
using UnityEngine;

namespace MH
{
    public abstract class BaseSolverMB : MonoBehaviour
    {
        public abstract IKSolverType solverType { get; }
    }
}
