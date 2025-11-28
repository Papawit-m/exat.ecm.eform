using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EXAT.ECM.Business.Models.FED.IMPORT
{
    
    public class T_FED_LOGMESSAG
    {
        #region property        
        public Guid? LOGID { get; set; } = Guid.NewGuid();
        public DateTime? LOGDATE { get; set; } = DateTime.Now;
        public string LOGFUNCTION { get; set; }
        public string LOGINPUTDATA { get; set; }
        public string LOGRESULT { get; set; }
        #endregion        
    }
}
