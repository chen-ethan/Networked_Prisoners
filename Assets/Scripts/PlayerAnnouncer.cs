using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using TMPro;

public class PlayerAnnouncer :  NetworkBehaviour
{

    public static event Action<NetworkIdentity> OnLocalPlayerUpdated;
    private int option;

    private ButtonHandler buttons;

    public GameObject canvas;

    public override void OnStartLocalPlayer(){
        base.OnStartLocalPlayer();
        OnLocalPlayerUpdated?.Invoke(base.netIdentity);
        Debug.Log("announcer: startlocalplayer");

        canvas = GameObject.FindGameObjectWithTag("Canvas");

        //StartCoroutine(__RandomizeColor());
        if(isServer){
            StartCoroutine(RunServer());
        }


        //maybe move to where prompt gets displayed
        buttons = GetComponent<ButtonHandler>();
        Debug.Log("buttons found: "+ buttons);
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

    private IEnumerator RunServer(){
        WaitForSeconds wait = new WaitForSeconds(1f);
        //server set up == create a button to say when all players joined.
        Debug.Log(canvas);


        //get references to all players.
        while(true){


            //send out prompt

            //wait for responses

            //once all received --> calculate 


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

    [Command]
    public void CmdConfirmSelection(int i){
        //option = buttons.option;
        option = i;
        Debug.Log("confirming option:" + option);
        /*
        NetworkIdentity identity = GetComponent<NetworkIdentity>();
        if(identity.isServer){
            Debug.Log("this is server");

        }
        */
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
