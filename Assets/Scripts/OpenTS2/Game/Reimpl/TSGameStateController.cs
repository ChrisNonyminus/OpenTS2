using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OpenTS2.Game.Reimpl
{
    public class TSGameStateController
    {
        public uint unk24; // CurrentState
        public bool field_70; // IsTripOngoing
        public int field_68; // FamilyIDToMoveIn
        public int field_74;
        public int field_7C;
        public long field_78; // GetTripHomeLotID
        public byte field_80;
        public bool IsTransitionInProgress;
        public int field_88;
        public TSNeighborhoodInfo CurrentNeighborhoodInfo;
        public TSNeighborhoodInfo NextNeighborhoodInfo;
        public TSNeighborhoodInfo LastNeighborhoodInfo;
        public TSLotInfo CurrentLotInfo;
        public TSLotInfo NextLotInfo;
        public TSLotInfo LastLotInfo;
        public uint CurrentState()
        {
            return unk24;
        }
        public TSFamily CurrentFamily()
        {
            if (!this.IsTransitionInProgress)
            {
                return null;
            }

            TSGlobals globals = TS.Globals();
            if (globals == null)
            {
                return null;
            }

            TSFamilyManager familyManager = globals.FamilyManager();
            if (familyManager == null)
            {
                return null;
            }

            if (this.field_70)
            {
                return familyManager.GetFamily(this.field_78);
            }

            var currentLotInfo = this.CurrentLotInfo;
            return familyManager.GetFamily(currentLotInfo.LotGroupName);
        }

        public bool InCommunityLot()
        {
            if (CurrentLotInfo == null)
            {
                return false;
            }

            var currentLotInfo = CurrentLotInfo;

            var baseLotInfo = currentLotInfo.GetBaseLotInfo();
            return baseLotInfo.LotType == TSBaseLotInfo.ZoneType.kZoneType_Community;

        }
        public bool InHotelLot()
        {
            if (CurrentLotInfo == null)
            {
                return false;
            }

            var currentLotInfo = CurrentLotInfo;

            var baseLotInfo = currentLotInfo.GetBaseLotInfo();
            return baseLotInfo.LotType == TSBaseLotInfo.ZoneType.kZoneType_Hotel;

        }
        public bool IsTripOngoing()
        {
            return this.field_70;
        }
        public long GetTripHomeLotID()
        {
            return this.field_78;
        }
        public class LotTripInfo
        {
            public bool IsTripOngoing;
            public int dword4;
            public long TripHomeLotID;
            public int dwordC;
            public byte byte10;
        }
        public byte GetTripInfo(ref LotTripInfo outLotTripInfo)
        {
            outLotTripInfo.IsTripOngoing = this.field_70;
            outLotTripInfo.dword4 = this.field_74;
            outLotTripInfo.TripHomeLotID = this.field_78;
            outLotTripInfo.dwordC = this.field_7C;
            byte result = this.field_80;
            outLotTripInfo.byte10 = result;
            return result;
        }
        public void SetTripInfo(LotTripInfo inLotTripInfo)
        {
            this.field_70 = inLotTripInfo.IsTripOngoing;
            this.field_74 = inLotTripInfo.dword4;
            this.field_78 = inLotTripInfo.TripHomeLotID;
            this.field_7C = inLotTripInfo.dwordC;
            this.field_80 = inLotTripInfo.byte10;
        }
        public bool InCASLot()
        {
            var currentLotInfo = CurrentLotInfo;
            if (currentLotInfo == null)
                return false;

            string lotName = currentLotInfo.LotName;
            return TS.IsCASLotName(lotName);
        }
        public bool TransitionsEnabled()
        {
            return this.field_88 > 0;
        }
        public int field_8C;
        public bool SaveLotEnabled()
        {
            if (InCASLot() || (InCommunityLot() && !field_70) /*|| InSecretLot()*/)
                return false;
            if (CurrentNeighborhoodInfo == null)
            {
                return false;
            }
            if (CurrentNeighborhoodInfo.IsEqual("Tutorial"))
                return false;
            return field_8C > 0;
        }
        public bool RefundValue(TSFamilyManager familyManager, TSNeighborManager neighborManager, TSNeighborhood neighborhood, TSLotInfo lotInfo)
        {
            if (familyManager == null || neighborManager == null || neighborhood == null || lotInfo == null)
                return false;

            if (lotInfo.GetLotType() != TSBaseLotInfo.ZoneType.kZoneType_Community)
            {
                if (lotInfo.GetLotType() != TSBaseLotInfo.ZoneType.kZoneType_Residential)
                    return false;

                if (!neighborhood.CurrentNeighborhoodInfo.IsNeighborhoodValid())
                    return false;
            }

            var neighbor = neighborManager.GetNeighbor(lotInfo.LotID, 1);
            if (neighbor != null)
            {
                var family = familyManager.GetFamily(neighbor.GetPersonData(61));
                if (family != null)
                {
                    var lotId = lotInfo.LotID;
                    if (neighborhood.GetNeighborhoodData(lotId, out int toRefund, out string v19, out char v18, 1))
                    {
                        int familyFunds = family.FamilyFunds;
                        var newFunds = toRefund + familyFunds;
                        family.FamilyFunds = newFunds;
                        family.NetWorth = newFunds;
                    }
                }
            }

            return true;
        }
        public bool SaveNeighborhoodLotToCatalog(TSLotInfo lotInfo)
        {
            var globals = TS.Globals();
            TSLotCatalog catalog = globals.GetLotCatalog();
            if (catalog == null)
            {
                return false;
            }

            lotInfo.RefreshLotInfo();
            //if (!catalog.RefCount())
            //{
            //    return false;
            //}

            TS.PersistSystem().Save(lotInfo.LotID, 1);
            return true;
        }
        public bool RemoveNeighborhoodLot(TSLotInfo lot)
        {
            TSNeighborhood neighborhood = TS.Globals().GetNeighborhood();
            if (neighborhood == null)
            {
                return false;
            }
            return neighborhood.RemoveNeighborhoodLot(lot.LotID);
        }
        public bool EvictFamily(TSFamilyManager familyManager,
                                TSNeighborhood neighborhood,
                                TSLotInfo lotInfo)
        {
            if (familyManager == null || neighborhood == null || lotInfo == null)
            {
                return false;
            }
            TSFamily family = familyManager.GetFamilyByLotID(lotInfo.LotID);
            if (family == null)
            {
                return false;
            }
            TSBaseLotInfo baseLotInfo = lotInfo.GetBaseLotInfo();
            if (neighborhood.GetLotFileInfo(lotInfo.LotID, out int v13, out var v15, out var v14, 1) && ((int)lotInfo.GetLotType() - 2 > 1))
            {
                var familyFunds = family.FamilyFunds;
                var newFunds = v13 + familyFunds;
                family.FamilyFunds = newFunds;
                family.NetWorth = newFunds;
            }
            family.SetLotID((uint)0);
            family.SetLotGroupID((uint)0);
            baseLotInfo.SetLotState((uint)8);
            lotInfo.SetFamilyID((uint)0);
            return true; // TODO
        }
        public uint GetCurrentLotGroupID()
        {
            TSPersistSystem persistSystem = TS.PersistSystem();
            if (CurrentLotInfo != null || persistSystem != null)
            {
                return persistSystem.GetLotGroupID(CurrentLotInfo.LotID);
            }
            return 0;
        }
        public uint GetCurrentLotID()
        {
            return CurrentLotInfo.LotID;
        }

        public string DumpCurrentState() // ???
        {
            TSFamily family = this.CurrentFamily();
            return family != null ? family.Name : "*undefined*";
        }

        public void NotifyNoValidNeighborhoods()
        {
            TS.ShowMessage("There are no valid neighborhoods.");
        }
        public TSNeighborhoodInfo field_60; // backup info?
        public void SetCurrentNeighborhoodInfo(TSNeighborhoodInfo info)
        {
            if (LastNeighborhoodInfo != CurrentNeighborhoodInfo)
            {
                LastNeighborhoodInfo = CurrentNeighborhoodInfo;
            }
            if (info != null)
            {
                CurrentNeighborhoodInfo = info;
            }
            else
            {
                if (CurrentNeighborhoodInfo != field_60)
                {
                    CurrentNeighborhoodInfo = field_60;
                }
            }
            if (NextNeighborhoodInfo != CurrentNeighborhoodInfo)
            {
                NextNeighborhoodInfo = CurrentNeighborhoodInfo;
            }
        }
        public TSSaveNeighborhoodController SaveNeighborhoodController;
        public TSLoadNeighborhoodController LoadNeighborhoodController;
        public TSSaveLotController SaveLotController;
        public TSLoadLotController LoadLotController;
        public TSStartupController StartupController;
        public Dictionary<long, long> field_A0;
        public void EnterLot(uint a2, bool a3, TSLotInfo lot, int a5, uint a6)
        {
            // EnterLotBegin
            NextLotInfo = lot;
            unk24 = 0x2C1805B0;
            if (a3)
            {
                if (!TS.IsCASLotName(lot.LotGroupName))
                {
                    // todo: see cTSGameStateController::EnterLotBegin in BV Mac OS X binary
                }
            }
            if (LastLotInfo != CurrentLotInfo)
            {
                LastLotInfo = CurrentLotInfo;
            }
            CurrentLotInfo = lot;
            // todo: see cTSGameStateController::EnterLotBegin in BV Mac OS X binary

            // EnterLotEnd
            // todo: see cTSGameStateController::EnterLotEnd in BV Mac OS X binary
            TSSimulator simulator = TS.Simulator();
            if (simulator.Running)
            {
                simulator.Mode = 1; // Live Mode?
            }
        }
    }
}
