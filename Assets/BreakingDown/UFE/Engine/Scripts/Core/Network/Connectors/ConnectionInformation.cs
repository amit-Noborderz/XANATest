using UnityEngine;
using System.Collections;
using System;
//using UnityEngine.Networking.Match;
//using UnityEngine.Networking.Types;

public class ConnectionInformation{
	public int port = 0;
	public string publicAddress = null;
	public string privateAddress = null;
	public NodeID unityNodeId = NodeID.Invalid;

	public ConnectionInformation(){}

	public ConnectionInformation(string address, int port) : this(address, address, port){}

	public ConnectionInformation(string privateAddress, string publicAddress, int port) : 
	this (privateAddress, publicAddress, port, NodeID.Invalid){}

	public ConnectionInformation(
		string privateAddress, 
		string publicAddress, 
		int port, 
		NodeID unityNodeId
	){
		this.privateAddress = privateAddress;
		this.publicAddress = publicAddress;
		this.port = port;
		this.unityNodeId = unityNodeId;
	}

#if !UNITY_2019_1_OR_NEWER
    public ConnectionInformation(MatchInfoSnapshot.MatchInfoDirectConnectSnapshot info) : this(
		info.privateAddress,
		info.publicAddress,
		UFE.config.networkOptions.port,
		info.nodeId
	){}
#endif
}

public struct NodeID
{
    public ushort Value;

    // Define an Invalid value that can be used in your project as a replacement for NodeID.Invalid
    public static readonly NodeID Invalid = new NodeID { Value = ushort.MaxValue };

    // Optional: Override ToString() for better debugging experience
    public override string ToString()
    {
        return Value == ushort.MaxValue ? "Invalid" : Value.ToString();
    }

    // Optional: Add Equals method to compare NodeID values easily
    public override bool Equals(object obj)
    {
        if (obj is NodeID other)
        {
            return Value == other.Value;
        }
        return false;
    }

    // Optional: Add GetHashCode method for dictionary support
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    // Optional: Add equality operators for easy comparison
    public static bool operator ==(NodeID left, NodeID right)
    {
        return left.Value == right.Value;
    }

    public static bool operator !=(NodeID left, NodeID right)
    {
        return !(left == right);
    }




    //public static NodeID Invalid = new NodeID(0);
    //public static NodeID Server = new NodeID(1);
    //public static NodeID Local = new NodeID(2);

    //public NodeID(ulong value)
    //{
    //    this.value = value;
    //}

    //public ulong value;

    //public override bool Equals(object obj)
    //{
    //    if (obj is NodeID)
    //    {
    //        return ((NodeID)obj).value == value;
    //    }
    //    return false;
    //}

    //public override int GetHashCode()
    //{
    //    return value.GetHashCode();
    //}

    //public static bool operator ==(NodeID x, NodeID y)
    //{
    //    return x.value == y.value;
    //}

    //public static bool operator !=(NodeID x, NodeID y)
    //{
    //    return x.value != y.value;
    //}

    //public static implicit operator NodeID(ulong value)
    //{
    //    return new NodeID(value);
    //}

    //public static implicit operator ulong(NodeID id)
    //{
    //    return id.value;
    //}

}

public struct NetworkID
{
    public ulong Value;

    // Define an Invalid value that can be used in your project as a replacement for NetworkID.Invalid
    public static readonly NetworkID Invalid = new NetworkID { Value = ulong.MaxValue };

    // Optional: Override ToString() for better debugging experience
    public override string ToString()
    {
        return Value == ulong.MaxValue ? "Invalid" : Value.ToString();
    }

    // Optional: Add Equals method to compare NetworkID values easily
    public override bool Equals(object obj)
    {
        if (obj is NetworkID other)
        {
            return Value == other.Value;
        }
        return false;
    }

    // Optional: Add GetHashCode method for dictionary support
    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    // Optional: Add equality operators for easy comparison
    public static bool operator ==(NetworkID left, NetworkID right)
    {
        return left.Value == right.Value;
    }

    public static bool operator !=(NetworkID left, NetworkID right)
    {
        return !(left == right);
    }




    //public NetworkID(ulong value)
    //{
    //    this.Value = value;
    //}

    //public ulong Value;

    //public override bool Equals(object obj)
    //{
    //    if (obj is NetworkID)
    //    {
    //        return ((NetworkID)obj).Value == Value;
    //    }
    //    return false;
    //}

    //public override int GetHashCode()
    //{
    //    return Value.GetHashCode();
    //}

    //public static bool operator ==(NetworkID x, NetworkID y)
    //{
    //    return x.Value == y.Value;
    //}

    //public static bool operator !=(NetworkID x, NetworkID y)
    //{
    //    return x.Value != y.Value;
    //}

    //public static implicit operator NetworkID(ulong value)
    //{
    //    return new NetworkID(value);
    //}

    //public static implicit operator ulong(NetworkID id)
    //{
    //    return id.Value;
    //}
}

public struct NetworkAccessToken
{
    public byte[] Value;

    // Define an Invalid token that can be used in place of an uninitialized or invalid token
    public static readonly NetworkAccessToken Invalid = new NetworkAccessToken { Value = Array.Empty<byte>() };

    // Constructor to create a NetworkAccessToken from a byte array
    public NetworkAccessToken(byte[] value)
    {
        Value = value ?? Array.Empty<byte>();
    }

    // Optional: Create a constructor from a string, useful for encoding tokens as strings
    public NetworkAccessToken(string value)
    {
        Value = string.IsNullOrEmpty(value) ? Array.Empty<byte>() : System.Text.Encoding.UTF8.GetBytes(value);
    }

    // Optional: Method to get the token as a base64 string for easier logging or transmission
    public string ToBase64String()
    {
        return Value.Length == 0 ? "Invalid" : Convert.ToBase64String(Value);
    }

    // Override ToString() for debugging
    public override string ToString()
    {
        return Value.Length == 0 ? "Invalid" : BitConverter.ToString(Value);
    }

    // Optional: Add Equals method to compare NetworkAccessToken values
    public override bool Equals(object obj)
    {
        if (obj is NetworkAccessToken other)
        {
            if (Value == null && other.Value == null) return true;
            if (Value == null || other.Value == null) return false;

            if (Value.Length != other.Value.Length) return false;
            for (int i = 0; i < Value.Length; i++)
            {
                if (Value[i] != other.Value[i])
                {
                    return false;
                }
            }
            return true;
        }
        return false;
    }

    // Optional: Add GetHashCode method for dictionary support
    public override int GetHashCode()
    {
        return Value != null ? BitConverter.ToString(Value).GetHashCode() : 0;
    }

    internal string GetByteString()
    {
        return BitConverter.ToString(Value);
    }

    // Optional: Add equality operators for easy comparison
    public static bool operator ==(NetworkAccessToken left, NetworkAccessToken right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(NetworkAccessToken left, NetworkAccessToken right)
    {
        return !(left == right);
    }





    //public byte[] Value;

    //// Define an Invalid value that can be used in your project as a replacement for NetworkAccessToken.Invalid
    //public static readonly NetworkAccessToken Invalid = new NetworkAccessToken { Value = null };

    //// Optional: Override ToString() for better debugging experience
    //public override string ToString()
    //{
    //    return Value == null ? "Invalid" : Value.ToString();
    //}

    //// Optional: Add Equals method to compare NetworkAccessToken values easily
    //public override bool Equals(object obj)
    //{
    //    if (obj is NetworkAccessToken other)
    //    {
    //        return Value == other.Value;
    //    }
    //    return false;
    //}

    //// Optional: Add GetHashCode method for dictionary support
    //public override int GetHashCode()
    //{
    //    return Value.GetHashCode();
    //}

    //// Optional: Add equality operators for easy comparison
    //public static bool operator ==(NetworkAccessToken left, NetworkAccessToken right)
    //{
    //    return left.Value == right.Value;
    //}

    //public static bool operator !=(NetworkAccessToken left, NetworkAccessToken right)
    //{
    //    return !(left == right);
    //}
}