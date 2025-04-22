using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Globalization;
using System.Text;
using WebApplication1.Data.Options;

namespace WebApplication1.Helpers;

public interface IFileStreamingHelper
{
    /// <summary>
    /// 串流上傳檔案
    /// </summary>
    /// <param name="request"></param>
    /// <param name="createStream"></param>
    /// <returns></returns>
    public Task<FormValueProvider> StreamFile(HttpRequest request, Func<FileMultipartSection, Stream> createStream);
}

public class FileStreamingHelper : IFileStreamingHelper
{
    // 注入所需的服務和配置
    private readonly IMultipartRequestHelper _multipartRequestHelper;
    private readonly FileUploadOptions _fileUploadOptions;

    public FileStreamingHelper(IMultipartRequestHelper multipartRequestHelper, IOptions<FileUploadOptions> fileUploadOptions)
    {
        _multipartRequestHelper = multipartRequestHelper;
        _fileUploadOptions = fileUploadOptions.Value;
    }

    public async Task<FormValueProvider> StreamFile(HttpRequest request, Func<FileMultipartSection, Stream> createStream)
    {
        if (!_multipartRequestHelper.IsMultipartContentType(request.ContentType))
        {
            throw new Exception($"Expected a multipart request, but got {request.ContentType}");
        }

        // 將 request 中的 Form 按照 Key 和 Value 存到此物件
        var formAccumulator = new KeyValueAccumulator();

        var boundary = _multipartRequestHelper.GetBoundary(
            MediaTypeHeaderValue.Parse(request.ContentType),
            _fileUploadOptions.MultipartBoundaryLengthLimit);
        var reader = new MultipartReader(boundary, request.Body);

        var section = await reader.ReadNextSectionAsync();
        while (section != null)
        {
            // 逐個取出 Form 的欄位內容
            ContentDispositionHeaderValue contentDisposition;
            var hasContentDispositionHeader = ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out contentDisposition);

            if (hasContentDispositionHeader)
            {
                // 處理文件
                if (_multipartRequestHelper.HasFileContentDisposition(contentDisposition))
                {
                    // 文件名和擴展名檢查
                    var fileName = Path.GetFileName(section.AsFileSection().FileName);
                    var fileExtension = Path.GetExtension(fileName).ToLowerInvariant();
                    var allowedExtensions = _fileUploadOptions.AllowedExtensions;

                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        throw new InvalidDataException($"不允許的文件擴展名：{fileExtension}。");
                    }

                    // 這邊可以加入 掃描病毒

                    // 如果這一部分是文件，則將其寫入到 Stream 中
                    using (var targetStream = createStream(section.AsFileSection()))
                    {
                        await section.Body.CopyToAsync(targetStream);
                    }
                }
                // 處理表單資料（非文件類型的資料）而非文件上傳本身
                else if (_multipartRequestHelper.HasFormDataContentDisposition(contentDisposition))
                {
                    // 從 Content-Disposition 中取得欄位的鍵名，並去除雙引號（如果存在）
                    var key = HeaderUtilities.RemoveQuotes(contentDisposition.Name).Value;

                    // 根據請求部分內容取得相應的編碼格式
                    var encoding = GetEncoding(section);

                    // 使用 StreamReader 逐字讀取表單欄位的值
                    using (var streamReader = new StreamReader(
                        section.Body,          // 表單欄位的流
                        encoding,              // 編碼方式，通常是 UTF-8
                        detectEncodingFromByteOrderMarks: true, // 自動檢測 Byte Order Mark (BOM)
                        bufferSize: 4096,      // 設定緩衝區大小，優化性能
                        leaveOpen: true))      // 保持流處於打開狀態，允許後續讀取
                    {
                        // 讀取表單欄位的完整值
                        var value = await streamReader.ReadToEndAsync();

                        // 如果欄位值是 "undefined" 字符串，將其轉換為空字符串（可能是客戶端上傳的特殊標記）
                        if (String.Equals(value, "undefined", StringComparison.OrdinalIgnoreCase))
                        {
                            value = String.Empty;
                        }

                        // 將欄位的鍵和值添加到表單累加器中，以便後續綁定到模型
                        formAccumulator.Append(key, value);

                        // 檢查表單欄位的數量是否超過配置中的最大限制
                        if (formAccumulator.ValueCount > _fileUploadOptions.ValueCountLimit)
                        {
                            // 如果表單欄位過多，拋出異常，防止表單欄位濫用
                            throw new InvalidDataException($"表單鍵數量限制 {_fileUploadOptions.ValueCountLimit} 已超過。");
                        }
                    }
                }
            }

            // 取得 Form 的下一個欄位
            section = await reader.ReadNextSectionAsync();
        }

        // 這邊可以加入掃描病毒

        // 將表單數據綁定到模型
        var formValueProvider = new FormValueProvider(
            BindingSource.Form,
            new FormCollection(formAccumulator.GetResults()),
            CultureInfo.CurrentCulture);

        return formValueProvider;
    }

    private static Encoding GetEncoding(MultipartSection section)
    {
        MediaTypeHeaderValue mediaType;
        var hasMediaTypeHeader = MediaTypeHeaderValue.TryParse(section.ContentType, out mediaType);
        // UTF-7 是不安全的，不應被使用。大多數情況下 UTF-8 會被使用。
        if (!hasMediaTypeHeader || Encoding.UTF7.Equals(mediaType.Encoding))
        {
            return Encoding.UTF8;
        }
        return mediaType.Encoding;
    }
}
