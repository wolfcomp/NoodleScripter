using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoodleScripter.Models.NoodleScripter
{
    public class Propigator : EventGenerator
    {
        public int StartProp { get; set; }
        public int EndProp { get; set; }
        public double Offset { get; set; }
        public bool Toggle { get; set; }

        public override IEnumerable<Event> GenerateEvents()
        {
            Amount *= ((Math.Max(EndProp, StartProp) - Math.Min(EndProp, StartProp)) + 1);
            var stepTime = Duration / (Amount - 1);
            var list = new List<Event>();
            int? previousProp = null;
            for (var i = 0; i < Amount; i++)
            {
                var curProp = previousProp.HasValue ? previousProp == EndProp ? StartProp : (StartProp < EndProp ? previousProp.Value + 1 : previousProp.Value - 1) : StartProp;
                list.Add(new Event
                {
                    Direction = Direction,
                    CounterLock = CounterLock,
                    Filter = Filter,
                    Reset = Reset,
                    StartTime = StartTime + stepTime * i,
                    Type = Type,
                    Prop = Prop,
                    Speed = Speed,
                    Step = Step,
                    PropMult = PropMult,
                    SpeedMult = SpeedMult,
                    StepMult = StepMult,
                    Value = Value,
                    LightID = LightID,
                    PropID = curProp
                });
                if (Toggle)
                    list.Add(new Event
                    {
                        Direction = Direction,
                        CounterLock = CounterLock,
                        Filter = Filter,
                        Reset = Reset,
                        StartTime = StartTime + stepTime * (i + 1) + Offset,
                        Type = Type,
                        Prop = Prop,
                        Speed = Speed,
                        Step = Step,
                        PropMult = PropMult,
                        SpeedMult = SpeedMult,
                        StepMult = StepMult,
                        Value = LightType.Off,
                        LightID = LightID,
                        PropID = curProp
                    });
                previousProp = curProp;
            }
            return list.ToArray();
        }
    }
}
