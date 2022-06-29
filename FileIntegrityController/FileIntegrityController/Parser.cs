using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace FileIntegrityController
{
    /**
     * <summary>Класс, считывающий пары (имя_файла : хэш) из Json файла, а также сортирующий пары по дискам.</summary>
     */
    public class Parser
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /**
         * <summary>Метод, который считывает пары (имя_файла : хэш) из JSON'а.</summary>
         * <param name="jsonPath">Путь к JSON файлу.</param>
         * <returns>Словарь с парами (имя_файла : хэш).</returns>
         */
        public static Dictionary<string, string> ParseJSON(string jsonPath)
        {
            Dictionary<string, string> fileHash;
            if (File.Exists(jsonPath))
            {
                string jsonString = File.ReadAllText(jsonPath);
                if (jsonString.Length != 0)
                {
                    try
                    {
                        fileHash = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
                    }
                    catch (Exception exc)
                    {
                        logger.Error(exc, "Failed to deserialize Json string");
                        fileHash = null;
                    }
                }
                else
                {
                    logger.Info("There are no <file_name : hash> pairs in Json file.");
                    fileHash = null;
                }
            }
            else
            {
                logger.Warn("File \"" + jsonPath + "\" does not exist.");
                fileHash = null;
            }
            return fileHash;
        }

        /**
         * <summary>Метод, сортирующий словарь с парами (имя_файла : хэш) по дискам.</summary>
         * <remarks>Если входной словарь не пустой, но содержит пары с несуществующими файлами / некорректными ключами, то они не добавляются в выходной массив.</remarks>
         * <param name="filesHashes">Словарь с парами (имя_файла : хэш).</param>
         * <returns>Возвращает лист объектов FileGroup, каждый из которых хранит информацию о файлах с одного диска. Если входной словарь пустой, возвращает null.</returns>
         */
        public static List<FileGroup> SortFilesByDisks(Dictionary<string, string> filesHashes)
        {
            if (filesHashes.Count != 0)
            {
                List<FileGroup> fileGroups = new List<FileGroup>();
                Dictionary<string, FileGroup> volumeGroup = new Dictionary<string, FileGroup>();
                foreach (KeyValuePair<string, string> fileHash in filesHashes)
                {
                    try
                    {
                        string driveName = GetDriveName(fileHash.Key);
                        if (driveName != null)
                        {
                            string volume = driveName;
                            if (volumeGroup.ContainsKey(volume))    // Уже встречали файл на этом разделе
                            {
                                FileGroup fileGroup;
                                volumeGroup.TryGetValue(volume, out fileGroup);
                                fileGroup.FilesHashes.Add(fileHash.Key, fileHash.Value);
                            }
                            else   // Ещё не встречали файл на этом разделе
                            {
                                string serialNumber = (new StorageInfo()).GetDiskSerialNumber(driveName);
                                if (fileGroups.Count == 0)
                                {
                                    Dictionary<string, string> newDict = new Dictionary<string, string>();
                                    newDict.Add(fileHash.Key, fileHash.Value);
                                    FileGroup newGroup = new FileGroup(serialNumber, newDict);
                                    fileGroups.Add(newGroup);
                                    volumeGroup.Add(volume, newGroup);
                                }
                                else
                                {
                                    bool isGroupExists = false;
                                    foreach (FileGroup fileGroup in fileGroups)
                                    {
                                        if (fileGroup.DiskSerialNumber == serialNumber)     // Группа файлов, у которой серийный номер диска совпадает с серийным номер диска, на котором определён раздел, существует
                                        {
                                            fileGroup.FilesHashes.Add(fileHash.Key, fileHash.Value);
                                            volumeGroup.Add(volume, fileGroup);
                                            isGroupExists = true;
                                            break;
                                        }
                                    }
                                    if (!isGroupExists)     // Нет группы с тем же серийным номером, что и серийный номер диска, на котором находится раздел
                                    {
                                        Dictionary<string, string> newDict = new Dictionary<string, string>();
                                        newDict.Add(fileHash.Key, fileHash.Value);
                                        FileGroup newGroup = new FileGroup(serialNumber, newDict);
                                        fileGroups.Add(newGroup);
                                        volumeGroup.Add(volume, newGroup);
                                    }
                                }
                            }
                        }
                        else
                        {
                            logger.Info("Failed to add pair <" + fileHash.Key + "; " + fileHash.Value + ">: this file does not exist.");
                        }
                    }
                    catch (Exception exc)
                    {
                        logger.Error(exc, "Failed to add pair <{Key}; {Value}>", fileHash.Key, fileHash.Value);
                    }
                }
                return fileGroups;
            }
            else
            {
                logger.Info("Failed to sort files\' hashes by disks: dictionary is empty.");
                return new List<FileGroup>();
            }
        }

        /**
         * <summary>Метод, возвращающий информацию о носителе, на котором находится файл.</summary>
         * <param name="path">Путь к файлу</param>
         * <returns>Имя раздела, если файл существует. В обратном случае - null.</returns>
         */
        public static string GetDriveName(string path)
        {
            return File.Exists(path) ? (new DriveInfo((new FileInfo(path)).Directory.Root.FullName)).Name : null;
        }
    }
}