using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MatchController : MonoBehaviour
{
    #region Templates
    
    public GameObject CardTemplate;
    public GameObject PlayerDisplayTemplate;
    
    #endregion

    public GameObject handDisplay;
    public GameObject playersDisplay;
    public TMP_InputField ResponseInput;

    private PlayerDisplay[] _playerDisplays;
    private bool _isRunning = false;
    private MatchState _state;

    void Start()
    {
        // populate player displays
        _populatePlayerDisplays();

        _isRunning = true;
        StartCoroutine(FetchStates());
    }

    private void _populatePlayerDisplays() {
        var preConfig = Global.Instance.PreConfig;
        var pCount = preConfig.playerCount;
        _playerDisplays = new PlayerDisplay[pCount];
        for (int i = 0; i < pCount; i++) {
            var pDisplay = Instantiate(PlayerDisplayTemplate);
            pDisplay.transform.SetParent(playersDisplay.transform);

            _playerDisplays[i] = pDisplay.GetComponent<PlayerDisplay>();
            _playerDisplays[i].playerI = (i + preConfig.myI + 1) % pCount;
        }
    }


    #region State loaders

    private void _loadState(MatchState state) {
        _state = state;
        _updatePlayerData();

        // if (state.request == "action")
            // _respond("pass");
            // print("action");
    }

    private void _updatePlayerData() {
        foreach (var pd in _playerDisplays) {
            pd.Load(_state);
        }
    }

    #endregion

    public void SendResponseInInput() {
        _respond(ResponseInput.text);
    }

    private void _respond(string response) {
        // StartCoroutine(_waitRespond(response));
        NetUtil.Write(Global.Instance.Stream, response);
    }

    IEnumerator _waitRespond(string response) {
        yield return new WaitForSeconds(1);
        NetUtil.Write(Global.Instance.Stream, response);
    }

    IEnumerator FetchStates() {
        while(_isRunning) {
            try {
                
                var message = NetUtil.Read(Global.Instance.Stream);
                var state = MatchState.FromJson(message);
                _loadState(state);

            } catch (IOException) {}

            yield return null;
        }
    }
}
