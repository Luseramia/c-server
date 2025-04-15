
using System.Dynamic;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using models.User;
using models.ManageFile;
using MySql.Data.MySqlClient;
using models.Product;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Controller;

public static class ControllerConfig
{
    public static async Task<IResult> Controller(ExpandoObject jsonBody, MySqlConnection connection, HttpContext context,List<Func<ExpandoObject, MySqlConnection, HttpContext, Task<(IResult Result, object Data)>>> models)
        {
            try
            {
            // เก็บค่า jsonBody เริ่มต้น
        var originalBody = new Dictionary<string, object>();
        foreach (var item in jsonBody as IDictionary<string, object>)
        {
            originalBody[item.Key] = item.Value;
        }
        var results = new Dictionary<string, object>(originalBody);
        var updatedJsonBody = new ExpandoObject() as IDictionary<string, object>;
        foreach (var item in originalBody)
        {
            updatedJsonBody[item.Key] = item.Value;
        }
        
        // Dictionary สำหรับเก็บผลลัพธ์รวม
  
        // ทำงานกับแต่ละ model
        foreach (var model in models)
        {
            var (result, data) = await model(updatedJsonBody as ExpandoObject, connection, context);
            // ตรวจสอบว่า result เป็น OK
          int statusCode = 0;
        var statusCodeProperty = result.GetType().GetProperty("StatusCode");
        if (statusCodeProperty != null)
        {
            statusCode = (int)statusCodeProperty.GetValue(result);
        }

        // ตรวจสอบว่าเป็น status code 200 (OK) หรือไม่
            if (statusCode == 200)
            {
                if (data is List<object> listData)
                {
                    var methodName = model.Method.Name;
                    Console.WriteLine(methodName);
                    results["data"] = listData;
                    updatedJsonBody[methodName] = listData;
                }
                else{
                    Console.WriteLine("test2");
                }
                // else if (data != null)
                // {
                //     // ใช้ชื่อ method เป็น key
                //     var modelName = model.Method.Name;
                //     results[modelName] = data;
                // }
            }
            else{
                Console.WriteLine($"Model returned non-OK result: {result.GetType().Name}");
            }
            Console.WriteLine(updatedJsonBody);
        }
       Console.WriteLine("\nResults after all models:");
        LogDictionary(results);

        // ทำ intersection กับ jsonBody ต้นฉบับ
                var finalResult = new Dictionary<string, object>(originalBody);
        foreach (var item in results)
        {
            finalResult[item.Key] = item.Value; // เพิ่มผลลัพธ์จาก model
        }

        
        return Results.Ok(finalResult);
                }
            
            catch (Exception ex)
            {
                Console.WriteLine($"Error in combined actions: {ex.Message}");
                return Results.StatusCode(500);
            }
        }
     private static void LogDictionary(Dictionary<string, object> dict)
{
    foreach (var item in dict)
    {
        Console.WriteLine($"Key: {item.Key}");
        
        if (item.Value is List<object> list)
        {
            Console.WriteLine($"  Value: List with {list.Count} items");
            if (list.Count > 0)
            {
                Console.WriteLine($"  First item type: {list[0].GetType().Name}");
                
                // ถ้าเป็น anonymous type จะมี property ให้ดู
                var props = list[0].GetType().GetProperties();
                if (props.Length > 0)
                {
                    Console.WriteLine($"  Properties: {string.Join(", ", props.Select(p => p.Name))}");
                    
                    // Log ค่าของ property แรกเป็นตัวอย่าง
                    if (props.Length > 0)
                    {
                        var firstProp = props[0];
                        Console.WriteLine($"  Sample value for {firstProp.Name}: {firstProp.GetValue(list[0])}");
                    }
                }
            }
        }
        else if (item.Value != null)
        {
            Console.WriteLine($"  Value type: {item.Value.GetType().Name} - {item.Value}");
        }
        else
        {
            Console.WriteLine($"  Value: null");
        }
    }
}
    }