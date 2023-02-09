using static BotEngine;
using System;
using System.Linq;

namespace Sanderling
{
    public struct Motion
    {
        public Vektor2DInt mousePosition;
        private MouseButtonIdEnum[] mouseButtonDown;
        private bool windowToForeground;
        private MouseButtonIdEnum[] mouseButtonUp;

        public Motion(Vektor2DInt mousePosition, MouseButtonIdEnum[]? mouseButtonDown, MouseButtonIdEnum[]? mouseButtonUp, bool windowToForeground)
        {
            this.mousePosition = mousePosition;
            this.mouseButtonDown = mouseButtonDown;
            this.mouseButtonUp = mouseButtonUp;
            this.windowToForeground = windowToForeground;
        }
    }
    public class MotionResult
    {
        public Int64 MotionRecommendationId;

        public bool Success;
    }
}