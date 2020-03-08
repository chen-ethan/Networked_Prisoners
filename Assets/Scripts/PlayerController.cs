using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //float value between 0.0 and 1.0
    public float health; 

    public int money;

    public int jailTime;

    //public int option;
    // Start is called before the first frame update
    void Start()
    {
        transform.position+= new Vector3(
            Random.Range(-4f,4f),
            Random.Range(-4f,4f),
            0f);
        health = 1f;
        money = Random.Range(-100,100);   //starting game with uneven money = born into rich family / poor etc. unfair but interesting mechanic to think about
        jailTime = 0;

    }

    // Update is called once per frame
    void Update()
    {
        health -= (Time.deltaTime * .01f);
    }
/*

    public void selectOption(int i){
        option = i;
        Debug.Log("chose option " + option + "\t this player has money "+ money);
    }

*/
}
