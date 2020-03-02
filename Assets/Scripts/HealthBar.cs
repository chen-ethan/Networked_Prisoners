using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
public class HealthBar : NetworkBehaviour
{

    public PlayerController _playerScript;
    public Image _healthFillBar;
    
    // Start is called before the first frame update
    private void Awake()
    {
        PlayerUpdated(ClientScene.localPlayer);
        //listen for additional local player updates
        PlayerAnnouncer.OnLocalPlayerUpdated+=PlayerUpdated;
    }

    private void Update(){
        if(_playerScript == null){
            return;
        }else{
        _healthFillBar.fillAmount = _playerScript.health;
        Debug.Log("health amt = "+ _healthFillBar.fillAmount);
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
