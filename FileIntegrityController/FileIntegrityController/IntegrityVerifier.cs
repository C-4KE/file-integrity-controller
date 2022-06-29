using System;
using System.Security.Cryptography;

namespace FileIntegrityController
{
    /**
     * <summary>Класс, отвечающий за проверку целостности файлов.</summary>
     */
    public class IntegrityVerifier
    {
        /**
         * <summary>Метод, проверяющий кусок файла.</summary>
         * <param name="md5">Объект, выполняющий вычисление хэша и накапливающий его.</param>
         * <param name="part">Байтовый кусок файла.</param>
         * <param name="readAmount">Количество байтов в буфере, считанных на шаге.</param>
         */
        public static void CheckPart(MD5 md5, byte[] part, int readAmount)
        {
            md5.TransformBlock(part, 0, readAmount, part, 0);
        }

        /**
         * <summary>Метод, производящий завершение формирования хэша и возвращающий его.</summary>
         * <param name="md5">Объект md5, хранящий в себе незавершенный (не применён метод TransformFinalBlock) хэш всех кусков файла.</param>
         * <returns>Строка хэша.</returns>
         */
        public static string GetFullHash(MD5 md5)
        {
            md5.TransformFinalBlock(new byte[] { }, 0, 0);
            return BitConverter.ToString(md5.Hash);
        }

        /**
         * <summary>Метод, который проверяет целостность одного файла.</summary>
         * <param name="md5">Объект md5, хранящий в себе незавершенный (не применён метод TransformFinalBlock) хэш всех кусков файла.</param>
         * <param name="hash">Хэш для сравнения.</param>
         * <returns>Возвращает true, если файл не изменялся, иначе - возващает false.</returns>
         */
        public static bool VerifyHash(MD5 md5, string hash)
        {
            return GetFullHash(md5) == hash;
        }
    }
}