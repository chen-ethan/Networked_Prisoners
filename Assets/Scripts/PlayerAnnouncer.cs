using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;
using UnityEngine.UI;
using TMPro;

public class PlayerAnnouncer : NetworkBehaviour
{

    public static event Action<NetworkIdentity> OnLocalPlayerUpdated;
    private int option = -1;

    private ButtonHandler buttons;

    public GameObject canvas;

    //server
    private int num_players;
    private int num_free;

    private int text_separators = 8;

    // -1 --> no response; 1--> yes; 0 --> no
    private int[] responses = new int[4]; //size 4 for 4 players

    /*
    private string regPrompt;

    private string jailPrompt;
    */
    
    private string[,] prompts;
    private int prompts_len;




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
        num_players = NetworkServer.connections.Count;
        LoadPromptsFromTxt();
        //responses = new int[num_players];
        StartCoroutine(RunServer());
        //RunServer();
    }

    //Server
    private void LoadPromptsFromTxt()
    {

        //reminder for now -- ignore first line
        //loads promptlines and puts each into an array slot
        TextAsset promptsTxt = Resources.Load("prompts") as TextAsset;
        string[] promptsLines = promptsTxt.text.Split('\n');
        prompts_len = promptsLines.Length -1; //-1 because it would count \n after last line.

        //created 2d array for prompts broken up by | category
        prompts = new string[prompts_len, text_separators];

        //reminder for now -- ignore first line i = 1
        string[] breaker;
        for (int i = 0; i < prompts_len; ++i)
        {
            breaker = promptsLines[i].Split('|');
            for (int x = 0; x < text_separators; ++x)
            {
                prompts[i, x] = breaker[x];
            }

        }


        //printing prompts[0]
        Debug.Log("prompts[0]: " + prompts[0, 0] + " | " + prompts[0, 1] + " | " + prompts[0, 2]);
        Debug.Log("prompts[0]: " + prompts[1, 0] + " | " + prompts[1, 1] + " | " + prompts[1, 2]);

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
    private void TargetSetPrompt(NetworkConnection connection, string[] regPrompt, string[] jailPrompt){
        GetComponent<PlayerController>().RegPrompt = regPrompt;
        GetComponent<PlayerController>().JailPrompt = jailPrompt;
        GetComponent<PlayerController>().promptUp = true;
    }

    //server code
    private void calculatePrompt(){
        num_free = 0;
        for (int i = 0; i < num_players; ++i){
            if(NetworkServer.connections[i].identity.GetComponent<PlayerController>().jailTime <=0 ){
                num_free++;
            }
        }
        int selected_prompt = Random.Range(0,prompts_len-1);
        Debug.Log("selected" + selected_prompt);
        int jail_selected_prompt;
        //for testing remove v
        selected_prompt = 1;
        jail_selected_prompt = 1;




        int reward = 50000;
        //float prob = 100/num_free;

        int cost = 3000;
        int jailRed = 2;
        //regPrompt = "rob a bank? prob of succ = #of players who join / " + num_free + " and the reward is $" + reward + "/#of players who join";
        //jailPrompt = "pay " + cost + " to get out of " + jailRed + " years of jail time?";

        //send out prompt
        string[] regPrompt = { prompts[selected_prompt, 0], prompts[selected_prompt, 1], prompts[selected_prompt, 2], prompts[selected_prompt, 3], prompts[selected_prompt, 4], prompts[selected_prompt, 5], prompts[selected_prompt, 6], prompts[selected_prompt, 7] };
        //make its own
        string[] jailPrompt = regPrompt;

        for (int i = 0; i < num_players; ++i)
        {
            //NetworkServer.connections[i].identity.GetComponent<PlayerAnnouncer>().RpcSetHealth();
            NetworkServer.connections[i].identity.GetComponent<PlayerAnnouncer>().TargetSetPrompt(NetworkServer.connections[i], regPrompt, jailPrompt);
        }
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
        int yes_count = 0;
        int jail_yes_count = 0;
        for (int i = 0; i < num_players; i++)
        {
            if (responses[i] == 1){
                if (NetworkServer.connections[i].identity.GetComponent<PlayerController>().jailTime <= 0)
                {
                    yes_count++;
                }
                else
                {
                    jail_yes_count++;
                }
            }
            responses[i] = -1;
        }
        //calculate regular results
        if (num_free > 0)
        {
            float frac = (float)yes_count / num_free;
            float rand_res = Random.Range(0.0f, 1.0f);
            if (rand_res <= frac)
            {
                Debug.Log("Regular prompt sucess");
            }
            else
            {
                Debug.Log("Regular prompt failure");

            }
        }
        //calculate jail results;
        if (num_players-num_free>0) {
            float frac1 = (float)jail_yes_count / (num_players - num_free);
            float rand_res1 = Random.Range(0.0f, 1.0f);
            if (rand_res1 <= frac1)
            {
                Debug.Log("Jail prompt sucess");
            }
            else
            {
                Debug.Log("Jail prompt failure");

            }
        }


    }


    
    private IEnumerator RunServer(){
        WaitForSeconds wait = new WaitForSeconds(1f);
        //NetworkServer.connections.Keys.Count;

        //get references to all players.
        var s = 0;
        while(true){ //change to # of turns in game
            Debug.Log("server: " + s);
            calculatePrompt(); //& sends prompt
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
