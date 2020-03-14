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
    private bool gameStarted = false;
    private int num_players;

    public override void OnStartLocalPlayer(){
        base.OnStartLocalPlayer();
        OnLocalPlayerUpdated?.Invoke(base.netIdentity);
        Debug.Log("announcer: startlocalplayer");

        canvas = GameObject.FindGameObjectWithTag("Canvas");

        //StartCoroutine(__RandomizeColor());
        if(isServer){
            canvas.gameObject.transform.GetChild(2).gameObject.SetActive(true);

            
        }


        //maybe move to where prompt gets displayed
        buttons = GetComponent<ButtonHandler>();
        Debug.Log("buttons found: "+ buttons);
    }

    public void startGame()
    {
        canvas.gameObject.transform.GetChild(2).gameObject.SetActive(false);
        Debug.Log("starte");
        gameStarted = true;
        num_players = NetworkServer.connections.Count;
        StartCoroutine(RunServer());
    }

    public void activatePrompt()
    {
        Debug.Log("act prompt: " + canvas);
        canvas.transform.GetChild(1).gameObject.SetActive(true);

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
        //canvas.gameObject.transform.GetChild(2).gameObject.SetActive(true);
        //NetworkServer.connections.Keys.Count;

        //get references to all players.

        while(true){
            for (int i = 0; i < num_players; ++i)
            {
                Debug.Log("calling activate: " + i);
                NetworkServer.connections[0].identity.GetComponent<PlayerAnnouncer>().activatePrompt();
            }
            //NetworkServer.connections[0].identity.GetComponent<PlayerAnnouncer>().canvas.transform.GetChild(1).gameObject.SetActive(true);
            //Debug.Log("#Connections: " + NetworkServer.connections.Count);
            //NetworkServer.connections[1].identity.GetComponent<PlayerAnnouncer>().canvas.transform.GetChild(1).gameObject.SetActive(true);

            //send out prompt

            //wait for responses

            //once all received --> calculate 


            CmdChangeColor();
            Debug.Log("player count: " + NetworkServer.connections.Keys.Count );
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
