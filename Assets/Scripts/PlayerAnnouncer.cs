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


    private string regPrompt;

    private string jailPrompt;

    public override void OnStartLocalPlayer(){
        base.OnStartLocalPlayer();
        OnLocalPlayerUpdated?.Invoke(base.netIdentity);
        Debug.Log("announcer: startlocalplayer");

        canvas = GameObject.FindGameObjectWithTag("Canvas");
        //canvas = ClientScene.
        if(canvas){
            CmdChangeColor();
        }

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

    [Command]
    private void CmdSetHealth(){
        Debug.Log("calling set health");
        GetComponent<PlayerController>().health = .25f;
        RpcSetHealth();
    }

    [ClientRpc]
    private void RpcSetHealth(){
        Debug.Log("calling RPCset health");

        GetComponent<PlayerController>().health = .25f;

    }

    [TargetRpc]
    private void TargetSetPrompt(NetworkConnection connection, string regPrompt, string jailPrompt){
        GetComponent<PlayerController>().promptUp = true;
    }

    //server code
    private void calculatePrompt(){
        int num_free = 0;
        for (int i = 0; i < num_players; ++i){
            if(NetworkServer.connections[i].identity.GetComponent<PlayerController>().jailTime <=0 ){
                num_free++;
            }
        }
        int reward = 50000;
        float prob = 100/num_free;

        int cost = 3000;
        int jailRed = 2;
        regPrompt = "rob a bank? prob of succ = #of players who join / " + num_free + " and the reward is $" + reward + "/#of players who join";
        jailPrompt = "pay " + cost + " to get out of " + jailRed + " years of jail time?";

    }

    private IEnumerator RunServer(){
        WaitForSeconds wait = new WaitForSeconds(1f);
        //server set up == create a button to say when all players joined.
        Debug.Log(canvas);
        //NetworkServer.connections.Keys.Count;

        //get references to all players.

        while(true){
            

            int num_free = 0;
            //calculate prompt
            for (int i = 0; i < num_players; ++i){
                if(NetworkServer.connections[i].identity.GetComponent<PlayerController>().jailTime <=0 ){
                    num_free++;
                }
            }
            //send out prompt
            for (int i = 0; i < num_players; ++i){
                //NetworkServer.connections[i].identity.GetComponent<PlayerAnnouncer>().RpcSetHealth();
                NetworkServer.connections[i].identity.GetComponent<PlayerAnnouncer>().TargetSetPrompt(NetworkServer.connections[i]);
            }
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


    //DELETE THIS
    private IEnumerator __RandomizeColor()
    {
        WaitForSeconds wait = new WaitForSeconds(1f);
        while (true){
            CmdChangeColor();
            yield return wait;
        }
    }

    //Updates on Server (Client->server)
    [Command]
    private void CmdChangeColor(){
        Color32 c = Random.ColorHSV();
        SetColor(c);

        RpcChangeColor(c);

        //identity.connectionToServer --> usually null, not if running as lan host
        //NetworkIdentity identity = GetComponent<NetworkIdentity>();
        //TargetChangeColor(identity.connectionToClient, c);
        
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
