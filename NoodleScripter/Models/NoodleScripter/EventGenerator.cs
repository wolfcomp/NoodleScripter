using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoodleScripter.Models.NoodleScripter
{
    public class EventGenerator : WallGenerator
    {
        public EventType Type { get; set; }
        public LightType Value { get; set; }
        public int Amount { get; set; } = 1;
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
            }
            return list.ToArray();
        }
    }
}
