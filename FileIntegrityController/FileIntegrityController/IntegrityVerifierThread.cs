using System;
using System.Collections.Generic;
using System.Text;

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
         * <value>Список фалов, проваливших проверку на целостность.</value>
         */
        public List<string> InvalidFiles
        {
            get
            {
                return _invalidFiles;
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
