using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Security.Cryptography;

namespace FileIntegrityController
{
    /**
     * <summary>Класс, отвечающий за открытие файла, отправку задания на проверку целостности и возврат результата.</summary>
     */
    public class Producer
    {
        private FileGroup _fileGroup;
        private BufferBlock<Task> _producerBuffer;
        private BufferBlock<Task> _consumerBuffer;

        /**
         * <summary>Конструктор</summary>
         * <param name="consumerBuffer">Буфер потребителя, к которому будет подключен буфер производителя для передачи заданий.</param>
         * <param name="fileGroup">Группа файлов, лежащая на одном диске.</param>
         */
        public Producer(FileGroup fileGroup, BufferBlock<Task> consumerBuffer)
        {
            _fileGroup = fileGroup;
            _consumerBuffer = consumerBuffer;
            _producerBuffer = new BufferBlock<Task>();
            _producerBuffer.LinkTo(_consumerBuffer);
        }

        public BufferBlock<Task> ProducerBuffer
        {
            get
            {
                return _producerBuffer;
            }
        }

        /**
         * <summary>Метод, рассылающий задания на проверку целостности потребителю и возвращающий результат.</summary>
         * <returns>Словарь пар (путь_к_файлу : результат_проверку (true/false))</returns>
         */
        public Dictionary<string, bool> Execute()
        {
            Dictionary<string, bool> results = new Dictionary<string, bool>();
            foreach (KeyValuePair<string, string> fileHash in _fileGroup.FilesHashes)
            {
                try
                {
                    using (FileStream fileStream = new FileStream(fileHash.Key, FileMode.Open))
                    {
                        using (MD5 md5 = MD5.Create())
                        {
                            Task task = null;
                            byte[] buffer = new byte[4096];
                            int readAmount = 0;

                            // Чтение файла по кускам и отправка заданий на подсчёт хэша
                            while ((readAmount = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                task = new Task(() => IntegrityVerifier.CheckPart(md5, buffer, readAmount));
                                _producerBuffer.SendAsync(task);
                                if (task != null) task.Wait();
                            }
                            task.Wait();

                            // Отправка задания на непосредственно проверку
                            task = new Task<bool>(() => IntegrityVerifier.VerifyHash(md5, fileHash.Value));
                            _producerBuffer.SendAsync(task);
                            task.Wait();

                            // Добавление записи в результативный словарь
                            results.Add(fileHash.Key, ((Task<bool>)task).Result);
                        }
                    }
                }
                catch (Exception exc)
                {
                    Console.WriteLine($"Error while sending task to verify {fileHash.Key}: {exc.Message}");
                }
            }

            // Завершение работы с буфером отправки
            _producerBuffer.Complete();

            return results;
        }
    }
}
