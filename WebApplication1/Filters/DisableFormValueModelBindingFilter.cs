using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WebApplication1.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class DisableFormValueModelBindingFilter : Attribute, IResourceFilter
{
    // 過濾器
    // 自行處理 Request 來的資料，所以要把 原本的 Model Binding 移除 
    // 建立一個 Attribute 註冊在大型檔案上傳的 API，透過 Resource Filter 在 Model Binding 之前把它移除
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var factories = context.ValueProviderFactories;
        factories.RemoveType<FormValueProviderFactory>();
        factories.RemoveType<FormFileValueProviderFactory>();
        factories.RemoveType<JQueryFormValueProviderFactory>();
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
    }
}

