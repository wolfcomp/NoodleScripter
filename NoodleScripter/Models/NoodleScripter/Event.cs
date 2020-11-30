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

        public EventData[] GenerateEvent()
        {
            EventCustomData customData;
            switch (Type)
            {
                case EventType.LightBackTopLasers:
                case EventType.LightTrackRingNeons:
                case EventType.LightLeftLasers:
                case EventType.LightRightLasers:
                case EventType.LightBottomBackSideLasers:
                case EventType.Lasers:
                case EventType.BackTopTrackRing:
                    customData = new EventCustomData
                    {
                        Color = Color.Copy(),
                        PropID = PropID.Copy(),
                        LightID = LightID.Copy()
                    };
                    break;
                case EventType.RotationAllTrackRings:
                case EventType.RotationSmallTrackRings:
                    customData = new RingCustomData
                    {
                        CounterSpin = CounterLock.Copy(),
                        Direction = Direction.Copy(),
                        NameFilter = Filter.Copy(),
                        PropMult = PropMult.Copy(),
                        Reset = Reset.Copy(),
                        SpeedMult = SpeedMult.Copy(),
                        StepMult = StepMult.Copy(),
                        Prop = Prop.Copy(),
                        Speed = Speed.Copy(),
                        Step = Step.Copy()
                    };
                    break;
                case EventType.RotatingLeftLasers:
                case EventType.RotatingRightLasers:
                case EventType.RotatingLasers:
                    customData = new LaserCustomData
                    {
                        Direction = Direction.Copy(),
                        LockPosition = CounterLock.Copy(),
                        PreciseSpeed = Speed.Copy()
                    };
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            if (Type == EventType.Lasers || Type == EventType.RotatingLasers || Type == EventType.BackTopTrackRing)
                return new[]{
                    new EventData
                    {
                        CustomData = customData,
                        Value = (int)Value,
                        Type = Type == EventType.Lasers ? 2 : Type == EventType.RotatingLasers ? 12 : 0,
                        Time = (float)StartTime
                    },
                    new EventData
                    {
                        CustomData = customData,
                        Value = (int)Value,
                        Type = Type == EventType.Lasers ? 3 :  Type == EventType.RotatingLasers ? 13 : 1,
                        Time = (float)StartTime
                    },
                };
            else
                return new[]{
                    new EventData
                    {
                        CustomData = customData,
                        Value = (int)Value,
                        Type = (int)Type,
                        Time = (float)StartTime
                    }
                };
        }
    }

    public enum EventType
    {
        LightBackTopLasers = 0,
        LightTrackRingNeons = 1,
        LightLeftLasers = 2,
        LightRightLasers = 3,
        LightBottomBackSideLasers = 4,
        RotationAllTrackRings = 8,
        RotationSmallTrackRings = 9,
        RotatingLeftLasers = 12,
        RotatingRightLasers = 13,
        RotatingLasers = 20,
        Lasers = 21,
        BackTopTrackRing = 22,
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
