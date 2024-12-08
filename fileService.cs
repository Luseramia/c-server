

using System.IO;

public class FileService
{
    public string SaveImageToFileSystem(string fileName, byte[] fileContent)
    {
        // สร้างชื่อไฟล์แบบสุ่ม
        // string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
        // string fullPath = Path.Combine(uploadPath, fileName);

        // บันทึกไฟล์ลงใน File System
        // using (var stream = new FileStream(fullPath, FileMode.Create))
        // {
        //     file.CopyTo(stream);
        // }
        var uploadPath = Path.Combine(Directory.GetCurrentDirectory(),"uploads", fileName);
        File.WriteAllBytes(uploadPath, fileContent);

        return fileName; // Return ชื่อไฟล์เพื่อนำไปเก็บในฐานข้อมูล
    }

    public void DeleteImageFromFileSystem(string fileName, string uploadPath)
    {
        string fullPath = Path.Combine(uploadPath, fileName);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath); // ลบไฟล์ออกจาก File System
        }
    }
}