using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSSqlMcpDotNet.Tests;

public class CryptoHelperTest
{

    [Fact]
    public static void TestEncryptionDecryption()
    {
        string originalText = "Hello, World!";
        string encryptedText = CryptoHelper.Encrypt(originalText);
        string decryptedText = CryptoHelper.Decrypt(encryptedText);
        Assert.Equal(originalText, decryptedText);
    }
    



}