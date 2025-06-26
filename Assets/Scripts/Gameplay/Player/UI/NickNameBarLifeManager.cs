using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NickNameBarLifeManager : MonoBehaviour
{
    public static NickNameBarLifeManager Instance;

    [SerializeField] private NickNameBarLife _itemPrefab;

    void Awake()
    {
        Instance = this;
    }
    
    public NickNameBarLife CreateNewItem(NetworkPlayer networkPlayer)
    {
        var item = Instantiate(_itemPrefab);
        return item;
    }
}