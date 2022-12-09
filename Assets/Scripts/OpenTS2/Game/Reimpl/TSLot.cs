using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Game.Reimpl
{
    public class LotAsset : AbstractAsset
    {
        public string FileName { get; set; }
        public short Version { get; set; }
        public uint LotSize1 { get; set; }
        public uint LotSize2 { get; set; }
        public TSBaseLotInfo.ZoneType LotType { get; set; }
        public byte Roads { get; set; }
        public byte Rotation { get; set; }
        public uint Flags { get; set; }
        public string LotName { get; set; }
        public string LotDescription { get; set; }
    }
}
