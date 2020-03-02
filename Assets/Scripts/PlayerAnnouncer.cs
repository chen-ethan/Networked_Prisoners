using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerAnnouncer :  NetworkBehaviour
{

    public static event Action<NetworkIdentity> OnLocalPlayerUpdated;

    public override void OnStartLocalPlayer(){
        base.OnStartLocalPlayer();
        OnLocalPlayerUpdated?.Invoke(base.netIdentity);
        Debug.Log("announcer: startlocalplayer");
    }

    private void OnDestroy(){
        if(base.isLocalPlayer) OnLocalPlayerUpdated?.Invoke(null);
        Debug.Log("announcer: onDestroy");

    }

}
