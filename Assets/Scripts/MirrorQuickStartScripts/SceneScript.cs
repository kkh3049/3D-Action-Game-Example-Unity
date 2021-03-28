using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneScript : NetworkBehaviour
{
    public Text canvasStatusText;

    public Valken playerScript;

    public SceneReference sceneReference;

    [SyncVar(hook = nameof(OnStatusTextChanged))]
    public string statusText;

    void OnStatusTextChanged(string _Old, string _New)
    {
        //called from sync var hook to update info on screen for all players
        canvasStatusText.text = statusText;
    }

    public void ButtonSendMessage()
    {
        if (playerScript != null)
        {
            playerScript.CmdSendPlayerMessage();
        }
    }

    public void ButtonChangeScene()
    {
        if (isServer)
        {
            var scene = SceneManager.GetActiveScene();
            NetworkManager.singleton.ServerChangeScene(scene.name == "MyScene" ? "MyOtherScene" : "MyScene");
        }
        else
        {
            Debug.Log("You are not Host.");
        }
    }
}