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
    private int option = -1;

    private ButtonHandler buttons;

    public GameObject canvas;

    //server
    private bool gameStarted = false;
    private int num_players;

    // -1 --> no response; 1--> yes; 0 --> no
    private int[] responses = new int[4]; //size 4 for 4 players


    private string regPrompt;

    private string jailPrompt;




    public override void OnStartLocalPlayer(){
        base.OnStartLocalPlayer();
        OnLocalPlayerUpdated?.Invoke(base.netIdentity);
        Debug.Log("announcer: startlocalplayer");

        canvas = GameObject.FindGameObjectWithTag("Canvas");
        if(isServer){

            //server set up == create a button to say when all players joined.
            canvas.gameObject.transform.GetChild(2).gameObject.SetActive(true);

            
        }


        //maybe move to where prompt gets displayed
        buttons = GetComponent<ButtonHandler>();
        Debug.Log("buttons found: "+ buttons);
    }

    //server
    public void startGame()
    {
        canvas.gameObject.transform.GetChild(2).gameObject.SetActive(false);
        Debug.Log("started");
        gameStarted = true;
        num_players = NetworkServer.connections.Count;
        //responses = new int[num_players];
        StartCoroutine(RunServer());
        //RunServer();
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
        GetComponent<PlayerController>().RegPrompt[0] = "New Title";
        GetComponent<PlayerController>().RegPrompt[1] = "New Question?";
        GetComponent<PlayerController>().RegPrompt[2] = regPrompt;
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
        //float prob = 100/num_free;

        int cost = 3000;
        int jailRed = 2;
        regPrompt = "rob a bank? prob of succ = #of players who join / " + num_free + " and the reward is $" + reward + "/#of players who join";
        jailPrompt = "pay " + cost + " to get out of " + jailRed + " years of jail time?";


    }


    //server
    private IEnumerator waitForResponses()
    {
        WaitForSeconds wait = new WaitForSeconds(.5f);
        bool done = false;
        while (!done)
        {
            done = true;
            for (int i = 0; i < num_players; i++)
            {
                if (responses[i] == -1)
                {
                    done = false;
                    if (NetworkServer.connections[i].identity.GetComponent<PlayerAnnouncer>().option >= 0)
                    {
                        responses[i] = NetworkServer.connections[i].identity.GetComponent<PlayerAnnouncer>().option;
                        NetworkServer.connections[i].identity.GetComponent<PlayerAnnouncer>().option = -1;
                    }
                }
            }
            yield return wait;
        }
        Debug.Log("waiting done!");
        //calcResults();
    }


    //server
    private void calcResults()
    {
        Debug.Log("CALC");

    }


    
    private IEnumerator RunServer(){
        WaitForSeconds wait = new WaitForSeconds(1f);
        //NetworkServer.connections.Keys.Count;

        //get references to all players.
        var s = 0;
        while(true){ //change to # of turns in game
            Debug.Log("server: " + s);
            calculatePrompt();
            //send out prompt
            for (int i = 0; i < num_players; ++i){
                //NetworkServer.connections[i].identity.GetComponent<PlayerAnnouncer>().RpcSetHealth();
                NetworkServer.connections[i].identity.GetComponent<PlayerAnnouncer>().TargetSetPrompt(NetworkServer.connections[i],regPrompt,jailPrompt);
            }
            //wait for responses
            for (int i = 0; i < num_players; i++)
            {
                responses[i] = -1;
            }
            yield return StartCoroutine(waitForResponses());
            
            Debug.Log("done w loop");

            //once all received --> calculate 
            calcResults();


            CmdChangeColor();
            //yield return wait;
            s++;
        }
    }
    
    /*
    private void RunServer()
    {
        for(int s = 0; s <30;)
        { //change to # of turns in game
            Debug.Log("server: " + s);

            int num_free = 0;
            //calculate prompt
            for (int i = 0; i < num_players; ++i)
            {
                if (NetworkServer.connections[i].identity.GetComponent<PlayerController>().jailTime <= 0)
                {
                    num_free++;
                }
            }
            //send out prompt
            for (int i = 0; i < num_players; ++i)
            {
                //NetworkServer.connections[i].identity.GetComponent<PlayerAnnouncer>().RpcSetHealth();
                calculatePrompt();
                NetworkServer.connections[i].identity.GetComponent<PlayerAnnouncer>().TargetSetPrompt(NetworkServer.connections[i], regPrompt, jailPrompt);
            }
            //wait for responses
            for (int i = 0; i < num_players; i++)
            {
                responses[i] = -1;
            }
            StartCoroutine(waitForResponses(s));
            bool done = false;
            while (!done)
            {
                done = true;
                for (int i = 0; i < num_players; i++)
                {

                    if (responses[i] == -1)
                    {
                        Debug.Log(i + "is -1");
                        done = false;
                        if (NetworkServer.connections[i].identity.GetComponent<PlayerAnnouncer>().option >= 0)
                        {
                            responses[i] = NetworkServer.connections[i].identity.GetComponent<PlayerAnnouncer>().option;
                            NetworkServer.connections[i].identity.GetComponent<PlayerAnnouncer>().option = -1;
                        }
                    }
                }
            }
            

            Debug.Log("done w loop");

            //once all received --> calculate 


            CmdChangeColor();
        }
    }

    */

    private void OnDestroy(){
        if(base.isLocalPlayer) OnLocalPlayerUpdated?.Invoke(null);
        Debug.Log("announcer: onDestroy");

    }

    
    [Command]
    public void CmdConfirmSelection(int i){
        //option = buttons.option;
        option = i;
        Debug.Log("confirming option:" + option);
        GetComponent<PlayerController>().promptUp = false;
        RpcConfirmSelection(i);
        /*
        NetworkIdentity identity = GetComponent<NetworkIdentity>();
        if(identity.isServer){
            Debug.Log("this is server");

        }
        */
    }

    [ClientRpc]
    private void RpcConfirmSelection(int i)
    {
        option = i;
        GetComponent<PlayerController>().promptUp = false;
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
