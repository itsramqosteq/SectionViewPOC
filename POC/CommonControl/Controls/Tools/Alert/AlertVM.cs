using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace POC
{
    public class AlertVM : ViewModelBase
    {
        private MessageBoxResult _messageBoxResult;
        public MessageBoxResult MessageBoxResult
        {
            
                get => _messageBoxResult;
                set => SetProperty(ref _messageBoxResult, value);
            
        }
    }
}
