#if UNITY_5 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#define U5
#endif

//#define HAS_ZIP
//#define HAS_EVTSET
#define HAS_JSON
//#define HAS_MAPUTIL
//#define HAS_POOL

using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if HAS_ZIP
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
#endif

#if HAS_JSON
//using LitJson;
using FullSerializer;
#endif

#if HAS_MAPUTIL
using Pathfinding;
using Path = System.IO.Path;
#endif

using Random = UnityEngine.Random;
using ExtMethods;

namespace MH
{

    /// <summary>
    /// some methods for Vector3
    /// </summary>
    public class V3Ext
    {
        public static void ReplaceXY(ref Vector3 v, float x, float y)
        {
            v.x = x;
            v.y = y;
        }
        public static void ReplaceXY2(ref Vector3 v3, ref Vector2 v2)
        {
            v3.x = v2.x;
            v3.y = v2.y;
        }
        public static void ReplaceXY(ref Vector3 v3, ref Vector3 src)
        {
            v3.x = src.x;
            v3.y = src.y;
        }
        public static void ReplaceXZ(ref Vector3 v3, float x, float z)
        {
            v3.x = x;
            v3.z = z;
        }
        public static void ReplaceXZ2(ref Vector3 v3, ref Vector2 v2)
        {
            v3.x = v2.x;
            v3.z = v2.y;
        }
        public static void ReplaceXZ(ref Vector3 v3, ref Vector3 src)
        {
            v3.x = src.x;
            v3.y = src.y;
        }

        public static Vector3 MultiplyComp(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

        public static Vector3 DivideComp(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }

        /// <summary>
        /// = Mathf.Abs(Vector3.Dot(a, b))
        /// </summary>
        public static float AbsDot(Vector3 a, Vector3 b)
        {
            return Mathf.Abs(Vector3.Dot(a, b));
        }
    }

    public class Misc
    {

        public static string
        GetTransformPath(Transform tr)
        {
            StringBuilder bld = new StringBuilder();
            while (tr != null)
            {
                if (bld.Length != 0)
                    bld.Insert(0, '/');
                bld.Insert(0, tr.name);
                tr = tr.parent;
            }
            return bld.ToString();
        }

        /// <summary>
        /// use GameObject to Find specified GO,
        /// if not found, create one
        /// </summary>
        public static GameObject
        ForceGetGO(string gopath, GameObject baseGO = null)
        {
            if (gopath == null || gopath.Length == 0)
                return null;
             
            GameObject go = null;
            Transform tr = baseGO == null ? null : baseGO.transform;

            string[] paths = gopath.Split('/');
            for (int idx = 0; idx < paths.Length; ++idx)
            {
                go = _ForceGetGO(paths[idx], tr);
                tr = go.transform;
            }

            Dbg.Assert(go != null, "Misc.ForceGetGO: failed to get go for path: {0}", gopath);

            return go;
        }

        private static GameObject
        _ForceGetGO(string onePath, Transform baseTR)
        {
            if( baseTR == null )
            {
                string pureName = onePath;
                if (pureName.StartsWith("/"))
                    pureName = pureName.Substring(1);

                GameObject go = GameObject.Find("/" + pureName);
                if (go == null)
                    go = new GameObject(pureName);

                return go;
            }
            else
            {
                Transform tr = baseTR.Find(onePath);
                if( tr == null )
                {
                    GameObject go = new GameObject(onePath);
                    Misc.AddChild(baseTR, go);
                    return go;
                }
                else
                {
                    return tr.gameObject;
                }
            }
        }

        public static Transform
        ForceGetChildTr(Transform baseTr, string childName)
        {
            Transform childTr = baseTr.Find(childName);
            if( childTr == null )
            {
                GameObject go = new GameObject(childName);
                Misc.AddChild(baseTr, go);
                return go.transform;
            }
            else
            {
                return childTr;
            }
        }

        /// <summary>
        /// add `child' as children of `parent'
        /// </summary>
        public static void
        AddChild(GameObject parent, GameObject child, bool keepLocalTr = false)
        {
            if (child == null)
                return;
            
            AddChild(parent != null ? parent.transform : null,
                child.transform,
                keepLocalTr);
        }        
        public static void
        AddChild(GameObject parent, Transform child, bool keepLocalTr = false)
        {
            if (child == null)
                return;
            
            AddChild(parent != null ? parent.transform : null, child, keepLocalTr);
        }
        public static void
        AddChild(Transform parent, GameObject child, bool keepLocalTr = false )
        {
            if (child == null)
                return;

            AddChild(parent, child.transform, keepLocalTr);
        }
        public static void
        AddChild(Transform parent, Transform child, bool keepLocalTr = false)
        {
            child.SetParent(parent, !keepLocalTr);

            //XformData data = null;
            //if( keepLocalTr )
            //{
            //    data = new XformData();
            //    data.CopyFrom(child);
            //}
            //child.parent = parent;
            //if( keepLocalTr )
            //{
            //    data.Apply(child);
            //}
        }

        public static byte[] FromBase64(string es)
        {
            return System.Convert.FromBase64String(es);
        }

        /// <summary>
        /// convert a byte[] to a Base64 string
        /// </summary>
        public static string ToBase64(byte[] bytes)
        {
            return System.Convert.ToBase64String(bytes);
        }
        /// <summary>
        /// count the consecutive zero bits on the right side
        /// </summary>
        public static int CountZeroOnRight(uint v)
        {
            int c = 32;
            if (v != 0) c--;
            if ((v & 0x0000FFFF) != 0) { c -= 16; v &= 0x0000FFFF; }
            if ((v & 0x00FF00FF) != 0) { c -= 8; v &= 0x00FF00FF; }
            if ((v & 0x0F0F0F0F) != 0) { c -= 4; v &= 0x0F0F0F0F; }
            if ((v & 0x33333333) != 0) { c -= 2; v &= 0x33333333; }
            if ((v & 0x55555555) != 0) { c -= 1; v &= 0x55555555; }

            return c;
        }

        /// <summary>
        /// swap two elements in a IList<T>
        /// </summary>
        public static void Swap<T>(IList<T> lst, int idx1, int idx2)
        {
            T tmp = lst[idx1];
            lst[idx1] = lst[idx2];
            lst[idx2] = tmp;
        }

        public static void Swap<T>(ref T lhs, ref T rhs) 
        {
            T tmp = lhs;
            lhs = rhs;
            rhs = tmp;
        }

        /// <summary>
        /// given a rotation radian(-inf, inf),
        /// transform it to [0, 2*PI]
        /// </summary>
        public static float NormalizeRotation(float radian)
        {
            float pi2 = Mathf.PI * 2f;
            float r = radian % pi2;
            r = (r < 0) ? (r + pi2) : r;
            return r;
        }

        /// <summary>
        /// given a rotation angle (-inf, inf)
        /// transform it to [0, 360]
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        public static float NormalizeAngle(float angle)
        {
            float pi2 = 360f;
            float r = angle % pi2;
            r = (r < 0) ? (r + pi2) : r;
            return r;
        }

        /// <summary>
        /// given a rotation angel (-inf, inf)
        /// transform into [-180, 180]
        /// </summary>
        public static float NormalizeAnglePI(float angle)
        {
            float pi2 = 360f;
            float r = angle % pi2;
            float abs = Mathf.Abs(r);
            float s = Mathf.Sign(r);
            if (abs > 180f)
                r += -s * pi2;
            return r;
        }

        public static Vector3 NormalizeAnglePI(Vector3 vec)
        {
            return new Vector3(
                NormalizeAnglePI(vec.x),
                NormalizeAnglePI(vec.y),
                NormalizeAnglePI(vec.z)
                );
        }

        /// <summary>
        /// given vec, return a vector which is projection of it on a plane defined by normal 'n'
        /// </summary>
        public static Vector3 ProjectOnPlane(Vector3 vec, Vector3 n)
        {
            Vector3 np = Vector3.Project(vec, n);
            return vec - np;
        }

        /// <summary>
        /// get the abs distance of two angles
        /// for signed distance of angles, use Mathf.DeltaAngle
        /// </summary>
        public static float AngleDist(float lhs, float rhs)
        {
            //float a = Misc.NormalizeAngle(lhs);
            //float b = Misc.NormalizeAngle(rhs);
            //float d1 = Mathf.Abs(a - b);
            //float d2 = Mathf.Abs(a - b - 360f);
            //float d3 = Mathf.Abs(a - b + 360f);

            //return Mathf.Min(d3, Mathf.Min(d1, d2));

            return Mathf.Abs(Mathf.DeltaAngle(lhs, rhs));
        }

        private static StringBuilder ms_bld = null;
        public static string ListToString<T>(IList<T> lst, Func<T, string> toStr = null)
        {
            if( ms_bld == null )
            {
                ms_bld = new StringBuilder();
            }
            
            if( toStr == null )
            {
                for (int i = 0; i < lst.Count; ++i)
                {
                    ms_bld.AppendFormat("{0} ", lst[i]);
                }
            }
            else
            {
                for (int i = 0; i < lst.Count; ++i)
                {
                    ms_bld.AppendFormat("{0} ", toStr(lst[i]));
                }
            }
            

            string s = ms_bld.ToString();
            ms_bld.Remove(0, ms_bld.Length);
            return s;
        }

        public static string IEnumToString<T>(IEnumerator<T> ie, Func<T, string> toStr = null)
        {
            if (ms_bld == null)
            {
                ms_bld = new StringBuilder();
            }

            if (toStr == null)
            {
                for (; ie.MoveNext(); )
                {
                    ms_bld.AppendFormat("{0} ", ie.Current.ToString());
                }
            }
            else
            {
                for (; ie.MoveNext(); )
                {
                    ms_bld.AppendFormat("{0} ", toStr(ie.Current));
                }
            }


            string s = ms_bld.ToString();
            ms_bld.Remove(0, ms_bld.Length);
            return s;
        }

        public static T RandomGetElem<T>(IList<T> lst)
        {
            int cnt = lst.Count;
            int idx = Random.Range(0, cnt);
            return lst[idx];
        }

        /// <summary>
        /// given a value, return [0, range], the calculate simulates ping pong,
        /// could process negative value
        /// e.g.: range = 1, value = 1.4, => result 0.6
        ///                  value = 2.3  => result 0.3
        /// </summary>
        public static float PingPongnize(float value, float range)
        {
            float bigRange = 2 * range;
            float v = value % bigRange;
            if (v < 0)
                v += bigRange;

            if (v > range)
            {
                v = bigRange - v;
            }

            return v;
        }

        public static void LockCursor(bool bLock)
        {
#if ! U5
            Screen.lockCursor = bLock;
#else
            Cursor.visible = !bLock;
            Cursor.lockState = bLock ? CursorLockMode.Locked : CursorLockMode.None;
#endif
        }

        public static bool IsLockCursor()
        {
#if ! U5
            return Screen.lockCursor;
#else
            return Cursor.lockState == CursorLockMode.Locked;
#endif
        }

        /// <summary>
        /// check if a point is inside the camera's range, 
        /// NOTE: in range doesn't mean visible or not be blocked
        /// </summary>
        public static bool IsInCameraRange(Vector3 wpos, Camera cam)
        {
            Vector3 vpt = cam.WorldToViewportPoint(wpos);

            return 0 <= vpt.x && vpt.x <= 1f &&
                   0 <= vpt.y && vpt.y <= 1f &&
                   cam.nearClipPlane <= vpt.z && vpt.z <= cam.farClipPlane;
        }

        /// <summary>
        /// return the value with bigger abs
        /// </summary>
        public static float AbsMax(float lhs, float rhs)
        {
            float l = Mathf.Abs(lhs);
            float r = Mathf.Abs(rhs);

            return l > r ? lhs : rhs;
        }

        /// <summary>
        /// return the value with smaller abs
        /// </summary>
        public static float AbsMin(float lhs, float rhs)
        {
            float l = Mathf.Abs(lhs);
            float r = Mathf.Abs(rhs);

            return l < r ? lhs : rhs;
        }

        /// <summary>
        /// check if two vectors form acute angle ( less Than 90deg )
        /// </summary>
        public static bool IsAcuteAngle(Vector3 a, Vector3 b)
        {
            float dot = Vector3.Dot(a, b);
            return dot > 0;
        }

        /// <summary>
        /// check if two vectors form obtuse angle ( greater Than 90deg )
        /// </summary>
        public static bool IsObtuseAngle(Vector3 a, Vector3 b)
        {
            float dot = Vector3.Dot(a, b);
            return dot < 0;
        }

        /// <summary>
        /// if tr is null then return dir, 
        /// else tr.TransformDirection(dir)
        /// </summary>
        public static Vector3 TransformDirection(Transform tr, Vector3 dir)
        {
            if (tr == null)
                return dir;
            else
                return tr.TransformDirection(dir);
        }

        /// <summary>
        /// if tr is null then return dir, 
        /// else tr.InverseTransformDirection(dir)
        /// </summary>
        public static Vector3 InverseTransformDirection(Transform tr, Vector3 dir)
        {
            if (tr == null)
                return dir;
            else
                return tr.InverseTransformDirection(dir);
        }

        /// <summary>
        /// return <p0,p1> dot <p0,p2>
        /// </summary>
        public static float VecDot(Vector3 p0, Vector3 p1, Vector3 p2, bool normalize = true)
        {
            Vector3 p01 = p1 - p0;
            Vector3 p02 = p2 - p0;
            if (normalize)
            {
                p01.Normalize();
                p02.Normalize();
            }
            return Vector3.Dot(p01, p02);
        }

        /// <summary>
        /// return <p0,p1> dot <p2,p3>
        /// </summary>
        public static float VecDot(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, bool normalize = true)
        {
            Vector3 p01 = p1 - p0;
            Vector3 p23 = p3 - p2;
            if (normalize)
            {
                p01.Normalize();
                p23.Normalize();
            }
            return Vector3.Dot(p01, p23);
        }

        /// <summary>
        /// return <p0, p1> cross <p0, p2>
        /// </summary>
        public static Vector3 VecCross(Vector3 p0, Vector3 p1, Vector3 p2, bool normalize = true)
        {
            Vector3 p01 = p1 - p0;
            Vector3 p02 = p2 - p0;
            if (normalize)
            {
                p01.Normalize();
                p02.Normalize();
            }

            return Vector3.Cross(p01, p02);
        }

        /// <summary>
        /// return <p0,p1> cross <p2,p3>
        /// </summary>
        public static Vector3 VecCross(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, bool normalize = true)
        {
            Vector3 p01 = p1 - p0;
            Vector3 p23 = p3 - p2;
            if (normalize)
            {
                p01.Normalize();
                p23.Normalize();
            }
            return Vector3.Cross(p01, p23);
        }

        /// <summary>
        /// calculate the angle of (from -> to), and the related rotation axis,
        /// the axis would be near "hintAxis"
        /// if hintAxis is coplanar with from, to, then axis will be the ('from' X 'to')
        /// the returned angle is in [-180f, 180f]
        /// </summary>
        public static float ToAngleAxis(Vector3 from, Vector3 to, Vector3 hintAxis)
        {
            Vector3 tmpAxis;
            return ToAngleAxis(from, to, hintAxis, out tmpAxis);
        }
        public static float ToAngleAxis(Vector3 from, Vector3 to, Vector3 hintAxis, out Vector3 axis)
        {
            Quaternion q = Quaternion.FromToRotation(from, to);
            float angle = 0f;
            q.ToAngleAxis(out angle, out axis);
            if (Misc.IsObtuseAngle(axis, hintAxis))
            {
                angle = -angle;
                axis = -axis;
            }

            angle = Misc.NormalizeAnglePI(angle);

            return angle;
        }



        /// <summary>
        /// new an array, and create each element in array
        /// </summary>
        public static T[] CreateArray<T>(int sz) where T : class, new()
        {
            T[] arr = new T[sz];
            for( int i=0; i<sz; ++i )
            {
                arr[i] = new T();
            }
            return arr;
        }
        public static T[] CreateArray<T>(int sz, System.Func<T> initor ) where T : class, new()
        {
            T[] arr = new T[sz];
            for (int i = 0; i < sz; ++i)
            {
                arr[i] = initor();
            }
            return arr;
        }

        public static List<T> CreateList<T>(int sz)
        {
            List<T> lst = new List<T>();
            lst.Resize(sz);
            return lst;
        }
    }

    /// <summary>
    /// data conversion
    /// </summary>
    public class DataUtil
    {
        public static int ParseAsInt(string s, int def = 0)
        {
            int ret = 0;
            bool bOk = Int32.TryParse(s, out ret);
            return bOk ? ret : def;
        }

        public static uint ParseAsUint(string s, uint def = 0)
        {
            uint ret = 0;
            bool bOk = UInt32.TryParse(s, out ret);
            return bOk ? ret : def;
        }

        public static float ParseAsFloat(string s, float def = 0)
        {
            float ret = 0;
            bool bOk = Single.TryParse(s, out ret);
            return bOk ? ret : def;
        }

        public static double ParseAsDouble(string s, double def = 0)
        {
            double ret = 0;
            bool bOk = Double.TryParse(s, out ret);
            return bOk ? ret : def;
        }

        public static T ParseAsEnum<T>(string s, Type eType)
        {
            try{
                T ret = (T)Enum.Parse(eType, s);
                return ret;
            }catch(ArgumentException){
                Dbg.LogErr("DataUtil.ParseAsEnum: failed to parse {0} as {1}", s, eType.ToString());
                return default(T);
            }
        }


    }



    #if HAS_ZIP
    /// <summary>
    /// utility about zip/unzip
    /// </summary>
    public class ZUtil
    {
        public static void ZipFile(string inputFile, string outputFile)
        {
            Dbg.Assert(File.Exists(inputFile), "ZUtil.ZipFile: the inputfile not exist: {0}", inputFile);

            FastZip fzip = new FastZip();
            fzip.CreateZip(outputFile, Path.GetDirectoryName(inputFile), true, Path.GetFileName(inputFile));
        }

        public static void ZipDir(string inputDir, string outputFile)
        {
            Dbg.Assert(Directory.Exists(inputDir), "ZUtil.ZipDir: the inputDir not exist: {0}", inputDir);

            FastZip fzip = new FastZip();
            fzip.CreateZip(outputFile, inputDir, true, null);
        }

        public static void UnzipFile(string inputFile, string outBaseDir)
        {
            Dbg.Assert(File.Exists(inputFile), "ZUtil.UnzipFile: the inputFile not exist: {0}", inputFile);

            FastZip fzip = new FastZip();
            if (!Directory.Exists(outBaseDir))
            {
                Directory.CreateDirectory(outBaseDir);
            }
            fzip.ExtractZip(inputFile, outBaseDir, FastZip.Overwrite.Always, null, null, null, false);
        }

        public static void UnzipStream(Stream iStream, string outBaseDir)
        {
            if (!Directory.Exists(Path.GetDirectoryName(outBaseDir)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outBaseDir));
            }

            ZipInputStream zipInputStream = new ZipInputStream(iStream);
            ZipEntry zipEntry = zipInputStream.GetNextEntry();
            while (zipEntry != null)
            {
                String entryFileName = zipEntry.Name;
                // to remove the folder from the entry:- entryFileName = Path.GetFileName(entryFileName);
                // Optionally match entrynames against a selection list here to skip as desired.
                // The unpacked length is available in the zipEntry.Size property.

                byte[] buffer = new byte[4096];     // 4K is optimum

                // Manipulate the output filename here as desired.
                String fullZipToPath = Path.Combine(outBaseDir, entryFileName);
                string directoryName = Path.GetDirectoryName(fullZipToPath);
                if (directoryName.Length > 0)
                    Directory.CreateDirectory(directoryName);

                // Unzip file in buffered chunks. This is just as fast as unpacking to a buffer the full size
                // of the file, but does not waste memory.
                // The "using" will close the stream even if an exception occurs.
                using (FileStream streamWriter = File.Create(fullZipToPath))
                {
                    StreamUtils.Copy(zipInputStream, streamWriter, buffer);
                }
                zipEntry = zipInputStream.GetNextEntry();
            }

        }

    }
    #endif

#if HAS_POOL
    /// <summary>
    /// alloc/dealloc object by pool
    /// </summary>
    public class Mem
    {
        public static T New<T>() where T : class, new()
        {
            ObjectPool<T> pool = ObjectPool<T>.ForceAcquire();
            return pool.Spawn();
        }

        public static void Del<T>(T obj) where T : class, new()
        {
            ObjectPool<T> pool = ObjectPool<T>.ForceAcquire();
            pool.Despawn(obj);
        }

        public static void SetResetAction<T>(Action<T> resetAction) where T : class, new()
        {
            ObjectPool<T> pool = ObjectPool<T>.ForceAcquire();
            pool.ResetAction = resetAction;
        }

        public static void SetFirstInitAction<T>(Action<T> initAction) where T : class, new()
        {
            ObjectPool<T> pool = ObjectPool<T>.ForceAcquire();
            pool.InitAction = initAction;
        }

        //public static GameObject NewPrefabGO(string poolName, Transform parent)
        //{
        //    ResPrefabPool<GameObject> pool = PoolMgr.Instance.Get(poolName) as ResPrefabPool<GameObject>;
        //    if (pool == null)
        //    {
        //        pool = new ResPrefabPool<GameObject>(poolName);
        //        PoolMgr.Instance.Add(pool);
        //    }

        //    GameObject obj = pool.Spawn();
        //    obj.transform.parent = parent;

        //    return obj;
        //}

        //public static T NewPrefab<T>(string poolName) where T : UnityEngine.Object
        //{
        //    ResPrefabPool<T> pool = PoolMgr.Instance.Get(poolName) as ResPrefabPool<T>;
        //    if (pool == null)
        //    {
        //        pool = new ResPrefabPool<T>(poolName);
        //        PoolMgr.Instance.Add(pool);
        //    }

        //    T obj = pool.Spawn();

        //    return obj;
        //}

        //public static void DelPrefabGO(GameObject obj)
        //{
        //    PoolTicket ticket = obj.GetComponent<PoolTicket>();
        //    Dbg.Assert(ticket != null, "Mem.DelPrefabGO: the obj doesn't have PoolTicket");
        //    ResPrefabPool<GameObject> pool = PoolMgr.Instance.Get(ticket.PoolName) as ResPrefabPool<GameObject>;
        //    Dbg.Assert(null != pool, "Mem.DelPrefabGO: failed to get pool: {0}", ticket.PoolName);
        //    pool.Despawn(obj);
        //}
    }
#endif

    /// <summary>
    /// get UID in a multi-queue style
    /// </summary>
    public class UID
    {        
        public enum Q{
            CR,  //coroutine            
            Q_CNT
        };

        public static uint[] s_ids = Enumerable.Repeat((uint)0, (int)Q.Q_CNT).ToArray();

        public static uint Get(Q que)
        {
            return s_ids[(int)que]++;
        }
        public static string GetAsString(Q que)
        {
            uint v = Get(que);
            return v.ToString();
        }

        /// <summary>
        /// peek next id of specified queue
        /// </summary>
        public static uint Peek(Q que)
        {
            return s_ids[(int)que];
        }
    }

    /// <summary>
    /// utility pair
    /// </summary>
    public class Pair<T, U>
    {
        public T first;
        public U second;

        public Pair(T f, U s) { first = f; second = s; }

        public T x { get { return first; } set { first = value; } }
        public U y { get { return second; } set { second = value; } }
    }
    
    /// <summary>
    /// some extra utilities on IO
    /// </summary>
    public class IOUtil
    {
        /// <summary>
        /// write to file, automatically create directories
        /// </summary>
        public static void WriteAllText(string pathName, string content)
        {
#if !UNITY_WEBPLAYER && !UNITY_WINRT && !UNITY_IPHONE
            Directory.CreateDirectory(Path.GetDirectoryName(pathName));
            File.WriteAllText(pathName, content, Encoding.UTF8);
#endif
        }

        /// <summary>
        /// write to file, automatically create directories
        /// </summary>
        public static void WriteAllBytes(string pathName, byte[] bytes)
        {
#if !UNITY_WEBPLAYER && !UNITY_WINRT && !UNITY_IPHONE
            Directory.CreateDirectory(Path.GetDirectoryName(pathName));
            File.WriteAllBytes(pathName, bytes);
#endif
        }
    }


    public class PathUtil
    {
        public static string GetDocumentPath()
        {
            return Application.persistentDataPath;
        }

        public static string GetDocumentPath(string filename)
        {
            return Combine (Application.persistentDataPath, filename);
        }

        /// <summary>
        /// combine the paths, and force use the directory separator used by Resources.Load ('/')
        /// </summary>
        public static string Combine(string path1, string path2)
        {
            string combined = Path.Combine(path1, path2);
            return ForceForwardSlash(combined);
        }

        /// <summary>
        /// strip extension if the path has
        /// </summary>
        public static string StripExtension(string path)
        {
            string dir = Path.GetDirectoryName(path);
            string nameNoExt = Path.GetFileNameWithoutExtension(path);
            if( dir.Length == 0 )
            {
                return nameNoExt;
            }
            else
            {
                return dir + "/" + nameNoExt;
            }
        }

        /// <summary>
        /// convert all "\" to "/" in path
        /// </summary>
        public static string ForceForwardSlash(string path)
        {
            return path.Replace('\\', '/');
        }

        /// <summary>
        /// given "xxx/yyy/A.asset", return "xxx/yyy/B.asset"
        /// </summary>
        public static string ReplaceFileNameWithoutExtension(string path, string newFileName)
        {
            string ext = Path.GetExtension(path);
            return Path.GetDirectoryName(path) + "/" + newFileName + 
                ((ext.Length != 0) ? ext : "");
        }

        /// <summary>
        /// given "xxx/yyy/A.asset", return "xxx/yyy/A$append$.asset"
        /// </summary>
        public static string AppendFileNameWithoutChangeExtension(string path, string append)
        {
            string ext = Path.GetExtension(path);
            string dir = Path.GetDirectoryName(path);
            string nameNoExt = Path.GetFileNameWithoutExtension(path);
            return dir + "/" + nameNoExt + append + ((ext.Length != 0) ? ext : "");
        }

        /// <summary>
        /// given "xxx/yyy/A.asset" return "xxx/yyy/A$append$$newExt$"
        /// 
        /// the newext should have '.' at head
        /// </summary>
        public static string AppendFileNameWithChangeExtension(string path, string append, string newext)
        {
            string dir = Path.GetDirectoryName(path);
            string nameNoExt = Path.GetFileNameWithoutExtension(path);
            return dir + "/" + nameNoExt + append + newext;
        }

        #if UNITY_EDITOR
        /// <summary>
        /// editor used method;
        /// given "XXXXX", return "f:/Project/Game/Assets/Resources/XXXXX"
        /// </summary>
        public static string ResourcesPath2FullPath(string pathName)
        {
            return string.Format("{0}/Resources/{1}", Application.dataPath, pathName);
        }

        /// <summary>
        /// given "XXXXX", return "Assets/Resources/XXXXX"
        /// </summary>
        public static string ResourcesPath2ProjectPath(string pathName)
        {
            return Path.Combine("Assets/Resources", pathName);
        }

        /// <summary>
        /// given "Assets/Resources/XXXXX", return "XXXXX"
        /// </summary>
        public static string ProjectPath2ResourcesPath(string Prjpath)
        {
            Dbg.Assert(Prjpath.StartsWith("Assets/Resources"), "PathUtil.ProjectPath2ResourcesPath: invalid path");
            return Prjpath.Substring("Assets/Resources".Length+1);
        }
        
        /// <summary>
        /// given "f:/Project/Game/Assets/xxx" return "Assets/xxx"
        /// </summary>
        public static string FullPath2ProjectPath(string fullpath)
        {
            Dbg.Assert(fullpath.StartsWith(Application.dataPath), "PathUtil.FullPath2ProjectPath: The path is not in Assets directory: {0}", fullpath);
            return fullpath.Substring(Application.dataPath.Length - 6);
        }

        #endif
    }

    #if HAS_JSON
    //public class Json
    //{
    //    private static JsonWriter s_PrettyWriter;

    //    static Json(){
    //        s_PrettyWriter = new JsonWriter();
    //        s_PrettyWriter.PrettyPrint = true;
    //    }

    //    /// <summary>
    //    /// convert object to json string
    //    /// </summary>
    //    public static string ToStr(object obj) 
    //    {
    //        return ToStr(obj, true);
    //    }
    //    public static string ToStr(object obj, bool bIndented)
    //    {
    //        if( bIndented )
    //        {
    //            s_PrettyWriter.Reset();
    //            JsonMapper.ToJson(obj, s_PrettyWriter);
    //            string o = s_PrettyWriter.ToString();
    //            return o;
    //        }
    //        else
    //        {
    //            string o = JsonMapper.ToJson(obj);
    //            return o;
    //        }
    //    }

    //    /// <summary>
    //    /// convert json string to object
    //    /// </summary>
    //    public static T ToObj<T>(string str)
    //    {
    //        T o = JsonMapper.ToObject<T>(str);
    //        return o;
    //    }

    //    public static object ToObj(string str, Type t)
    //    {
    //        object o = JsonMapper.ToObject(str, t);
    //        return o;
    //    }

    //}

    public class Json
    {
        private static readonly fsSerializer _serializer;

        static Json()
        {
            _serializer = new fsSerializer();
        }

        /// <summary>
        /// convert object to json string
        /// </summary>
        public static string ToStr(object obj)
        {
            return ToStr(obj, true);
        }
        public static string ToStr(object obj, bool bIndented)
        {
            fsData fsdata;
            _serializer.TrySerialize(obj.GetType(), obj, out fsdata).AssertSuccess();

            if(bIndented)
                return fsJsonPrinter.PrettyJson(fsdata);
            else
                return fsJsonPrinter.CompressedJson(fsdata);
        }

        /// <summary>
        /// convert json string to object
        /// </summary>
        public static T ToObj<T>(string str)
        {
            T o = default(T);
            fsData fsdata = fsJsonParser.Parse(str);
            _serializer.TryDeserialize<T>(fsdata, ref o).AssertSuccess();
            return o;
        }

        public static object ToObj(string str, Type t)
        {
            object o = null;
            fsData fsdata = fsJsonParser.Parse(str);
            _serializer.TryDeserialize(fsdata, t, ref o).AssertSuccess();
            return o;
        }

    }

    /// <summary>
    /// dump object to file
    /// </summary>
    public class Dumper
    {
        public static void Dump<T>(T obj)
        {
            string filename = DateTime.Now.ToString("yyyyMMdd_hhmmss") + ".dump";
            Dump(obj, filename);
        }

        public static void Dump<T>(T obj, string path)
        {
            //string serialized = JsonConvert.SerializeObject(obj, Formatting.Indented);
            string serialized = Json.ToStr(obj, true);
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.Write(serialized);
            }
        }

       
    }
    #endif
    
    /// <summary>
    /// used by QUtil.LookAtXXX series function, 
    /// to specify how to calc Quaternion with given parameters
    /// </summary>
    public enum LookAtMtd
    {
        XY, //lookat x, reference y 
        XZ, //lookat x, reference z
        YX, //lookat y, reference x
        YZ, //lookat y, reference z
        ZX, //lookat z, reference x
        ZY  //lookat z, reference y
    }

    public class QUtil
    {
        public static Quaternion LookAt(LookAtMtd mtd, Vector3 lookDir, Vector3 refDir)
        {
            switch (mtd)
            {
                case LookAtMtd.XY: return LookAtXY(lookDir, refDir);
                case LookAtMtd.XZ: return LookAtXZ(lookDir, refDir);
                case LookAtMtd.YX: return LookAtYX(lookDir, refDir);
                case LookAtMtd.YZ: return LookAtYZ(lookDir, refDir);
                case LookAtMtd.ZX: return LookAtZX(lookDir, refDir);
                case LookAtMtd.ZY: return LookAtZY(lookDir, refDir);
                default:
                    Dbg.LogErr("QUtil.LookAt: unexpected method: {0}", mtd);
                    return Quaternion.identity;
            }
        }

        public static Quaternion LookAtZY(Vector3 lookDir, Vector3 refDir)
        {
            Vector3 newZ = lookDir;
            Vector3 newY = refDir; //ref
            Vector3 newX; //calc

            if (newZ == Vector3.up && newY == Vector3.up)
            {
                newY = -Vector3.forward; //ref
                newX = Vector3.right; //calc
            }
            else
            {
                newX = Vector3.Cross(newY, newZ); //calc
                if (newX == Vector3.zero)
                {
                    newY = Vector3.up; //ref
                    newX = Vector3.Cross(newY, newZ); //calc
                }
                newY = Vector3.Cross(newZ, newX); //recalc ref
            }

            Quaternion q = Quaternion.LookRotation(newZ, newY);
            return q;
        }

        public static Quaternion LookAtZX(Vector3 lookDir, Vector3 refDir)
        {
            Vector3 newZ = lookDir;
            Vector3 newX = refDir; //ref
            Vector3 newY; //calc

            if (newZ == Vector3.right && newX == Vector3.right)
            {
                newX = -Vector3.forward; //ref
                newY = Vector3.up; //calc
            }
            else
            {
                newY = Vector3.Cross(newZ, newX); //calc
                if (newY == Vector3.zero)
                {
                    newX = Vector3.right; //ref
                    newY = Vector3.Cross(newZ, newX); //calc
                }
                newX = Vector3.Cross(newY, newZ); //recalc ref
            }

            Quaternion q = Quaternion.LookRotation(newZ, newY);
            return q;
        }

        public static Quaternion LookAtXY(Vector3 lookDir, Vector3 refDir)
        {
            Vector3 newX = lookDir;
            Vector3 newY = refDir;
            Vector3 newZ; //calc-vec

            if (newX == Vector3.up && newY == Vector3.up)
            {
                newY = -Vector3.forward; //ref
                newZ = -Vector3.right; //calc
            }
            else
            {
                newZ = Vector3.Cross(newX, newY); //calc
                if (newZ == Vector3.zero)
                {
                    newY = Vector3.up; //ref
                    newZ = Vector3.Cross(newX, newY); //calc
                }
                newY = Vector3.Cross(newZ, newX); //recalc ref-vec
            }

            Quaternion q = Quaternion.LookRotation(newZ, newY);
            return q;
        }

        public static Quaternion LookAtXZ(Vector3 lookDir, Vector3 refDir)
        {
            Vector3 newX = lookDir;
            Vector3 newZ = refDir;
            Vector3 newY; //calc-vec

            if (newX == Vector3.forward && newZ == Vector3.forward)
            {
                newZ = -Vector3.right; //ref-vec
                newY = Vector3.up; //calc-vec
            }
            else
            {
                newY = Vector3.Cross(newZ, newX); //calc-vec
                if (newY == Vector3.zero)
                {
                    newZ = Vector3.forward; //ref-vec
                    newY = Vector3.Cross(newZ, newX); //calc-vec
                }
                newZ = Vector3.Cross(newX, newY); //recalc ref-vec
            }

            Quaternion q = Quaternion.LookRotation(newZ, newY);
            return q;
        }

        public static Quaternion LookAtYX(Vector3 lookDir, Vector3 refDir)
        {
            Vector3 newY = lookDir;
            Vector3 newX = refDir;
            Vector3 newZ; //calc-vec

            if (newY == Vector3.right && newX == Vector3.right)
            {
                newX = Vector3.down; //ref-vec
                newZ = Vector3.forward; //calc-vec
            }
            else
            {
                newZ = Vector3.Cross(newX, newY); //calc-vec
                if (newZ == Vector3.zero) //calc-vec
                {
                    newX = Vector3.right; //ref-vec
                    newZ = Vector3.Cross(newX, newY); //calc-vec
                }
                newX = Vector3.Cross(newY, newZ); //recalc ref-vec
            }

            Quaternion q = Quaternion.LookRotation(newZ, newY);
            return q;
        }

        public static Quaternion LookAtYZ(Vector3 lookDir, Vector3 refDir)
        {
            Vector3 newY = lookDir;
            Vector3 newZ = refDir;
            Vector3 newX; //calc-vec

            if (newY == Vector3.forward && newZ == Vector3.forward)
            {
                newZ = Vector3.down; //ref-vec
                newX = Vector3.right; //calc-vec
            }
            else
            {
                newX = Vector3.Cross(newY, newZ); //calc-vec
                if (newX == Vector3.zero) //calc-vec
                {
                    newZ = Vector3.forward; //ref-vec
                    newX = Vector3.Cross(newY, newZ); //calc-vec
                }
                newZ = Vector3.Cross(newX, newY); //recalc ref-vec
            }

            Quaternion q = Quaternion.LookRotation(newZ, newY);
            return q;
        }
        
        /// <summary>
        /// make quaternion's fwd and up vector to reflect along (1,0,0)
        /// </summary>
        public static Quaternion ReflectX_YZ(Quaternion q)
        {
            Vector3 fwd = q * Vector3.forward;
            Vector3 up = q * Vector3.up;

            Vector3 newFwd = Vector3.Reflect(fwd, Vector3.right);
            Vector3 newUp = Vector3.Reflect(up, Vector3.right);

            return Quaternion.LookRotation(newFwd, newUp);
        }

        public static Quaternion ReflectX_XZ(Quaternion q)
        {
            Vector3 fwd = q * Vector3.forward;
            Vector3 right = q * Vector3.right;

            Vector3 newFwd = Vector3.Reflect(fwd, Vector3.right);
            Vector3 newRight = Vector3.Reflect(right, Vector3.right);

            Vector3 newUp = Vector3.Cross(newFwd, newRight);
            return Quaternion.LookRotation(newFwd, newUp);
        }

        public static Quaternion ReflectX_XY(Quaternion q)
        {
            Vector3 right = q * Vector3.right;
            Vector3 up = q * Vector3.up;

            Vector3 newRight = Vector3.Reflect(right, Vector3.right);
            Vector3 newUp = Vector3.Reflect(up, Vector3.right);

            Vector3 newFwd = Vector3.Cross(newRight, newUp);
            return Quaternion.LookRotation(newFwd, newUp);
        }

        public static Quaternion Reflect_YZ(Quaternion q, Vector3 planeNormal)
        {
            Vector3 fwd = q * Vector3.forward;
            Vector3 up = q * Vector3.up;

            Vector3 newFwd = Vector3.Reflect(fwd, planeNormal);
            Vector3 newUp = Vector3.Reflect(up, planeNormal);

            return Quaternion.LookRotation(newFwd, newUp);
        }

        public static Quaternion Reflect_XZ(Quaternion q, Vector3 planeNormal)
        {
            Vector3 fwd = q * Vector3.forward;
            Vector3 right = q * Vector3.right;

            Vector3 newFwd = Vector3.Reflect(fwd, planeNormal);
            Vector3 newRight = Vector3.Reflect(right, planeNormal);

            Vector3 newUp = Vector3.Cross(newFwd, newRight);
            return Quaternion.LookRotation(newFwd, newUp);
        }

        public static Quaternion Reflect_XY(Quaternion q, Vector3 planeNormal)
        {
            Vector3 right = q * Vector3.right;
            Vector3 up = q * Vector3.up;

            Vector3 newRight = Vector3.Reflect(right, planeNormal);
            Vector3 newUp = Vector3.Reflect(up, planeNormal);

            Vector3 newFwd = Vector3.Cross(newRight, newUp);
            return Quaternion.LookRotation(newFwd, newUp);
        }

        public static Quaternion Normalize(Quaternion rot)
        {
            float norm = Mathf.Sqrt(rot.x * rot.x + rot.y * rot.y + rot.z * rot.z + rot.w * rot.w);
            Dbg.Assert(norm != 0, "QUtil.Normalize: the given Quaternion is all 0");
            rot.x /= norm;
            rot.y /= norm;
            rot.z /= norm;
            rot.w /= norm;

            return rot;
        }

        public static float Magnitude(Quaternion q)
        {
            float mag = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
            return mag;
        }

        /// <summary>
        /// return the angle of q, make into [-PI, PI]
        /// </summary>
        public static float AnglePI(Quaternion q)
        {
            Vector3 axis; float angle;
            q.ToAngleAxis(out angle, out axis);
            return Misc.NormalizeAnglePI(angle);
        }
    }

    public class RTUtil
    {
        public static Texture2D GetRTPixels(RenderTexture rt = null)
        {
            if (rt != null)
            {
                RenderTexture currentActiveRT = RenderTexture.active;
                RenderTexture.active = rt;
                Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
                tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
                tex.Apply();
                RenderTexture.active = currentActiveRT;
                return tex;
            }
            else
            {
                rt = RenderTexture.active;
                Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);
                tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);
                tex.Apply();
                return tex;
            }

        }

        public static Texture2D GetRTPixels(RenderTexture rt, Rect area)
        {
            Texture2D tex = GetRTPixels(rt);

            int w = (int)area.width;
            int h = (int)area.height;

            Color[] pix = tex.GetPixels((int)area.x, (int)area.y, w, h);
            Texture2D ret = new Texture2D(w, h);
            ret.SetPixels(0, 0, w, h, pix);
            ret.Apply();

            return ret;
        }

        public static Texture2D GetScreenTex()
        {
            return GetScreenTex(new Rect(0, 0, Screen.width, Screen.height));
        }

        public static Texture2D GetScreenTex(Rect area)
        {
            int x = (int)area.x;
            int y = (int)area.y;
            int w = (int)area.width;
            int h = (int)area.height;

            Texture2D tex = new Texture2D(w, h, TextureFormat.ARGB32, false);
            tex.ReadPixels(new Rect(x, y, w, h), 0, 0);
            tex.Apply();
            return tex;
        }

        public static float DecodeFloatRGBA( Color enc )
        {	        
	        return Vector4.Dot( enc, DECODE_FLOAT_RGBA);
        }

        public static float DecodeFloatRG(Color enc)
        {
            Vector2 rg = new Vector2(enc.r, enc.g);
            return Vector2.Dot(rg, DECODE_FLOAT_RG);
        }

        public static readonly Vector4 DECODE_FLOAT_RGBA = new Vector4(1.0f, 1f / 255.0f, 1f / 65025.0f, 1f / 16581375.0f);
        public static readonly Vector2 DECODE_FLOAT_RG = new Vector2(1.0f, 1f / 255.0f);
    }

    /// <summary>
    /// matrix utility
    /// </summary>
    public class MtxUtil
    {
        public static Vector3 GetPosition(ref Matrix4x4 m)
        {
            return m.GetColumn(3);
        }

        public static Quaternion GetRotation(ref Matrix4x4 m)
        {
            Vector3 z = m.GetColumn(2);
            Vector3 y = m.GetColumn(1);
            return Quaternion.LookRotation(z, y);
        }

        /// <summary>
        /// WARNING: this method requires that the matrix is a non-skewed matrix, a normal localMatrix of transform is fit
        /// </summary>
        public static Vector3 GetScale(ref Matrix4x4 m)
        {
            Vector3 z = m.GetColumn(2);
            Vector3 y = m.GetColumn(1);
            Vector3 x = m.GetColumn(0);

            Vector3 scale = Vector3.zero;
            scale.x = (x).magnitude;
            scale.y = (y).magnitude;
            scale.z = (z).magnitude;

            return scale;
        }

        /// <summary>
        /// calculate a matrix with the given tr's worldPos & worldRot only
        /// </summary>
        public static Matrix4x4 GetMatrix_TR(Transform tr)
        {
            Vector3 worldPos = tr.position;
            Quaternion worldRot = tr.rotation;

            return Matrix4x4.TRS(worldPos, worldRot, Vector3.one);
        }
        public static Matrix4x4 GetMatrix_TR(Rigidbody rb)
        {
            Vector3 worldPos = rb.position;
            Quaternion worldRot = rb.rotation;
            return Matrix4x4.TRS(worldPos, worldRot, Vector3.one);
        }

        public static Matrix4x4 FlipX(ref Matrix4x4 m)
        {
            Matrix4x4 mtx = m;
            Vector4 r0 = mtx.GetRow(0);
            r0 *= -1f;
            mtx.SetRow(0, r0);
            return mtx;
        }

        /// <summary>
        /// matrix equation: AX = B, calculate X
        /// </summary>
        public static Matrix4x4 Solve_AX_eq_B(ref Matrix4x4 mA, ref Matrix4x4 mB)
        {
            return mA.inverse * mB;
        }
        public static Matrix4x4 Solve_AX_eq_B(Vector3 wposA, Quaternion wrotA, Vector3 wposB)
        {
            Matrix4x4 mA = Matrix4x4.TRS(wposA, wrotA, Vector3.one);
            Matrix4x4 mB = Matrix4x4.TRS(wposB, Quaternion.identity, Vector3.one);
            return mA.inverse * mB;
        }
        public static Matrix4x4 Solve_AX_eq_B(Vector3 wposA, Quaternion wrotA, Vector3 wposB, Quaternion wrotB)
        {
            Matrix4x4 mA = Matrix4x4.TRS(wposA, wrotA, Vector3.one);
            Matrix4x4 mB = Matrix4x4.TRS(wposB, wrotB, Vector3.one);
            return mA.inverse * mB;
        }

        /// <summary>
        /// A B   x    E  
        ///     *    =  
        /// C D   y    F 
        /// 
        /// calculate matrix [A,B/C,D] * [X,Y] = [E,F]
        /// if det == 0 return false, x y will be 0
        /// </summary>
        public static bool SolveEquation(float A, float B, float C, float D, float E, float F, out float x, out float y)
        {
            x = y = 0;
            float down = A * D - B * C;
            if (Mathf.Approximately(down, 0))
                return false;

            float A1 = D * E - B * F;
            float A2 = A * F - C * E;

            x = A1 / down;
            y = A2 / down;

            return true;
        }
    }

    /// <summary>
    /// geometry math util
    /// </summary>
    public class GeoUtil
    {
        /// <summary>
        /// make a ray with mainCamera & scrPos, and cast against the collider
        /// NOTE: left-bottom is <0,0>
        /// </summary>
        public static bool ColliderCast(Collider col, Vector2 scrPos, out RaycastHit hit)
        {
            Ray ray = Camera.main.ScreenPointToRay(scrPos);
            return ColliderCast(col, ray, out hit);
        }
        public static bool ColliderCast(Collider col, Ray ray, out RaycastHit hit)
        {
            return col.Raycast(ray, out hit, float.MaxValue);
        }

        /// <summary>
        /// return the distance of 'pt' to line defined by 'linePt0' & 'linePt1'
        /// </summary>
        public static float DistPointToLine(Vector3 pt, Vector3 linePt0, Vector3 linePt1)
        {
            Vector3 seg0 = pt - linePt0;
            Vector3 seg1 = linePt1 - linePt0;

            float cross = Vector3.Cross(seg0, seg1).magnitude;
            float len1 = seg1.magnitude;

            float D = cross / len1;
            return D;
        }

        /// <summary>
        /// assume the mesh is closed, this is 
        /// "The trick is to calculate the signed volume of a tetrahedron - based on your triangle and topped off at the origin. The sign of the volume comes from whether your triangle is pointing in the direction of the origin."
        /// 
        /// Reference: http://stackoverflow.com/questions/1406029/how-to-calculate-the-volume-of-a-3d-mesh-object-the-surface-of-which-is-made-up
        /// </summary>
        public static float CalcMeshVolume(Mesh mesh)
        {
            Vector3[] posLst = mesh.vertices;
            int[] tris = mesh.triangles;
            int triCnt = tris.Length;

            float vol = 0;

            for(int i=0; i<triCnt; i+=3)
            {
                Vector3 v0 = posLst[tris[i]];
                Vector3 v1 = posLst[tris[i + 1]]; 
                Vector3 v2 = posLst[tris[i + 2]];

                float v = Vector3.Dot(v0, Vector3.Cross(v1, v2)) / 6f;
                vol += v;
            }

            return vol;
        }

        /// <summary>
        /// two-pass check: ep = pt + right * bound.x_width * 2
        /// 1. use segment ray <ep, pt> to recur hit, count hit number HA; if no hit, false
        /// 2. use segment ray <pt, ep> to recur hit, count hit number HB;
        /// 
        /// if HA+HB == odd, true;
        /// else false
        /// </summary>
        //public static bool IsInsideMeshCollider(MeshCollider col, Vector3 pt, List<Vector3> ipts, List<Vector3> opts)
        public static bool IsInsideMeshCollider(Collider col, Vector3 pt)
        {
            Dbg.CAssert(col, col.gameObject.activeInHierarchy, "GeoUtil.IsInsideMeshCollider: collider GO not active: {0}", col.name);
            Vector3 refDir = Vector3.forward;
            Bounds bd = col.bounds;
            float len = bd.size.x * 2f;
            Vector3 ept = pt + refDir * len;

            int hitA = 0;
            int hitB = 0;

            // 1
            {
                Vector3 dir = -refDir;
                Ray r = new Ray(ept, dir);
                RaycastHit hit;
                float leftLen = len;

                while (true)
                {
                    if (!col.Raycast(r, out hit, leftLen))
                        break;

                    //ipts.Add(hit.point);

                    hitA++;
                    r.origin = hit.point + dir * 0.05f; //move origin a bit
                    leftLen = (r.origin - pt).magnitude; //shrink len limit
                }

                if (hitA == 0)
                    return false; //early quit
            }

            // 2
            {
                Vector3 dir = refDir;
                Ray r = new Ray(pt, dir);
                RaycastHit hit;
                float leftLen = len;

                while (true)
                {
                    if (!col.Raycast(r, out hit, leftLen))
                        break;

                    //opts.Add(hit.point);

                    hitB++;
                    r.origin = hit.point + dir * 0.05f; //move origin a bit
                    leftLen = (r.origin - ept).magnitude; //shrink len limit
                }
            }

            //DbgWatcher.AddEntry("Hit", "Inward {0}, outward {1}", hitA, hitB);
            return (hitA + hitB) % 2 == 1;
        }

        /// <summary>
        ///  | ax  ay |  
        ///  | bx  by |  equal 0 <==> parallel
        /// </summary>
        public static bool IsParallel(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x == 0;
        }

        /// <summary>
        ///  | ax  ay |   | ax  az |    | ay  az |
        ///  | bx  by | = | bx  bz |  = | by  bz | = 0  <==> parallel
        /// </summary>
        public static bool IsParallel(Vector3 a, Vector3 b)
        {
            return (a.x * b.y - a.y * b.x == 0) &&
                   (a.x * b.z - a.z * b.x == 0);
        }

    }
    
    public class CryptoUtil
    {
        private static StringBuilder ms_bld = new StringBuilder();

        public static string MD5Sum(string strToEncrypt)
        {
            System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
            byte[] bytes = ue.GetBytes(strToEncrypt);

            // encrypt bytes
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hashBytes = md5.ComputeHash(bytes);

            // Convert the encrypted bytes back to a string (base 16)

            StringBuilder bld = ms_bld;

            for (int i = 0; i < hashBytes.Length; i++)
            {
                bld.Append(hashBytes[i].ToString("X2"));
            }

            string ret = bld.ToString().PadLeft(32, '0');
            bld.Remove(0, bld.Length);

            return ret;
        }
    }

}

namespace ExtMethods
{
    public static class DictExt
    {
        /// <summary>
        /// try to get the value of given key, return null if not found
        /// </summary>
        public static V TryGet<K,V>(this Dictionary<K,V> dict, K key) where V : class
        {
            V ret;
            if( dict.TryGetValue(key, out ret))
            {
                return ret;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// simulate C++ map's operator[], if the key
        /// </summary>
        public static V ForceGet<K,V>(this Dictionary<K,V> dict, K key, V defVal) where V : class
        {
            V ret = null;
            if( dict.TryGetValue(key, out ret) )
            {
                return ret;
            }
            else
            {
                dict.Add(key, defVal);
                return defVal;
            }
        }
        public static V ForceGet<K,V>(this Dictionary<K,V> dict, K key) where V : class
        {
            return ForceGet(dict, key, null);
        }
    }

    public static class TransformExt
    {
        public static void CopyLocal(this Transform tr, Transform from)
        {
            tr.localPosition = from.localPosition;
            tr.localRotation = from.localRotation;
            tr.localScale = from.localScale;
        }

        public static void CopyLocalPR(this Transform tr, Transform from)
        {
            tr.localPosition = from.localPosition;
            tr.localRotation = from.localRotation;
        }

        /// <summary>
        /// scale will still use local
        /// </summary>
        public static void CopyWorld(this Transform tr, Transform from)
        {
            tr.position = from.position;
            tr.rotation = from.rotation;
            tr.localScale = from.localScale;
        }

        public static void CopyWorldPR(this Transform tr, Transform from)
        {
            tr.position = from.position;
            tr.rotation = from.rotation;
        }

        /// <summary>
        /// 6 lookat functions
        /// </summary>
        public static void LookAtZY(this Transform tr, Vector3 targetPos, Vector3 newY)
        {
            tr.LookAt(targetPos, newY);
        }
        public static void LookAtZX(this Transform tr, Vector3 targetPos, Vector3 newX)
        {
            Vector3 newZ = targetPos - tr.position;
            Quaternion q = MH.QUtil.LookAtZX(newZ, newX);
            tr.rotation = q;
        }
        public static void LookAtXY(this Transform tr, Vector3 targetPos, Vector3 newY)
        {
            Vector3 newX = targetPos - tr.position;
            Quaternion q = MH.QUtil.LookAtXY(newX, newY);
            tr.rotation = q;
        }
        
        public static void LookAtXZ(this Transform tr, Vector3 targetPos, Vector3 newZ)
        {
            Vector3 newX = targetPos - tr.position;
            Quaternion q = MH.QUtil.LookAtXZ(newX, newZ);
            tr.rotation = q;
        }
        public static void LookAtYX(this Transform tr, Vector3 targetPos, Vector3 newX)
        {
            Vector3 newY = targetPos - tr.position;
            Quaternion q = MH.QUtil.LookAtYX(newY, newX);
            tr.rotation = q; 
        }
        public static void LookAtYZ(this Transform tr, Vector3 targetPos, Vector3 newZ)
        {
            Vector3 newY = targetPos - tr.position;
            Quaternion q = MH.QUtil.LookAtYZ(newY, newZ);
            tr.rotation = q; 
        }

        /// <summary>
        /// Rotate this transform, by the from->to rotation, under specified Space
        /// </summary>
        public static void RotateFromTo(this Transform tr, Vector3 from, Vector3 to, Space space = Space.Self)
        {
            Quaternion q = Quaternion.FromToRotation(from, to);
            if (space == Space.World)
            {
                tr.rotation = q * tr.rotation;
            }
            else
            {
                tr.localRotation = q * tr.localRotation;
            }
        }

        /// <summary>
        /// check if tr has 'pCnt' or more level of parent 
        /// </summary>
        public static bool HasParentLevel(this Transform tr, int pCnt)
        {
            bool bOK = true;
            Dbg.Assert(tr != null, "Transform.HasParentLevel: transform == null");

            for (int i = 0; i < pCnt; ++i)
            {
                tr = tr.parent;
                if (tr == null)
                {
                    bOK = false;
                    break;
                }
            }

            return bOK;
        }

        /// <summary>
        /// check if tr has 'expectLevel' or more level of parent,
        /// if not, 'retLevel' will be filled for actual parent level
        /// </summary>
        public static bool HasParentLevel(this Transform tr, int expectLevel, out int retLevel)
        {
            bool bOK = true;
            Dbg.Assert(tr != null, "Transform.HasParentLevel: transform == null");

            int i = 0;
            for (; i < expectLevel; ++i)
            {
                tr = tr.parent;
                if (tr == null)
                {
                    bOK = false;
                    break;
                }
            }

            retLevel = i;
            return bOK;
        }

        /// <summary>
        /// like Transform.Find, but will not take `/' as hierachy separator.
        /// will just recursive find all children for exact matching name,
        /// if not found, return null
        /// </summary>
        public static Transform FindByName(this Transform tr, string name)
        {
            if (tr.name == name)
                return tr;

            for( int idx = 0; idx < tr.childCount; ++idx )
            {
                Transform ctr = tr.GetChild(idx);
                Transform retTr = FindByName(ctr, name);
                if( retTr != null )
                {
                    return retTr;
                }
            }

            return null;
        }

        /// <summary>
        /// find the first transform that has given tag, and is child or self of given `parent'
        /// </summary>
        public static Transform
        FindWithTag(this Transform parent, string tag)
        {
            GameObject[] withTags = GameObject.FindGameObjectsWithTag(tag);
            for (int i = 0; i < withTags.Length; ++i)
            {
                Transform tr = withTags[i].transform;
                if (tr.IsChildOf(parent))
                {
                    return tr;
                }
            }
            return null;
        }

        /// <summary>
        /// find the first transform of self and self's ancestors, which has the given tag
        /// </summary>
        public static Transform
        FindParentWithTag(this Transform self, string tag)
        {
            Transform tr = self;
            while (tr != null )
            {
                if( tr.tag == tag )
                {
                    return tr;
                }
                else
                {
                    tr = tr.parent;
                }
            }
            return null;
        }

        public static T
        GetChildComponent<T>(this Transform self, string childName) where T : Component
        {
            Transform childTr = self.Find(childName);
            Dbg.CAssert(self, childTr!=null, "TransformExt.GetChildComponent: failed to get child: {0}", childName);

            T cp = childTr.GetComponent<T>();

            return cp;
        }

        public static TrRecurEnumerator
        GetRecurEnumerator(this Transform tr)
        {
            return new TrRecurEnumerator(tr);
        }

        public struct TrRecurEnumerator
        {
            private MH.SList<int> m_stack;
            private Transform m_curTr;
            private Transform m_retTr;

            public TrRecurEnumerator(Transform tr)
            {
                m_curTr = tr;
                m_retTr = null;

                m_stack = new MH.SList<int>();
                m_stack.Add(0);
            }

            public bool MoveNext()
            {
                if (m_curTr == null)
                    return false;

                m_retTr = m_curTr;

                // set curTr's next position
                while (true)
                {
                    int idx = m_stack[m_stack.Count - 1]; m_stack.RemoveAt(m_stack.Count - 1); //pop
                    if (idx >= m_curTr.childCount)
                    { // end for this transform, if stack is empty then over, or back upward
                        if (m_stack.Count == 0)
                        {
                            m_curTr = null;
                            break;
                        }
                        else
                        {
                            m_curTr = m_curTr.parent;
                        }
                    }
                    else
                    { //dive
                        m_curTr = m_curTr.GetChild(idx);
                        m_stack.Add(idx + 1);
                        m_stack.Add(0);
                        break;
                    }
                }

                return m_retTr != null;
            }

            public Transform Current
            {
                get { return m_retTr; }
            }
        }

    }

    public static class GOExt
    {
        public enum FIELD{NONE = 0, X=1, Y=2, XY=3, Z=4, XZ=5, YZ=6, XYZ=7};
        private static int[] FIELDCNT = {0, 1, 1, 2, 1, 2, 2, 3 };

        public static T ForceGetComponent<T>(this GameObject go) where T : Component
        {
            T comp = go.GetComponent<T>();
            if( comp == null )
            {
                comp = go.AddComponent<T>();
            }
            return comp;
        }

        public static T AssertGetComponent<T>(this GameObject go) where T : Component
        {
            T comp = go.GetComponent<T>();
            Dbg.CAssert(go, comp != null, "GOExt.AssertGetComponent: failed to get component");
            return comp;
        }

        public static void SilentDestroyComponent<T> (this GameObject go) where T : Component
        {
            T comp = go.GetComponent<T>();
            if( comp != null )
            {
                GameObject.Destroy(comp);
            }
        }

        public static Vector3 GetColliderExtents(this GameObject go)
        {
            return GetColliderExtents(go, Vector3.zero);
        }
        public static Vector3 GetColliderExtents(this GameObject go, Vector3 defExt)
        {
            Collider c = go.GetComponent<Collider>();
            if( c == null )
            {
                return defExt;
            }
            else
            {
                return c.bounds.extents;
            }
        }

        public static void SetLocalPosition(this GameObject go, FIELD f, params float[] p)
        {
            Vector3 pos = go.transform.localPosition;
            Vector3 oldPos = pos;
            int fieldCnt = FIELDCNT[(uint)f];
            if (fieldCnt == 0)
                return;
            Dbg.Assert(p.Length == fieldCnt, "GOExt.SetLocalPosition: param cnt not fit: {0} != {1}", p.Length, fieldCnt);

            int idx = 0;
            int bits = (int)f;
            if( (bits & (int)FIELD.X) != 0 )
            {
                pos.x = p[idx++];
            }
            if( (bits & (int)FIELD.Y) != 0)
            {
                pos.y = p[idx++];
            }
            if( (bits & (int)FIELD.Z) != 0)
            {
                pos.z = p[idx++];
            }

            if( oldPos == pos )
            {
                go.transform.localPosition = pos;
            }
        }

        public static void SetLocalScale(this GameObject go, FIELD f, params float[] p)
        {
            Transform trans = go.transform;
            Vector3 oldscale = trans.localScale;
            Vector3 scale = oldscale;

            int fieldCnt = FIELDCNT[(uint)f];
            if (fieldCnt == 0)
                return;
            Dbg.Assert(p.Length == fieldCnt, "GOExt.SetLocalScale: param cnt not fit: {0} != {1}", p.Length, fieldCnt);

            int idx = 0;
            int bits = (int)f;
            if ((bits & (int)FIELD.X) != 0)
            {
                scale.x = p[idx++];
            }
            if ((bits & (int)FIELD.Y) != 0)
            {
                scale.y = p[idx++];
            }
            if ((bits & (int)FIELD.Z) != 0)
            {
                scale.z = p[idx++];
            }

            if( scale != oldscale )
            {
                go.transform.localScale = scale;
            }
        }
    }

    public static class CompExt
    {
        public static T ForceGetComponent<T>(this Component cp) where T : Component
        {
            T comp = cp.GetComponent<T>();
            if (comp == null)
            {
                comp = cp.gameObject.AddComponent<T>();
            }
            return comp;
        }

        public static T AssertGetComponent<T>(this Component cp) where T : Component
        {
            T comp = cp.GetComponent<T>();
            Dbg.CAssert(cp, comp != null, "CompExt.AssertGetComponent: failed to get component");
            return comp;
        }
    }

    public static class ListExt
    {
        /// <summary>
        /// just use default(T) to fill the new space
        /// </summary>
        public static void Resize<T>(this List<T> lst, int newsz)
        {
            int cnt = lst.Count;
            if( cnt > newsz )
            {
                lst.RemoveRange(newsz, cnt - newsz);
            }
            else if (cnt < newsz )
            {
                for( int idx = 0; idx < newsz - cnt; ++idx )
                {
                    lst.Add(default(T));
                }
            }
            //do nothing if equal
        }

        /// <summary>
        /// T is class and new-able()
        /// </summary>
        public static void ResizeC<T>(this List<T> lst, int newsz) where T : class, new()
        {
            int cnt = lst.Count;
            if (cnt > newsz)
            {
                lst.RemoveRange(newsz, cnt - newsz);
            }
            else if (cnt < newsz)
            {
                for (int idx = 0; idx < newsz - cnt; ++idx)
                {
                    lst.Add(new T());
                }
            }
            //do nothing if equal
        }

        public static void AddMany<T>(this List<T> lst, params T[] objs)
        {
            for(int idx = 0; idx < objs.Length; ++idx)
            {
                lst.Add(objs[idx]);
            }
        }

        public static void AddMulti<T>(this IList<T> lst, T v0, T v1)
        {
            lst.Add(v0);
            lst.Add(v1);
        }
        public static void AddMulti<T>(this IList<T> lst, T v0, T v1, T v2)
        {
            lst.Add(v0);
            lst.Add(v1);
            lst.Add(v2);
        }
        public static void AddMulti<T>(this IList<T> lst, T v0, T v1, T v2, T v3)
        {
            lst.Add(v0);
            lst.Add(v1);
            lst.Add(v2);
            lst.Add(v3);
        }

        /// <summary>
        /// enable to get element by negative index
        /// </summary>
        public static T ExtGet<T>(this IList<T> lst, int idx)
        {
            int cnt = lst.Count;
            if (cnt == 0)
                return default(T);

            if( idx < 0 ) { idx = (idx % cnt) + cnt; }
            if (idx >= cnt) { idx = idx % cnt; }
            //Dbg.Assert(0 <= idx && idx < cnt, "ListExt.ExtGet: invalid idx: {0}, count: {1}", idx, cnt);
            return lst[idx];
        }

        /// <summary>
        /// add a int list to be from 'from' to 'to', both inclusive
        /// </summary>
        public static void AddRangeValue(this IList<int> lst, int from, int to)
        {
            lst.Clear();
            int inc = Math.Sign(to - from);
            for( int i = from; i <= to; i+=inc )
            {
                lst.Add(i);
            }
        }

        public static void SetRangeValue(this IList<int> lst, int from, int to)
        {
            int cnt = Mathf.Abs(to-from) + 1;
            Dbg.Assert(lst.Count == cnt, "SetRangeValue: length not match");
            int inc = Math.Sign(to - from);
            int v = from;
            for( int i=0; i<cnt; i++)
            {
                lst[i] = v;
                v += inc;
            }
        }

        /// <summary>
        /// shuffle given list
        /// </summary>
        public static void ShuffleList<T>(this IList<T> lst)
        {
            int sz = lst.Count;
            for (int i = 0; i < sz; ++i)
            {
                int tgtIdx = Random.Range(i, sz);
                if (tgtIdx != i)
                {
                    T tmp = lst[i];
                    lst[i] = lst[tgtIdx];
                    lst[tgtIdx] = tmp;
                }
            }
        }

        /// <summary>
        /// will make specified list to be filled with [0, size-1], 
        /// and shuffle it
        /// </summary>
        public static void GenShuffledIntList(this List<int> toFillLst, int size)
        {
            toFillLst.AddRangeValue(0, size-1);
            ShuffleList(toFillLst);
        }

        public static bool Contains<T>(this IList<T> lst, T o)
        {
            int sz = lst.Count;
            for(int i=0; i<sz; ++i)
            {
                if (lst[i].Equals(o))
                    return true;
            }
            return false;
        }

        public static void SetValue<T>(this IList<T> lst, int fromIdx, int sz, T v)
        {
            for (int i = fromIdx; i < fromIdx + sz; ++i)
            {
                lst[i] = v;
            }
        }
        public static void SetAllValue<T>(this IList<T> lst, T v)
        {
            int sz = lst.Count;
            for(int i=0; i<sz; ++i)
            {
                lst[i] = v;
            }
        }

        public static T RandomGetElem<T>(this IList<T> lst)
        {
            int cnt = lst.Count;
            int idx = Random.Range(0, cnt);
            return lst[idx];
        }

        public static T Last<T>(this IList<T> lst)
        {
            if (lst.Count == 0)
                return default(T);
            else
                return lst[lst.Count - 1];
        }
    }

    public static class BitArrayExt
    {
        /// <summary>
        /// test if all value in `arr' are all `bVal'
        /// </summary>
        public static bool IsAll(this BitArray arr, bool bVal)
        {
            for( int idx = 0; idx < arr.Count; ++idx )
            {
                if (bVal != arr.Get(idx))
                    return false;
            }
            return true;
        }
    }
}