using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ButtonHandler : NetworkBehaviour
{
    public int option;

    public PlayerAnnouncer PAscript;


    public void selectOption(int i){
        option = i;
        Debug.Log("selected " + option);
    }

    public void makeSelection(){
        ClientScene.localPlayer.GetComponent<PlayerAnnouncer>().CmdConfirmSelection(option);
        ClientScene.localPlayer.GetComponent<PlayerController>().jailTime = option;
        NetworkIdentity identity = GetComponent<NetworkIdentity>();
        if(identity.isServer){
            Debug.Log("this is server");
            ClientScene.localPlayer.GetComponent<PlayerAnnouncer>().CmdConfirmSelection(option);

        }else{
            Debug.Log("this is NOT server");
            ClientScene.localPlayer.GetComponent<PlayerAnnouncer>().CmdConfirmSelection(10);

        }
    }


    



    
}
