using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LobbyUI : MonoBehaviour
{
    [Header("NetworkRunnerHandler")]
    [SerializeField] private NetworkRunnerHandler _networkHandler;

    [Header("Panels")]
    [SerializeField] private GameObject _initialPanel;
    [SerializeField] private GameObject _sessionPanel;
    [SerializeField] private GameObject _hostPanel;
    [SerializeField] private GameObject _statusPanel;
    
    [Header("Texts")]
    [SerializeField] private TMP_Text _statusText;
    
    [Header("InputFields")]
    [SerializeField] private TMP_InputField _nicknameInput;
    [SerializeField] private TMP_InputField _hostSessionNameInput;
    
    [Header("Buttons")]
    [SerializeField] private Button _carA;
    [SerializeField] private Button _carB;
    [SerializeField] private Button _backBTN; //botone de volver hacia atras.
    [SerializeField] private Button _joinLobbyBTN; //me lleva al panel con lista de salas
    [SerializeField] private Button _goToHostPanelBTN;//me lleva al panel para ponerle nombre a mi sala
    [SerializeField] private Button _hostBTN;//btn para terminar de crear una sala (donde soy host)
    
    [Header("Car Button Colors")]
    [Tooltip("Color de fondo cuando NO está seleccionado")]
    [SerializeField] private Color _normalColor   = Color.white;
    [Tooltip("Color de fondo cuando SÍ está seleccionado")]
    [SerializeField] private Color _selectedColor = Color.grey;
    
    // referencias a los Image de cada botón
    private Image _carAImage;
    private Image _carBImage;

    private void Awake()
    {
        // agarramos la Image que está en el mismo GameObject del Button
        _carAImage = _carA.GetComponent<Image>();
        _carBImage = _carB.GetComponent<Image>();
    }
    
    private void Start()
    {
        _joinLobbyBTN.onClick.AddListener(Btn_JoinLobby);
        _goToHostPanelBTN.onClick.AddListener(Btn_ShowHostPanel);
        _hostBTN.onClick.AddListener(Btn_CreateGameSession);

        _networkHandler.OnJoinedLobby += () =>
        {
            _statusPanel.SetActive(false);
            _sessionPanel.SetActive(true);
        };
        
        // Inicial: ningún coche seleccionado
        PlayerData.CarSelected = -1;
        UpdateCarButtonVisuals();
        ValidateJoinLobbyButton();
        
        _carA.onClick.AddListener(() =>
        {
            PlayerData.CarSelected = 0;
            UpdateCarButtonVisuals();
            ValidateJoinLobbyButton();
        });
        _carB.onClick.AddListener(() =>
        {
            PlayerData.CarSelected = 1;
            ValidateJoinLobbyButton();
            UpdateCarButtonVisuals();
        });
        
        // Cada vez que cambies el nickname, re‑validamos
        _nicknameInput.onValueChanged.AddListener(_ => ValidateJoinLobbyButton());
        
        // Listener final de Join Lobby
        _joinLobbyBTN.onClick.AddListener(() =>
        {
            // guardo nickname y lanzo la unión
            PlayerData.Nickname = _nicknameInput.text;
            _networkHandler.JoinLobby();
            // ... resto de tu lógica de mostrar paneles
        });
        
        _backBTN.onClick.AddListener(Btn_Back);
    }
    
    private void UpdateCarButtonVisuals()
    {
        _carAImage.color = (PlayerData.CarSelected == 0) ? _selectedColor : _normalColor;
        _carBImage.color = (PlayerData.CarSelected == 1) ? _selectedColor : _normalColor;
    }
    
    private void ValidateJoinLobbyButton()
    {
        bool carSelected = PlayerData.CarSelected == 0 || PlayerData.CarSelected == 1;
        bool nickValid   = !string.IsNullOrWhiteSpace(_nicknameInput.text);
        _joinLobbyBTN.interactable = carSelected && nickValid;
    }
    
    void Btn_JoinLobby()
    {
        _networkHandler.JoinLobby();
        
        PlayerData.Nickname = _nicknameInput.text;
        
        ShowPanel(_statusPanel);
        _statusText.text = "Joining Lobby...";
    }
    
    void Btn_ShowHostPanel()
    {
        _sessionPanel.SetActive(false);
        _hostPanel.SetActive(true);
    }
    
    void Btn_CreateGameSession()
    {
        _hostBTN.interactable = false;
        _networkHandler.CreateGame(_hostSessionNameInput.text, "Gameplay");
    }
    
    private void Btn_Back()
    {
        SceneManager.LoadScene("MainMenu");
        //TODO: hacer algo parecido a esto que tenia
        //ir del panel actual al anterior, en caso de no haber mas paneles volver a la scene "MainMenu"
        //tener en cuenta que hay un panel que crea una sala, si se vuelve atras en ese panel borrar la sala creada (de ser necesario)
        /*if (roomPanel.activeSelf)
        {
            ShowPanel(selectCarPanel);
        }
        else if (selectCarPanel.activeSelf)
        {
            ShowPanel(nicknamePanel);
        }
        else if (nicknamePanel.activeSelf)
        {
            SceneManager.LoadScene("MainMenu");
        }*/
    }
    
    private void ShowPanel(GameObject panelToShow)
    {
        _initialPanel.SetActive(false);
        _sessionPanel.SetActive(false);
        _hostPanel.SetActive(false);
        _statusPanel.SetActive(false);

        panelToShow.SetActive(true);
    }
}