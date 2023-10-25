using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// using Shared;

public class MatchClient : TcpClient{
    public MatchClient() : base() {
        
    }
}

public class MatchConnection : MonoBehaviour
{
    public TMP_InputField AddressInput;
    public void ConnectToMatch() {
        var url = AddressInput.text;
        var split = url.Split(":");
        if (split.Length != 2) {
            throw new Exception("Incorrect URL format");
        }
        var address = split[0];
        var port = int.Parse(split[1]);

        var client = new MatchClient();
        client.Connect(address, port);

        // read configuration
        var preConfigRaw = NetUtil.Read(client.GetStream());
        var preConfig = JsonUtility.FromJson<MatchPreConfig>(preConfigRaw);

        client.ReceiveTimeout = 20;

        Global.Instance.PreConfig = preConfig;
        Global.Instance.Client = client;
        Global.Instance.Stream = client.GetStream();
        SceneManager.LoadScene("Match");
    }
}
