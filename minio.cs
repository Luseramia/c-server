using System;
using Minio;
using Minio.Exceptions;
using Minio.DataModel;
using Minio.Credentials;
using Minio.DataModel.Args;
using System.Threading.Tasks;
using Minio.ApiEndpoints;

namespace FileUploader
{
    class FileUpload
    {
        public async static Task Run(IMinioClient minio)
        {
            var bucketName = "shopping";
            var objectName = "tttt.jpg";
            var filePath = "./tttt.jpg";
            var contentType = "image/jpeg";
            var fileFullPath = Path.GetFullPath("./tttt.jpg");
            if (!File.Exists(fileFullPath)) {
                Console.WriteLine($"❌ File not found: {fileFullPath}");
                return;
            }
            var fileInfo = new FileInfo(fileFullPath);
            Console.WriteLine($"📦 Uploading {fileInfo.Name}, size: {fileInfo.Length} bytes");
            try
            {
                // Make a bucket on the server, if not already present.
                var beArgs = new BucketExistsArgs()
                    .WithBucket(bucketName);
                bool found = await minio.BucketExistsAsync(beArgs).ConfigureAwait(false);
                if (!found)
                {
                    var mbArgs = new MakeBucketArgs()
                        .WithBucket(bucketName);
                    await minio.MakeBucketAsync(mbArgs).ConfigureAwait(false);
                }
           
            using (var fs = File.OpenRead(fileFullPath))
                {
                    var putObjectArgs = new PutObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectName)
                        .WithStreamData(fs)
                        .WithObjectSize(fs.Length)
                        .WithContentType(contentType);

                    await minio.PutObjectAsync(putObjectArgs);
                }
                // Upload a file to bucket.
                // var putObjectArgs = new PutObjectArgs()
                //     .WithBucket("shopping")
                //     .WithObject(objectName)
                //     .WithFileName(fileFullPath)
                //     .WithContentType(contentType);
                // await minio.PutObjectAsync(putObjectArgs).ConfigureAwait(false);
                Console.WriteLine("Successfully uploaded " + objectName );
                Console.WriteLine($"📄 Full path: {fileFullPath}");
                Console.WriteLine($"📦 Uploading {objectName} to bucket {bucketName}");
                
                var statArgs = new StatObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject("BAEK-JI-HEON-2025-New-Profile-documents-3.jpeg");
                var stat = await minio.StatObjectAsync(statArgs).ConfigureAwait(false);
                Console.WriteLine($"✅ Object confirmed on server: {stat.ObjectName}, Size: {stat.Size}");

            }
            catch (MinioException e)
            {
                Console.WriteLine("File Upload Error: {0}", e.Message);
            }
        }

    }
}