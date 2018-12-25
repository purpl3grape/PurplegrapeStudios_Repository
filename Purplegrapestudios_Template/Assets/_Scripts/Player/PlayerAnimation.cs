using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlayerCameraView
{
    FirstPerson,
    ThirdPerson,
}

public class PlayerAnimation : MonoBehaviour {

    public NetworkPlayerMovement networkPlayerMovement;
    public PlayerMovement playerMovement;
    public PlayerShooting playerShooting;
    public MouseLook mouseLook;
    public Animator animator_3rdPerson;
    public Animator animator_1stPerson;
    public PhotonView photonView;
    public Camera PlayerCamera;
    public Camera DeathCamera;

    //ANIMATOR PARAMETERS (FOR HASHING)
    int Param_3rdPersonLowerBody;
    int Param_3rdPersonUpperBody;
    int Param_3rdPerson_AimAngle;
    int JumpBool;
    int DeathBool;
    int Param_1stPersonUpperBody_AR;

    //VARIABLES TO TRACK ANIMATION STATES
    int stateInt_3rdPersonUpperBody;
    int stateInt_3rdPersonLowerBody;
    bool stateBool_3rdPersonJump;
    bool stateBool_3rdPersonDeath;
    int stateInt_1stPersonArms;

    //OTHER VARIABLES
    bool armPriorityAnimation;
    bool shotPriorityAnimation;

    //PLAYER CAMERA VIEW VARIABLES
    public PlayerCameraView playerCameraView;
    private PlayerCameraView originalPlayerCamerView;

    // Use this for initialization
    void Start () {

        playerCameraView = PlayerCameraView.FirstPerson;

        if (playerMovement.movementType.Equals(MovementType.Player))
        {
            Param_3rdPersonLowerBody = Animator.StringToHash("Param_3rdPersonLowerBody");
            Param_3rdPersonUpperBody = Animator.StringToHash("Param_3rdPersonUpperBody");
            Param_3rdPerson_AimAngle = Animator.StringToHash("Param_3rdPerson_AimAngle");
            JumpBool = Animator.StringToHash("JumpBool");
            DeathBool = Animator.StringToHash("DeathBool");
            Param_1stPersonUpperBody_AR = Animator.StringToHash("Param_1stPersonUpperBody_AR");
        }
    }

    // Update is called once per frame
    void Update () {

        //1ST PERSON ANIMATION UPDATE
        if (photonView.isMine)
        {
            if (playerMovement.movementType.Equals(MovementType.Player))
            {
                AnimationUpdate_1stPerson();
            }

            if (InputManager.Instance.GetKeyDown(InputCode.SwitchPerspective))
            {
                if (EventManager.Instance.GetScore(photonView.owner.NickName, PlayerStatCodes.Health) > 0)
                {
                    if (playerCameraView.Equals(PlayerCameraView.FirstPerson))
                    {
                        SwitchCameraPerspective(PlayerCameraView.ThirdPerson);
                    }
                    else if (playerCameraView.Equals(PlayerCameraView.ThirdPerson))
                    {
                        SwitchCameraPerspective(PlayerCameraView.FirstPerson);
                    }
                }
            }
        }
        else
        {
            //3RD PERSON ANIMATION UPDATE
            AnimationUpdate_3rdPerson();
        }

    }


    //3RD PERSON ANIMATION
    void AnimationUpdate_3rdPerson()
    {
        if (networkPlayerMovement.networkedMovementType.Equals(MovementType.Player))
        {

            if (networkPlayerMovement.otherPlayerHealth > 0)
            {
                networkPlayerMovement.otherPlayerCurrentAimAngle = Mathf.Lerp(networkPlayerMovement.otherPlayerCurrentAimAngle, networkPlayerMovement.otherPlayerAimAngle, .2f);
                SetStateFloat_3rdPersonAimAngle(networkPlayerMovement.otherPlayerCurrentAimAngle / 90);


                //OTHER PLAYER(S) ON NETWORK IS ALIVE
                //1) SET THEIR RECEIVED DEATH STATE TO FALSE
                if (networkPlayerMovement.receivedNetworked_DeathState)
                {
                    networkPlayerMovement.receivedNetworked_DeathState = false;
                    SetStateBool_3rdPersonDeath(networkPlayerMovement.receivedNetworked_DeathState);
                }


                if (!networkPlayerMovement.otherPlayerFloorDetected)
                {
                    if (networkPlayerMovement.receivedNetworked_JumpState != true)
                    {
                        networkPlayerMovement.receivedNetworked_JumpState = true;
                        SetStateBool_3rdPersonJump(networkPlayerMovement.receivedNetworked_JumpState);
                    }
                }
                else
                {
                    if (networkPlayerMovement.receivedNetworked_JumpState != false)
                    {
                        networkPlayerMovement.receivedNetworked_JumpState = false;
                        SetStateBool_3rdPersonJump(networkPlayerMovement.receivedNetworked_JumpState);
                    }
                    if (networkPlayerMovement.otherPlayerVelocity.magnitude > 15)
                    {
                        if (networkPlayerMovement.receivedNetworked_LegRunningStateID != 1)
                        {
                            networkPlayerMovement.receivedNetworked_LegRunningStateID = 1;
                            SetStateInt_3rdPersonLowerBody(networkPlayerMovement.receivedNetworked_LegRunningStateID);   //Networked PlayerLeg Mesh Anim (Received)
                        }
                    }
                    else if (networkPlayerMovement.otherPlayerVelocity.magnitude <= 15 && networkPlayerMovement.otherPlayerVelocity.magnitude > 0)
                    {
                        if (networkPlayerMovement.receivedNetworked_LegRunningStateID != 1)
                        {
                            networkPlayerMovement.receivedNetworked_LegRunningStateID = 1;
                            SetStateInt_3rdPersonLowerBody(networkPlayerMovement.receivedNetworked_LegRunningStateID);   //Networked PlayerLeg Mesh Anim (Received)
                        }
                    }
                    else
                    {
                        if (networkPlayerMovement.receivedNetworked_LegRunningStateID != 0)
                        {
                            networkPlayerMovement.receivedNetworked_LegRunningStateID = 0;
                            SetStateInt_3rdPersonLowerBody(networkPlayerMovement.receivedNetworked_LegRunningStateID);   //Networked PlayerLeg Mesh Anim (Received)
                        }
                    }
                }

                //OTHER PLAYER UPPER BODY HANDLED HERR
                if (networkPlayerMovement.currentNetworked_Assault_StateID != networkPlayerMovement.receivedNetworked_Assault_StateID)
                {
                    networkPlayerMovement.currentNetworked_Assault_StateID = networkPlayerMovement.receivedNetworked_Assault_StateID;
                    SetStateInt_3rdPersonUpperBody(networkPlayerMovement.currentNetworked_Assault_StateID);   //Networked PlayerArm Mesh Anim (Received)
                }
            }
            else
            {
                //DEATH: OTHER PLAYER HEALTH LESS THAN OR EQUAL 0
                if (!networkPlayerMovement.receivedNetworked_DeathState)
                {
                    networkPlayerMovement.receivedNetworked_DeathState = true;
                    SetStateBool_3rdPersonDeath(networkPlayerMovement.receivedNetworked_DeathState);

                    SetStateFloat_3rdPersonAimAngle(0);
                }
            }
        }
    }

    //

    //1ST PERSON ANIMATION
    void AnimationUpdate_1stPerson()
    {

        //PLAYER IS ALIVE
        if (EventManager.Instance.GetScore(photonView.owner.NickName, PlayerStatCodes.Health) > 0)
        {
            SetStateFloat_3rdPersonAimAngle(mouseLook.GetCameraRotationY() / 90);    ///TO SHOW THE ANIMATION (OPTIONAL - FOR DEBUGGING)

            //1) SET DEATHBOOL TO FALSE (SET BOTH 1ST AND 3RD PERSON - AND AFFECTS WHOLE BODY)
            if (stateBool_3rdPersonDeath)
            {
                DeathCamera.enabled = false;
                PlayerCamera.enabled = true;

                if (originalPlayerCamerView.Equals(PlayerCameraView.FirstPerson))
                    GetComponent<PlayerAnimation>().SwitchCameraPerspective(PlayerCameraView.FirstPerson);

                stateBool_3rdPersonDeath = false;
                SetStateBool_3rdPersonDeath(stateBool_3rdPersonDeath);
            }

            //SHOOTING (UPPER BODY)
            if (playerShooting.isFiringBullet)
            {
                armPriorityAnimation = true;

                if (playerShooting.isAiming)
                {
                    //AIM FIRE UPPER BODY
                    if (stateInt_1stPersonArms != 3)
                    {
                        stateInt_1stPersonArms = 3;
                        stateInt_3rdPersonUpperBody = 3;
                        SetStateInt_1stPersonArms(stateInt_1stPersonArms);  //Local AR AimFire
                        SetStateInt_3rdPersonUpperBody(stateInt_3rdPersonUpperBody);    ///TO SHOW THE ANIMATION (OPTIONAL - FOR DEBUGGING)
                    }
                }
                else
                {
                    //FIRE UPPER BODY
                    if (stateInt_1stPersonArms != 2)
                    {
                        stateInt_1stPersonArms = 2;
                        stateInt_3rdPersonUpperBody = 2;
                        SetStateInt_1stPersonArms(stateInt_1stPersonArms);  //Local AR Fire
                        SetStateInt_3rdPersonUpperBody(stateInt_3rdPersonUpperBody);    ///TO SHOW THE ANIMATION (OPTIONAL - FOR DEBUGGING)
                    }
                }
            }
            else if (playerShooting.isAiming)
            {
                armPriorityAnimation = true;
                if (playerShooting.isFiringBullet)
                {
                    if (stateInt_1stPersonArms != 2)
                    {
                        stateInt_1stPersonArms = 2;
                        stateInt_3rdPersonUpperBody = 2;
                        SetStateInt_1stPersonArms(stateInt_1stPersonArms);  //Local AR Fire
                        SetStateInt_3rdPersonUpperBody(stateInt_3rdPersonUpperBody);    ///TO SHOW THE ANIMATION (OPTIONAL - FOR DEBUGGING)
                    }
                }
                else
                {    //AIM UPPER BODY
                    if (stateInt_1stPersonArms != 1)
                    {
                        stateInt_1stPersonArms = 1;
                        stateInt_3rdPersonUpperBody = 1;
                        SetStateInt_1stPersonArms(stateInt_1stPersonArms);  //Local AR Aim
                        SetStateInt_3rdPersonUpperBody(stateInt_3rdPersonUpperBody);    ///TO SHOW THE ANIMATION (OPTIONAL - FOR DEBUGGING)
                    }
                }
            }
            else if (!playerShooting.isAiming && !playerShooting.isFiringBullet)
            {
                //IDLE UPPER BODY (NOT SHOOTING)
                if (!armPriorityAnimation)
                {
                    stateInt_1stPersonArms = 0;
                    stateInt_3rdPersonUpperBody = 0;
                    SetStateInt_1stPersonArms(stateInt_1stPersonArms);  //Local Arms Idle
                    SetStateInt_3rdPersonUpperBody(stateInt_3rdPersonUpperBody);    ///TO SHOW THE ANIMATION (OPTIONAL - FOR DEBUGGING)
                }
                armPriorityAnimation = false;
            }


            //MOVEMENT (WHOLE BODY)
            if (playerMovement.floorDetected)
            {
                //JUMP
                if (stateBool_3rdPersonJump != false)
                {
                    stateBool_3rdPersonJump = false;
                    SetStateBool_3rdPersonJump(stateBool_3rdPersonJump);    ///TO SHOW THE ANIMATION (OPTIONAL - FOR DEBUGGING)
                }

                if (playerMovement.playerVelocity.magnitude > 15)
                {
                    //RUNNING LOWER BODY
                    if (stateInt_3rdPersonLowerBody != 1)
                    {
                        stateInt_3rdPersonLowerBody = 1;
                        SetStateInt_3rdPersonLowerBody(stateInt_3rdPersonLowerBody);    ///TO SHOW THE ANIMATION (OPTIONAL - FOR DEBUGGING)
                    }
                    //RUNNING UPPER BODY
                    if (stateInt_1stPersonArms != 5 && !armPriorityAnimation)
                    {
                        stateInt_1stPersonArms = 5;
                        stateInt_3rdPersonUpperBody = 5;
                        SetStateInt_1stPersonArms(stateInt_1stPersonArms);  //Local Arms Run
                        SetStateInt_3rdPersonUpperBody(stateInt_3rdPersonUpperBody);    ///TO SHOW THE ANIMATION (OPTIONAL - FOR DEBUGGING)
                    }
                }
                else if (playerMovement.playerVelocity.magnitude <= 15 && playerMovement.playerVelocity.magnitude > 0)
                {
                    //WALKING LOWER BODY
                    if (stateInt_3rdPersonLowerBody != 1)
                    {
                        stateInt_3rdPersonLowerBody = 1;
                        SetStateInt_3rdPersonLowerBody(stateInt_3rdPersonLowerBody);    ///TO SHOW THE ANIMATION (OPTIONAL - FOR DEBUGGING)
                    }
                    //WALKING UPPER BODY
                    if (stateInt_1stPersonArms != 4 && !armPriorityAnimation)
                    {
                        stateInt_1stPersonArms = 4;
                        stateInt_3rdPersonLowerBody = 4;
                        SetStateInt_1stPersonArms(stateInt_1stPersonArms);  //Local Arms Walk
                        SetStateInt_3rdPersonUpperBody(stateInt_3rdPersonLowerBody);    ///TO SHOW THE ANIMATION (OPTIONAL - FOR DEBUGGING)
                    }
                }
                else
                {
                    //IDLE LOWER BODY (HANDLED IN MOVEMENT/JUMP BECAUSE WANT JUMP ANIMATION TO TAKE PRIORITY)
                    if (stateInt_3rdPersonLowerBody != 0)
                    {
                        stateInt_3rdPersonLowerBody = 0;
                        SetStateInt_3rdPersonLowerBody(stateInt_3rdPersonLowerBody);    ///TO SHOW THE ANIMATION (OPTIONAL - FOR DEBUGGING)
                    }
                }
            }
            else
            {
                //JUMPING (SET BOTH 1ST AND 3RD PERSON - AND AFFECTS WHOLE BODY)
                if (stateBool_3rdPersonJump != true)
                {
                    stateBool_3rdPersonJump = true;
                    SetStateBool_3rdPersonJump(stateBool_3rdPersonJump);    ///TO SHOW THE ANIMATION (OPTIONAL - FOR DEBUGGING)
                }

            }


        }
        else
        {
            //DEATH ANIMATION: LESS THAN OR EQUAL TO 0 HP
            if (!stateBool_3rdPersonDeath)
            {
                originalPlayerCamerView = playerCameraView;
                if (originalPlayerCamerView.Equals(PlayerCameraView.FirstPerson))
                    GetComponent<PlayerAnimation>().SwitchCameraPerspective(PlayerCameraView.ThirdPerson);

                DeathCamera.enabled = true;
                PlayerCamera.enabled = false;
                stateBool_3rdPersonDeath = true;
                SetStateBool_3rdPersonDeath(stateBool_3rdPersonDeath);

                SetStateFloat_3rdPersonAimAngle(0);    ///TO SHOW THE ANIMATION (OPTIONAL - FOR DEBUGGING)
            }

        }

    }
    //


    //SET ANIMATOR STATES
    public void SetStateInt_3rdPersonLowerBody(int val)
    {
        if ((playerCameraView.Equals(PlayerCameraView.ThirdPerson) || !photonView.isMine) && animator_3rdPerson.gameObject.GetActive())
            animator_3rdPerson.SetFloat(Param_3rdPersonLowerBody, val);
    }

    public void SetStateInt_3rdPersonUpperBody(int val)
    {
        if ((playerCameraView.Equals(PlayerCameraView.ThirdPerson) || !photonView.isMine) && animator_3rdPerson.gameObject.GetActive())
            animator_3rdPerson.SetInteger(Param_3rdPersonUpperBody, val);
    }

    public void SetStateBool_3rdPersonJump(bool val)
    {
        if ((playerCameraView.Equals(PlayerCameraView.ThirdPerson) || !photonView.isMine) && animator_3rdPerson.gameObject.GetActive())
            animator_3rdPerson.SetBool(JumpBool, val);
    }

    public void SetStateBool_3rdPersonDeath(bool val)
    {
        if ((playerCameraView.Equals(PlayerCameraView.ThirdPerson) || !photonView.isMine) && animator_3rdPerson.gameObject.GetActive())
            animator_3rdPerson.SetBool(DeathBool, val);
    }

    public void SetStateFloat_3rdPersonAimAngle(float val)
    {
        if ((playerCameraView.Equals(PlayerCameraView.ThirdPerson) || !photonView.isMine) && animator_3rdPerson.gameObject.GetActive())
            animator_3rdPerson.SetFloat(Param_3rdPerson_AimAngle, val);
    }

    public void SetStateInt_1stPersonArms(int val)
    {
        if (playerCameraView.Equals(PlayerCameraView.FirstPerson))
            animator_1stPerson.SetInteger(Param_1stPersonUpperBody_AR, val);
    }


    //SWITCH BETWEEN FIRST PERSON AND THIRD PERSON CAMERA VIEW
    public void SwitchCameraPerspective(PlayerCameraView view)
    {
        if (playerMovement.movementType.Equals(MovementType.BumperCar))
            playerMovement.PlayerEvents_ChangeMovementType(photonView.owner.ID, MovementType.Player);

        //GetComponent<PlayerObjectComponents>().PlayerCamera.GetComponent<Camera>().cullingMask ^= 1 << LayerMask.NameToLayer("Player");
        stateInt_3rdPersonUpperBody = -1;
        stateInt_3rdPersonLowerBody = -1;
        stateBool_3rdPersonJump = !stateBool_3rdPersonJump;
        stateInt_1stPersonArms = -1;

        if (view.Equals(PlayerCameraView.FirstPerson))
        {
            GetComponent<PlayerObjectComponents>().CameraConatiner.GetComponent<CameraCollision>().RestDollyPosition();
            GetComponent<PlayerObjectComponents>().PlayerCamera.GetComponent<Camera>().cullingMask &= ~(1 << LayerMask.NameToLayer("Player"));
            playerCameraView = PlayerCameraView.FirstPerson;
            GetComponent<PlayerObjectComponents>().ThirdPersonPlayer.SetActive(false);
            GetComponent<PlayerObjectComponents>().FirstPersonPlayer.SetActive(true);
        }
        else if (view.Equals(PlayerCameraView.ThirdPerson))
        {
            GetComponent<PlayerObjectComponents>().CameraConatiner.GetComponent<CameraCollision>().RestDollyPosition();
            GetComponent<PlayerObjectComponents>().PlayerCamera.GetComponent<Camera>().cullingMask |= 1 << LayerMask.NameToLayer("Player");
            playerCameraView = PlayerCameraView.ThirdPerson;
            GetComponent<PlayerObjectComponents>().ThirdPersonPlayer.SetActive(true);
            GetComponent<PlayerObjectComponents>().FirstPersonPlayer.SetActive(false);

        }
    }

}