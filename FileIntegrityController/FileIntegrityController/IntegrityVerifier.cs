using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;

namespace FileIntegrityController
{
    /**
     * <summary>Класс, отвечающий за проверку целостности файлов.</summary>
     */
    public class IntegrityVerifier
    {
        /**
         * <summary>Метод, который проверяет целостность одного файла.</summary>
         * <param name="fileHash">Пара (путь_к_файлу : хеш)</param>
         * <returns>Вовзращает true, если файл не изменён (снова подсчитанный хеш равен данному в Json), иначе - возвращает false.</returns>
         */
        public static bool VerifyFile(KeyValuePair<string, string> fileHash)
        {
            bool result = false;
            if (File.Exists(fileHash.Key))
            {
                string actualHash;
                try
                {
                    using (MD5 md5 = MD5.Create())
                    {
                        using (FileStream fstream = new FileStream(fileHash.Key, FileMode.Open))
                        {
                            byte[] byteHash = md5.ComputeHash(fstream);
                            actualHash = BitConverter.ToString(byteHash);
                        }
                    }
                    result = (actualHash == fileHash.Value);
                }
                catch (Exception exc)
                {
                    Console.WriteLine("Error while trying to compute hash: " + exc.Message);
                }
            }
            return result;
        }
    }
}
