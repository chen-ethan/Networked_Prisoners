using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using TMPro;

public class HealthBar : NetworkBehaviour
{

    public PlayerController _playerScript;
    public Image _healthFillBar;
    public TextMeshProUGUI MoneyText;
    public TextMeshProUGUI JailText;


    public GameObject prompt;

    
    // Start is called before the first frame update
    private void Awake()
    {
        PlayerUpdated(ClientScene.localPlayer);
        //listen for additional local player updates
        PlayerAnnouncer.OnLocalPlayerUpdated+=PlayerUpdated;
    }


    //change to update only once every round (when info changes)? performance increase //note watch for new UI additions that constantly change when in play
    private void Update(){
        if(_playerScript == null){
            return;
        }else{
            _healthFillBar.fillAmount = _playerScript.health;
            //Debug.Log("health amt = "+ _healthFillBar.fillAmount);
            if(_playerScript.money > 0)
            {
                MoneyText.text = "Money: $" + _playerScript.money;
            }
            else
            {
                MoneyText.text = "Money: -$" + Mathf.Abs(_playerScript.money);
            }
            JailText.text = "Jail Time: " + _playerScript.jailTime + " years";
            if(_playerScript.promptUp){
                prompt.SetActive(true);
            }
        }
    }

    private void OnDestroy(){
        PlayerAnnouncer.OnLocalPlayerUpdated-=PlayerUpdated;

    }

    private void PlayerUpdated(NetworkIdentity localPlayer){
        if(localPlayer!=null){
            _playerScript = localPlayer.GetComponent<PlayerController>();

            this.enabled = (localPlayer!=null);
        }
    }
    
}
