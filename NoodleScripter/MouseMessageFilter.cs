using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoodleScripter
{
    public class MouseMessageFilter : IMessageFilter, IDisposable
    {
        public MouseMessageFilter()
        {
        }

        public void Dispose()
        {
            StopFiltering();
        }

        #region IMessageFilter Members

        public bool PreFilterMessage(ref Message m)
        {
            Console.WriteLine(m.Msg);
            return false;
        }

        #endregion

        public void StartFiltering()
        {
            StopFiltering();
            Application.AddMessageFilter(this);
        }

        public void StopFiltering()
        {
            Application.RemoveMessageFilter(this);
        }
    }
}
