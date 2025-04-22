using System.Collections;
using System.Reflection;

namespace WebApplication1.Extensions;

public static  class PrintProperties
{
    public static void PrintAllMembers<T>(this T obj, int indentLevel = 0)
    {
        if (obj == null)
        {
            PrintIndent(indentLevel);
            Console.WriteLine("Object is null");
            return;
        }

        Type type = obj.GetType();
        PrintIndent(indentLevel);
        Console.WriteLine($"Object type: {type.Name}");

        // 列印所有屬性
        foreach (PropertyInfo prop in type.GetProperties())
        {
            try
            {
                var propValue = prop.GetValue(obj, null);
                PrintIndent(indentLevel);
                Console.WriteLine($"{prop.Name} (Property): {propValue}");
                // 如果是可枚舉類型，遞歸列印
                if (propValue is IEnumerable && !(propValue is string))
                {
                    foreach (var item in (IEnumerable)propValue)
                    {
                        PrintAllMembers(item, indentLevel + 1);
                    }
                }
            }
            catch (Exception ex)
            {
                PrintIndent(indentLevel);
                Console.WriteLine($"{prop.Name} (Property): Could not retrieve value - {ex.Message}");
            }
        }

        // 列印所有字段
        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            try
            {
                var fieldValue = field.GetValue(obj);
                PrintIndent(indentLevel);
                Console.WriteLine($"{field.Name} (Field): {fieldValue}");

                // 特別處理 FormCollection 或其他集合類型
                if (fieldValue is Microsoft.AspNetCore.Http.FormCollection formCollection)
                {
                    PrintIndent(indentLevel);
                    Console.WriteLine("FormCollection fields:");
                    foreach (var key in formCollection.Keys)
                    {
                        PrintIndent(indentLevel + 1);
                        Console.WriteLine($"{key}: {formCollection[key]}");
                    }
                }
                else if (fieldValue is IEnumerable && !(fieldValue is string))
                {
                    foreach (var item in (IEnumerable)fieldValue)
                    {
                        PrintAllMembers(item, indentLevel + 1);
                    }
                }
            }
            catch (Exception ex)
            {
                PrintIndent(indentLevel);
                Console.WriteLine($"{field.Name} (Field): Could not retrieve value - {ex.Message}");
            }
        }
    }

    private static void PrintIndent(int level)
    {
        Console.Write(new string(' ', level * 2));
    }
}
