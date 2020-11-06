using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NoodleScripter.Models.NoodleScripter
{
    public class Pattern : WallGenerator
    {
        public Vector WallOffset { get; set; }
        public int Repeat { get; set; } = 1;

        public override IEnumerable<Wall> GenerateWalls()
        {
            var stepTime = Duration / Amount;
            var list = new List<Wall>();
            for (var i = 0; i < Amount; i++)
            {
                list.Add(new Wall
                {
                    Bomb = Bomb,
                    Width = Width,
                    Height = Height,
                    StartTime = StartTime + stepTime * i,
                    StartHeight = StartHeight,
                    StartRow = StartRow,
                    Duration = Duration,
                    NJS = NJS,
                    Offset = Offset,
                    Track = Track
                });
                for (var i1 = 0; i1 < Repeat; i1++)
                {
                    list.Add(new Wall
                    {
                        Bomb = Bomb,
                        Width = Width,
                        Height = Height,
                        StartTime = StartTime + stepTime * i,
                        StartHeight = StartHeight + (WallOffset.X * (i1 + 1)),
                        StartRow = StartRow + (WallOffset.Y * (i1 + 1)),
                        Duration = Duration,
                        NJS = NJS,
                        Offset = Offset,
                        Track = Track
                    });
                }
            }
            return list.ToArray();
        }
    }
}
