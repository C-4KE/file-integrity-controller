using System;
using System.Security.Cryptography;
using System.IO;
using System.Collections.Generic;

namespace FileIntegrityController
{
    /**
     * <summary>Класс, отвечающий за проверку целостности файлов.</summary>
     */
    public class IntegrityVerifier
    {
        /**
         * <summary>Метод, который проверяет целостность одного файла.</summary>
         * <param name="fileStream">Открытый поток файла.</param>
         * <param name="hash">Хэш файла для проверки целостности.</param>
         * <returns>Возвращает true, если файл не изменён (снова подсчитанный хэш равен данному в Json), иначе - возвращает false.</returns>
         */
        public static KeyValuePair<string, bool> VerifyFile(FileStream fileStream, string hash)
        {
            bool result = false;
            string actualHash;
            try
            {
                using (MD5 md5 = MD5.Create())
                {
                    byte[] byteHash = md5.ComputeHash(fileStream);
                    actualHash = BitConverter.ToString(byteHash);
                }
                result = (actualHash == hash);
            }
            catch (Exception exc)
            {
                Console.WriteLine("Error while trying to compute hash: " + exc.Message);
            }
            return new KeyValuePair<string, bool>(fileStream.Name, result);
        }
    }
}