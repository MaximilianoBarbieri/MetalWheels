using UnityEngine;

public class PlayerGlobalUIManager : MonoBehaviour
{
    public static PlayerGlobalUIManager Instance;
    [SerializeField] private PlayerGlobalUIHandler _itemPrefab;

    void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    public PlayerGlobalUIHandler CreateNewItem()
    {
        var item = Instantiate(_itemPrefab);
        return item;
    }
}