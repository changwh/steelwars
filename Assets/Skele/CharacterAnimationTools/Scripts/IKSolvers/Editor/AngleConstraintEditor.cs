using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using MH.Skele;

namespace MH.IKConstraint
{
    [CustomEditor(typeof(AngleConstraintMB))]
    public class AngleConstraintEditor : Editor, IOnSceneGUI
    {
		#region "configurable data"
	    // configurable data
	
	    #endregion "configurable data"
	
		#region "data"
	    // data

        private static bool ms_inited = false;
        private static float ms_markerSize = 1f;
        private static Color ms_arcColor;


        private static bool ms_showDisplaySetting = true;

	    #endregion "data"
	
		#region "unity event handlers"
	    // unity event handlers

        void OnEnable()
        {
            if (!ms_inited)
            {
                ms_markerSize = Pref.IKConMarkerSize;
                ms_arcColor = Pref.IKAngleConstraintArcColor;
                ms_inited = true;
            }

            AngleConstraintMB mb = (AngleConstraintMB)target;
            if (mb == null)
                return;//this is possible

            Transform mbtr = mb.transform;
            if (mbtr.childCount > 0 && mb.nextJoint == null)
            {
                var child = mbtr.GetChild(0);
                mb.nextJoint = child;
                _OnNextJointChanged();
                SceneView.RepaintAll();
            }
        }

        public override void OnInspectorGUI()
        {
            AngleConstraintMB mb = (AngleConstraintMB)target;

            mb.enabled = EditorGUILayout.Toggle("Enabled", mb.enabled);

            EditorGUI.BeginChangeCheck();
            mb.nextJoint = EditorGUILayout.ObjectField("nextJoint", mb.nextJoint, typeof(Transform), true) as Transform;
            if (EditorGUI.EndChangeCheck())
            {
                _OnNextJointChanged();
                EUtil.RepaintSceneView();
            }

            if (mb.nextJoint == null)
            {
                EditorGUILayout.LabelField("Set the nextJoint first...");
                return;
            }
            else
            {
                EUtil.PushGUIEnable(mb.enabled);

                GUILayout.BeginHorizontal();
                {
                    GUILayout.Space(30f);
                    if (EUtil.Button("ReInit", "decide all the parameters with default method", Color.green))
                    {
                        Undo.RecordObject(mb, "ReInit");
                        _AutoSetParameters();
                        EUtil.RepaintSceneView();
                    }
                    GUILayout.Space(30f);
                }
                GUILayout.EndHorizontal();

                // min/max limit
                float min = mb.minLimit;
                float max = mb.maxLimit;
                EditorGUI.BeginChangeCheck();
                EUtil.DrawMinMaxSlider("Angle Limits", ref min, ref max, -180f, 180f);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(mb, "Modify Angle limits");
                    mb.minLimit = min;
                    mb.maxLimit = max;
                    EUtil.RepaintSceneView();
                }

                EditorGUI.BeginChangeCheck();
                Vector3 newRotAxis = EUtil.DrawV3P(new GUIContent("Rotation Axis", "in parent space, in world space if no parent joint"), mb.rotAxis);
                Vector3 newPrimAxis = EUtil.DrawV3P(new GUIContent("Primary Axis", "in parent space, in world space if no parent joint"), mb.primAxis);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(mb, "Modify Constraint parameters");
                    mb.rotAxis = newRotAxis;
                    mb.primAxis = newPrimAxis;
                    mb.CalcInitData(); //!! recalc the startLocalRot
                    EUtil.RepaintSceneView();
                }

                ms_showDisplaySetting = EditorGUILayout.Foldout(ms_showDisplaySetting, "Display Settings:");
                if (ms_showDisplaySetting)
                {
                    EditorGUI.BeginChangeCheck();
                    ms_markerSize = EditorGUILayout.FloatField("ArcSize", ms_markerSize);
                    ms_arcColor = EditorGUILayout.ColorField("ArcColor", ms_arcColor);
                    if (EditorGUI.EndChangeCheck())
                    {
                        EUtil.RepaintSceneView();
                    }
                }

                EUtil.PopGUIEnable();
            }
            
        }

        private void _OnNextJointChanged()
        {
            AngleConstraintMB mb = (AngleConstraintMB)target;
            var nextJoint = mb.nextJoint;
            if (nextJoint != null)
            {
                if (nextJoint.parent == mb.transform)
                    _AutoSetParameters();
                else
                {
                    EUtil.ShowNotification(nextJoint.name + " is not children of " + mb.name, 5f);
                    mb.nextJoint = null;
                }
            }
        }

        public void OnSceneGUI()
        {
            AngleConstraintMB mb = (AngleConstraintMB)target;

            if (mb == null)
                return;

            Transform j = mb.transform;
            Transform jparent = j.parent;
            Transform jchild = mb.nextJoint;

            if (jchild == null)
                return;

            var oldColor = Handles.color;

            float szMul = EUtil.GetHandleSize(j.position) * ms_markerSize;

            Vector3 worldRotAxis = Misc.TransformDirection(jparent, mb.rotAxis).normalized;
            Vector3 worldPrimAxis = Misc.TransformDirection(jparent, mb.primAxis).normalized;

            float minLimit = mb.minLimit;
            float maxLimit = mb.maxLimit;
            Vector3 worldFromVec = Quaternion.AngleAxis(minLimit, worldRotAxis) * worldPrimAxis;
            Handles.color = ms_arcColor;
            Handles.DrawSolidArc(j.position, worldRotAxis, worldFromVec,
                maxLimit - minLimit, szMul);
            
            Handles.color = Color.red;
            Handles.DrawAAPolyLine(5f, j.position, j.position + worldRotAxis*szMul); //rot axis
            Handles.color = Color.green;
            Handles.DrawAAPolyLine(5f, j.position, j.position + worldPrimAxis*szMul); //start dir

            Handles.color = oldColor;
        }
	
	    #endregion "unity event handlers"
	
		#region "public method"
	    // public method
	
	    #endregion "public method"
	
		#region "private method"

        private void _AutoSetParameters()
        {
            AngleConstraintMB mb = (AngleConstraintMB)target;
            Transform j = mb.transform;
            Transform jparent = j.parent;
            Transform jchild = mb.nextJoint;

            // decide the axis
            {
                if (jparent == null)
                {
                    mb.rotAxis = j.up;
                }
                else if( jchild == null )
                {
                    mb.rotAxis = j.up;
                }
                else
                {
                    var ppos = jparent.position;
                    var spos = j.position;
                    var cpos = jchild.position;

                    Vector3 cross = Misc.VecCross(spos, cpos, ppos, spos);
                    if (cross == Vector3.zero)
                    {
                        mb.rotAxis = j.up;
                    }
                    else
                    {
                        mb.rotAxis = cross.normalized;
                    }

                    mb.rotAxis = Misc.InverseTransformDirection(jparent, mb.rotAxis); //convert to parent space
                }
            }

            // decide the primAxis
            {
                Vector3 fwd = j.right;
                if (jparent != null)
                {
                    fwd = (j.position - jparent.position).normalized;
                }
                else if (jchild != null)
                {
                    fwd = (jchild.position - j.position).normalized;
                }
                mb.primAxis = Misc.InverseTransformDirection(jparent, fwd);
            }

            // decide the min/max limit
            {
                mb.minLimit = DEF_MINLIM;
                mb.maxLimit = DEF_MAXLIM;
            }

            mb.CalcInitData(); //calc startlocalRot
        }
		
	    #endregion "private method"
	
		#region "constant data"
	    // constant data

        private const float DEF_MINLIM = -135f;
        private const float DEF_MAXLIM = 0f;
	
	    #endregion "constant data"

        
    }
}