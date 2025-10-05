using PurrNet;
using UnityEngine;

public class NetworkEndpoints : NetworkIdentity
{
    [SerializeField] private NetworkIdentity _identity;

    protected override void OnSpawned()
    {
        base.OnSpawned();

        if (!isServer)
            return;

        Instantiate(_identity, Vector2.zero, Quaternion.identity);
    }
}
