using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NickNameBarLife : MonoBehaviour
{
    public TextMeshProUGUI nicknameText;
    public Slider healthBar;

    private Transform target;
    private ModelPlayer playerModel;
    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
    }

    public void Init(string nickname, Transform playerTransform, ModelPlayer model)
    {
        nicknameText.text = nickname;
        target = playerTransform;
        playerModel = model;

        healthBar.maxValue = playerModel.MaxHealth;
    }

    void Update()
    {
        if (target == null || playerModel == null) return;

        transform.position = target.position + Vector3.up * 2f; // ajuste vertical
        if (_camera != null) transform.LookAt(_camera.transform);
        healthBar.value = playerModel.CurrentHealth;
    }

    public void UpdateNickName(string newNick)
    {
        nicknameText.text = newNick;
    }
}