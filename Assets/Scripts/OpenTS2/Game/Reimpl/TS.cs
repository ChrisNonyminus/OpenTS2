namespace OpenTS2.Game.Reimpl
{
    public static class TS
    {
        public static bool IsCASLotName(string lotName)
        {
            return lotName.StartsWith("CAS!") || lotName.StartsWith("YACAS!");
        }
    }
}