using MagicExtended.Models;
using MagicExtended.Types;
using System;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace MagicExtended.Helpers
{
    internal class PlayerArmatureHelper
    {
        public string prefix = "ME_attach";

        public string playerHeadPath = "Visual/Armature/Hips/Spine/Spine1/Spine2/Neck/Head";
        public string playerJawPath = "Visual/Armature/Hips/Spine/Spine1/Spine2/Neck/Head/Jaw";
        public string playerJawEndPath = "Visual/Armature/Hips/Spine/Spine1/Spine2/Neck/Head/Jaw/Jaw_end";
        public string playerNeckPath = "Visual/Armature/Hips/Spine/Spine1/Spine2/Neck";

        public string playerSpine2Path = "Visual/Armature/Hips/Spine/Spine1/Spine2";
        public string playerSpine1Path = "Visual/Armature/Hips/Spine/Spine1";
        public string playerSpinePath = "Visual/Armature/Hips/Spine";
        public string playerHipsPath = "Visual/Armature/Hips";

        public string playerShoulderLeftPath = "Visual/Armature/Hips/Spine/Spine1/Spine2/LeftShoulder/LeftArm";
        public string playerElbowLeftPath = "Visual/Armature/Hips/Spine/Spine1/Spine2/LeftShoulder/LeftArm/LeftForeArm";
        public string playerWristLeftPath = "Visual/Armature/Hips/Spine/Spine1/Spine2/LeftShoulder/LeftArm/LeftForeArm/LeftHand";
        public string playerHandLeftPath = "Visual/Armature/Hips/Spine/Spine1/Spine2/LeftShoulder/LeftArm/LeftForeArm/LeftHand/LeftHand_Attach";

        public string playerShoulderRightPath = "Visual/Armature/Hips/Spine/Spine1/Spine2/RightShoulder/RightArm";
        public string playerElbowRightPath = "Visual/Armature/Hips/Spine/Spine1/Spine2/RightShoulder/RightArm/RightForeArm";
        public string playerWristRightPath = "Visual/Armature/Hips/Spine/Spine1/Spine2/RightShoulder/RightArm/RightForeArm/RightHand";
        public string playerHandRightPath = "Visual/Armature/Hips/Spine/Spine1/Spine2/RightShoulder/RightArm/RightForeArm/RightHand/RightHand_Attach";
  
        public string playerHipLeftPath = "Visual/Armature/Hips/LeftUpLeg";
        public string playerKneeLeftPath = "Visual/Armature/Hips/LeftUpLeg/LeftLeg";
        public string playerAnkleLeftPath = "Visual/Armature/Hips/LeftUpLeg/LeftLeg/LeftFoot";
        public string playerToesLeftPath = "Visual/Armature/Hips/LeftUpLeg/LeftLeg/LeftFoot/LeftToeBase";

        public string playerHipRightPath = "Visual/Armature/Hips/RightUpLeg";
        public string playerKneeRightPath = "Visual/Armature/Hips/RightUpLeg/RightLeg";
        public string playerAnkleRightPath = "Visual/Armature/Hips/RightUpLeg/RightLeg/RightFoot";
        public string playerToesRightPath = "Visual/Armature/Hips/RightUpLeg/RightLeg/RightFoot/RightToeBase";

        public GameObject player;
        public GameObject playerHead;
        public GameObject playerJaw;
        public GameObject playerJawEnd;
        public GameObject playerNeck;

        public GameObject playerSpine2;
        public GameObject playerSpine1;
        public GameObject playerSpine;
        public GameObject playerHips;

        public GameObject playerShoulderLeft;
        public GameObject playerElbowLeft;
        public GameObject playerWristLeft;
        public GameObject playerHandLeft;

        public GameObject playerShoulderRight;
        public GameObject playerElbowRight;
        public GameObject playerWristRight;
        public GameObject playerHandRight;

        public GameObject playerHipLeft;
        public GameObject playerKneeLeft;
        public GameObject playerAnkleLeft;
        public GameObject playerToesLeft;

        public GameObject playerHipRight;
        public GameObject playerKneeRight;
        public GameObject playerAnkleRight;
        public GameObject playerToesRight;

        public PlayerArmatureHelper(GameObject playerPrefab)
        {
            try
            {
                player = playerPrefab;

                playerHead = InitField(playerHeadPath);
                playerJaw = InitField(playerJawPath);
                playerJawEnd = InitField(playerJawEndPath);
                playerNeck = InitField(playerNeckPath);

                playerSpine2 = InitField(playerSpine2Path);
                playerSpine1 = InitField(playerSpine1Path);
                playerSpine = InitField(playerSpinePath);
                playerHips = InitField(playerHipsPath);

                playerShoulderLeft = InitField(playerShoulderLeftPath);
                playerElbowLeft = InitField(playerElbowLeftPath);
                playerWristLeft = InitField(playerWristLeftPath);
                playerHandLeft = InitField(playerHandLeftPath);

                playerShoulderRight = InitField(playerShoulderRightPath);
                playerElbowRight = InitField(playerElbowRightPath);
                playerWristRight = InitField(playerWristRightPath);
                playerHandRight = InitField(playerHandRightPath);

                playerHipLeft = InitField(playerHipLeftPath);
                playerKneeLeft = InitField(playerKneeLeftPath);
                playerAnkleLeft = InitField(playerAnkleLeftPath);
                playerToesLeft = InitField(playerToesLeftPath);

                playerHipRight = InitField(playerHipRightPath);
                playerKneeRight = InitField(playerKneeRightPath);
                playerAnkleRight = InitField(playerAnkleRightPath);
                playerToesRight = InitField(playerToesRightPath);

                //playerHead = playerPrefab.transform.Find(playerHeadPath).gameObject;
                //new GameObject(prefix).transform.SetParent(playerHead.transform);
                //playerJaw = playerPrefab.transform.Find(playerJawPath).gameObject;
                //new GameObject(prefix).transform.SetParent(playerJaw.transform);
                //playerJawEnd = playerPrefab.transform.Find(playerJawEndPath).gameObject;
                //new GameObject(prefix).transform.SetParent(playerJawEnd.transform);
                //playerNeck = playerPrefab.transform.Find(playerNeckPath).gameObject;
                //new GameObject(prefix).transform.SetParent(playerNeck.transform);

                //playerSpine2 = playerPrefab.transform.Find(playerSpine2Path).gameObject;
                //new GameObject(prefix).transform.SetParent(playerSpine2.transform);
                //playerSpine1 = playerPrefab.transform.Find(playerSpine1Path).gameObject;
                //new GameObject(prefix).transform.SetParent(playerSpine1.transform);
                //playerSpine = playerPrefab.transform.Find(playerSpinePath).gameObject;
                //new GameObject(prefix).transform.SetParent(playerSpine.transform);
                //playerHips = playerPrefab.transform.Find(playerHipsPath).gameObject;
                //new GameObject(prefix).transform.SetParent(playerHips.transform);

                //playerShoulderLeft = playerPrefab.transform.Find(playerShoulderLeftPath).gameObject;
                //new GameObject(prefix).transform.SetParent(playerShoulderLeft.transform);
                //playerElbowLeft = playerPrefab.transform.Find(playerElbowLeftPath).gameObject;
                //new GameObject(prefix).transform.SetParent(playerElbowLeft.transform);
                //playerWristLeft = playerPrefab.transform.Find(playerWristLeftPath).gameObject;
                //new GameObject(prefix).transform.SetParent(playerWristLeft.transform);
                //playerHandLeft = playerPrefab.transform.Find(playerHandLeftPath).gameObject;
                //new GameObject(prefix).transform.SetParent(playerHandLeft.transform);

                //playerShoulderRight = playerPrefab.transform.Find(playerShoulderRightPath).gameObject;
                //new GameObject(prefix).transform.SetParent(playerShoulderRight.transform);
                //playerElbowRight = playerPrefab.transform.Find(playerElbowRightPath).gameObject;
                //new GameObject(prefix).transform.SetParent(playerElbowRight.transform);
                //playerWristRight = playerPrefab.transform.Find(playerWristRightPath).gameObject;
                //new GameObject(prefix).transform.SetParent(playerWristRight.transform);
                //playerHandRight = playerPrefab.transform.Find(playerHandRightPath).gameObject;
                //new GameObject(prefix).transform.SetParent(playerHandRight.transform);

                //playerHipLeft = playerPrefab.transform.Find(playerHipLeftPath).gameObject;
                //new GameObject(prefix).transform.SetParent(playerHipLeft.transform);
                //playerKneeLeft = playerPrefab.transform.Find(playerKneeLeftPath).gameObject;
                //new GameObject(prefix).transform.SetParent(playerKneeLeft.transform);
                //playerAnkleLeft = playerPrefab.transform.Find(playerAnkleLeftPath).gameObject;
                //new GameObject(prefix).transform.SetParent(playerAnkleLeft.transform);
                //playerToesLeft = playerPrefab.transform.Find(playerToesLeftPath).gameObject;
                //new GameObject(prefix).transform.SetParent(playerToesLeft.transform);

                //playerHipRight = playerPrefab.transform.Find(playerHipRightPath).gameObject;
                //new GameObject(prefix).transform.SetParent(playerHipRight.transform);
                //playerKneeRight = playerPrefab.transform.Find(playerKneeRightPath).gameObject;
                //new GameObject(prefix).transform.SetParent(playerKneeRight.transform);
                //playerAnkleRight = playerPrefab.transform.Find(playerAnkleRightPath).gameObject;
                //new GameObject(prefix).transform.SetParent(playerAnkleRight.transform);
                //playerToesRight = playerPrefab.transform.Find(playerToesRightPath).gameObject;
                //new GameObject(prefix).transform.SetParent(playerToesRight.transform);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not initialise player aramture helper: " + error);
            }
        }

        public void AddPrefab(PlayerArmatureType type, GameObject prefab, AddPrefabToArmatureOptions options = null)
        {
            try
            {
                if (prefab == null)
                    throw new Exception("Prefab is null");

                GameObject gameObject = GetGameObject(type);

                if (gameObject == null)
                    throw new Exception("Could not find amature game object by type");

                Transform target = gameObject.transform;

                if (target == null)
                    throw new Exception("Could not find target");

                prefab.transform.SetParent(target);
                Jotunn.Logger.LogWarning("Added "+ prefab.name + " to "+ target.name);

                if (options != null)
                    prefab.transform.localPosition = options.position;

                if (options != null)
                    prefab.transform.localScale = options.scale;

                if (options != null && options.rotation != null)
                {
                    Quaternion rotation = new Quaternion(0, 0, 0, 0);
                    rotation.eulerAngles = options.rotation;
                    prefab.transform.localRotation = rotation;
                }

                if (options != null)
                    prefab.SetActive(options.active);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not add prefab to player armature: " + error);
            }
        }

        public void SetActive(PlayerArmatureType type, string path, bool active)
        {
            try
            {
                GameObject gameObject = GetGameObject(type);
                if (gameObject == null)
                    throw new Exception("Could not find game object of type");

                Transform target = gameObject.transform.Find(path);

                if (target == null)
                   throw new Exception("Could not find target");
                    
                target.gameObject.SetActive(active);
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not set active on player armature: " + error);
            }

        }

        public GameObject GetGameObject(PlayerArmatureType type)
        {
            try
            {
                switch (type)
                {
                    case PlayerArmatureType.HEAD:
                        return playerHead;
                    case PlayerArmatureType.JAW:
                        return playerJaw;
                    case PlayerArmatureType.JAWEND:
                        return playerJawEnd;
                    case PlayerArmatureType.NECK:
                        return playerNeck;
                    case PlayerArmatureType.SPINE2:
                        return playerSpine2;
                    case PlayerArmatureType.SPINE1:
                        return playerSpine1;
                    case PlayerArmatureType.SPINE:
                        return playerSpine;
                    case PlayerArmatureType.HIPS:
                        return playerHips;
                    case PlayerArmatureType.LEFTSHOULDER:
                        return playerShoulderLeft;
                    case PlayerArmatureType.LEFTELBOW:
                        return playerElbowLeft;
                    case PlayerArmatureType.LEFTWRIST:
                        return playerWristLeft;
                    case PlayerArmatureType.LEFTHAND:
                        return playerHandLeft;
                    case PlayerArmatureType.RIGHTSHOULDER:
                        return playerShoulderRight;
                    case PlayerArmatureType.RIGHTELBOW:
                        return playerElbowRight;
                    case PlayerArmatureType.RIGHTWRIST:
                        return playerWristRight;
                    case PlayerArmatureType.RIGHTHAND:
                        return playerHandRight;
                    case PlayerArmatureType.LEFTHIP:
                        return playerHipLeft;
                    case PlayerArmatureType.LEFTKNEE:
                        return playerKneeLeft;
                    case PlayerArmatureType.LEFTANKLE:
                        return playerAnkleLeft;
                    case PlayerArmatureType.LEFTTOES:
                        return playerToesLeft;
                    case PlayerArmatureType.RIGHTHIP:
                        return playerHipRight;
                    case PlayerArmatureType.RIGHTKNEE:
                        return playerKneeRight;
                    case PlayerArmatureType.RIGHTANKLE:
                        return playerAnkleRight;
                    case PlayerArmatureType.RIGHTTOES:
                        return playerToesRight;
                    default:
                        return null;
                }
            }
            catch (Exception error)
            {
                Jotunn.Logger.LogError("Could not get game object from player armature: " + error);
                return null;
            }
        }

        // TODO: AddPrefab gaat mis op nieuw ME_attach object
        public GameObject InitField(string path)
        {
            GameObject gameObject = player.transform.Find(path).gameObject;
            //GameObject attach = new GameObject();
            //Quaternion rotation = new Quaternion(0f, 0f, 0f, 0f);

            //rotation.eulerAngles = new Vector3(0f, 0f, 0f);
            //attach.name = prefix;
            //attach.transform.localPosition = new Vector3(0f, 0f, 0f);
            //attach.transform.localRotation = rotation;
            //attach.transform.localScale = new Vector3(1f, 1f, 1f);
            //attach.transform.SetParent(gameObject.transform);                       
            return gameObject;
        }
    }
}
