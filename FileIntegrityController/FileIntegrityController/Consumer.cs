using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace FileIntegrityController
{
    public class Consumer
    {
        private BufferBlock<Task<KeyValuePair<string, bool>>> _consumerBuffer;
        private List<BufferBlock<Task<KeyValuePair<string, bool>>>> _producerBuffers;

        public Consumer(BufferBlock<Task<KeyValuePair<string, bool>>> consumerBuffer, List<BufferBlock<Task<KeyValuePair<string, bool>>>> producerBuffers)
        {
            _consumerBuffer = consumerBuffer;
            _producerBuffers = producerBuffers;
            Task[] completionTasks = new Task[_producerBuffers.Count];
            int count = 0;
            foreach (var buffer in _producerBuffers)
            {
                completionTasks[count] = buffer.Completion;
                count++;
            }
            Task.WhenAll(completionTasks).ContinueWith(_ => _consumerBuffer.Complete());
        }

        /**
         * <summary>Метод, принимающий задания на проверку от производителей и запускающий их.</summary>
         */
        async public void Execute()
        {
            while (await _consumerBuffer.OutputAvailableAsync())
            {
                while (_consumerBuffer.TryReceive(out Task<KeyValuePair<string, bool>> task))
                {
                    task.Start();
                }
            }
        }
    }
}
