using System;
using System.Windows.Forms;
using VNTU2.Models;
using VNTU2.Services;

namespace VNTU2
{
    public partial class SettingsForm : Form
    {
        public delegate void LoadSelectedFiles(string path);
        public delegate void RefreshSelectedFiles();

        public LoadSelectedFiles OnLoadSelectedFiles;
        public RefreshSelectedFiles OnRefreshSelectedFiles;
        
        private readonly AppSettingsService _appSettingsService;
        
        public SettingsForm()
        {
            InitializeComponent();
            _appSettingsService = new AppSettingsService();
            this.MaximizeBox = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var path = string.Empty;
            
            using (var folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                    path = folderDialog.SelectedPath;
            };
            
            var appSetting = new AppSetting
            {
                FilePath = path
            };
            _appSettingsService.Write(appSetting);
            OnRefreshSelectedFiles?.Invoke();
            OnLoadSelectedFiles?.Invoke(path);
        }
    }
}