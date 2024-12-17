using System.Security.Cryptography;
namespace AESEnCAndDeC;
public class AESEncryption
{

    public static byte[] EncryptStringToBytes_Aes(byte[] plainText, byte[] Key, byte[] IV)
    {
        // Check arguments.
        if (plainText == null || plainText.Length <= 0)
            throw new ArgumentNullException("plainText");
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException("Key");
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException("IV");

        byte[] encrypted;

        // Create an Aes object with the specified key and IV
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;
            aesAlg.Padding = PaddingMode.PKCS7;  // Set padding mode for AES

            // Create an encryptor to perform the stream transform
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for encryption
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    // Write all data to the stream
                    csEncrypt.Write(plainText, 0, plainText.Length);
                }

                encrypted = msEncrypt.ToArray();  // Get the encrypted data
            }
        }

        // Return the encrypted bytes from the memory stream
        return encrypted;
    }



}

public class AESDecryption
{
    public static byte[] DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
    {
        // Check arguments.
        if (cipherText == null || cipherText.Length <= 0)
            throw new ArgumentNullException("cipherText");
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException("Key");
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException("IV");

        byte[] plaintext;

        // Create an Aes object with the specified key and IV
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;
            aesAlg.Padding = PaddingMode.PKCS7;  // Set padding mode for AES

            // Create a decryptor to perform the stream transform
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for decryption
            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    // Use MemoryStream to hold the decrypted data
                    using (var msOutput = new MemoryStream())
                    {
                        csDecrypt.CopyTo(msOutput);  // Copy decrypted data to MemoryStream
                        plaintext = msOutput.ToArray();  // Return the decrypted byte array
                    }
                }
            }
        }

        return plaintext;  // Return the decrypted data
    }
}