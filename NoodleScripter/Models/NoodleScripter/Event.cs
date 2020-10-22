using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoodleScripter.Models.BeatSaber;

namespace NoodleScripter.Models.NoodleScripter
{
    public class Event
    {
        public double StartTime { get; set; }
        public EventType Type { get; set; }
        public Color? Color { get; set; }
        public LightType Value { get; set; }
        public int? Direction { get; set; }
        public bool? CounterLock { get; set; }
        public float? Speed { get; set; }
        public float? Step { get; set; }
        public float? Prop { get; set; }
        public float? SpeedMult { get; set; }
        public float? StepMult { get; set; }
        public float? PropMult { get; set; }
        public string Filter { get; set; }
        public bool? Reset { get; set; }
        public int? PropID { get; set; }
        public int? LightID { get; set; }

        public EventData GenerateEvent()
        {
            EventCustomData customData;
            switch (Type)
            {
                case EventType.LightBackTopLasers:
                case EventType.LightTrackRingNeons:
                case EventType.LightLeftLasers:
                case EventType.LightRightLasers:
                case EventType.LightBottomBackSideLasers:
                    customData = new EventCustomData
                    {
                        Color = Color,
                        PropID = PropID,
                        LightID = LightID
                    };
                    break;
                case EventType.RotationAllTrackRings:
                case EventType.RotationSmallTrackRings:
                    customData = new RingCustomData
                    {
                        CounterSpin = CounterLock,
                        Direction = Direction,
                        NameFilter = Filter,
                        PropMult = PropMult,
                        Reset = Reset,
                        SpeedMult = SpeedMult,
                        StepMult = StepMult,
                        Prop = Prop,
                        Speed = Speed,
                        Step = Step
                    };
                    break;
                case EventType.RotatingLeftLasers:
                case EventType.RotatingRightLasers:
                    customData = new LaserCustomData
                    {
                        Direction = Direction,
                        LockPosition = CounterLock,
                        PreciseSpeed = Speed
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return new EventData
            {
                CustomData = customData,
                Value = (int)Value,
                Type = (int)Type,
                Time = (float)StartTime
            };
        }
    }

    public enum EventType
    {
        LightBackTopLasers=0,
        LightTrackRingNeons=1,
        LightLeftLasers=2,
        LightRightLasers=3,
        LightBottomBackSideLasers=4,
        RotationAllTrackRings=8,
        RotationSmallTrackRings=9,
        RotatingLeftLasers=12,
        RotatingRightLasers=13,
    }

    public enum LightType
    {
        Off,
        BlueOn,
        BlueFlash,
        BlueFade,
        RedOn = 5,
        RedFlash = 6,
        RedFade = 7
    }
}
