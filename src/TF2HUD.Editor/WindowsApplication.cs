using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HUDEditor
{
    public class WindowsApplication : IApplication
    {
        public void Shutdown()
        {
            System.Windows.Application.Current.Shutdown();
        }
    }
}
