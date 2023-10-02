using System;
using System.IO;
using System.Windows.Forms;
using NAudio.Wave;
using System.Drawing;
using System.Linq;
using VNTU2.Enums;
using VNTU2.Services;

namespace VNTU2
{
    public partial class Form1 : Form
    {
        private readonly Timer _timer;
        private WaveOut _outputDevice; 
        private AudioFileReader _audioFile; 
        private readonly string _selectedFolder;
        private readonly SettingsForm _settingsForm;
        private ListenTypes _listenType;

        public Form1()
        {
            InitializeComponent();
            SoundBar.Scroll += SoundBar_Scroll;
            progressTrackBar.Scroll += progressTrackBar_Scroll;
            SoundBar.Maximum = 200;

            _timer = new Timer();
            _timer.Interval = 1000;
            _timer.Tick += timer_Tick;
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            this.KeyPreview = true;

            _settingsForm = new SettingsForm();
            _settingsForm.OnRefreshSelectedFiles += RefreshFiles;
            _settingsForm.OnLoadSelectedFiles += LoadFiles;
            var appSettingsService = new AppSettingsService();
            _selectedFolder = appSettingsService.Read().FilePath;
            LoadFiles(_selectedFolder);
        }

        private void DisplayAlbumArt(string audioFilePath)
        {
            try
            {
                using (var file = TagLib.File.Create(audioFilePath))
                {
                    if (file.Tag.Pictures.Length > 0)
                    {
                        var picture = file.Tag.Pictures[0];
                        
                        using (var stream = new MemoryStream(picture.Data.Data))
                        {
                            pictureBox1.Image = new Bitmap(stream);
                        }
                    }
                    else
                    {
                        pictureBox1.Image = null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка при извлечении обложки: " + ex.Message);
            }
        }
        
        private void PlayNextSong()
        {
            if (listBox1.SelectedIndex < listBox1.Items.Count - 1)
            {
                listBox1.SelectedIndex++;
            }
            else
            {
                listBox1.SelectedIndex = 0;
            }
            
            string selectedSong = listBox1.SelectedItem.ToString();
            string songPath = Path.Combine(_selectedFolder, selectedSong);
            InitializePlayback(songPath);
        }
        
        private void PlayPreviousSong()
        {
            if (listBox1.SelectedIndex > 0)
            {
                listBox1.SelectedIndex--;
            }
            else
            {
                listBox1.SelectedIndex = listBox1.Items.Count -1;
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            var folderPath = string.Empty;
            
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    folderPath = folderDialog.SelectedPath;
                    
                    listBox1.Items.Clear();

                    LoadFiles(folderPath);
                }
            }
        }

        private void LoadFiles(string path)
        {
            var files = Directory.GetFiles(path, "*.mp3");
            foreach (var file in files)
            {
                var trackName = Path.GetFileNameWithoutExtension(file);
                listBox1.Items.Add(trackName);
            }
        }

        private void RefreshFiles()
        {
            listBox1.Items.Clear();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                string selectedSong = listBox1.SelectedItem.ToString();
                
                string songPath = Path.Combine(_selectedFolder, selectedSong);

                if (System.IO.File.Exists(songPath))
                {
                    if (_outputDevice != null)
                    {
                        _outputDevice.Stop();
                        _outputDevice.Dispose();
                        _outputDevice = null;
                    }
                    
                    _audioFile = new AudioFileReader(songPath);
                    _outputDevice = new WaveOut();
                    _outputDevice.Init(_audioFile);
                    _outputDevice.Play();
                    
                    progressTrackBar.Maximum = (int)_audioFile.TotalTime.TotalSeconds;
                    
                    _outputDevice.PlaybackStopped += OutputDevice_PlaybackStopped;
                    
                    label1.Text = Path.GetFileNameWithoutExtension(selectedSong);
                    
                    DisplayAlbumArt(songPath);
                    
                    _timer.Start();
                }
            }
        }

        private void buttonPlay_Click(object sender, EventArgs e)
        {
            if (_outputDevice != null)
            {
                if (_outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    _outputDevice.Pause();
                }
                else if (_outputDevice.PlaybackState == PlaybackState.Paused)
                {
                    _outputDevice.Play();
                }
            }
        }

        private void SoundBar_Scroll(object sender, EventArgs e)
        {
            if (_outputDevice != null)
            {
                float volume = (float)SoundBar.Value / SoundBar.Maximum;

                _outputDevice.Volume = volume;
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (_audioFile != null && _outputDevice != null)
            {
                progressTrackBar.Value = (int)_audioFile.CurrentTime.TotalSeconds;

                if (_audioFile.CurrentTime >= _audioFile.TotalTime)
                {
                    _timer.Stop();
                }

                label3.Text = $"{_audioFile.CurrentTime:mm\\:ss} / {_audioFile.TotalTime:mm\\:ss}";
            }
        }

        private void progressTrackBar_Scroll(object sender, EventArgs e)
        {
            if (_audioFile != null)
            {
                _audioFile.CurrentTime = TimeSpan.FromSeconds(progressTrackBar.Value);
            }
        }

        private void buttonRight_Click(object sender, EventArgs e)
        {
            PlayNextSong();
            listBox1_SelectedIndexChanged(sender, e);
        }

        private void buttonLeft_Click(object sender, EventArgs e)
        {
            if (_outputDevice != null)
            {
                if (_outputDevice.PlaybackState == PlaybackState.Playing)
                {
                    double currentTimeSeconds = _audioFile.CurrentTime.TotalSeconds;

                    if (currentTimeSeconds < 3)
                    {
                        PlayPreviousSong();
                        listBox1_SelectedIndexChanged(sender, e);
                    }
                    else
                    {
                        _outputDevice.Stop();
                        _outputDevice.Dispose();
                        _outputDevice = null;

                        string selectedSong = listBox1.SelectedItem.ToString();
                        string songPath = Path.Combine(_selectedFolder, selectedSong);

                        InitializePlayback(songPath);
                    }
                }
                else if (_outputDevice.PlaybackState == PlaybackState.Paused)
                {
                    _outputDevice.Play();
                }
            }
        }
        private void buttonSettings_Click(object sender, EventArgs e)
        {
            _settingsForm.ShowDialog();
        }
        
        private void OutputDevice_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (e.Exception == null)
            {
                PlayNextSong();
                listBox1_SelectedIndexChanged(sender, e);
            }
        }
        
        private void InitializePlayback(string songPath)
        {
            if (System.IO.File.Exists(songPath))
            {
                if (_outputDevice != null)
                {
                    _outputDevice.Stop();
                    _outputDevice.Dispose();
                    _outputDevice = null;
                }

                _audioFile = new AudioFileReader(songPath);
                _outputDevice = new WaveOut();
                _outputDevice.Init(_audioFile);

                _outputDevice.PlaybackStopped += OutputDevice_PlaybackStopped;

                progressTrackBar.Maximum = (int)_audioFile.TotalTime.TotalSeconds;

                label1.Text = Path.GetFileNameWithoutExtension(songPath);
                DisplayAlbumArt(songPath);

                _outputDevice.Play();

                _timer.Start();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            Image imageToShow = pictureBox1.Image;

            PhotoForm fullScreenForm = new PhotoForm(imageToShow);

            fullScreenForm.ShowDialog();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            var textBox = (TextBox)sender;
            var searchedFiles = Directory.GetFiles(_selectedFolder)
                .Where(f => Path.GetFileNameWithoutExtension(f).Contains(textBox.Text))
                .Select(f => (object)Path.GetFileNameWithoutExtension(f))
                .ToArray();
            listBox1.Items.Clear();
            listBox1.Items.AddRange(searchedFiles);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            var length = Enum.GetNames(typeof(ListenTypes)).Length;
            var currentIndex = (int)_listenType;

            if (currentIndex < length-1)
            {
                _listenType = ++_listenType;
            }
            else
            {
                _listenType = ListenTypes.Default;
            }
            
            switch(_listenType)
            {
                case ListenTypes.Default:
                    this.button2.Text = ">>";
                    break;
                case ListenTypes.Random:
                    this.button2.Text = "?";
                    break;
                case ListenTypes.Repeat:
                    this.button2.Text = ")(";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_listenType));
            }
        }
    }
}
