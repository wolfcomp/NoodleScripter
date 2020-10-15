using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media.Media3D;
using NoodleScripter.Models.BeatSaber;

namespace NoodleScripter.Models.NoodleScripter
{
    public class Wall
    {
        public bool Bomb { get; set; }
        public Vector3D? Color { get; set; }
        public Vector3D Rotation { get; set; }
        public Vector3D LocalRotation { get; set; }
        public float? NJS { get; set; }
        public float? Offset { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double StartHeight { get; set; }
        public double StartRow { get; set; }
        public double Duration { get; set; }
        public double StartTime { get; set; }
        public string Track { get; set; }

        public ObjectData GenerateObstacle()
        {
            var wall = ToValid();
            return new ObjectWall
            {
                Type = 0,
                LineIndex = 0,
                Width = 0,
                Duration = (float)wall.Duration,
                Time = (float)StartTime,
                CustomData = new ObstacleCustom
                {
                    Offset = wall.Offset,
                    NJS = wall.NJS,
                    Color = wall.Color,
                    LocalRotation = wall.LocalRotation,
                    Position = new Vector(wall.StartRow, wall.StartHeight),
                    Rotation = wall.Rotation,
                    Scale = new Vector(wall.Width, wall.Height),
                    Track = wall.Track
                }
            };
        }

        private Wall ToValid()
        {
            var wall = this.Copy();
            if (wall.Width < 0)
            {
                wall.StartRow += wall.Width;
                wall.Width *= -1;
            }

            if (wall.Height < 0)
            {
                wall.StartHeight += wall.Height;
                wall.Height *= -1;
            }

            wall.Width = Math.Max(wall.Width, 0.005);
            wall.Height = Math.Max(wall.Height, 0.005);

            wall.Duration = Math.Max(wall.Duration, -4);
            wall.StartTime = Math.Min(wall.StartTime, 0.005);

            return wall;
        }
    }
}
