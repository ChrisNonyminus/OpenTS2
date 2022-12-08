//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace OpenTS2.Game.Reimpl
//{
//    public class TSGameStateController
//    {
//        public int unk24; // CurrentState
//        public bool field_70; // IsTripOngoing
//        public int field_68; // FamilyIDToMoveIn
//        public int field_74;
//        public int field_7C;
//        public long field_78; // GetTripHomeLotID
//        public byte field_80;
//        public int field_88;
//        public int CurrentState()
//        {
//            return unk24;
//        }
//        public TSFamily CurrentFamily()
//        {
//            if (!this.IsTransitionInProgress())
//            {
//                return null;
//            }

//            var globals = TS.Globals();
//            if (globals == null)
//            {
//                return null;
//            }

//            var familyManager = globals.FamilyManager();
//            if (familyManager == null)
//            {
//                return null;
//            }

//            if (this.field_70)
//            {
//                return familyManager.GetFamily(this.field_78);
//            }
            
//            var currentLotInfo = this.CurrentLotInfo();
//            return familyManager.GetFamily(currentLotInfo.LotGroupName());
//        }
//        public bool InCommunityLot()
//        {
//            if (CurrentLotInfo() == null)
//            {
//                return false;
//            }

//            var currentLotInfo = CurrentLotInfo();
//            if (currentLotInfo == null)
//            {
//                return false;
//            }

//            var baseLotInfo = currentLotInfo.GetBaseLotInfo();
//            return baseLotInfo.GetZoneType() == TSBaseLotInfo.ZoneType.kZoneType_Community;

//        }
//        public bool InHotelLot()
//        {
//            if (CurrentLotInfo() == null)
//            {
//                return false;
//            }

//            var currentLotInfo = CurrentLotInfo();
//            if (currentLotInfo == null)
//            {
//                return false;
//            }

//            var baseLotInfo = currentLotInfo.GetBaseLotInfo();
//            return baseLotInfo.GetZoneType() == TSBaseLotInfo.ZoneType.kZoneType_Hotel;

//        }
//        public bool IsTripOngoing()
//        {
//            return this.field_70;
//        }
//        public long GetTripHomeLotID()
//        {
//            return this.field_78;
//        }
//        public class LotTripInfo
//        {
//            public bool IsTripOngoing;
//            public int dword4;
//            public long TripHomeLotID;
//            public int dwordC;
//            public byte byte10;
//        }
//        public byte GetTripInfo(ref LotTripInfo outLotTripInfo)
//        {
//            outLotTripInfo.IsTripOngoing = this.field_70;
//            outLotTripInfo.dword4 = this.field_74;
//            outLotTripInfo.TripHomeLotID = this.field_78;
//            outLotTripInfo.dwordC = this.field_7C;
//            byte result = this.field_80;
//            outLotTripInfo.byte10 = result;
//            return result;
//        }
//        public void SetTripInfo(LotTripInfo inLotTripInfo)
//        {
//            this.field_70 = inLotTripInfo.IsTripOngoing;
//            this.field_74 = inLotTripInfo.dword4;
//            this.field_78 = inLotTripInfo.TripHomeLotID;
//            this.field_7C = inLotTripInfo.dwordC;
//            this.field_80 = inLotTripInfo.byte10;
//        }
//        public bool InCASLot()
//        {
//            var currentLotInfo = CurrentLotInfo();
//            if (currentLotInfo == null)
//                return false;

//            var lotName = currentLotInfo.LotName();
//            return TS.IsCASLotName(lotName);
//        }
//        public bool TransitionsEnabled()
//        {
//            return this.field_88 > 0;
//        }
//        public int field_8C;
//        public bool SaveLotEnabled()
//        {
//            if (InCASLot() || (InCommunityLot() && !field_70) || InSecretLot())
//                return false;
//            if (GetNeighborhoodInfo() == null)
//            {
//                return false;
//            }
//            if (GetNeighborhoodInfo().IsEqual("Tutorial"))
//                return false;
//            return field_8C > 0;
//        }
//        public bool RefundValue(TSFamilyManager familyManager, TSNeighborManager neighborManager, TSNeighborhood neighborhood, TSLotInfo lotInfo)
//        {
//            if (familyManager == null || neighborManager == null || neighborhood == null || lotInfo == null)
//                return false;

//            if (lotInfo.GetLotType() != TSBaseLotInfo.ZoneType.kZoneType_Community)
//            {
//                if (lotInfo.GetLotType() != TSBaseLotInfo.ZoneType.kZoneType_Residential)
//                    return false;
                
//                if (!neighborhood.GetNeighborhoodInfo().IsNeighborhoodValid())
//                    return false;
//            }

//            var neighbor = neighborManager.GetNeighbor(lotInfo.GetLotID(), 1);
//            if (neighbor != null)
//            {
//                var family = familyManager.GetFamily(neighbor.GetPersonData(61));
//                if (family != null)
//                {
//                    var lotId = lotInfo.GetLotID();
//                    if (neighborhood.GetNeighborhoodData(lotId, out int toRefund, out string v19, out char v18, 1))
//                    {
//                        var familyFunds = family.GetFamilyFunds();
//                        var newFunds = toRefund + familyFunds;
//                        family.SetFamilyFunds(newFunds);
//                        family.SetFamilyBankFunds(newFunds);
//                    }
//                }
//            }

//            return true;
//        }
//        public bool SaveNeighborhoodLotToCatalog(TSLotInfo lotInfo)
//        {
//            var globals = TS.Globals();
//            var catalog = globals.GetLotCatalog();
//            if (catalog == null)
//            {
//                return false;
//            }

//            lotInfo.RefreshLotInfo();
//            //if (!catalog.RefCount())
//            //{
//            //    return false;
//            //}
            
//            TS.PersistSystem().Save(lotInfo.GetLotID(), 1);
//            return true;
//        }
//        public bool RemoveNeighborhoodLot(TSLotInfo lot)
//        {
//            var neighborhood = TS.Globals().GetNeighborhood();
//            if (neighborhood == null)
//            {
//                return false;
//            }
//            return neighborhood.RemoveNeighborhoodLot(lot.GetLotID());
//        }
//        public bool EvictFamily(TSFamilyManager familyManager, 
//                                TSNeighborhood neighborhood, 
//                                TSLotInfo lotInfo)
//        {
//            if (familyManager == null || neighborhood == null || lotInfo == null)
//            {
//                return false;
//            }
//            TSFamily family = familyManager.GetFamilyByLotID(lotInfo.GetLotID());
//            if (family == null)
//            {
//                return false;
//            }
//            TSBaseLotInfo baseLotInfo = lotInfo.GetBaseLotInfo();
//            if (neighborhood.GetLotFileInfo(lotInfo.GetLotID(), out var v13, out var v15, out var v14, 1) && (lotInfo.GetLotType() - 2 > 1))
//            {
//                var familyFunds = family.GetFamilyFunds();
//                var newFunds = v13 + familyFunds;
//                family.SetFamilyFunds(newFunds);
//                family.SetFamilyBankFunds(newFunds);
//            }
//            family.SetLotID(0);
//            family.SetLotGroupID(0);
//            baseLotInfo.SetLotState(8);
//            lotInfo.SetFamilyID(0);
//            return true; // TODO
//        }
//    }
//}
