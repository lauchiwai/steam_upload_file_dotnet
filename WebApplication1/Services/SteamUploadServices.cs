using WebApplication1.Dtos.Common;
using WebApplication1.Helpers;
using WebApplication1.Extensions;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace WebApplication1.Services;

public interface ISteamUploadServices
{
    /// <summary>
    /// 串流上傳檔案到本地
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<ResultDTO<object>> UploadFileAsync(HttpRequest request);

    /// <summary>
    /// 串流上傳檔案到雲端
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public Task<ResultDTO<object>> AzureUploadFileAsync(HttpRequest request);
}

public class SteamUploadServices : ISteamUploadServices
{
    // 注入所需的服務和配置
    private readonly IFileStreamingHelper _fileStreamingHelper;
    private readonly IConfiguration _configuration;

    public SteamUploadServices(IFileStreamingHelper fileStreamingHelper, IConfiguration configuration)
    {
        _fileStreamingHelper = fileStreamingHelper;
        _configuration = configuration;
    }

    public async Task<ResultDTO<object>> UploadFileAsync(HttpRequest request)
    {
        var result = new ResultDTO<object>() { Success = true };
        // 定義本地保存檔案的目錄
        var folderPath = Path.Combine(_configuration["folderPath"] ?? Directory.GetCurrentDirectory(), Guid.NewGuid().ToString());
        // 保存全部檔案完整路徑
        var fullFilePathList = new List<string>();
        try
        {
            // 如果目錄不存在，創建該目錄
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // 調用 FileStreamingHelper 來處理檔案上傳，並保存到指定路徑
            var formValueProvider = await _fileStreamingHelper.StreamFile(request, section =>
            {
                // 為每個上傳的檔案生成唯一檔名，避免檔名衝突
                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(section.FileName)}";

                // 確定檔案的完整保存路徑
                var fullFilePath = Path.Combine(folderPath, uniqueFileName);

                // 將檔案完整路徑都存到 list
                fullFilePathList.Add(fullFilePath);

                // 創建 FileStream 用於將檔案寫入磁碟
                return new FileStream(fullFilePath, FileMode.Create);
            });

            Console.WriteLine("///////////////////////////////////////// ");
            formValueProvider.PrintAllMembers();

            Console.WriteLine("///////////////////////////////////////// ");
            Console.WriteLine(formValueProvider.GetValue("name"));

            // 返回上傳成功的訊息
            result.Message = "檔案上傳成功。";
            return result;
        }
        catch (Exception ex)
        {
            // 錯誤處理，捕獲並返回錯誤訊息
            result.Success = false;
            result.Message = $"上傳檔案時出錯: {ex.Message}";
            return result;
        }
        finally
        {
            DeleteFile(folderPath);
        }
    }

    public async Task<ResultDTO<object>> AzureUploadFileAsync(HttpRequest request)
    {
        var result = new ResultDTO<object>() { Success = true };
        // 設定文件夾名稱
        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), Guid.NewGuid().ToString());
        var fullFilePathList = new List<string>();
        try
        {
            // 先將文件存到本地
            // 如果目錄不存在，創建該目錄
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // 調用 FileStreamingHelper 來處理檔案上傳，並保存到指定路徑
            var formValueProvider = await _fileStreamingHelper.StreamFile(request, section =>
            {
                // 為每個上傳的檔案生成唯一檔名，避免檔名衝突
                var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(section.FileName)}";

                // 確定檔案的完整保存路徑
                var fullFilePath = Path.Combine(folderPath, uniqueFileName);

                // 將檔案完整路徑都存到 list
                fullFilePathList.Add(fullFilePath);

                // 創建 FileStream 用於將檔案寫入磁碟
                return new FileStream(fullFilePath, FileMode.Create);
            });

            // 將對應的資料存到資料庫内
            // 資料庫儲存沒有錯誤的話
            // 才從本地獲取文件上傳到雲端
            // UploadLocalFileToBlobAsync()
            // 刪除本地文件
            // 上面步驟都沒有錯誤的話
            // 資料庫 savechange()

            // 返回上傳成功的訊息
            result.Message = "檔案上傳成功。";
            return result;
        }
        catch (Exception ex)
        {
            // 錯誤處理，捕獲並返回錯誤訊息
            result.Success = false;
            result.Message = $"上傳檔案時出錯: {ex.Message}";
            return result;
        } 
        finally
        {
            DeleteFile(folderPath);
        }
    }

    public static async Task UploadLocalFileToBlobAsync(BlobContainerClient containerClient, List<string> fullFilePathList)
    {
        List<string> successfullyUploadedFiles = new List<string>();

        try
        {
            foreach (var fullFilePath in fullFilePathList)
            {
                string fileName = Path.GetFileName(fullFilePath);
                BlobClient blobClient = containerClient.GetBlobClient(fileName);

                await using (var fileStream = new FileStream(fullFilePath, FileMode.Open, FileAccess.Read))
                {
                    await blobClient.UploadAsync(fileStream, overwrite: true);
                    successfullyUploadedFiles.Add(fullFilePath);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to upload files: {ex.Message}");
            await RollbackUploadsAsync(containerClient, successfullyUploadedFiles); 
            throw; 
        }
    }

    public static async Task RollbackUploadsAsync(BlobContainerClient containerClient, List<string> uploadedFiles)
    {
        foreach (var filePath in uploadedFiles)
        {
            string fileName = Path.GetFileName(filePath);
            try
            {
                BlobClient blobClient = containerClient.GetBlobClient(fileName);
                await blobClient.DeleteIfExistsAsync();  // 刪除Blob
                Console.WriteLine($"Rolled back {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error rolling back {fileName}: {ex.Message}");
            }
        }
    }

    public static void DeleteFile(string uploadPath)
    {
        if (Directory.Exists(uploadPath))
            Directory.Delete(uploadPath, true);
    }
}
