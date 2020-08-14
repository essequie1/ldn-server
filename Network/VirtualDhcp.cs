using System;
using System.Collections.Generic;
using System.Text;

namespace LanPlayServer
{
    /// <summary>
    /// Try to give clients unique virtual IPs that haven't been used before.
    /// </summary>
    class VirtualDhcp
    {
        private object _lock = new object();

        private uint _nextIp;

        private HashSet<uint> TakenIps = new HashSet<uint>();

        private uint _baseAddress;
        private uint _subnetMask;
        private uint _invSubnetMask;

        public uint BroadcastAddress => _baseAddress | _invSubnetMask;

        public VirtualDhcp(uint baseAddress, uint subnetMask)
        {
            _baseAddress = baseAddress;
            _subnetMask = subnetMask;
            _invSubnetMask = ~subnetMask;

            _nextIp = baseAddress + 1;
        }

        private bool IsIpValid(uint ip)
        {
            return ip != _baseAddress && ip != (_baseAddress | _invSubnetMask);
        }

        private void CycleNextIp()
        {
            do
            {
                _nextIp = _baseAddress | ((_nextIp + 1) & _invSubnetMask);
            }
            while (!IsIpValid(_nextIp));
        }

        public uint RequestIpV4()
        {
            lock (_lock)
            {
                while (TakenIps.Contains(_nextIp))
                {
                    CycleNextIp();
                }

                uint result = _nextIp;
                TakenIps.Add(result);

                CycleNextIp();

                return result;
            }
        }

        public void ReturnIpV4(uint ip)
        {
            lock (_lock)
            {
                TakenIps.Remove(ip);
            }
        }
    }
}
