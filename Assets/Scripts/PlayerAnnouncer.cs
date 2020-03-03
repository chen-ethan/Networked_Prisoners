using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class PlayerAnnouncer :  NetworkBehaviour
{

    public static event Action<NetworkIdentity> OnLocalPlayerUpdated;

    public override void OnStartLocalPlayer(){
        base.OnStartLocalPlayer();
        OnLocalPlayerUpdated?.Invoke(base.netIdentity);
        Debug.Log("announcer: startlocalplayer");


        StartCoroutine(__RandomizeColor());
    }



    //DELETE THIS
    private IEnumerator __RandomizeColor()
    {
        WaitForSeconds wait = new WaitForSeconds(1f);
        while (true){
            CmdChangeColor();
            yield return wait;
        }
    }

    private void OnDestroy(){
        if(base.isLocalPlayer) OnLocalPlayerUpdated?.Invoke(null);
        Debug.Log("announcer: onDestroy");

    }

    //Updates on Server (Client->server)
    [Command]
    private void CmdChangeColor(){
        Color32 c = Random.ColorHSV();
        SetColor(c);

        //RpcChangeColor(c);

        //identity.connectionToServer --> usually null, not if running as lan host
        NetworkIdentity identity = GetComponent<NetworkIdentity>();
        TargetChangeColor(identity.connectionToClient, c);
    }


    private void SetColor(Color32 color)
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.color = color;
    }
    

    //Updates on Clients (all clients)
    [ClientRpc]
    private void RpcChangeColor(Color32 color)
    {
        SetColor(color);
    }

    //send event to single client 
    //probably useful for updating specific users 
    [TargetRpc]
    private void TargetChangeColor(NetworkConnection conn, Color32 color)
    {
        SetColor(color);
    }





}
