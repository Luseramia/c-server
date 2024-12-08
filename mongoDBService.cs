using System.Net.Http.Headers;
using MongoDB.Bson;
using MongoDB.Driver;
using Routes;

namespace MongoDBService;

public class MongoDBConnection
{
    public static async Task<bool> InsertData(string id, byte[] data)
    {
        var client = new MongoClient("mongodb://fileManager:FROMIS_9@192.168.1.53:27017/FileStorage");
        var database = client.GetDatabase("FileStorage");
        var collection = database.GetCollection<BsonDocument>("Images");
        try
        {
            var document = new BsonDocument{
            { "img_id", id},
            { "data", data },
        };
            await collection.InsertOneAsync(document);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return false;
        }
        // var list = await collection.Find(new BsonDocument("Name", "Jack"))
        //     .ToListAsync();

        // foreach (var document in list)
        // {
        //     Console.WriteLine(document["Name"]);
        // }

    }
    public static async Task<List<BsonDocument>>  FindData(string id )
    {
        var client = new MongoClient("mongodb://fileManager:FROMIS_9@192.168.1.53:27017/FileStorage");
        var database = client.GetDatabase("FileStorage");
        var collection = database.GetCollection<BsonDocument>("Images");
        try
        {
            List<BsonDocument> list = await collection.Find(new BsonDocument("img_id", id))
    .ToListAsync();

           return list;
        }
        catch (Exception ex)
        {
            
            Console.WriteLine(ex.Message);
            return new List<BsonDocument>();
        }
     

    }

        public static async Task DeleteData(string id)
    {
        var client = new MongoClient("mongodb://fileManager:FROMIS_9@192.168.1.53:27017/FileStorage");
        var database = client.GetDatabase("FileStorage");
        var collection = database.GetCollection<BsonDocument>("Images");
        try
        {
            await collection.DeleteOneAsync(id);

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
     

    }
}
