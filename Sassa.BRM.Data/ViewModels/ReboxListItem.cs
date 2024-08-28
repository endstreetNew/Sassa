namespace Sassa.BRM.ViewModels
{
    public class ReboxListItem
    {
        public string BoxNo { get; set; }
        public int? MiniBox { get; set; }
        public string AltBoxNo { get; set; }
        public string ClmNo { get; set; }
        public string BrmNo { get; set; }
        public string IdNo { get; set; }
        public string FullName { get; set; }
        public string GrantType { get; set; }
        public bool Scanned { get; set; }
        public bool BoxLocked
        {
            get
            {
                return TdwBatch > 1;
            }
        }
        public bool BoxOpen
        {
            get
            {
                return TdwBatch == 0;
            }
        }
        public string RegType { get; set; }
        public int TdwBatch { get; set; }

    }
}
