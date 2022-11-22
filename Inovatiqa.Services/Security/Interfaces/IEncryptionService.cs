namespace Inovatiqa.Services.Security.Interfaces
{
    public interface IEncryptionService 
    {
        string CreateSaltKey(int size);

        string CreatePasswordHash(string password, string saltKey, string passwordFormat);
        
        string EncryptText(string plainText, string encryptionPrivateKey = "");

        string DecryptText(string cipherText, string encryptionPrivateKey = "");
    }
}
