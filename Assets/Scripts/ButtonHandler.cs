using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class ButtonHandler : NetworkBehaviour
{
    public int option;

    public PlayerAnnouncer PAscript;

    public Button Yes;
    public Button No;

    public Color selected;
    public Color notSelected;


    public void selectOption(int i){
        option = i;
        if (i==1)
        {
            //Debug.Log("selected " + option);
            Yes.image.color = selected;
            No.image.color = notSelected;
        }
        else
        {
            Yes.image.color = notSelected;
            No.image.color = selected;
        }
    }

    public void makeSelection(){
        Yes.image.color = notSelected;
        No.image.color = notSelected;
        ClientScene.localPlayer.GetComponent<PlayerAnnouncer>().CmdConfirmSelection(option);
        NetworkIdentity identity = GetComponent<NetworkIdentity>();

        /*
        if (identity.isServer){
            Debug.Log("this is server");
            ClientScene.localPlayer.GetComponent<PlayerAnnouncer>().CmdConfirmSelection(option);

        }else{
            Debug.Log("this is NOT server");
            ClientScene.localPlayer.GetComponent<PlayerAnnouncer>().CmdConfirmSelection(option);

        }*/
    }

    public void startGame()
    {
        if (GetComponent<NetworkIdentity>().isServer)
        {
            ClientScene.localPlayer.GetComponent<PlayerAnnouncer>().startGame();
        }






    }
}
