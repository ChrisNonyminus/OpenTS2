using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTS2.Game.Reimpl
{
    public class TSBaseLotInfo
    {
        public ZoneType LotType { get; internal set; }

        internal void SetLotState(uint v)
        {
            throw new NotImplementedException();
        }

        public enum ZoneType : byte
        {
            kZoneType_Residential = 0,
            kZoneType_Community = 1,
            kZoneType_Hotel = 6,
        }
    }
}
