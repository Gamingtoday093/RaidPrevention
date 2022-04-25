using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RaidPrevention.Models
{
    public class LocalBattle
    {
        public Vector3 Point { get; set; }
        public float Radius { get; set; }
        public LocalBattle(Vector3 point, float radius)
        {
            Point = point;
            Radius = radius;
        }
    }
}
