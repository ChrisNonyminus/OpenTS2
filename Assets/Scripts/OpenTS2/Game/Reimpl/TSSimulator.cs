using System;
using System.Collections.Generic;

namespace OpenTS2.Game.Reimpl
{
    public class TSSimulator
    {
        public bool Running = false;
        public uint Mode = 0;
        public int CurrentObjects = 0;
        public int AllocatedObjects = 0;
        public byte TickMode = 0;
        public short Speed = 0;
        public Dictionary<int, object> Globals = new Dictionary<int, object>();
        public void InitForNewLot()
        {
            Init();
            InitShared();
        }

        public void UpdateTotalObjectsValue()
        {
            int totalObjects = CurrentObjects + AllocatedObjects;
            SetGlobal(48, (short)(totalObjects / 1000));
            SetGlobal(49, (short)(totalObjects % 1000));
        }
        enum TimeOfDay { Day, Evening, Night, Morning, };
        public short Hour = 0;

        TimeOfDay ComputeTimeOfDay()
        {
            short hour = Hour;
            if (hour <= 5)
                return TimeOfDay.Night;
            if (hour == 6)
                return TimeOfDay.Morning;
            if (hour == 18)
                return TimeOfDay.Evening;
            if (hour >= 19)
                return TimeOfDay.Night;
            return TimeOfDay.Day;
        }

        int GetTimeOfDay()
        {
            return GetGlobal<int>(4);
        }

        void SetTimeOfDay(int value)
        {
            SetGlobal(4, value);
        }

        public void SetGlobal(int index, object value)
        {
            Globals[index] = value;
        }

        public T GetGlobal<T>(int index)
        {
            return (T)Globals[index];
        }

        private int newGUID = 0;

        public bool Paused = true;

        public uint Ticks = 0;
        
        public int GetNewInteractionGUID()
        {
            if (this.newGUID == 0)
            {
                this.newGUID = 1;
            }
            int result = (ushort)this.newGUID;
            this.newGUID = result + 1;
            return result;
        }

        public TSFamily ActiveFamily;
        public TSLotInfo ActiveLot;
        public int LotObjectsValue = 0;
        public int FamilyObjectsValue = 0;
        public int TotalObjectsValue => LotObjectsValue + FamilyObjectsValue;
        public int ArchValue = 0;
        public int LotValue => ActiveLot.LotValue;


        private void InitShared()
        {
            throw new NotImplementedException();
        }

        private void Init()
        {
            throw new NotImplementedException();
        }
    }
}