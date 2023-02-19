using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Ryujinx.Common.Memory;

namespace LanPlayServer
{
    class MacAddressMemory
    {
        private HashSet<string> _reservedAddresses = new HashSet<string>();
        private ConcurrentDictionary<string, Array6<byte>> _idToAddress = new ConcurrentDictionary<string, Array6<byte>>();
        private Random _random = new Random();
        private object _lock = new object();

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
