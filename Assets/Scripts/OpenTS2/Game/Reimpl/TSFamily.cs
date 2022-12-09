using System;

namespace OpenTS2.Game.Reimpl
{
    public class TSFamily
    {
        public int FamilyFunds { get; internal set; }
        public int NetWorth { get; internal set; }
        public string Name { get; internal set; }

        internal void SetLotGroupID(uint v)
        {
            throw new NotImplementedException();
        }

        internal void SetLotID(uint v)
        {
            throw new NotImplementedException();
        }
    }
}