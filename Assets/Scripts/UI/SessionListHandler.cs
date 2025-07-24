using System.Collections.Generic;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionListHandler : MonoBehaviour
{
    [SerializeField] private SessionInfoItem _sessionItemPrefab;
    
    [SerializeField] private NetworkRunnerHandler _runnerHandler;

    [SerializeField] private TMP_Text _statusText;

    [SerializeField] private VerticalLayoutGroup _verticalLayoutGroup;

    private void OnEnable()
    {
        _runnerHandler.OnSessionListUpdate += ReceiveSessionList;
    }

    private void OnDisable()
    {
        _runnerHandler.OnSessionListUpdate -= ReceiveSessionList;
    }

    void ClearBrowser()
    {
        foreach (Transform child in _verticalLayoutGroup.transform)
        {
            Destroy(child.gameObject);
        }
        
        _statusText.gameObject.SetActive(false);
    }
    
    void ReceiveSessionList(List<SessionInfo> sessions)
    {
        ClearBrowser();
        
        if (sessions.Count == 0)
        {
            NoSessionsFound();
            return;
        }

        foreach (var session in sessions)
        {
            AddToSessionBrowser(session);
        }
    }

    void NoSessionsFound()
    {
        _statusText.text = "No sessions found";
        _statusText.gameObject.SetActive(true);
    }

    void AddToSessionBrowser(SessionInfo sessionInfo)
    {
        var newItem = Instantiate(_sessionItemPrefab, _verticalLayoutGroup.transform);
        newItem.SetSessionInfo(sessionInfo);
        newItem.OnJoinSession += JoinSelectedSession;
    }

    void JoinSelectedSession(SessionInfo session)
    {
        _runnerHandler.JoinGame(session);
    }
}
