using System;

namespace BeatKeeper
{
    /// <summary>
    /// タイミングを表す構造体
    /// </summary>
    public readonly struct TimingKey : IEquatable<TimingKey>
    {
        /// <summary>小節</summary>
        public readonly int Bar;
        
        /// <summary>拍</summary>
        public readonly int Beat;
        
        /// <summary>16分音符単位</summary>
        public readonly int Unit;
        
        public TimingKey(int bar, int beat, int unit)
        {
            Bar = bar;
            Beat = beat;
            Unit = unit;
        }
        
        public override bool Equals(object obj) => 
            obj is TimingKey other && Equals(other);

        public bool Equals(TimingKey other) => 
            Bar == other.Bar && Beat == other.Beat && Unit == other.Unit;

        public override int GetHashCode() => 
            HashCode.Combine(Bar, Beat, Unit);
            
        public static bool operator ==(TimingKey left, TimingKey right) => 
            left.Equals(right);

        public static bool operator !=(TimingKey left, TimingKey right) => 
            !left.Equals(right);
            
        public override string ToString() => 
            $"Bar:{Bar}, Beat:{Beat}, Unit:{Unit}";
    }
}
