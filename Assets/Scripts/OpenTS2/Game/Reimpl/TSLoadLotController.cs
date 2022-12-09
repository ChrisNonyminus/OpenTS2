namespace OpenTS2.Game.Reimpl
{
    public class TSLoadLotController
    {
        public TSLotInfo LotInfo { get; set; }
        public TSFamilyManager FamilyManager { get; set; }
        public TSNeighborhood Neighborhood { get; set; }
        public TSSimulator Simulator { get; set; }
        public EdithObjectModule ObjectModule { get; set; }
        public LotAsset Lot { get; set; }
        public TSGameStateController GameStateController { get; set; }
        public TSPersistSystem PersistSystem { get; set; }
        public TSWorld.WorldDB WorldDB { get; set; }
        public TSNeighborManager NeighborManager { get; set; }
        
        // See cTSLoadLotController::LoadLot(void) in the unstripped Mac OS X binary for Bon Voyage
        public void LoadLot()
        {
            
        }
    }
}