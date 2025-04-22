using WebApplication1.Enums;

namespace WebApplication1.Dtos.Common;
public interface IResultDto
{
    bool Success { get; set; } 

    string Message { get; set; }

    MessageEnum MessageNo { get; set; }

    string Description { get; set; }

    IEnumerable<string> Remark { get; set; }
}

public class ResultDTO<T> : IResultDto
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; } = true;

    /// <summary>
    /// 訊息種類
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// 訊息種類編號
    /// </summary>
    public MessageEnum MessageNo { get; set; }

    /// <summary>
    /// 詳細訊息
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// 備註
    /// </summary>
    public IEnumerable<string> Remark { get; set; } = new List<string>();

    /// <summary>
    /// 萬用欄位
    /// </summary>
    public T? Object { get; set; }
}
