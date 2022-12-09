using OpenTS2.Content;
using OpenTS2.Content.DBPF;
using System;

namespace OpenTS2.Game.Reimpl
{
    public class TSLotInfo : TSObject
    {
        public TSLotInfo(ObjectDefinitionAsset ObjectDefinition) : base(ObjectDefinition)
        {
        }

        public string LotGroupName { get; set; }
        public string LotName { get; internal set; }
        public uint LotID => GUID;
        public int LotValue { get; set; }

        public TSBaseLotInfo GetBaseLotInfo()
        {
            throw new NotImplementedException();
        }

        internal TSBaseLotInfo.ZoneType GetLotType()
        {
            return GetBaseLotInfo().LotType;
        }

        internal void RefreshLotInfo()
        {
            throw new NotImplementedException();
        }

        internal void SetFamilyID(uint v)
        {
            throw new NotImplementedException();
        }
    }
}