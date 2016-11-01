using System;
using System.Threading;
using HostingWindowsForms.Host;

namespace HostingWindowsForms.EPDM
{
    public class MyProcess
    {
        private readonly ManualResetEvent _doneEvent;

        public MyProcess(ManualResetEvent doneEvent)
        {
            _doneEvent = doneEvent;
        }

        public void MyProcessThreadPoolCallback(object threadContext)
        {
            int threadIndex = (int)threadContext;

            Program.HostForm.richTextBoxLog.Invoke(new Action(() => Program.HostForm.richTextBoxLog.SelectedText += ("thread {0} started..." + threadIndex + "\r\n")));
            StartProcess();
            Program.HostForm.richTextBoxLog.Invoke(new Action(() => Program.HostForm.richTextBoxLog.SelectedText += ("thread {0} end..." + threadIndex + "\r\n")));

            // Indicates that the process had been completed
            _doneEvent.Set();
        }

        public void StartProcess()
        {
            var c = new ClassOfTasks();

            c.Taskes();
        }
    }
}