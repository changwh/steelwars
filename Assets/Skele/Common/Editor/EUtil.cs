#if UNITY_5
#define U5
#endif

#if UNITY_5 && (UNITY_5_1 || UNITY_5_2 || UNITY_5_3 || UNITY_5_4 || UNITY_5_5)
#define U5_1_ABOVE
#endif

#define HAS_RCALL

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections;

using Object = UnityEngine.Object;

/// <summary>
/// Editor Utility
/// </summary>
namespace MH
{
    using EditorMap = Dictionary<Object, Editor>;

    public class EUtil
    {
        public static Stack<Color> ms_clrStack = new Stack<Color>();
        public static Stack<Color> ms_contentClrStack = new Stack<Color>();
        public static Stack<bool> ms_enableStack = new Stack<bool>();
        public static Stack<Color> ms_BackgroundClrStack = new Stack<Color>();
        public static Stack<float> ms_LabelWidthStack = new Stack<float>();
        public static Stack<float> ms_FieldWidthStack = new Stack<float>();

        private static double ms_notificationHideTime = double.MinValue;

        private static string ms_lastFocusControl = string.Empty;

        private static EditorMap ms_editorMap;

        static EUtil()
        {
            ms_editorMap = new EditorMap();
        }

	    #region "controls"
	    // "controls" 

        public static void PushLabelWidth(float w)
        {
            ms_LabelWidthStack.Push(EditorGUIUtility.labelWidth);
            EditorGUIUtility.labelWidth = w;
        }

        public static float PopLabelWidth()
        {
            float w = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = ms_LabelWidthStack.Pop();
            return w;
        }

        public static void PushFieldWidth(float w)
        {
            ms_FieldWidthStack.Push(EditorGUIUtility.fieldWidth);
            EditorGUIUtility.fieldWidth = w;
        }

        public static float PopFieldWidth()
        {
            float w = EditorGUIUtility.fieldWidth;
            EditorGUIUtility.fieldWidth = ms_FieldWidthStack.Pop();
            return w;
        }

        public static void PushGUIColor(Color newClr)
        {
            ms_clrStack.Push(GUI.color);
            GUI.color = newClr;
        }

        public static Color PopGUIColor()
        {
            Color r = GUI.color;
            GUI.color = ms_clrStack.Pop();
            return r;
        }

        public static void PushBackgroundColor(Color newClr)
        {
            ms_BackgroundClrStack.Push(GUI.backgroundColor);
            GUI.backgroundColor = newClr;
        }

        public static Color PopBackgroundColor()
        {
            Color r = GUI.backgroundColor;
            GUI.backgroundColor = ms_BackgroundClrStack.Pop();
            return r;
        }

        public static void PushContentColor(Color clr)
        {
            ms_contentClrStack.Push(GUI.contentColor);
            GUI.contentColor = clr;
        }

        public static Color PopContentColor()
        {
            Color r = GUI.contentColor;
            GUI.contentColor = ms_contentClrStack.Pop();
            return r;
        }

        public static void PushGUIEnable(bool newState)
        {
            ms_enableStack.Push(GUI.enabled);
            GUI.enabled = newState;
        }

        public static bool PopGUIEnable()
        {
            bool r = GUI.enabled;
            GUI.enabled = ms_enableStack.Pop();
            return r;
        }

        public static bool Button(string msg, Color c)
        {
            EUtil.PushBackgroundColor(c);
            bool bClick = GUILayout.Button(msg);
            EUtil.PopBackgroundColor();
            return bClick;
        }

        public static bool Button(string msg, string tips)
        {
            bool bClick = GUILayout.Button(new GUIContent(msg, tips));
            return bClick;
        }

        public static bool Button(string msg, string tips, Color c)
        {
            EUtil.PushBackgroundColor(c);
            bool bClick = GUILayout.Button(new GUIContent(msg, tips));
            EUtil.PopBackgroundColor();
            return bClick;
        }

        public static bool Button(string msg, string tips, Color c, params GUILayoutOption[] options)
        {
            EUtil.PushBackgroundColor(c);
            bool bClick = GUILayout.Button(new GUIContent(msg, tips), options);
            EUtil.PopBackgroundColor();
            return bClick;
        }

        public static bool Button(Texture2D tex, string tips, params GUILayoutOption[] options)
        {
            bool bClick = GUILayout.Button(new GUIContent(tex, tips), options);
            return bClick;
        }

        public static bool Button(Texture2D tex, string tips, GUIStyle style, params GUILayoutOption[] options)
        {
            bool bClick = GUILayout.Button(new GUIContent(tex, tips), style, options);
            return bClick;
        }

        public static bool Button(Texture2D tex, string tips, Color c, params GUILayoutOption[] options)
        {
            EUtil.PushBackgroundColor(c);
            bool bClick = GUILayout.Button(new GUIContent(tex, tips), options);
            EUtil.PopBackgroundColor();
            return bClick;
        }

        public static bool Button(Texture2D tex, string tips, Color c, GUIStyle style, params GUILayoutOption[] options)
        {
            EUtil.PushBackgroundColor(c);
            bool bClick = GUILayout.Button(new GUIContent(tex, tips), style, options);
            EUtil.PopBackgroundColor();
            return bClick;
        }

        /// <summary>
        /// this IntField will only affect input value when enter is pressed
        /// when enter is pressed, the focus is lost
        /// 
        /// return value indicates whether confirmed
        /// </summary>
        private static int ms_editingInt = 0;
        public static bool IntField(string name, ref int val)
        {
            return IntField(name, ref val, null);
        }
        
        public static bool IntField(string name, ref int val, params GUILayoutOption[] options)
        {
            GUI.SetNextControlName(name);

            if (GUI.GetNameOfFocusedControl() != name)
            {
                EditorGUILayout.IntField(val, options);
                return false;
            }

            /////////////////////////////
            // drawing focusing field
            /////////////////////////////

            if (ms_lastFocusControl != name)
            { //when just change focus control, put value into editingWeight
                ms_lastFocusControl = name;
                ms_editingFloat = val;
            }

            bool applying = false;

            if (Event.current.isKey)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        val = ms_editingInt;
                        applying = true;
                        GUI.FocusControl(string.Empty); //lose focus
                        Event.current.Use();    // Ignore event, otherwise there will be control name conflicts!
                        break;
                }
            }

            ms_editingInt = EditorGUILayout.IntField(ms_editingInt, options);
            return applying;
        }

        public static bool IntField(string name, GUIContent label, ref int val, params GUILayoutOption[] options)
        {
            GUI.SetNextControlName(name);

            if (GUI.GetNameOfFocusedControl() != name)
            {
                EditorGUILayout.IntField(label, val, options);
                return false;
            }

            /////////////////////////////
            // drawing focusing field
            /////////////////////////////

            if (ms_lastFocusControl != name)
            { //when just change focus control, put value into editingWeight
                ms_lastFocusControl = name;
                ms_editingFloat = val;
            }

            bool applying = false;

            if (Event.current.isKey)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        val = ms_editingInt;
                        applying = true;
                        GUI.FocusControl(string.Empty); //lose focus
                        Event.current.Use();    // Ignore event, otherwise there will be control name conflicts!
                        break;
                }
            }

            ms_editingInt = EditorGUILayout.IntField(label, ms_editingInt, options);
            return applying;
        }

        /// <summary>
        /// this FloatField will only affect input value when enter is pressed
        /// when enter is pressed, the focus is lost
        /// 
        /// return value indicates whether confirmed
        /// </summary>
        private static float ms_editingFloat = 0;
        public static bool FloatField(string name, ref float val)
        {
            return FloatField(name, ref val, null);
        }
        public static bool FloatField(string name, ref float val, params GUILayoutOption[] options)
        {
            GUI.SetNextControlName(name);

            if (GUI.GetNameOfFocusedControl() != name)
            {
                EditorGUILayout.FloatField(val, options);
                return false;
            }

            /////////////////////////////
            // drawing focusing field
            /////////////////////////////

            if (ms_lastFocusControl != name)
            { //when just change focus control, put value into editingWeight
                ms_lastFocusControl = name;
                ms_editingFloat = val;
            }

            bool applying = false;

            if (Event.current.isKey)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        val = ms_editingFloat;
                        applying = true;
                        GUI.FocusControl(string.Empty); //lose focus
                        Event.current.Use();    // Ignore event, otherwise there will be control name conflicts!
                        break;
                }
            }

            ms_editingFloat = EditorGUILayout.FloatField(ms_editingFloat, options);
            return applying;
        }

        private static string ms_editingString = string.Empty;
        public static bool StringField(string name, ref string val)
        {
            return StringField(name, ref val, null);
        }
        public static bool StringField(string name, ref string val, params GUILayoutOption[] options)
        {
            GUI.SetNextControlName(name);

            if (GUI.GetNameOfFocusedControl() != name)
            {
                EditorGUILayout.TextField(val, options);
                return false;
            }

            /////////////////////////////
            // drawing focusing field
            /////////////////////////////

            if (ms_lastFocusControl != name)
            { //when just change focus control, put value into editingWeight
                ms_lastFocusControl = name;
                ms_editingString = val;
            }

            bool applying = false;

            if (Event.current.isKey)
            {
                switch (Event.current.keyCode)
                {
                    case KeyCode.Return:
                    case KeyCode.KeypadEnter:
                        val = ms_editingString;
                        applying = true;
                        GUI.FocusControl(string.Empty); //lose focus
                        Event.current.Use();    // Ignore event, otherwise there will be control name conflicts!
                        break;
                }
            }

            ms_editingString = EditorGUILayout.TextField(ms_editingString, options);
            return applying;
        }
	
	    #endregion "controls"
        
        public static void RotateView(Quaternion rot, bool ortho = false)
        {
            var v = GetSceneView();
            v.orthographic = ortho;
            v.LookAt(v.pivot, rot);
        }

        /// <summary>
        /// WARN: used undocumented API
        /// </summary>
        public static void AlignViewToPos(Vector3 pos, float dist = 40f)
        {
            SceneView sv = GetSceneView();

            Transform camTr = sv.camera.transform;
            Vector3 dir = (camTr.position - pos).normalized;

            var target = new GameObject();
            Transform targetTr = target.transform;
            targetTr.position = pos + dist * dir;
            targetTr.rotation = Quaternion.LookRotation(-dir);
            sv.AlignViewToObject(target.transform);
            
            GameObject.DestroyImmediate(target);
        }

        /// <summary>
        /// WARN: used undocumented API
        /// </summary>
        public static void AlignViewToObj(GameObject go, float dist = 40f)
        {
            SceneView sv = GetSceneView();
            Transform tr = go.transform;
            Vector3 pos = tr.position;
            Renderer render = go.GetComponent<Renderer>();

            if (render != null)
            {
                Bounds bd = render.bounds;
                dist = Mathf.Max(bd.size.x, bd.size.y, bd.size.z);
            }

            Transform camTr = sv.camera.transform;
            Vector3 dir = (camTr.position - tr.position).normalized;

            var target = new GameObject();
            Transform targetTr = target.transform;
            targetTr.position = pos + dist * dir;
            targetTr.rotation = Quaternion.LookRotation(-dir);
            sv.AlignViewToObject(target.transform);
            GameObject.DestroyImmediate(target);

        }

        /// <summary>
        /// WARN: used undocumented API
        /// </summary>
        public static void FrameSelected()
        {
            SceneView.lastActiveSceneView.FrameSelected();
        }

        /// <summary>
        /// WARN: used undocumented API
        /// </summary>
        public static void SceneViewLookAt(Vector3 pos)
        {
            GetSceneView().LookAt(pos);
        }
        public static void SceneViewLookAt(Vector3 pos, float sz)
        {
            SceneView sv = GetSceneView();
            Transform tr = sv.camera.transform;

            sv.LookAt(pos, tr.rotation, sz);
        }


        public static SceneView GetSceneView()
        {
            return SceneView.lastActiveSceneView == null ?
                EditorWindow.GetWindow<SceneView>() :
                SceneView.lastActiveSceneView;
        }

        /// <summary>
        /// like HandlesUtility.GetHandleSize, but can limit the max size by limiting z
        /// </summary>
        public static float GetHandleSize(Vector3 position, float maxZ = 7f)
        {
            Camera curCam = Camera.current;
            position = Handles.matrix.MultiplyPoint(position);
            if (curCam)
            {
                Transform camTr = curCam.transform;
                Vector3 camPos = camTr.position;
                float z = Vector3.Dot(position - camPos, camTr.TransformDirection(new Vector3(0f, 0f, 1f)));
                z = Mathf.Min(z, maxZ);
                Vector3 a = curCam.WorldToScreenPoint(camPos + camTr.TransformDirection(new Vector3(0f, 0f, z)));
                Vector3 b = curCam.WorldToScreenPoint(camPos + camTr.TransformDirection(new Vector3(1f, 0f, z)));
                float magnitude = (a - b).magnitude;
                return 80f / Mathf.Max(magnitude, 0.0001f);
            }
            return 20f;
        }

        public static void RepaintSceneView()
        {
            GetSceneView().Repaint();
        }

        public static void ShowNotification(string msg, float duration = 1.5f)
        {
            if (ms_notificationHideTime < 0)
                EditorApplication.update += _NotificationUpdate;

            EUtil.GetSceneView().ShowNotification(new GUIContent(msg));
            ms_notificationHideTime = EditorApplication.timeSinceStartup + duration;
            EUtil.GetSceneView().Repaint();
        }

        public static void HideNotification()
        {
            EUtil.GetSceneView().RemoveNotification();
            EUtil.GetSceneView().Repaint();
        }

        private static void _NotificationUpdate()
        {
            if (EditorApplication.timeSinceStartup > ms_notificationHideTime)
            {
                HideNotification();
                EditorApplication.update -= _NotificationUpdate;
                ms_notificationHideTime = double.MinValue;
            }
        }

        public static Vector2 DrawV2(Vector2 v)
        {
            var o = GUILayout.MinWidth(30f);
            float oldWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 16f;

            EditorGUILayout.BeginHorizontal();
            {
                v.x = EditorGUILayout.FloatField("X", v.x, o);
                v.y = EditorGUILayout.FloatField("Y", v.y, o);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = oldWidth;

            return v;
        }
        public static Vector2 DrawV2(string label, Vector2 v)
        {
            var o = GUILayout.MinWidth(30f);
            float oldWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 16f;

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(label, GUILayout.MaxWidth(80f));
                v.x = EditorGUILayout.FloatField("X", v.x, o);
                v.y = EditorGUILayout.FloatField("Y", v.y, o);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = oldWidth;

            return v;
        }

        public static Vector3 DrawV3(Vector3 v)
        {
            var o = GUILayout.MinWidth(30f);
            float oldWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 16f;

            EditorGUILayout.BeginHorizontal();
            {
                v.x = EditorGUILayout.FloatField("X", v.x, o);
                v.y = EditorGUILayout.FloatField("Y", v.y, o);
                v.z = EditorGUILayout.FloatField("Z", v.z, o);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = oldWidth;

            return v;
        }
        public static Vector3 DrawV3(string label, Vector3 v)
        {
            var o = GUILayout.MinWidth(30f);
            float oldWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 16f;

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(label, GUILayout.MaxWidth(80f));
                v.x = EditorGUILayout.FloatField("X", v.x, o);
                v.y = EditorGUILayout.FloatField("Y", v.y, o);
                v.z = EditorGUILayout.FloatField("Z", v.z, o);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = oldWidth;

            return v;
        }
        public static Vector3 DrawV3P(GUIContent label, Vector3 v)
        {
            var o = GUILayout.MinWidth(30f);
            float oldWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 16f;

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField(label, GUILayout.MaxWidth(80f));
                v.x = EditorGUILayout.FloatField("X", (float)Math.Round(v.x, 3), o);
                v.y = EditorGUILayout.FloatField("Y", (float)Math.Round(v.y, 3), o);
                v.z = EditorGUILayout.FloatField("Z", (float)Math.Round(v.z, 3), o);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUIUtility.labelWidth = oldWidth;

            return v;
        }

        public static void DrawMinMaxSlider(string label, ref float min, ref float max, float minLimit, float maxLimit)
        {
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Label(label);
                min = EditorGUILayout.FloatField(min);
                EditorGUILayout.MinMaxSlider(ref min, ref max, minLimit, maxLimit);
                max = EditorGUILayout.FloatField(max);
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// [HACK_TRICK]
        /// check if UAW(UnityAnimationWindow) is open
        /// </summary>
        public static bool IsUnityAnimationWindowOpen()
        {
            return GetUnityAnimationWindow() != null;
        }

#if HAS_RCALL
        /// <summary>
        /// [HACK TRICK]
        /// get UAW if there is, else null
        /// </summary>
        public static object GetUnityAnimationWindow()
        {
            IList lst = (IList)RCall.CallMtd("UnityEditor.AnimationWindow", "GetAllAnimationWindows", null, null);
            if (lst.Count > 0)
                return lst[0];
            else
                return null;
        }

        public static object GetUnityAnimationWindowState(object uaw)
        {
#if U5
            object animEditor = RCall.GetField("UnityEditor.AnimationWindow", "m_AnimEditor", uaw);
            object uawstate = RCall.GetField("UnityEditor.AnimEditor", "m_State", animEditor);
#else
            object uawstate = RCall.GetProp("UnityEditor.AnimationWindow", "state", uaw);
#endif
            return uawstate;
        }

#endif
        //public static object Call(string className, string mtd, params object[] ps)
        //{
        //    Type t = typeof(AssetDatabase);
        //    Dbg.Log(t);
        //    //Dbg.Assert(t != null, "failed to get class: {0}", className);
        //    //MethodInfo method
        //    //     = t.GetMethod(mtd, BindingFlags.Static | BindingFlags.Public);
        //    //Dbg.Assert(method != null, "failed to get method: {0}", mtd); 

        //    //return method.Invoke(null, ps);
        //    return null;
        //}

        public static void StartInputModalWindow(System.Action<string> onSuccess, System.Action onCancel, string prompt = "Input", string title = "", Texture2D bg = null)
        {
            InputModalWindow wndctrl = new InputModalWindow(onSuccess, onCancel, title, prompt, bg);
            GUIWindowMgr.Instance.Add(wndctrl);
        }

        public static void StartObjRefModalWindow(System.Action<Object> onSuccess, System.Action onCancel, Type tp, string prompt = "Object Reference", Texture2D bg = null)
        {
            if( tp == null )
                tp = typeof(Object);

            ObjectRefModalWindow wndctrl = new ObjectRefModalWindow(onSuccess, onCancel, tp, prompt, bg);
            GUIWindowMgr.Instance.Add(wndctrl);
        }

        /// <summary>
        /// will create entry in assetdatabase if the path is not taken,
        /// will replace the old entry if there's already one, and keep the ref valid
        /// </summary>
        public static void SaveAnimClip(AnimationClip clip, string path)
        {
            AnimationClip existClip = AssetDatabase.LoadAssetAtPath(path, typeof(AnimationClip)) as AnimationClip;
            if (existClip != null)
            {
                EditorUtility.CopySerialized(clip, existClip);
            }
            else
            {
                AssetDatabase.CreateAsset(clip, path);
            }
        }

        /// <summary>
        /// cache the transform recursively
        /// </summary>
        public static List<XformData> CacheXformData(Transform root)
        {
            List<XformData> lst = new List<XformData>();

            _CacheXformData(root, lst);

            return lst;
        }

        private static void _CacheXformData(Transform cur, List<XformData> lst)
        {
            XformData d = new XformData();
            d.CopyFrom(cur);
            lst.Add(d);

            for (int idx = 0; idx < cur.childCount; ++idx )
            {
                Transform childTr = cur.GetChild(idx);
                _CacheXformData(childTr, lst);
            }
        }

        public static void ApplyXformData(Transform root, List<XformData> lst)
        {
            int idx = 0;
            _ApplyXformData(root, lst, ref idx);
        }

        private static void _ApplyXformData(Transform cur, List<XformData> lst, ref int idx)
        {
            XformData d = lst[idx];
            d.Apply(cur);

            for(int cidx =0; cidx < cur.childCount; ++cidx )
            {
                Transform childTr = cur.GetChild(cidx);
                ++idx;
                _ApplyXformData(childTr, lst, ref idx);
            }
        }

        /// <summary>
        /// make a raycast on given mesh
        /// </summary>
        public delegate bool RaycastDele(Ray ray, Mesh mesh, Matrix4x4 mat, out RaycastHit hit);
        public static bool Raycast(Ray ray, Mesh mesh, Matrix4x4 l2wMat, out RaycastHit hit)
        {
            object[] parameters = new object[] { ray, mesh, l2wMat, null };
            bool bRes = (bool)RCall.CallMtdDeleType("UnityEditor.HandleUtility", "IntersectRayMesh", 
                typeof(RaycastDele), null, parameters);

            if( bRes )
            {
                hit = (RaycastHit)parameters[3];
            }
            else
            {
                hit = new RaycastHit();
            }

            hit = bRes ? ((RaycastHit)parameters[3]) : (new RaycastHit());

            return bRes;
        }
        public static bool Raycast(Ray ray, MeshFilter mf, out RaycastHit hit)
        {
            return Raycast(ray, mf.sharedMesh, mf.transform.localToWorldMatrix, out hit);
        }

        /// <summary>
        /// keep shooting ray until cannot get new result, 
        /// collect and return all results
        /// </summary>
        public static RaycastHit[] RaycastAll(Ray ray, Mesh mesh, Matrix4x4 mat)
        {
            List<RaycastHit> hitLst = new List<RaycastHit>();
            RaycastHit hit;

            bool bHit = true;

            int cnt = 0;
            do
            {
                if( ++cnt > 100 )
                {
                    Dbg.LogWarn("EUtil.RaycastAll: Inf-Loop!? break out!");
                    break;
                }

                bHit = Raycast(ray, mesh, mat, out hit);
                if( bHit )
                {
                    hitLst.Add(hit);
                    ray.origin = hit.point + ray.direction * 0.005f;
                }
            } while (bHit);

            return hitLst.ToArray();
        }

        public static float ScreenDist(Vector3 p0, Vector3 p1)
        {
            Vector2 sp0 = HandleUtility.WorldToGUIPoint(p0);
            Vector2 sp1 = HandleUtility.WorldToGUIPoint(p1);

            return Vector2.Distance(sp0, sp1);
        }

        public static void SetEditorWindowTitle(EditorWindow w, string title, Texture icon = null)
        {
#if U5_1_ABOVE
            if (icon != null)
                w.titleContent = new GUIContent(title, icon); 
            else
                w.titleContent = new GUIContent(title);
#else
            w.title = title;
#endif
        }

        public static bool SaveMeshToAssetDatabase(Mesh m)
        {
            string assetPath = EditorUtility.SaveFilePanelInProject("Save Mesh",
                                    m.name,
                                    "asset",
                                    "Specify path for mesh asset");

            if( !string.IsNullOrEmpty(assetPath) )
            {
                return SaveMeshToAssetDatabase(m, assetPath);
            }          
            else
            {
                return false;
            }
        }
        public static bool SaveMeshToAssetDatabase(Mesh m, string assetPath)
        {
            string verbose;
            return SaveMeshToAssetDatabase(m, assetPath, out verbose);
        }
        public static bool SaveMeshToAssetDatabase(Mesh m, string assetPath, out string verbose)
        {
            verbose = string.Empty;
            if (!string.IsNullOrEmpty(assetPath))
            {
                Mesh assetMesh = m;
                //Dbg.Log("exist-path: {0}\ntargetPath:{1}", AssetDatabase.GetAssetOrScenePath(assetMesh), assetPath);
                if (AssetDatabase.Contains(assetMesh))
                {
                    string oldPath = AssetDatabase.GetAssetPath(assetMesh);
                    verbose = string.Format("SUCCESS: Mesh already in AssetDatabase: {0}", oldPath);
                    return true;
                }
                else
                {
                    AssetDatabase.CreateAsset(assetMesh, assetPath);
                    verbose = string.Format("SUCCESS: Saved mesh to path: {0}", assetPath);
                    return true;
                }
            }
            else
            {
                verbose = "FAIL: AssetPath is empty";
                return false;
            }
        }

        public static bool IsHierarchyHasFocus()
        {
            EditorWindow wnd = EditorWindow.focusedWindow;
            if (wnd == null)
                return false;

#if U5_1_ABOVE
            return wnd.titleContent.text == "UnityEditor.HierarchyWindow";
#else
            return wnd.title == "UnityEditor.HierarchyWindow";
#endif
        }

        public static void DrawSerializedObject(UnityEngine.Object obj)
        {
            SerializedObject serObj = new SerializedObject(obj);
            bool enterChildren = true;

            serObj.Update();

            for (var ie = serObj.GetIterator(); ie.NextVisible(enterChildren); )
            {
                if (ie.name == "m_Script")
                    continue;

                if (ie.hasVisibleChildren && !ie.isExpanded)
                {
                    enterChildren = false;
                }
                else
                {
                    enterChildren = true;
                }

                EditorGUILayout.PropertyField(ie);
            }

            serObj.ApplyModifiedProperties();
        }

        public static Editor GetEditor(UnityEngine.Object obj)
        {
            Editor e;
            if (ms_editorMap.TryGetValue(obj, out e))
            {
                return e;
            }
            else
            {
                e = Editor.CreateEditor(obj);
                ms_editorMap.Add(obj, e);
                return e;
            }
        }
    }



    /// <summary>
    /// this is a default modal window used to get a object reference
    /// </summary>
    public class ObjectRefModalWindow : GUIWindow
    {
        private System.Action<UnityEngine.Object> m_onSuccess;
        private System.Action m_onCancel;
        private string m_Prompt = "Select An Object";
        private Texture2D m_background = null;
        private Type m_ObjType = null;

        private Object m_curInput = null;
        private State m_State = State.NONE;

        public ObjectRefModalWindow(System.Action<UnityEngine.Object> onSuccess, System.Action onCancel)            
        {
            m_onSuccess = onSuccess;
            m_onCancel = onCancel;
            m_ObjType = typeof(UnityEngine.Object);
        }
        public ObjectRefModalWindow(System.Action<UnityEngine.Object> onSuccess, System.Action onCancel, 
            Type objType, string prompt, Texture2D bg)
        {
            m_onSuccess = onSuccess;
            m_onCancel = onCancel;
            m_ObjType = objType;
            m_Prompt = prompt;
            m_background = bg;
        }

        public override EReturn OnGUI()
        {
            Rect rc = new Rect(Screen.width * 0.5f - 150, Screen.height * 0.5f - 50f, 300, 60);

            //GUI.ModalWindow(m_Index, rc, _Draw, m_Title);
            EUtil.PushGUIEnable(true);

            if (m_background != null)
                GUI.DrawTexture(rc, m_background);
            GUILayout.BeginArea(rc);
            {
                _Draw();
            }
            GUILayout.EndArea();

            EUtil.PopGUIEnable();

            if (m_State == State.OK)
            {
                if (m_onSuccess != null)
                    m_onSuccess(m_curInput);
                return EReturn.STOP;
            }
            else if (m_State == State.CANCEL)
            {
                if (m_onCancel != null)
                    m_onCancel();
                return EReturn.STOP;
            }

            return EReturn.MODAL;
        }

        private void _Draw()
        {
            GUILayout.Label(m_Prompt);

            m_curInput = EditorGUILayout.ObjectField(m_curInput, m_ObjType, true);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("OK"))
                {
                    m_State = State.OK;
                }
                if (GUILayout.Button("Cancel"))
                {
                    m_State = State.CANCEL;
                }
            }
            GUILayout.EndHorizontal();
        }

        private enum State
        {
            NONE,
            OK,
            CANCEL,
        }
    }

    /// <summary>
    /// this is a default modal window used to get a text input
    /// </summary>
    public class InputModalWindow : GUIWindow
    {
        private System.Action<string> m_onSuccess;
        private System.Action m_onCancel;
        private string m_Title = "Input Modal Window";
        private string m_Prompt = "Input:";
        private Texture2D m_background = null;

        private string m_curInput = string.Empty;
        private State m_State = State.NONE;

        public InputModalWindow(System.Action<string> onSuccess, System.Action onCancel)
        {
            m_onSuccess = onSuccess;
            m_onCancel = onCancel;
        }
        public InputModalWindow(System.Action<string> onSuccess, System.Action onCancel, string title, string prompt, Texture2D bg)
        {
            m_onSuccess = onSuccess;
            m_onCancel = onCancel;
            m_Title = title;
            m_Prompt = prompt;
            m_background = bg;
        }

        public string Title
        {
            get { return m_Title; }
            set { m_Title = value; }
        }

        public string Prompt
        {
            get { return m_Prompt; }
            set { m_Prompt = value; }
        }

        public override EReturn OnGUI()
        {
            Rect rc = new Rect(Screen.width * 0.5f - 150, Screen.height * 0.5f - 50f, 300, 60);

            //GUI.ModalWindow(m_Index, rc, _Draw, m_Title);
            EUtil.PushGUIEnable(true);

            if (m_background != null)
                GUI.DrawTexture(rc, m_background);
            GUILayout.BeginArea(rc);
            {
                _Draw();
            }
            GUILayout.EndArea();

            EUtil.PopGUIEnable();

            if (m_State == State.OK)
            {
                if (m_onSuccess != null)
                    m_onSuccess(m_curInput);
                return EReturn.STOP;
            }
            else if (m_State == State.CANCEL)
            {
                if (m_onCancel != null)
                    m_onCancel();
                return EReturn.STOP;
            }

            return EReturn.MODAL;
        }

        private void _Draw()
        {
            GUILayout.Label(m_Prompt);

            m_curInput = GUILayout.TextField(m_curInput);

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("OK"))
                {
                    m_State = State.OK;
                }
                if (GUILayout.Button("Cancel"))
                {
                    m_State = State.CANCEL;
                }
            }
            GUILayout.EndHorizontal();

            //Rect rc = new Rect(0, 0, Screen.width, Screen.height);
            //GUI.DrawTexture(rc, EditorGUIUtility.whiteTexture);
            //if( GUI.Button(rc, "XXSDFSDF") )
            //{
            //    Dbg.Log("xxx");
            //}
            //else
            //{
            //    Dbg.Log("yyy");
            //}
        }

        private enum State
        {
            NONE,
            OK,
            CANCEL,
        }
    }

}
