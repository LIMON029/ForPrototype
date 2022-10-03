using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _singleton;
    public static GameManager Singleton
    {
        get => _singleton;
        private set
        {
            if (_singleton == null)
                _singleton = value;
            else if (_singleton != value)
            {
                Debug.Log($"{nameof(GameManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Singleton = this;
    }

    public GameObject PlayerPrefab => playerPrefab;

    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;
}
