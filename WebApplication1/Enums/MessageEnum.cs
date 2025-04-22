using System.ComponentModel;

namespace WebApplication1.Enums;

public enum MessageEnum
{
    [Description("失敗")] Fail,
    [Description("登入失敗")] LoginFail,
    [Description("新增失敗")] AddFail,
    [Description("更改失敗")] EditFail,
    [Description("刪除失敗")] DeleteFail,
    [Description("送審失敗")] ReviewFail,
    [Description("匯入失敗")] ImportFail,
    [Description("禁用狀態")] Disable,
    [Description("獲取失敗")] GetFail,
    [Description("資料存在")] DataExist,
    [Description("審核失敗")] AgreeFail,
    [Description("退回失敗")] ReturnFail,
    [Description("計算失敗")] CalculateFail,
    [Description("轉換失敗")] ConversionFailed,
    [Description("上傳失敗")] UploadFail,
    [Description("下載失敗")] DownloadFail,
    [Description("尋找失敗")] NotFound,
    [Description("Azure上傳失敗")] AzureUploadFail,
    [Description("沒有傳入必要的檔案或參數")] invalidInput,
}
