using System;
using System.Collections.Generic;
using System.Management;
using System.Threading;

namespace FileIntegrityController
{
    /**
     * <summary>Класс, распределяющий проверку целостности в зависимости от типа носителя.</summary>
     */
    public class Distributor
    {
        /**
         * <summary>Метод, проверяющий является ли носитель SSD.</summary>
         * <remarks>
         * Метод сможет определить, является ли носитель SSD, только на Windows.
         * Для поиска используется следующая цепочка действий:
         * - По букве адреса ищется объект в коллекции объектов MSFT_Partition по полю DriveLetter. Из него берётся UniqueId.
         * Так как в этом объекте перед UniqueId находится число в фигурных скобках, то начало до } включая выбрачывается.
         * - По UniqueId ищется объект в коллекции объектов MSFT_Disk. Из него берётся SerialNumber.
         * - По SerialNumber ищется объект в коллекции объектов MSFT_PhysicalDisc. В найденном объекте берётся поле MediaType.
         * Если оно равно 4, то носитель - SSD. Иначе - нет.
         * </remarks>
         * <param name="fileGroup">Объект FileGroup, хранящий имя диска.</param>
         * <returns>Возвращает true, если носитель - SSD. Иначе - false.</returns>
         */
        public static bool IsSSD(FileGroup fileGroup)
        {
            bool isSSD = false;
            try
            {
                string driveName = fileGroup.DiskName.Split(new char[] { ':' })[0];
                string uniqueId = "";
                var rawDiskInfos = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", "SELECT * FROM MSFT_Partition");
                foreach (var rawDiskInfo in rawDiskInfos.Get())
                {
                    if (rawDiskInfo["DriveLetter"].ToString() == driveName)
                    {
                        uniqueId = rawDiskInfo["UniqueId"].ToString().Split(new char[] { '}' })[1];
                        break;
                    }
                }
                string serialNumber = "";
                rawDiskInfos = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", "SELECT * FROM MSFT_Disk");
                foreach (var rawDiskInfo in rawDiskInfos.Get())
                {
                    if (rawDiskInfo["UniqueId"].ToString() == uniqueId)
                    {
                        serialNumber = rawDiskInfo["SerialNumber"].ToString().Trim();
                        break;
                    }
                }
                rawDiskInfos = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", "SELECT * FROM MSFT_PhysicalDisk");
                foreach (var rawDiskInfo in rawDiskInfos.Get())
                {
                    if (rawDiskInfo["SerialNumber"].ToString().Trim() == serialNumber)
                    {
                        isSSD = (UInt16)rawDiskInfo["MediaType"] == 4;
                    }
                }
            }
            catch (Exception exc)
            {
                Console.WriteLine($"Error while checking {fileGroup.DiskName} disk: {exc.Message}");
            }
            return isSSD;
        }

        /**
         * <summary>Метод, распределяющий проверку целостности группы файлов по потокам.</summary>
         * <param name="fileGroup">Группа файлов, которая будет проверяться на целостность.</param>
         * <returns>Возвращает список файлов, не прошедших проверку на целостность.</returns>
         */
        public static List<string> DistributeToThreads(FileGroup fileGroup)
        {
            List<string> invalidFiles = new List<string>();
            int numberOfThreads = fileGroup.FilesHashes.Count / 2;
            if (numberOfThreads > 1)
            {
                numberOfThreads = numberOfThreads > Environment.ProcessorCount ? Environment.ProcessorCount : numberOfThreads;
                List<FileGroup> fileGroups = fileGroup.Split((uint)numberOfThreads);
                List<IntegrityVerifierThread> dataThreads = new List<IntegrityVerifierThread>();
                foreach (FileGroup group in fileGroups)
                {
                    dataThreads.Add(new IntegrityVerifierThread(group));
                }
                foreach (IntegrityVerifierThread dataThread in dataThreads)
                {
                    Thread newThread = new Thread(() => dataThread.VerifyFiles());
                    newThread.Start();
                    dataThread.Thread = newThread;
                }
                foreach (IntegrityVerifierThread dataThread in dataThreads)
                {
                    dataThread.Thread.Join();
                    invalidFiles.AddRange(dataThread.InvalidFiles);
                }
            }
            else
            {
                invalidFiles = IntegrityVerifier.VerifyGroup(fileGroup);
            }
            return invalidFiles;
        }
    }
}
