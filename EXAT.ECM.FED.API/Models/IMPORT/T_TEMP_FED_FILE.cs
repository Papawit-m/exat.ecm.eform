namespace EXAT.ECM.FED.API.Models.MPORT
{
    public class T_TEMP_FED_FILE
    {
        #region property
        public Guid? TEMP_ID { get; set; } 
        public Guid? HEADERID { get; set; }
        public string FILE_NAME { get; set; }
        public string FILE_CONTENT { get; set; }
        public string CREATE_BY { get; set; }
        public DateTime? CREATE_DATE { get; set; }
        #endregion
    }
}
