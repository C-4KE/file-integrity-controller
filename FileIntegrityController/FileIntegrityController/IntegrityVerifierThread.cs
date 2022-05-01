using System.Collections.Generic;
using System.Threading;

namespace FileIntegrityController
{
    /**
     * <summary>Класс, который будет использоваться для выполнения работы потоком при многопоточной проверке целостности.</summary>
     */
    public class IntegrityVerifierThread
    {
        // Поля
        private FileGroup _fileGroup;
        private List<string> _invalidFiles;
        private Thread _thread;

        /**
         * <value>Группа файлов.</value>
         */
        public FileGroup FileGroup
        {
            get
            {
                return _fileGroup;
            }
        }

        /**
         * <value>Список файлов, проваливших проверку на целостность.</value>
         */
        public List<string> InvalidFiles
        {
            get
            {
                return _invalidFiles;
            }
        }

        /**
         * <summary>Ссылка на поток, использующий данные этого объекта.</summary>
         */
        public Thread Thread
        {
            get
            {
                return _thread;
            }
            set
            {
                _thread = value;
            }
        }

        /**
         * <summary>Конструктор.</summary>
         * <param name="fileGroup">Группа файлов, которая будет проходить проверку на целостность.</param>
         */
        public IntegrityVerifierThread(FileGroup fileGroup)
        {
            _fileGroup = fileGroup;
            _invalidFiles = new List<string>();
        }

        /**
         * <summary>Метод, который будет запускаться потоком на выполнение.</summary>
         */
        public void VerifyFiles()
        {
            _invalidFiles = IntegrityVerifier.VerifyGroup(_fileGroup);
        }
    }
}
