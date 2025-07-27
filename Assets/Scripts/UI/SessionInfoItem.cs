using System;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionInfoItem : MonoBehaviour
{
    [SerializeField] private TMP_Text _sessionNameText;
    [SerializeField] private TMP_Text _playersCountText;
    [SerializeField] private Button _joinButton;

    private SessionInfo _sessionInfo;

    public event Action<SessionInfo> OnJoinSession;

    private void Awake()
    {
        _joinButton.onClick.AddListener(OnClick);
    }

    public void SetSessionInfo(SessionInfo sessionInfo)
    {
        _sessionInfo = sessionInfo;

        _sessionNameText.text = _sessionInfo.Name;
        _playersCountText.text = $"{_sessionInfo.PlayerCount}/{_sessionInfo.MaxPlayers}";

        _joinButton.enabled = _sessionInfo.PlayerCount < _sessionInfo.MaxPlayers;
    }

    void OnClick()
    {
        OnJoinSession?.Invoke(_sessionInfo);
    }
}
