using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEditor;
using ExtMethods;
using MH.Skele;

namespace MH
{
    /// <summary>
    /// will draw the bone lines and call each enabled constraints' OnSceneGUI
    /// </summary>
    [CustomEditor(typeof(CCDSolverMB))]
    public class CCDSolverEditor : Editor
    {
		#region "data"
	    // data

        private bool m_dbgShowHandle = true;
        private IEnumerator m_dbgStepIE = null;

        private static GUIStyle ms_splitterStyle = null;
	
	    #endregion "data"
	
		#region "unity event handlers"
	    // unity event handlers

        static CCDSolverEditor()
        {
            ms_splitterStyle = new GUIStyle();
            ms_splitterStyle.normal.background = EditorGUIUtility.whiteTexture;
            ms_splitterStyle.stretchWidth = true;
            ms_splitterStyle.margin = new RectOffset(0, 0, 7, 2);
        }

        void OnEnable()
        {
            if (!Application.isPlaying)//cannot let editor script to mess with in-game logic
            { 
                CCDSolverMB mb = (CCDSolverMB)target;
                if (mb == null)
                    return; //possible after switching scene
                mb.GetSolver().RefreshConstraint();
            }
        }
	
		public override void OnInspectorGUI()
        {
            CCDSolverMB mb = (CCDSolverMB)target;

            GUILayout.BeginHorizontal();
            GUILayout.Space(30f);
            if (EUtil.Button("ReInit", "force recreating the solver\ncollect all constraints", Color.green))
            {
                mb.GetSolver(true);
                EUtil.RepaintSceneView();
            }
            GUILayout.Space(30f);
            GUILayout.EndHorizontal();

            // bone count
            int newBoneCnt = EditorGUILayout.IntField(CONT_BoneCnt, mb.boneCount);
            if (GUI.changed)
            {
                if (newBoneCnt > 0 && newBoneCnt != mb.boneCount && mb.Tr.HasParentLevel(newBoneCnt))
                {
                    Undo.RecordObject(mb, "Set Bone Count");
                    mb.boneCount = newBoneCnt; //will trigger _InitSolver if not null
                }
            }

            EditorGUI.BeginChangeCheck();
            float newDistThres = EditorGUILayout.FloatField(CONT_DistThres, mb.distThres);
            if (EditorGUI.EndChangeCheck())
            {
                if (newDistThres > 0f)
                {
                    Undo.RecordObject(mb, "Set Dist Thres");
                    mb.distThres = newDistThres; 
                }                
            }

            mb.useDamp = EditorGUILayout.Toggle("Use Damp", mb.useDamp);
            if (mb.useDamp)
            {
                mb.globalDamp = EditorGUILayout.FloatField("Global damp", mb.globalDamp);
            }

            mb.revertOpt = (CCDSolver.RevertOption)EditorGUILayout.EnumPopup(CONT_RevertOpt, mb.revertOpt);

            if (Pref.showIKDebug)
            {
                {
                    Rect rc = GUILayoutUtility.GetRect(GUIContent.none, ms_splitterStyle, GUILayout.Height(1f));
                    EUtil.PushGUIColor(new Color(1,1,1,0.5f));
                    EditorGUI.DrawTextureAlpha(rc, Texture2D.whiteTexture);
                    EUtil.PopGUIColor();
                }

                EditorGUI.BeginChangeCheck();
                m_dbgShowHandle = EditorGUILayout.Toggle("Show Handle", m_dbgShowHandle);
                if (EditorGUI.EndChangeCheck())
                {
                    EUtil.RepaintSceneView();
                }

                // line 1
                GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button(CONT_ZeroAll))
                    { //zero-out all rotation
                        var solver = mb.GetSolver();
                        var joints = solver.GetJoints();
                        Undo.RecordObjects(joints, "Zero-out IK joints");
                        foreach (var j in joints)
                        {
                            j.localRotation = Quaternion.identity;
                        }
                    }
                    if (GUILayout.Button(CONT_Ret))
                    { // return target to endJoint
                        var solver = mb.GetSolver();
                        solver.Target = mb.transform.position;
                        EUtil.RepaintSceneView();
                    }
                }
                GUILayout.EndHorizontal();

                // line 2
                GUILayout.BeginHorizontal();
                {
                    bool bStartedStep = m_dbgStepIE != null;

                    EUtil.PushGUIEnable(!bStartedStep);
                    {
                        if (GUILayout.Button(CONT_GO))
                        {
                            var solver = mb.GetSolver();
                            Undo.RecordObjects(solver.GetJoints(), "IK Follow");
                            solver.Execute();
                        }
                    }
                    EUtil.PopGUIEnable();

                    Color c = bStartedStep ? Color.red : Color.green;
                    string s = bStartedStep ? "StopStep" : "StartStep";
                    if (EUtil.Button(s, c))
                    {
                        var solver = mb.GetSolver();
                        if (!bStartedStep)
                        {
                            m_dbgStepIE = solver.DBGExecute();
                        }
                        else
                        {
                            solver.dbg_interrupt = true;
                            m_dbgStepIE.MoveNext();
                            m_dbgStepIE = null;
                            bStartedStep = false;
                        }
                    }                    

                    EUtil.PushGUIEnable(bStartedStep);
                    {
                        if (GUILayout.Button(CONT_Step))
                        {
                            bool bNotOver = m_dbgStepIE.MoveNext();
                            if (!bNotOver)
                            {
                                m_dbgStepIE = null;
                                bStartedStep = false;
                            }
                        }
                        if (GUILayout.Button(CONT_Continue))
                        {
                            while (m_dbgStepIE.MoveNext() == true)
                            {
                                ;
                            }
                            m_dbgStepIE = null;
                            bStartedStep = false;
                        }
                    }
                    EUtil.PopGUIEnable();
                }
                GUILayout.EndHorizontal();
            }

        }

        public void OnSceneGUI()
        {
            CCDSolverMB mb = (CCDSolverMB)target;
            CCDSolver solver = mb.GetSolver();
            if (solver == null || solver.Count < 1)
                return;

            var joints = solver.GetJoints();
            Color saveColor = Handles.color;

            Handles.color = Pref.IKBoneLinkColor;
            //1. draw bone line
            for (int i = 0; i < joints.Length - 1; ++i)
            {
                var p0 = joints[i].position;
                var p1 = joints[i + 1].position;
                Handles.DrawAAPolyLine(3f, p0, p1);
            }

            //2. call each constraint's OnSceneGUI
            for (int i = 0; i < joints.Length - 1; ++i)
            {
                var cons = solver.GetConstraint(i);

                foreach (var con in cons)
                {
                    if (!con || !con.enabled)
                        continue;

                    Editor e = EUtil.GetEditor(con);
                    IOnSceneGUI igui = e as IOnSceneGUI;
                    if (igui != null)
                    {
                        igui.OnSceneGUI();
                    }                    
                }
            }

            Handles.color = saveColor;

            //3. debug draw
            if (Pref.showIKDebug)
            {
                if( m_dbgShowHandle )
                    solver.Target = Handles.PositionHandle(solver.Target, Quaternion.identity);
            }
        }

	    #endregion "unity event handlers"
	
		#region "public method"
	    // public method
	
	    #endregion "public method"
	
		#region "private method"


	
	    #endregion "private method"
	
		#region "constant data"
	    // constant data

        private readonly static GUIContent CONT_BoneCnt = new GUIContent("Bone Count", "how many bones in this IK chain");
        private readonly static GUIContent CONT_DistThres = new GUIContent("Dist Threshold", "IK calculation is considered finished if the end-joint is near the target position");
        private readonly static GUIContent CONT_RevertOpt = new GUIContent("Revert Option", "How to behave if IK cannot find out reasonable solution for given target");

        private readonly static GUIContent CONT_ZeroAll = new GUIContent("Zero", "zero-out all rotation");
        private readonly static GUIContent CONT_Ret = new GUIContent("Ret", "return the target to endJoint");
        private readonly static GUIContent CONT_Step = new GUIContent("Step", "Make a step of the IK calculation");
        private readonly static GUIContent CONT_Continue = new GUIContent("Continue", "step on till end");
        private readonly static GUIContent CONT_GO = new GUIContent("Follow", "calculate the IK result immediately");
        //private readonly static GUIContent CONT_Stop = new GUIContent("Stop", "Stop current in-progress IK calculation");
        //private readonly static GUIContent CONT_Start = new GUIContent("Start");
	
	    #endregion "constant data"
        
    }

    public interface IOnSceneGUI
    {
        void OnSceneGUI();
    }
}
