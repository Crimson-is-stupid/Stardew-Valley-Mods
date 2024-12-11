using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;

namespace Item_Every_Second
{
    internal class ModConfig
    {
        public float Time { get; set; } = 1f;
        public int Stacks { get; set; } = 1;

        public Dictionary<string, int> Chances { get; set; } = new Dictionary<string, int>();

        public Dictionary<string, bool> AllowChances { get; set; } = new Dictionary<string, bool>();

        public int RingChance { get; set; } = 1;

        public int ToolChance { get; set; } = 1;

        public bool CustomRingChance { get; set; } = false;

        public bool CustomToolChance { get; set; } = false;

        public bool NoUnShipItems { get; set; } = false;

        public bool No0SellPriceItems { get; set; } = false;

        public bool NoErrorItems {  get; set; } = true;

        public bool NoSpecialItems {  get; set; } = false;
    }
}
