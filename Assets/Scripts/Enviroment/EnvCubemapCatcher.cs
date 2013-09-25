using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnvCubemapCatcher : MonoBehaviour {
    public EnvCubemap cubemap;
    public Renderer myRenderer; // optional, assign in inspector
    public static List<EnvCubemap> allCubemaps = new List<EnvCubemap>();
    public bool useOnlyLosCubemaps = false; // if true: use only cubemaps with LoS... more expensive, must raycast
    public LayerMask losMask; // the raycast mask used to determine if cubemap has LoS to this script

    void Awake() {
        allCubemaps.Clear();
    }

  // Use this for initialization
	void Start () {
        if ( !myRenderer )
            myRenderer = renderer;

        if ( allCubemaps.Count == 0 ) {
            RefreshCubemapList();
        }

        StartCoroutine( UpdateCubemaps() );
	}

    public static void RefreshCubemapList() { // could be called in-game, perhaps, too?
        allCubemaps.AddRange( GameObject.FindObjectsOfType( typeof( EnvCubemap ) ) as EnvCubemap[] );
        Debug.Log( "EnvCubemapCatcher.Refresh(), grabbed " + allCubemaps.Count + " env_cubemaps!" );
    }

    IEnumerator UpdateCubemaps() {
        const float timestep = 0.5f; // cubemap detection doesn't have to happen that frequently
        while ( true ) {
            // cache old env_cubemap
            EnvCubemap oldCubemap = cubemap;

            // find the closest env_cubemap
            float closestSqrMagnitude = 100000f;
            foreach ( EnvCubemap cube in allCubemaps ) {
                // use a sqrMagnitude check as the cheapest possible early-out
                float sqrMagnitude = (cube.transform.position - transform.position).sqrMagnitude;
                if ( sqrMagnitude < closestSqrMagnitude && (!useOnlyLosCubemaps || !Physics.Raycast(transform.position, cube.transform.position - transform.position, Mathf.Sqrt(sqrMagnitude), losMask ) ) ) {
                    closestSqrMagnitude = sqrMagnitude;
                    cubemap = cube;
                }
            }

            if ( myRenderer ) {
                if ( oldCubemap != cubemap ) { // if the new cubemap doesn't match the old one, change it
                    Debug.DrawLine( transform.position, cubemap.transform.position, Color.magenta, 1f );
                    myRenderer.material.SetTexture( "_Cube", cubemap.cubemap );
                }
            } else {
                Debug.LogWarning( "EnvCubemapCatcher can't find its renderer! " + transform.position.ToString() );
            }
            yield return new WaitForSeconds(timestep);
        }
    }
}