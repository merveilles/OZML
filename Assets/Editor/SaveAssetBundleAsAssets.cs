using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

class SaveAssetBundleAsAssets : ScriptableWizard
{
    string m_assetBundleURL = string.Empty;
    WWW m_www = null;
    Object[] m_assetBundleObjects = null;
    int m_objectIndex = -1;
    AssetBundle m_assetBundle = null;
    Hashtable m_copiedGameObjects = null;
    int m_objectIndexPass2 = -1;
    bool m_assetBundleLoaded = false;
    Hashtable m_exportedShaders = null;

    [MenuItem("Assets/Save Asset Bundle As Assets")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard("Save Asset Bundle As Assets", typeof(SaveAssetBundleAsAssets), "Download The Asset Bundle");
    }

    void OnWizardOtherButton()
    {
        isValid = true;
    }

    void Init()
    {
        m_assetBundleObjects = null;
        m_objectIndex = -1;
        m_assetBundle = null;
        m_copiedGameObjects =
            new Hashtable();
        m_objectIndexPass2 = -1;
        m_assetBundleLoaded = false;
        m_exportedShaders =
            new Hashtable();
    }

    void OnEnable()
    {
        m_www = null;
        m_assetBundleURL = @"http://tagenigma.com/qa/Unity3d/AssetBundleTester/ThickSmoke.unity3d";
        Init();
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Asset Bundle URL", string.Empty);
        m_assetBundleURL =
            EditorGUILayout.TextArea(m_assetBundleURL);

        if (null == m_www)
        {
            if (GUI.Button(new Rect(10, 75, 300, 20), "Save Asset Bundle As Assets"))
            {
                Init();
                m_www =
                    new WWW(m_assetBundleURL);
            }
        }
    }

    static string GetShortType(Object obj)
    {
        return obj.GetType().ToString().Replace("UnityEngine.", string.Empty).Replace("UnityEditor.", string.Empty);
    }

    void Update()
    {
        if (null != m_www &&
            m_www.isDone)
        {
            if (null == m_www.assetBundle)
            {
                Debug.LogError("!!Asset bundle was not found");
                m_www = null;
                return;
            }

            m_assetBundle =
                m_www.assetBundle;

            m_www = null;

            Debug.Log("**Loading asset bundle assets");

            m_assetBundleObjects =
                m_assetBundle.LoadAll();

            m_assetBundleLoaded = true;
        }

        if (null != m_assetBundleObjects)
        {
            if ((m_objectIndex + 1) < m_assetBundleObjects.Length)
            {
                ++m_objectIndex;

                Object obj =
                    m_assetBundleObjects[m_objectIndex];

                InspectAndSaveObject(obj);
            }

            else if ((m_objectIndexPass2 + 1) < m_copiedGameObjects.Keys.Count)
            {
                int index = -1;
                foreach (string key in m_copiedGameObjects.Keys)
                {
                    ++index;

                    if (index < (m_objectIndexPass2+1))
                    {
                        continue;
                    }

                    ++m_objectIndexPass2;

                    if (string.IsNullOrEmpty(key))
                    {
                        Debug.LogError("**!!Empty object name skipping...");
                        continue;
                    }

                    Object obj =
                        (Object)m_copiedGameObjects[key];

                    if (null == obj)
                    {
                        Debug.LogError("**!!Null object skipping...");
                        continue;
                    }

                    if (obj.GetType() == typeof(Material))
                    {
                        Debug.Log("**Detected Material...");
                        Material material = obj as Material;

                        if (null == material)
                        {
                            Debug.Log("**Material reference is null");
                            continue;
                        }

                        if (null == material.shader)
                        {
                            Debug.Log("**Material shader reference is null");
                            continue;
                        }

                        Debug.Log(string.Format("**Need to fix shader reference to: {0}...", material.shader.name));
                        string findObject =
                            string.Format("{0}_{1}", GetShortType(material.shader), material.shader.name);
                        if (!m_copiedGameObjects.ContainsKey(findObject))
                        {
                            Debug.LogError(string.Format("!!Could not find referenced shader: {0}...", findObject));
                            continue;
                        }

                        Shader shader = m_copiedGameObjects[findObject] as Shader;

                        if (null == shader)
                        {
                            Debug.Log("**Shader is blank");
                            continue;
                        }

                        Debug.Log("**Fixing Shader Reference...");
                        material.shader = shader;

                        CopyTextures(material);                        
                    }
                }
            }
            
            else if ((m_objectIndexPass2 + 1) == m_copiedGameObjects.Keys.Count)
            {
                ++m_objectIndexPass2;

                Debug.Log("**Inspecting main asset...");

                Object obj =
                    m_assetBundle.mainAsset;

                string extension = "prefab";

                string assetPath =
                    string.Format("Assets/SavedAssets/{0}.{1}", obj.name, extension);

                Debug.Log("**Creating empty prefab");

                Object prefab =
                    EditorUtility.CreateEmptyPrefab(assetPath);

                Debug.Log(string.Format("**Replacing prefab with gameObject: {0}", obj.name));

                EditorUtility.ReplacePrefab((GameObject)obj, prefab, ReplacePrefabOptions.Default);

                Debug.Log(string.Format("**Inspected main asset: {0}", obj.name));
            }

            else
            {
                if (m_assetBundleLoaded)
                {
                    Debug.Log("**Saving assets...");
                    AssetDatabase.SaveAssets();

                    m_assetBundleLoaded = false;
                    Debug.Log("**Unloading Asset Bundle");
                    m_assetBundle.Unload(false);
                    m_assetBundleObjects = null;
                    m_assetBundle = null;

                    Debug.Log("**Assets saved successfully");
                }
            }
        }
    }

    void InspectAndSaveObject(Object obj)
    {
        if (null == obj)
        {
            Debug.LogError("!!Null object skipping...");
            return;
        }

        Debug.Log("**Create new object...");
        Object copyObj = null;
        string name = obj.name;
        string extension = string.Empty;

        Debug.Log(string.Format("**New object: {0} is Type: {1}...", name, GetShortType(obj)));

        if (obj.GetType() == typeof(Texture2D))
        {
            Debug.Log(string.Format("**Detected Texture2D... {0}", obj.name));
            extension = "asset";
            Texture2D texture2D = obj as Texture2D;
            copyObj = new Texture2D(texture2D.width, texture2D.height);
        }

        else if (obj.GetType() == typeof(Material))
        {
            Debug.Log(string.Format("**Detected Material... {0}", obj.name));
            extension = "mat";
            Material material = obj as Material;
            copyObj = new Material(material.shader);
        }

        else if (obj.GetType() == typeof(Shader))
        {
            Debug.Log(string.Format("**Detected Shader... {0}", obj.name));
            return;
            extension = "shader";
            Shader shader = obj as Shader;
            copyObj = Object.Instantiate(shader);
            Debug.Log(string.Format("**Old Shader Name: {0}", name));
            name = shader.name.Substring(shader.name.LastIndexOf("/") + 1, shader.name.Length - shader.name.LastIndexOf("/") - 1);
            Debug.Log(string.Format("**New Shader Name: {0}", name));
        }

        else if (obj.GetType() == typeof(MonoScript))
        {
            Debug.Log(string.Format("**Detected MonoScript... {0}", obj.name));
            extension = "asset";
            MonoScript monoScript = obj as MonoScript;
            if (null == monoScript)
            {
                Debug.LogError("!!Null MonoScript skipping...");
                return;
            }

            if (string.IsNullOrEmpty(monoScript.text))
            {
                Debug.LogError("!!Empty MonoScript text...");
                return;
            }

            Debug.Log(string.Format("**Found MonoScript text: {0}", monoScript.text));

            System.Type classType =
                monoScript.GetClass();

            if (null == classType)
            {
                Debug.Log(string.Format("**MonoScript class type is null: {0}", obj.name));
                GameObject gameObject =
                    m_assetBundle.mainAsset as GameObject;
                Component component =
                    gameObject.AddComponent(obj.name);
                return;
            }

            string className =
                classType.ToString();
            if (string.IsNullOrEmpty(className))
            {
                Debug.Log(string.Format("**MonoScript class name is empty: {0}", obj.name));
                return;
            }

            Debug.Log(string.Format("**MonoScript {0} class name: {1}", obj.name, className));

            copyObj = Object.Instantiate(monoScript);
            if (null != monoScript.GetClass() &&
                !string.IsNullOrEmpty(monoScript.GetClass().ToString()))
            {
                name = monoScript.GetClass().ToString();
            }

            return;
        }

        else if (obj.GetType() == typeof(GameObject))
        {
            Debug.Log(string.Format("**Detected GameObject... {0}", obj.name));
            extension = "asset";
            return;

            GameObject gameObject =
                obj as GameObject;

            if (null == gameObject)
            {
                return;
            }

            GameObject newObj =
                new GameObject(obj.name);

            foreach (Component component in gameObject.GetComponents(typeof(Component)))
            {
                Debug.Log(string.Format("**Adding component: {0}", GetShortType((Object)component)));
                newObj.AddComponent(component.GetType());
            }

            copyObj =
                (Object)newObj;

        }

        else
        {
            Debug.LogError(string.Format("!!Unsupported extraction type... {0}", obj.GetType().ToString()));
            return;
        }

        if (null == copyObj)
        {
            Debug.LogError("**!!Null copy object skipping...");
            return;
        }

        CopySerialized(obj, copyObj);

        string assetPath =
            string.Format("Assets/SavedAssets/{0}.{1}", name, extension);

        FixMaterialShader(copyObj);

        SaveAsset(copyObj, assetPath);
    }

    void CopySerialized(Object obj, Object copyObj)
    {
        if (null == copyObj)
        {
            Debug.LogError("**!!Null copy object skipping...");
            return;
        }

        Debug.Log("**Copy Serialized...");
        EditorUtility.CopySerialized(obj, copyObj);

        if (null == copyObj)
        {
            Debug.LogError("**!!Null copy object skipping...");
            return;
        }
    }

    void CopyTextures(Material material)
    {
        string[] commonTextureNames =            
        {
            "_AlphaTex",
            "_BumpMap",
            "_BackTex",
            "_BumpMap",
            "_Control",
            "_DecalTex",
            "_Detail",
            "_DownTex",
            "_FrontTex",
            "_LeftTex",
            "_LightMap",
            "_MainTex",
            "_RightTex",
            "_Splat0",
            "_Splat1",
            "_Splat2",
            "_Splat3",
            "_UpTex"
        };

        foreach (string texProperty in commonTextureNames)
        {
            if (string.IsNullOrEmpty(texProperty))
            {
                continue;
            }

            if (!material.HasProperty(texProperty))
            {
                continue;
            }

            Texture2D oldTexture =
                (Texture2D)material.GetTexture(texProperty);            

            Debug.Log(string.Format("**Found texture in: {0} reference to: {1}", material.name, texProperty));
            if (string.IsNullOrEmpty(oldTexture.name))
            {
                Debug.LogError("!!Texture reference name is blank");
                continue;
            }

            Debug.Log(string.Format("**Need to fix texture in: {0} reference to: {1}...", material.name, oldTexture.name));
            string findObject =
                string.Format("{0}_{1}", GetShortType(oldTexture), oldTexture.name);
            if (!m_copiedGameObjects.ContainsKey(findObject))
            {
                Debug.LogError(string.Format("!!Could not find referenced texture: {0}...", findObject));
                continue;
            }

            Texture2D texture2D = m_copiedGameObjects[findObject] as Texture2D;
            if (null == texture2D)
            {
                Debug.LogError("!!Find texture is blank");
                continue;
            }

            Debug.Log("**Fixing Main Texture Reference...");
            material.SetTexture(texProperty, texture2D);
        }
    }

    void FixMaterialShader(Object copyObj)
    {
        if (copyObj.GetType() == typeof(Material))
        {
            Debug.Log(string.Format("**Detected Material... {0}", copyObj.name));
            string extension = "mat";
            Material material = copyObj as Material;

            if (null == material)
            {
                return;
            }

            if (null == material.shader)
            {
                return;
            }

            Shader shader = material.shader as Shader;
            extension = "asset";

            if (shader == null)
            {
                return;
            }

            name = shader.name.Substring(shader.name.LastIndexOf("/") + 1, shader.name.Length - shader.name.LastIndexOf("/") - 1);

            if (m_exportedShaders.ContainsKey(name))
            {
                material.shader = m_exportedShaders[name] as Shader;
                return;
            }

            copyObj =
                Object.Instantiate(shader);

            m_exportedShaders[name] =
                copyObj;

            string assetPath =
                string.Format("Assets/SavedAssets/{0}.{1}", name, extension);

            SaveAsset(copyObj, assetPath);
        }
    }

    void SaveAsset(Object copyObj, string assetPath)
    {
        if (null == copyObj)
        {
            Debug.LogError("**!!Null copy object skipping...");
            return;
        }

        Debug.Log("**Deleting old asset...");
        if (AssetDatabase.DeleteAsset(assetPath))
        {
            Debug.Log("**Deleted old asset");
        }

        Debug.Log(string.Format("**Creating Asset Type: {0} Path: {1}", copyObj.GetType(), assetPath));
        AssetDatabase.CreateAsset(copyObj, assetPath);

        Debug.Log("**Saving assets...");
        AssetDatabase.SaveAssets();

        if (string.Empty.Equals(AssetDatabase.GetAssetPath(copyObj)))
        {
            Debug.LogError(string.Format("!!Failed to create Asset Type: {0} with empty Path: {1}", copyObj.GetType(), assetPath));
            return;
        }

        m_copiedGameObjects[string.Format("{0}_{1}", GetShortType(copyObj), copyObj.name)] = (System.Object)copyObj;
    }    
}