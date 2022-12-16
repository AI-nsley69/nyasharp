namespace nyasharp;

public class Events
{
    public delegate void PrintHandler(string print);

    public class PrintWorker
    {
        private PrintHandler _handler;
        public event PrintHandler OnPrint
        {
            add
            {
                _handler = (PrintHandler)Delegate.Combine(_handler, value);
            }

            remove
            {
                _handler = (PrintHandler)Delegate.Remove(_handler, value);
            }
        }

        public void Invoke(string str)
        {
            _handler.Invoke(str);
        }
    }

    public delegate void ErrorHandler(string err);

    public class ErrorWorker
    {
        private ErrorHandler _handler;
        public event ErrorHandler OnError {
            add
            {
                _handler = (ErrorHandler)Delegate.Combine(_handler, value);
            }

            remove
            {
                _handler = (ErrorHandler)Delegate.Remove(_handler, value);
            }
        }
        
        public void Invoke(string err)
        {
              _handler.Invoke(err);  
        }
    }
}