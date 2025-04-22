namespace WebApplication1.Data.Options;

public class FileUploadOptions
{
    /// <summary>
    /// 用於限制多部分請求中邊界字符串的最大長度
    /// </summary>
    public int MultipartBoundaryLengthLimit { get; set; }

    /// <summary>
    /// 請求中最大的鍵值對數量
    /// </summary>
    public int ValueCountLimit { get; set; }

    /// <summary>
    /// 多部分請求體的最大允許長度（文件大小限制）
    /// </summary>
    public long MultipartBodyLengthLimit { get; set; }

    /// <summary>
    /// 允許上傳的文件擴展名
    /// </summary>
    public string[] AllowedExtensions { get; set; } = [];
}
