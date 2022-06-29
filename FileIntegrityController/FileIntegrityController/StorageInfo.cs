using System;
using System.Management;

namespace FileIntegrityController
{
    /**
     * <summary>Класс, содержащий необходимые данные о носителях системы, чтобы было возможно было определить тип носителя.</summary>
     */
    public class StorageInfo
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        ManagementObjectSearcher _msftPartition;
        ManagementObjectSearcher _msftDisk;

        /**
         * <value>Данные из класса MSFT_Partition</value>
         */
        public ManagementObjectSearcher MSFTPartition
        {
            get
            {
                return _msftPartition;
            }
        }

        /**
         * <value>Данные из класса MSFT_Disk</value>
         */
        public ManagementObjectSearcher MSFTDisk
        {
            get
            {
                return _msftDisk;
            }
        }

        /**
         * <summary>Конструктор.</summary>
         */
        public StorageInfo()
        {
            _msftPartition = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", "SELECT * FROM MSFT_Partition");
            _msftDisk = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", "SELECT * FROM MSFT_Disk");
        }

        /**
         * <summary>Метод, возвращающий серийный номер диска по разделу.</summary>
         * <remarks>
         * Работает только на Windows.
         * Для поиска используется следующая цепочка действий:
         * - По букве адреса ищется объект в коллекции объектов MSFT_Partition по полю DriveLetter. Из него берётся UniqueId.
         * Так как в этом объекте перед UniqueId находится число в фигурных скобках, то начало до } включая выбрасывается.
         * - По UniqueId ищется объект в коллекции объектов MSFT_Disk. Из него берётся SerialNumber.
         * </remarks>
         * <param name="volumeName">Название раздела.</param>
         * <returns>Возвращает серийный номер диска.</returns>
         */
        public string GetDiskSerialNumber(string volumeName)
        {
            string serialNumber = "";
            try
            {
                string driveName = volumeName.Split(new char[] { ':' })[0];
                string uniqueId = "";
                foreach (var rawDiskInfo in _msftPartition.Get())
                {
                    if (rawDiskInfo["DriveLetter"].ToString() == driveName)
                    {
                        uniqueId = rawDiskInfo["UniqueId"].ToString().Split(new char[] { '}' })[1];
                        break;
                    }
                }
                foreach (var rawDiskInfo in _msftDisk.Get())
                {
                    if (rawDiskInfo["UniqueId"].ToString() == uniqueId)
                    {
                        serialNumber = rawDiskInfo["SerialNumber"].ToString().Trim();
                        break;
                    }
                }
            }
            catch (Exception exc)
            {
                logger.Error(exc, "Error while checking {volumeName} disk", volumeName);
            }
            return serialNumber;
        }
    }
}