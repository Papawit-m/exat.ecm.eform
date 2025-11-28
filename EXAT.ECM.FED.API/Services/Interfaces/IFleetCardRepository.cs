using EXAT.ECM.FED.API.Models.IMPORT;

namespace EXAT.ECM.FED.API.Services.Interfaces
{
    /// <summary>
    /// Interface สำหรับ Repository ที่จัดการการเชื่อมต่อฐานข้อมูล Fleet Card
    /// </summary>
    public interface IFleetCardRepository
    {
        /// <summary>
        /// Insert transaction record
        /// </summary>
        Task InsertTransactionAsync(FleetCardTransaction transaction);

        /// <summary>
        /// ค้นหารายการ Transactions ตามเงื่อนไขต่างๆ
        /// </summary>
        Task<IEnumerable<FleetCardTransaction>> SearchTransactionsAsync(TransactionSearchCriteria criteria);
    }
}
