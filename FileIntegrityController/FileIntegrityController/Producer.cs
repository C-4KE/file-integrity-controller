using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace FileIntegrityController
{
    /**
     * <summary>Класс, отвечающий за открытие файла, отправку задания на проверку целостности и возврат результата.</summary>
     */
    public class Producer
    {
        private FileGroup _fileGroup;
        private BufferBlock<Task<KeyValuePair<string, bool>>> _producerBuffer;
        private BufferBlock<Task<KeyValuePair<string, bool>>> _consumerBuffer;

        public Producer(FileGroup fileGroup, BufferBlock<Task<KeyValuePair<string, bool>>> consumerBuffer)
        {
            _fileGroup = fileGroup;
            _consumerBuffer = consumerBuffer;
            _producerBuffer = new BufferBlock<Task<KeyValuePair<string, bool>>>();
            _producerBuffer.LinkTo(_consumerBuffer);
        }

        public BufferBlock<Task<KeyValuePair<string, bool>>> ProducerBuffer
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
            List<Task<KeyValuePair<string, bool>>> tasks = new List<Task<KeyValuePair<string, bool>>>();
            List<FileStream> fileStreams = new List<FileStream>();
            
            // Открытие файловых потоков и отправка заданий потребителю
            foreach (KeyValuePair<string, string> fileHash in _fileGroup.FilesHashes)
            {
                try
                {
                    FileStream fileStream = new FileStream(fileHash.Key, FileMode.Open);
                    Task<KeyValuePair<string, bool>> newTask = new Task<KeyValuePair<string, bool>>(() => IntegrityVerifier.VerifyFile(fileStream, fileHash.Value));
                    _producerBuffer.SendAsync(newTask);
                    tasks.Add(newTask);
                    fileStreams.Add(fileStream);
                }
                catch (Exception exc)
                {
                    Console.WriteLine($"Error while sending task to verify {fileHash.Key}: {exc.Message}");
                }
            }

            // Завершение работы с буфером отправки
            _producerBuffer.Complete();

            // Ожидание результатов
            Task.WaitAll(tasks.ToArray());

            // Закрытие файловых потоков
            foreach (FileStream stream in fileStreams)
            {
                try
                {
                    stream.Close();
                }
                catch (Exception exc)
                {
                    Console.WriteLine($"Error while closing FileStream: {exc.Message}");
                }
            }

            // Формирование и возврат результата
            Dictionary<string, bool> results = new Dictionary<string, bool>();
            foreach (Task<KeyValuePair<string, bool>> task in tasks)
            {
                KeyValuePair<string, bool> result = task.Result;
                results.Add(result.Key, result.Value);
            }
            return results;
        }
    }
}
