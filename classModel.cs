public class FileData
{
    public string name { get; set; }
    public string type { get; set; }
    public long size { get; set; }
    public string content { get; set; }
}


public class ImageData
{
    public string fileName { get; set; }
    public byte[] imageData { get; set; } 
}


public class ProductData{
    public string productId { get; set;}
    public string productName { get; set;}
    public string productDescription { get; set;}
    public int productPrice { get; set;}
    public byte[] image { get; set;}
}