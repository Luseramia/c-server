using System.Text.Json;
using MySql.Data.MySqlClient;
using Controllers;
namespace Controllers.ManageFile;
public class ManageFile{
public static async Task SaveImageMetadataToDatabase(ImageModel model,MySqlConnection connection){
        // var fileElement = (JsonElement)model.file;
        // var fileData = JsonSerializer.Deserialize<FileData>(fileElement.ToString()!);
        var fileContent = Convert.FromBase64String(model.file.content);
        DateTime currentDateTime = DateTime.Now;
        try{
         string sql = "INSERT INTO images (img_id,file_name, file_data, UploadedAt) VALUES (@img_id,@file_name, @file_data,@UploadedAt)";
        await using (MySqlCommand command = new MySqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@img_id", model.imageId);
            command.Parameters.AddWithValue("@file_name", model.file.name);
            command.Parameters.AddWithValue("@file_data", fileContent);
            command.Parameters.AddWithValue("@UploadedAt", currentDateTime);
            command.ExecuteNonQuery();
        }
        }
        
        catch{
            Results.BadRequest();
        }
}
public static async Task UpdateImageMetadataOnDatabase(ImageModel model,MySqlConnection connection){
        if (model.file == null || string.IsNullOrEmpty(model.file.content))
        {
            throw new ArgumentException("File content is missing");
        }
        var fileContent = Convert.FromBase64String(model.file.content);
        DateTime currentDateTime = DateTime.Now;
        try{
         string sql = @"UPDATE images
            SET 
                file_name = @file_name, 
                file_data = @file_data, 
                UploadedAt = @UploadedAt
            WHERE img_id = @img_id";
        await using (MySqlCommand command = new MySqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@img_id", model.imageId);
            command.Parameters.AddWithValue("@file_name", model.file.name);
            command.Parameters.AddWithValue("@file_data", fileContent);
            command.Parameters.AddWithValue("@UploadedAt", currentDateTime);
            await command.ExecuteNonQueryAsync();
        }
        }
        catch{
            Results.BadRequest();
        }
}
public static async Task DeleteImageMetadataOnDatabase(ImageModel model,MySqlConnection connection){
        try{
        string sql = "DELETE FROM images WHERE img_id = @img_id";
        await using (MySqlCommand command = new MySqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@img_id", model.imageId);
            await command.ExecuteNonQueryAsync();
        }
        }
        catch{
            Results.BadRequest();
        }
}
}

    // public class ImageModel{
    //     public string imgId { get; set; }
    //     public FileData file {get;set;}
    // }