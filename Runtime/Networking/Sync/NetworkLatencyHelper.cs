using UnityEngine;

#if MIRROR
using Mirror;
#endif

namespace SteamCore.Networking
{
    public static class NetworkLatencyHelper
    {
        public static double ServerTime
        {
            get
            {
#if MIRROR
                return NetworkTime.time;
#else
                return Time.timeAsDouble;
#endif
            }
        }

        public static double RTT
        {
            get
            {
#if MIRROR
                return NetworkTime.rtt;
#else
                return 0;
#endif
            }
        }

        public static double GetRenderTime(float interpolationDelay = 0.1f)
        {
            return ServerTime - interpolationDelay;
        }

        public static Vector3 InterpolatePosition(
            Vector3 from, Vector3 to,
            double fromTime, double toTime,
            double renderTime)
        {
            if (toTime <= fromTime) return to;
            var t = (float)((renderTime - fromTime) / (toTime - fromTime));
            return Vector3.LerpUnclamped(from, to, t);
        }

        public static Quaternion InterpolateRotation(
            Quaternion from, Quaternion to,
            double fromTime, double toTime,
            double renderTime)
        {
            if (toTime <= fromTime) return to;
            var t = (float)((renderTime - fromTime) / (toTime - fromTime));
            return Quaternion.SlerpUnclamped(from, to, t);
        }

        public static bool IsWithinRange(Vector3 a, Vector3 b, float maxDistance)
        {
            return (a - b).sqrMagnitude <= maxDistance * maxDistance;
        }
    }
}
