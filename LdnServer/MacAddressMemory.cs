using LanPlayServer.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace LanPlayServer
{
    public class MacAddressMemory
    {
        private HashSet<string> _reservedAddresses = new();
        private ConcurrentDictionary<string, Array6<byte>> _idToAddress = new();
        private Random _random = new();
        private object _lock = new();

        private Array6<byte> GetNewMac()
        {
            Array6<byte> mac = new();
            string stringMac;

            lock (_lock)
            {
                do
                {
                    _random.NextBytes(mac.AsSpan());

                    stringMac = Convert.ToHexString(mac.AsSpan());
                }
                while (_reservedAddresses.Contains(stringMac));

                _reservedAddresses.Add(stringMac);
            }

            return mac;
        }

        public Array6<byte> TryFind(string id, Span<byte> macAddress, string newId)
        {
            Array6<byte> result;

            if (!_idToAddress.TryGetValue(id, out result) || !result.AsSpan().SequenceEqual(macAddress))
            {
                result = GetNewMac();
            }

            _idToAddress.TryAdd(newId, result);

            return result;
        }
    }
}