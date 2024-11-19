using MySql.Data.MySqlClient;
using models.User;
using Microsoft.AspNetCore.Http;
using Mysqlx.Expr;
using System.Runtime.CompilerServices;

namespace SQLtest{
class SQLQuery
{
public static IResult Query(Func<IResult> func)
{
    try
    {
        // Call the function (Login) passed as the delegate
        return func();
    }
    catch (MySqlException e)
    {
                Console.WriteLine($"MySQL Exception: {e.Message}");
        return Results.Problem("Database error occurred.");
    }
        catch (Exception ex)
    {
        Console.WriteLine($"General Exception: {ex.Message}");
        return Results.Problem($"An error occurred: {ex.Message}");
    }
}

}
}
