using UnityEngine;
using System.Collections;

public class EnvCubemap : MonoBehaviour {
    public Cubemap cubemap;
    public int cubemapRes = 128; // powers of two only
    public CameraClearFlags clearFlags = CameraClearFlags.Color;
    public Color clearColor = Color.black;
    public LayerMask cullingMask;
    public float near = 0.1f;
    public float far = 10000f;

    public void KillThis( GameObject go ) {
        DestroyImmediate( go );
    }

}