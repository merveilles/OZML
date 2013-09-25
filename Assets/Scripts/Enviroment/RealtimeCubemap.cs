using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class RealtimeCubemap : MonoBehaviour {

    public int cubemapSize = 512;
    public bool oneFacePerFrame = false;
    private Camera cam;
    private RenderTexture rtex;
    private GameObject go;

    [ExecuteInEditMode]
    void Start() {
        // render all six faces at startup
        UpdateCubemap(63);
    }

    void Update() {
       if (oneFacePerFrame) {
            int faceToRender = Time.frameCount % 6;
            int faceMask = 1 << faceToRender;
            UpdateCubemap(faceMask);
        } else {
            UpdateCubemap(63); // all six faces
        }
    }

    void UpdateCubemap(int faceMask) {
        if (!cam) {
            go = new GameObject("CubemapCamera");
            go.AddComponent(typeof(Camera));
            go.hideFlags = HideFlags.HideAndDontSave;
            go.transform.position = transform.position;
            go.transform.rotation = Quaternion.identity;
            cam = go.camera;
			cam.backgroundColor = Color.black;
			cam.cullingMask = ~(1 << 4);
            cam.farClipPlane = 100; // don't render very far into cubemap
            cam.enabled = false;
        }

        if (!rtex) {    
            rtex = new RenderTexture(cubemapSize, cubemapSize, 16);
            rtex.isCubemap = true;
            rtex.hideFlags = HideFlags.HideAndDontSave;
            renderer.sharedMaterial.SetTexture ("_Cube", rtex);
        }

        cam.transform.position = transform.position;
        cam.RenderToCubemap(rtex, faceMask);
    }

    void OnDisable() {
        DestroyImmediate (cam);
        DestroyImmediate (rtex);
    }
}