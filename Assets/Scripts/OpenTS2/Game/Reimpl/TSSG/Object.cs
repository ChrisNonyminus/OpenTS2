using OpenTS2.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Game.Reimpl.TSSG
{
    public class Object : AbstractAsset
    {
        public uint RecordType { get; set; } // always 0xFA1C39F7 (TypeIDs.OBJT)
        public uint Version { get; set; }
        public string TypeName { get; set; } // "cObject"
        public string ModelName { get; set; }
        public class Entry
        {
            public class SubEntry
            {
                public string LongName { get; set; }
                public string ShortName { get; set; }
            }

            public string Name { get; set; }
            public uint NameType { get; set; }
            public List<SubEntry> SubEntries { get; set; }
        }
        public List<Entry> Entries { get; set; }
        public class MainCoords // Main coords are relative to lot
        {
            public float XCoord { get; set; }
            public float YCoord { get; set; }
            public float Height { get; set; } // in meters
            public UnityEngine.Vector4 Rotation { get; set; } // quaternion
            public class CRESNodeEntryMaybe
            {
                public string NodeName { get; set; }
                public float XCoord { get; set; }
                public float YCoord { get; set; }
                public float Height { get; set; } // in meters
                public UnityEngine.Vector4 Rotation { get; set; } // quaternion
            }
            public List<CRESNodeEntryMaybe> CRESNodeEntries { get; set; } // Note: Entry 1's coords are relative to the lot. They change if the object is moved on the lot.
            public class BlendPairEntry
            {
                public string BlendName { get; set; }
                public string BlendPartner { get; set; }
                public uint TerminalZeroes { get; set; } // always 0?

            }
            public List<BlendPairEntry> BlendPairEntries { get; set; }
        }
        public MainCoords Coords { get; set; }
    }
}
