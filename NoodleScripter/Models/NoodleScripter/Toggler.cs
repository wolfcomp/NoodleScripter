using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoodleScripter.Models.NoodleScripter
{
    public class Toggler : EventGenerator
    {
        public double ToggleLength { get; set; }

        public override IEnumerable<Event> GenerateEvents()
        {
            var stepTime = Duration / Amount;
            var list = new List<Event>();
            for (var i = 0; i < Amount; i++)
            {
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
                    PropID = PropID
                });
                list.Add(new Event
                {
                    Direction = Direction,
                    CounterLock = CounterLock,
                    Filter = Filter,
                    Reset = Reset,
                    StartTime = (StartTime + stepTime * i) + ToggleLength,
                    Type = Type,
                    Prop = Prop,
                    Speed = Speed,
                    Step = Step,
                    PropMult = PropMult,
                    SpeedMult = SpeedMult,
                    StepMult = StepMult,
                    Value = LightType.Off,
                    LightID = LightID,
                    PropID = PropID
                });
            }
            return list.ToArray();
        }
    }
}
