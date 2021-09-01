using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HUDEditor
{
    public class WindowsFolderBrowserDialog : IFolderBrowserDialog
    {
        private readonly FolderBrowserDialog _dialog;

        public WindowsFolderBrowserDialog()
        {
            _dialog = new FolderBrowserDialog();
        }

        public DialogResult ShowDialog()
        {
            return _dialog.ShowDialog();
        }

        public string SelectedPath
        {
            get { return _dialog.SelectedPath; }
            set { _dialog.SelectedPath = value; }
        }

        public string Description
        {
            get { return _dialog.Description; }
            set { _dialog.Description = value; }
        }

        public bool UseDescriptionForTitle
        {
            get { return _dialog.UseDescriptionForTitle; }
            set { _dialog.UseDescriptionForTitle = value; }
        }

        public bool ShowNewFolderButton
        {
            get { return _dialog.ShowNewFolderButton; }
            set { _dialog.ShowNewFolderButton = value; }
        }

        public void Dispose()
        {
            _dialog.Dispose();
        }
    }
}
