using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NickNameBarLifeManager : MonoBehaviour
{
    public static NickNameBarLifeManager Instance;
    [SerializeField] private NickNameBarLife _itemPrefab;

    void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    
    public NickNameBarLife CreateNewItem(NetworkPlayer networkPlayer)
    {
        var item = Instantiate(_itemPrefab);
        return item;
    }
}