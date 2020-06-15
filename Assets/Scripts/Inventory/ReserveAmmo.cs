    [System.Serializable]
    public class ReserveAmmo
    {
        public string ReferenceType;
        public int CurrentReserveAmmo;
        public int MaxAmmo;

        public ReserveAmmo(string referenceType, int currentReserveAmmo, int maxAmmo)
        {
            ReferenceType = referenceType;
            CurrentReserveAmmo = currentReserveAmmo;
            MaxAmmo = maxAmmo;
        }
    }
