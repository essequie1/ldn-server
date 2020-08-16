using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace LanPlayServer
{
    class RateLimiterBucket
    {
        public int      Count;
        public DateTime StartTime;
    }

    static class RateLimiter
    {
        private static uint   _rateLimit;
        private static double _rateLimitTime;

        private static readonly Dictionary<IPAddress, RateLimiterBucket> _rateLimiterBucket = new Dictionary<IPAddress, RateLimiterBucket>();

        public static void SetRateLimit(uint rateLimit, double rateLimitTime)
        {
            _rateLimit     = rateLimit;
            _rateLimitTime = rateLimitTime;
        }

        public static bool IsRateLimited(IPAddress ipAddress)
        {
            if (!_rateLimiterBucket.ContainsKey(ipAddress))
            {
                _rateLimiterBucket[ipAddress] = new RateLimiterBucket()
                {
                    Count     = 1,
                    StartTime = DateTime.Now
                };

                return false;
            }
            else
            {
                TimeSpan elapsedTime = DateTime.Now - _rateLimiterBucket[ipAddress].StartTime;

                if (elapsedTime >= TimeSpan.FromMilliseconds(_rateLimitTime))
                {
                    _rateLimiterBucket[ipAddress].Count     = 1;
                    _rateLimiterBucket[ipAddress].StartTime = DateTime.Now;
                }
                else
                {
                    if (_rateLimiterBucket[ipAddress].Count >= _rateLimit)
                    {
                        return true;
                    }
                    else
                    {
                        _rateLimiterBucket[ipAddress].Count++;
                    }
                }
            }

            return false;
        }
    }
}