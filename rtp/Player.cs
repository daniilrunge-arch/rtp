using System;

namespace RTPPlugin
{
    public class PlayerCooldown
    {
        public int Index { get; }
        public DateTime LastUsed { get; set; } = DateTime.MinValue;

        public PlayerCooldown(int index)
        {
            Index = index;
        }
    }
}