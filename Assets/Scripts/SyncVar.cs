using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class SyncVar : NetworkBehaviour
{


    //whenever color variable is changed, receiving clients will call "set Color" methiod passing in this variable
    // just using [SyncVar] --> no methid is called, handle how you want
    [SyncVar(hook = nameof(SetColor))]
    private Color32 color = Color.red;


    // Unity makes a clone of the Material every time GetComponent<Renderer>().material is used.
    // Cache it here and Destroy it in OnDestroy to prevent a memory leak.
    Material cachedMaterial;

    public override void OnStartServer()
    {
        base.OnStartServer();
        StartCoroutine(__RandomizeColor());

    }


    private void SetColor(Color32 OldColor, Color32 NewColor)
    {
        if (cachedMaterial == null)
            cachedMaterial = GetComponent<Renderer>().material;

        cachedMaterial.color = NewColor;
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.color = NewColor;
    }

    //DELETE THIS
    private IEnumerator __RandomizeColor()
    {
        WaitForSeconds wait = new WaitForSeconds(1f);
        while (true)
        {
            yield return wait;
            color = Random.ColorHSV();
        }
    }


    void OnDestroy()
    {
        Destroy(cachedMaterial);
    }

}
