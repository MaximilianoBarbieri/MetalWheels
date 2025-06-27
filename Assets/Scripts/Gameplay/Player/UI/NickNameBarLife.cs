using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NickNameBarLife : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI nicknameText;
    [SerializeField] public Image healthBar;

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

        healthBar.fillAmount = playerModel.MaxHealth;
    }

    void Update()
    {
        if (target == null || playerModel == null) return;

        transform.position = target.position + Vector3.up * 2f; // ajuste vertical
        if (_camera != null) transform.LookAt(_camera.transform);
        healthBar.fillAmount = playerModel.CurrentHealth / playerModel.MaxHealth;
    }

    public void UpdateNickName(string newNick)
    {
        nicknameText.text = newNick;
    }
}