using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoodleScripter.Models.NoodleScripter
{
    public class Strober : EventGenerator
    {
        public bool RandomPropId { get; set; }
        public bool RandomLightId { get; set; }
        public bool Alternate { get; set; }
        public bool Toggle { get; set; }
        public bool CheckAll { get; set; }
        public int StartPropId { get; set; }
        public int EndPropId { get; set; }
        public int StartLightId { get; set; }
        public int EndLightId { get; set; }
        public double? ToggleDuration { get; set; }
        public int Count { get; set; } = 1;
        public float? StepTime { get; set; }

        public override IEnumerable<Event> GenerateEvents()
        {
            var stepTime = StepTime ?? Duration / Amount;
            var ret = new List<Event>();
            var alter = false;
            var toggle = false;
            Event[] prevEvents = new Event[Count];
            Event[] prevStoredEvents = new Event[Count];
            ToggleDuration -= stepTime - ToggleDuration;
            for (int i = 0; i < Amount; i++)
            {
                for (var k = 0; k < (Toggle ? 2 : 1); k++)
                {
                    for (var i1 = 0; i1 < Count; i1++)
                    {
                        var @event = new Event
                        {
                            Value = alter && Alternate ? getAlternateType() : Value,
                            StartTime = StartTime + ((Toggle && toggle && ToggleDuration.HasValue) ? (stepTime * i + ToggleDuration.Value) : stepTime * i),
                            Type = Type,
                            Prop = Prop,
                            Speed = Speed,
                            Step = Step,
                            PropMult = PropMult,
                            SpeedMult = SpeedMult,
                            StepMult = StepMult,
                            Direction = Direction,
                            CounterLock = CounterLock,
                            Filter = Filter,
                            Reset = Reset,
                            LightID = LightID,
                            PropID = PropID
                        };
                        if (RandomPropId && !(Toggle && toggle))
                        {
                            @event.PropID = Random.Next(StartPropId, EndPropId + 1);
                            while (prevEvents.Any(t => t != null && t.PropID == @event.PropID) || prevStoredEvents.Any(t => t != null &&  t.PropID == @event.PropID) || (CheckAll && ret.Any(t => t.PropID == @event.PropID)))
                                @event.PropID = Random.Next(StartPropId, EndPropId + 1);
                        }
                        else if (RandomPropId && Toggle && toggle)
                        {
                            @event.PropID = prevEvents[i1].PropID;
                        }
                        if (RandomLightId && !(Toggle && toggle))
                        {
                            @event.LightID = Random.Next(StartLightId, EndLightId + 1);
                            while (prevEvents.Any(t => t.LightID == @event.LightID) || prevStoredEvents.Any(t => t.LightID == @event.LightID) || (CheckAll && ret.Any(t => t.LightID == @event.LightID)))
                                @event.LightID = Random.Next(StartLightId, EndLightId + 1);
                        }
                        else if (RandomLightId && Toggle && toggle)
                        {
                            @event.LightID = prevEvents[i1].LightID;
                        }
                        if (Toggle && toggle)
                        {
                            @event.Value = LightType.Off;
                        }

                        prevEvents[i1] = @event;
                        ret.Add(@event);
                    }
                    toggle = !toggle;
                }
                alter = !alter;
                prevStoredEvents = prevEvents.Copy();
            }
            return ret.ToArray();
        }

        private LightType getAlternateType()
        {
            return Value switch
            {
                LightType.Off => LightType.Off,
                LightType.BlueOn => LightType.RedOn,
                LightType.BlueFlash => LightType.RedFlash,
                LightType.BlueFade => LightType.RedFade,
                LightType.RedOn => LightType.BlueOn,
                LightType.RedFlash => LightType.BlueFlash,
                LightType.RedFade => LightType.BlueFade
            };
        }
    }
}
