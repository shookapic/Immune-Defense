using System;
using Unity.Collections;
using Unity.Netcode;

public struct PlayerCardData : INetworkSerializable, IEquatable<PlayerCardData>
{
    public ulong ClientId;
    public FixedString32Bytes Name;
    public int ColorIndex;
    public short PingMs;
    public void NetworkSerialize<T>(BufferSerializer<T> s) where T : IReaderWriter
    {
        s.SerializeValue(ref ClientId);
        s.SerializeValue(ref Name);
        s.SerializeValue(ref ColorIndex);
        s.SerializeValue(ref PingMs);
    }

    public bool Equals(PlayerCardData other)
    {
        return ClientId == other.ClientId;
    }
}
