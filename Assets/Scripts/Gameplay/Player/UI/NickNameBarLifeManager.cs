using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NickNameBarLifeManager : MonoBehaviour
{
    public static NickNameBarLifeManager Instance;

    private List<NickNameBarLife> _allItems = new();

    [SerializeField] NickNameBarLife _itemPrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public NickNameBarLife CreateNewItem(NetworkPlayer owner)
    {
        var newItem = Instantiate(_itemPrefab, transform);
        _allItems.Add(newItem);

        newItem.SetOwner(owner);

        owner.OnPlayerDespawned += () =>
        {
            _allItems.Remove(newItem);
            Destroy(newItem.gameObject);
        };

        return newItem;
    }

    private void LateUpdate()
    {
        foreach (var item in _allItems) item.UpdatePosition();
    }
}