using System.ComponentModel.DataAnnotations;

namespace EXAT.ECM.Business.Models.APIModel
{
    public class ResponseModel
    {
        #region BaseResponse
        public class SuccessResponse //: BaseResponse
        {
            [Required]
            public string Status { get; set; }
            [Required]
            public string StatusCode { get; set; }
            public string Data { get; set; }          
        }

        public class SuccessResponse<T> //: BaseResponse
        {
            [Required]
            public string Status { get; set; }
            [Required]
            public string StatusCode { get; set; }
            public T? Data { get; set; }
        }

        public class ErrorResponse //: BaseResponse
        {
            public string Status { get; set; }
            public string StatusCode { get; set; }
            public string Message { get; set; }
        }
        #endregion

    }
}
