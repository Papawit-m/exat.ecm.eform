using System.ComponentModel.DataAnnotations;

namespace EXAT.ECM.Business.Models.FED.IMPORT
{
    public class T_FED_LOGUPLOADFILE
    {
        #region property
        public Guid? LOGID { get; set; } = Guid.NewGuid();
        public DateTime? LOGDATE { get; set; } = DateTime.Now;
        public string MODULE { get; set; }
        public Guid? HEADERID { get; set; }
        public Guid? FILEID { get; set; }
        public string STATUS { get; set; }
        public string MESSAGE { get; set; }
        public string FILEPATH { get; set; }
        public string FILEURL { get; set; }
        #endregion
    }
}
