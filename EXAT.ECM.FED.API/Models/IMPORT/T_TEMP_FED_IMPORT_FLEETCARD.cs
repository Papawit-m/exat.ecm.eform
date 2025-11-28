namespace EXAT.ECM.FED.API.Models.IMPORT
{
    public class T_TEMP_FED_IMPORT_FLEETCARD
    {
        public Guid?   ITEM_ID                       { get; set; } 
        public string IMPORT_DATE                   { get; set; }
        public string ACCOUNT_NO                    { get; set; }
        public string CREDIT_LINE                   { get; set; }
        public string FROM_DATE                     { get; set; }
        public string TODATE                        { get; set; }
        public string DEPARTMENT                    { get; set; }
        public string COST_CENTER                   { get; set; }
        public string CARD_NO                       { get; set; }
        public string PLATE_NO                      { get; set; }
        public string TRANSACTION_DATE              { get; set; }
        public string MERCHANT_ID                   { get; set; }
        public string TAX_ID                        { get; set; }
        public string MERCHANT_NAME                 { get; set; }
        public string LOCATION                      { get; set; }
        public string ADDRESS_ACCORDING             { get; set; }
        public string BRANCH_NUMBER                 { get; set; }
        public string INVOICE_NO                    { get; set; }
        public string PRODUCT                       { get; set; }
        public string QUANTITY_LITRE                { get; set; }
        public string QUANTITY_KM                   { get; set; }
        public string EXCLUDE_VAT_AMOUNT            { get; set; }
        public string VAT_AMOUNT                    { get; set; }
        public string AMOUNT                        { get; set; }
        public string UNIT_PRICE                    { get; set; }
        public string ODOMETER                      { get; set; }
        public string DISTANCE_KM                   { get; set; }
        public string FUEL_CONS_KM_LITRE            { get; set; }
        public string FUEL_CONS_BAHT_KM             { get; set; }
        public string NGV_CONS_KM_KG                { get; set; }
        public string NGV_CONS_BAHT_KM              { get; set; }
        public string LPG_CONS_KM_LITRE             { get; set; }
        public string LPG_CONS_BAHT_KM              { get; set; }
        public string FUEL_CONS_KM_LITRE2           { get; set; }
        public string TEMP_GROUP_ID                 { get; set; }
        public DateTime? INSERT_DATE                { get; set; }
    }
}
