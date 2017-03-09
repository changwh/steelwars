using System;
using System.Collections.Generic;
using UnityEngine;
using ExtMethods;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MH
{
    /// <summary>
    /// the container for CCDSolver,
    /// 
    /// this MB should be put on the endJoint gameObject, so it can handle different settings under a single tree-structure
    /// </summary>
    public class CCDSolverMB : BaseSolverMB
    {
		#region "configurable data"
	    // configurable data

        [SerializeField][Tooltip("the bone count")]
        private int m_boneCount = 2;
        [SerializeField][Tooltip("when the endJoint and target are within this distance, the calc is taken as success")]
        private float m_distThreshold = 0.00001f;
        [SerializeField][Tooltip("damp limits the rotate delta in one iteration")]
        private bool m_setUseDamp = true;
        [SerializeField][Tooltip("global damp limit for joints under this solver")]
        private float m_setGlobalDamp = 10f;
        [SerializeField][Tooltip("how to recover if the IK cannot reach reasonable solution to given target")]
        private CCDSolver.RevertOption m_revertOpt = CCDSolver.DEF_RevertOpt;
	
	    #endregion "configurable data"
	
		#region "data"
	    // data

        private Transform m_tr;
        private CCDSolver m_solver;
	
	    #endregion "data"
	
		#region "unity event handlers"
	    // unity event handlers

        void Awake()
        {
            m_tr = transform;
        }
	
	    #endregion "unity event handlers"
	
		#region "public method"
	    // public method

        public int boneCount
        {
            get { return m_boneCount; }
            set { m_boneCount = value; _InitSolver(); }
        }

        public Transform Tr
        {
            get
            {
                if (m_tr == null)
                    m_tr = transform;
                return m_tr;
            }
        }

        public override IKSolverType solverType
        {
            get { return IKSolverType.CCD; }
        }

        public float distThres
        {
            get
            {
                if (m_solver != null)
                {
                    m_distThreshold = m_solver.distThres;
                }
                return m_distThreshold;
            }
            set
            {
                m_distThreshold = value;
                if (m_solver != null)
                {
                    m_solver.distThres = m_distThreshold;
                }
            }
        }

        public bool useDamp
        {
            get
            {
                if (m_solver != null)
                {
                    m_setUseDamp = m_solver.useDamp;
                }
                return m_setUseDamp;
            }
            set
            {
                m_setUseDamp = value;
                if (m_solver != null)
                {
                    m_solver.useDamp = m_setUseDamp;
                }
            }
        }

        public float globalDamp
        {
            get
            {
                if (m_solver != null)
                {
                    m_setGlobalDamp = m_solver.globalDamp;
                }
                return m_setGlobalDamp;
            }
            set
            {
                m_setGlobalDamp = value;
                if (m_solver != null)
                {
                    m_solver.globalDamp = m_setGlobalDamp;
                }
            }
        }

        public CCDSolver.RevertOption revertOpt
        {
            get
            {
                if (m_solver != null)
                {
                    m_revertOpt = m_solver.revertOpt;
                }
                return m_revertOpt;
            }
            set
            {
                m_revertOpt = value;
                if (m_solver != null)
                {
                    m_solver.revertOpt = m_revertOpt;
                }
            }
        }

        public CCDSolver GetSolver(bool force = false)
        {
            if (force || m_solver == null || m_solver.Count != m_boneCount )
            {
                m_solver = null; //clear first

                if (m_boneCount > 0)
                {
                    m_solver = new CCDSolver();
                    _InitSolver();
                }                
            }

            return m_solver;
        }

	    #endregion "public method"
	
		#region "private method"

        private void _InitSolver()
        {
            if (m_solver != null)
            {
                int actualLevel = 0;
                if (!Tr.HasParentLevel(m_boneCount, out actualLevel))
                {
                    m_boneCount = actualLevel;
                }

                m_solver.Target = Tr.position;
                m_solver.useDamp = m_setUseDamp;
                m_solver.globalDamp = m_setGlobalDamp;
                m_solver.SetBones(Tr, m_boneCount);
            }            
        }

	    #endregion "private method"
	
		#region "constant data"
	    // constant data
	
	    #endregion "constant data"

    }
}
