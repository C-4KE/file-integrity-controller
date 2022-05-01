using System.Management;

namespace FileIntegrityController
{
    /**
     * <summary>Класс, содержащий необходимые данные о носителях системы, чтобы было возможно было определить тип носителя.</summary>
     */
    public class StorageInfo
    {
        ManagementObjectSearcher _msftPartition;
        ManagementObjectSearcher _msftDisk;
        ManagementObjectSearcher _msftPhysicalDisk;

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
         * <value>Данные из класса MSFT_PhysicalDisk</value>
         */
        public ManagementObjectSearcher MSFTPhysicalDisk
        {
            get
            {
                return _msftPhysicalDisk;
            }
        }

        /**
         * <summary>Конструктор.</summary>
         */
        public StorageInfo()
        {
            _msftPartition = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", "SELECT * FROM MSFT_Partition");
            _msftDisk = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", "SELECT * FROM MSFT_Disk");
            _msftPhysicalDisk = new ManagementObjectSearcher("root\\Microsoft\\Windows\\Storage", "SELECT * FROM MSFT_PhysicalDisk");
        }
    }
}
