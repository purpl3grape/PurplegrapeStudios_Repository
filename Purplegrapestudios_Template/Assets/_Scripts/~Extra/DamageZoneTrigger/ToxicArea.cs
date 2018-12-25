using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToxicArea : MonoBehaviour {

    int currentHealth;

    private void OnTriggerEnter(Collider other)
    {
        if (!PhotonNetwork.isMasterClient) return;
    
        PhotonView photonView = other.GetComponent<PhotonView>();
        if (photonView != null)
        {
            //scoringManager.GetComponent<PhotonView>().RPC("ChangeScore", PhotonTargets.All, photonView.owner.NickName, "health", -25);
            currentHealth = EventManager.Instance.GetScore(photonView.owner.NickName, PlayerStatCodes.Health);
            if (currentHealth > 0)
            {
                if (other.GetComponent<PlayerShooting>().enabled)
                {
                    EventManager.Instance.UpdateStat_Health(photonView.owner.NickName, PlayerStatCodes.Health, -20);
                    EventManager.Instance.UpdateStat_Health(photonView.owner.NickName, PlayerStatCodes.DamageReceived, 20);
                }
            }
            if (currentHealth-20 <= 0)
            {
                if (other.GetComponent<PlayerShooting>().enabled)
                {
                    other.GetComponent<PlayerShooting>().PlayerEvents_RespawnPlayer(other.GetComponent<PhotonView>().ownerId, other.GetComponent<PhotonView>().owner.NickName, string.Empty);
                }
            }

        }
    }

}
