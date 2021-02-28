using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using NAudio.Wave;
using System.Windows.Controls;
using Path = System.IO.Path;

namespace Projekt
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private WaveOut outputDevice;
        private Audio currentAudioFile;
        private Playlist currentPlaylist;
        private string selectedPlaylist;
        private string selectedPlaylistAudio;
        private ListPlaylists listOfPlaylists;
        private TreeViewItem currentFileNode = null;
        private TreeViewItem selectedFileNode = null;
        private TreeViewItem selectedDirectoryNode = null;
        private bool userIsDraggingSlider = false;
        private int trackTotalTime;
        private bool isStop;
        private bool isPause;
        private bool isPlay;
        private bool isAudio = false;
        private bool isPlaylist = false;
        private bool isPlayingPlaylist = false;
        private bool isSelectedPlaylistAudio = false;
        private bool isSelectedPlaylist = false;
        public string folderName = null;

        public MainWindow()
        {
            InitializeComponent();
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += TimerTick;
            timer.Start();
            textCurrentPlaylist.Visibility = Visibility.Hidden;
            titlePlaylist.Visibility = Visibility.Hidden;
            OpenAtBeginning();

        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (outputDevice != null)
                if ((!isStop) && (!isPause) && (!userIsDraggingSlider))
                {
                    durationSlider.Minimum = 0;
                    durationSlider.Maximum = trackTotalTime;
                    int currentTime = (int)(currentAudioFile.GetCurrentPosition() * 1d / outputDevice.OutputWaveFormat.AverageBytesPerSecond);
                    if (currentTime >= trackTotalTime && isPlayingPlaylist)
                    {
                        RoutedEventArgs args = new RoutedEventArgs();
                        NextSong(sender, args);
                    }
                    if (currentTime >= trackTotalTime && !isPlayingPlaylist)
                    {
                        durationSlider.Value = durationSlider.Maximum;
                        StopFile(sender, e);

                    }
                    if (currentTime < trackTotalTime)
                    {
                        durationSlider.Value = currentTime;
                        TimeSpan result = TimeSpan.FromSeconds(currentTime);
                        string fromTimeString = result.ToString("c");
                        textCurrentTime.Text = fromTimeString;
                    }

                }


        }


        private void OpenFilePath(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "Media files(*.mp3)|*.mp3";
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (currentFileNode != null) currentFileNode.Foreground = Brushes.White;
                if (currentAudioFile == null)
                {
                    InitializeFile(openFileDialog);
                    PlayFile(sender, e);
                }
                if (currentAudioFile != null)
                {
                    outputDevice.Stop();
                    if (outputDevice != null) outputDevice.Dispose();
                    outputDevice = null;
                    if (currentAudioFile != null) currentAudioFile.RemoveAudio();
                    currentAudioFile = null;
                    InitializeFile(openFileDialog);
                    durationSlider.Value = 0;
                    PlayFile(sender, e);
                    isPlayingPlaylist = false;
                    textCurrentPlaylist.Visibility = Visibility.Hidden;
                    titlePlaylist.Visibility = Visibility.Hidden;
                }
            }

        }

        private void InitializeFile(System.Windows.Forms.OpenFileDialog fileDialog)
        {
            string filePath;
            TagLib.File file;
            TagLib.IPicture picture;
            MemoryStream memoryStream;
            BitmapImage bitMap;

            filePath = fileDialog.FileName;
            if (currentAudioFile != null) currentAudioFile.RemoveAudio();
            currentAudioFile = new Audio(filePath);

            file = TagLib.File.Create(filePath, TagLib.ReadStyle.Average);
            try
            {
                trackTotalTime = (int)file.Properties.Duration.TotalSeconds;
                textTotalTime.Text = currentAudioFile.GetTotalTime();



                if (file.Tag.Album != null)
                {
                    album.Text = file.Tag.Album;
                }

                else
                {
                    album.Text = " ";
                }

                if (file.Tag.FirstPerformer != null)
                {
                    author.Text = file.Tag.FirstPerformer;
                }

                else
                {
                    author.Text = " ";
                }

                if (file.Tag.Title != null)
                {
                    if (file.Tag.Title.Length < 20)
                    {
                        title.FontSize = 13;
                        title.Text = file.Tag.Title;
                    }
                    else
                    {
                        title.FontSize = 10;
                        title.Text = file.Tag.Title;

                    }
                }
                else
                {
                    title.FontSize = 12;
                    title.Text = Path.GetFileName(file.Name.Replace(".mp3", ""));
                }

                if (file.Tag.Pictures.Length >= 1)
                {
                    picture = file.Tag.Pictures[0];
                    memoryStream = new MemoryStream(picture.Data.Data);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    bitMap = new BitmapImage();
                    bitMap.BeginInit();
                    bitMap.StreamSource = memoryStream;
                    bitMap.EndInit();
                    albumImage.Source = bitMap;
                    albumImage.Visibility = Visibility.Visible;
                }
                else
                {
                    albumImage.Visibility = Visibility.Hidden;
                }

            }
            catch (Exception)
            {

            }
            isPlay = false;
            isStop = false;
            isPause = false;
            isAudio = true;

        }

        private void PlayFile(object sender, EventArgs e)
        {

            if (isAudio)
            {
                if (outputDevice == null)
                {
                    currentAudioFile.SetVolume((float)volumeSlider.Value);
                    if (outputDevice != null) outputDevice.Dispose();
                    outputDevice = new WaveOut();
                    outputDevice.Init(currentAudioFile.GetAudioFile());

                }
                outputDevice.Play();
                playButton.Background = Brushes.LightSteelBlue;
                pauseButton.Background = Brushes.Transparent;
                isPlay = true;
                isPause = false;
                isStop = false;
            }
        }

        public void StopFile(object sender, EventArgs e)
        {

            if (isAudio)
            {
                if (outputDevice != null)
                    outputDevice.Stop();
                isStop = true;
                textCurrentTime.Text = "00:00:00";
                SetPositionFile(0);
                durationSlider.Value = 0;
                playButton.Background = Brushes.Transparent;
                pauseButton.Background = Brushes.Transparent;
            }

        }
        public void PauseFile(object sender, EventArgs e)
        {
            if (isAudio)
            {
                playButton.Background = Brushes.Transparent;
                pauseButton.Background = Brushes.LightSteelBlue;
                outputDevice?.Pause();
                isPause = true;
                isPlay = false;
                isStop = false;
            }
        }

        public void SetPositionFile(long pos)
        {
            if (currentAudioFile != null)
                currentAudioFile.GetAudioFile().Position = pos;
        }


        private void VolumeSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (outputDevice != null && isAudio)
            {
                currentAudioFile.SetVolume((float)volumeSlider.Value);

                if (volumeSlider.Value >= volumeSlider.Maximum)
                {
                    volumeSlider.Value = volumeSlider.Maximum;
                }
                if (volumeSlider.Value <= volumeSlider.Minimum)
                {
                    volumeSlider.Value = volumeSlider.Minimum;
                }
            }
        }

        private void DurationSliderDragStarted(object sender, DragStartedEventArgs e)
        {
            userIsDraggingSlider = true;

        }

        private void DurationSliderDragCompleted(object sender, DragCompletedEventArgs e)
        {

            userIsDraggingSlider = false;


        }

        private void DurationSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (userIsDraggingSlider)
            {
                if (isPlay && !isPause)
                {
                    outputDevice?.Pause();
                }
                if (outputDevice != null)
                {
                    if (durationSlider.Value >= durationSlider.Maximum)
                    {
                        durationSlider.Value = durationSlider.Maximum;
                    }
                    if (durationSlider.Value <= durationSlider.Minimum)
                    {
                        durationSlider.Value = durationSlider.Minimum;
                    }

                    if (userIsDraggingSlider)
                    {
                        double changedPosition = durationSlider.Value;
                        SetPositionFile((long)((changedPosition / trackTotalTime) * currentAudioFile.GetMaxPosition()));
                        int currentTime = (int)(currentAudioFile.GetCurrentPosition() * 1d / outputDevice.OutputWaveFormat.AverageBytesPerSecond);
                        durationSlider.Value = currentTime;
                        TimeSpan result = TimeSpan.FromSeconds(currentTime);
                        string fromTimeString = result.ToString("c");
                        textCurrentTime.Text = fromTimeString;
                    }
                }

                if (isPlay && !isPause)
                {
                    outputDevice.Play();
                }

            }
        }

        private static TreeViewItem CreateDirectoryNode(DirectoryInfo directoryInfo)
        {
            TreeViewItem directoryNode = new TreeViewItem();
            directoryNode.Header = directoryInfo.Name;
            directoryNode.DataContext = directoryInfo.FullName;
            directoryNode.Background = Brushes.Transparent;
            directoryNode.Foreground = Brushes.White;
            directoryNode.IsExpanded = true;

            foreach (var file in directoryInfo.GetFiles("*.mp3"))
            {

                TreeViewItem fileNode = new TreeViewItem();
                fileNode.Header = file.Name;
                fileNode.DataContext = directoryInfo.FullName;
                fileNode.Background = Brushes.Transparent;
                fileNode.Foreground = Brushes.White;
                directoryNode.Items.Add(fileNode);


            }
            return directoryNode;
        }

        private void CreatePlaylist(object sender, RoutedEventArgs e)
        {

            System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                DirectoryInfo folderDirectory = new DirectoryInfo(folderBrowserDialog.SelectedPath);
                if (folderDirectory.Name == "Nowy folder")
                    MessageBox.Show("Wystąpił problem z utworzeniem nowego folderu.Spróbuj ponownie!");
                else
                {
                    folderBrowserDialog.SelectedPath = folderBrowserDialog.SelectedPath;
                    MakePlaylist(folderDirectory);
                }
            }
        }

        private void MakePlaylist(DirectoryInfo folderDirectory)
        {
            if (currentPlaylist == null)
            {
                listOfPlaylists = new ListPlaylists();
                isPlaylist = true;
                listOfPlaylists.AddPlaylist(new Playlist(folderDirectory));
                currentPlaylist = listOfPlaylists.GetCurrentPlaylist();
                playlistTreeView.Items.Add(CreateDirectoryNode(folderDirectory));
            }
            else
            {
                Playlist newPlaylist = new Playlist(folderDirectory);
                if (!listOfPlaylists.Contains(folderDirectory.Name))
                {
                    isPlaylist = true;
                    listOfPlaylists.AddPlaylist(newPlaylist);
                    playlistTreeView.Items.Add(CreateDirectoryNode(folderDirectory));
                }
            }
        }

        private void NextSong(object sender, RoutedEventArgs e)
        {
            if (isPlaylist && isPlayingPlaylist)
            {
                if (currentPlaylist.GetPlaylistCount() > 1)
                {
                    if (currentFileNode != null) currentFileNode.Foreground = Brushes.White;

                    if (currentPlaylist.GetCurrentAudioIndex() < currentPlaylist.GetPlaylistCount() - 1 && listOfPlaylists.IsAnyPlaylist())
                    {
                        outputDevice.Stop();
                        if (outputDevice != null) outputDevice.Dispose();
                        outputDevice = null;
                        if (currentAudioFile != null) currentAudioFile.RemoveAudio();
                        currentAudioFile = null;
                        currentPlaylist.SetNextAudio();
                        InitPlaylist();
                        PlayFile(sender, e);
                        SetColorNode();
                    }
                    else
                    {
                        if (listOfPlaylists.IsAnyPlaylist())
                            outputDevice.Stop();
                        if (outputDevice != null) outputDevice.Dispose();
                        outputDevice = null;
                        if (currentAudioFile != null) currentAudioFile.RemoveAudio();
                        currentAudioFile = null;
                        currentPlaylist.SetNextAudio();
                        InitPlaylist();
                        PlayFile(sender, e);
                        SetColorNode();

                    }
                }
            }
        }

        private void SetColorNode()
        {
            TreeViewItem fileNode = GetItemFromDirectoryNode(currentPlaylist.GetCurrentPlaylistFile().GetTitle() + ".mp3", currentPlaylist.GetPlaylistName());
            fileNode.Foreground = Brushes.SteelBlue;
            currentFileNode = fileNode;
        }

        private void PreviousSong(object sender, RoutedEventArgs e)
        {
            if (isPlaylist && isPlayingPlaylist)
            {
                if (currentPlaylist.GetPlaylistCount() > 1)
                {
                    if (currentFileNode != null) currentFileNode.Foreground = Brushes.White;

                    if (currentPlaylist.GetCurrentAudioIndex() > 0)
                    {
                        outputDevice.Stop();
                        if (outputDevice != null) outputDevice.Dispose();
                        outputDevice = null;
                        if (currentAudioFile != null) currentAudioFile.RemoveAudio();
                        currentAudioFile = null;
                        currentPlaylist.SetPreviousAudio();
                        InitPlaylist();
                        PlayFile(sender, e);
                        SetColorNode();
                    }
                    else
                    {
                        outputDevice.Stop();
                        if (outputDevice != null) outputDevice.Dispose();
                        outputDevice = null;
                        if (currentAudioFile != null) currentAudioFile.RemoveAudio();
                        currentAudioFile = null;

                        if (!currentPlaylist.isShuffle)
                            currentPlaylist.SetCurrentPlaylistFile(currentPlaylist.GetPlaylist().Last());
                        else
                        {
                            currentPlaylist.SetPreviousAudio();
                        }

                        InitPlaylist();
                        PlayFile(sender, e);
                        SetColorNode();

                    }
                }
            }
        }
        private TreeViewItem GetItemFromTreeView(string directory)
        {
            TreeViewItem returnNode = null;
            foreach (var item in playlistTreeView.Items)
            {
                TreeViewItem dirNode = (TreeViewItem)item;
                if ((string)dirNode.DataContext == directory)
                {
                    returnNode = dirNode;
                    break;
                }
            }
            return returnNode;
        }

        private TreeViewItem GetItemFromDirectoryNode(string fileName, string directoryName)
        {
            TreeViewItem returnNode = new TreeViewItem();
            foreach (var item in playlistTreeView.Items)
            {
                TreeViewItem directoryNode = (TreeViewItem)item;
                if ((string)directoryNode.Header == directoryName)
                {

                    foreach (var file in directoryNode.Items)
                    {
                        TreeViewItem fileNode = (TreeViewItem)file;
                        if ((string)fileNode.Header == fileName)
                        {
                            returnNode = fileNode;
                            break;
                        }

                    }
                }
            }
            return returnNode;
        }

        private void InitPlaylist()
        {
            if (currentAudioFile != null) currentAudioFile.RemoveAudio();
            currentAudioFile = currentPlaylist.GetCurrentPlaylistFile();
            string filePath;
            TagLib.File file;
            TagLib.IPicture picture;
            MemoryStream memoryStream;
            BitmapImage bitMap;

            filePath = currentAudioFile.GetPath();
            if (currentAudioFile != null) currentAudioFile.RemoveAudio();
            currentAudioFile = new Audio(filePath);

            file = TagLib.File.Create(filePath, TagLib.ReadStyle.Average);
            try
            {
                trackTotalTime = (int)file.Properties.Duration.TotalSeconds;
                textTotalTime.Text = currentAudioFile.GetTotalTime();



                if (file.Tag.Album != null)
                {
                    album.Text = file.Tag.Album;
                }

                else
                {
                    album.Text = " ";
                }

                if (file.Tag.FirstPerformer != null)
                {
                    author.Text = file.Tag.FirstPerformer;
                }

                else
                {
                    author.Text = " ";
                }

                if (file.Tag.Title != null)
                {
                    if (file.Tag.Title.Length < 20)
                    {
                        title.FontSize = 13;
                        title.Text = file.Tag.Title;
                    }
                    else
                    {
                        title.FontSize = 10;
                        title.Text = file.Tag.Title;
                    }
                }
                else
                {
                    title.FontSize = 12;
                    title.Text = Path.GetFileName(file.Name.Replace(".mp3", ""));
                }




                if (file.Tag.Pictures.Length >= 1)
                {
                    picture = file.Tag.Pictures[0];
                    memoryStream = new MemoryStream(picture.Data.Data);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    bitMap = new BitmapImage();
                    bitMap.BeginInit();
                    bitMap.StreamSource = memoryStream;
                    bitMap.EndInit();
                    albumImage.Source = bitMap;
                    albumImage.Visibility = Visibility.Visible;
                }
                else
                {
                    albumImage.Visibility = Visibility.Hidden;
                }

            }
            catch (Exception)
            {

            }
            isPlay = false;
            isStop = false;
            isPause = false;
            isAudio = true;
        }

        private void PlayPlaylist(object sender, RoutedEventArgs e)
        {

            if (isPlaylist && listOfPlaylists.IsAnyPlaylist() && currentPlaylist.GetPlaylistCount() != 0 && isSelectedPlaylist && !isSelectedPlaylistAudio)
            {
                Console.WriteLine("1 IF");
                if (outputDevice != null)
                {
                    outputDevice.Stop();
                    outputDevice.Dispose();
                }
                if (currentFileNode != null) currentFileNode.Foreground = Brushes.White;

                outputDevice = null;
                if (currentAudioFile != null) currentAudioFile.RemoveAudio();
                currentAudioFile = null;
                InitPlaylist();
                PlayFile(sender, e);
                SetColorNode();
                isPlayingPlaylist = true;


            }

            if (isPlaylist && listOfPlaylists.IsAnyPlaylist() && currentPlaylist.GetPlaylistCount() != 0 && !isSelectedPlaylist && isSelectedPlaylistAudio)
            {

                Console.WriteLine("2 IF");

                if (outputDevice != null)
                {
                    outputDevice.Stop();
                    outputDevice.Dispose();
                }
                if (currentFileNode != null) currentFileNode.Foreground = Brushes.White;

                outputDevice = null;
                if (currentAudioFile != null) currentAudioFile.RemoveAudio();
                currentAudioFile = null;
                InitPlaylist();
                PlayFile(sender, e);
                SetColorNode();
                isPlayingPlaylist = true;
                isSelectedPlaylistAudio = false;

            }



            if (selectedPlaylist != null && isSelectedPlaylist && listOfPlaylists.GetPlaylist(selectedPlaylist).GetPlaylistCount() != 0 && !isSelectedPlaylistAudio)
            {
                Console.WriteLine("3 IF");
                if (isPlaylist && listOfPlaylists.IsAnyPlaylist())
                {
                    if (outputDevice != null)
                    {
                        outputDevice.Stop();
                        outputDevice.Dispose();
                    }
                    if (currentFileNode != null) currentFileNode.Foreground = Brushes.White;

                    outputDevice = null;
                    if (currentAudioFile != null) currentAudioFile.RemoveAudio();
                    currentAudioFile = null;
                    listOfPlaylists.SetCurrentPlaylist(listOfPlaylists.GetPlaylist(selectedPlaylist));
                    currentPlaylist = listOfPlaylists.GetCurrentPlaylist();
                    listOfPlaylists.GetCurrentPlaylist().SetFirst();
                    InitPlaylist();
                    PlayFile(sender, e);
                    SetColorNode();
                    isPlayingPlaylist = true;
                    isSelectedPlaylist = false;
                }

            }
            if (isPlaylist && listOfPlaylists.IsAnyPlaylist() && currentPlaylist.GetPlaylistCount() != 0 && isPlayingPlaylist)
            {
                textCurrentPlaylist.Visibility = Visibility.Visible;
                titlePlaylist.Text = currentPlaylist.GetPlaylistName();
                titlePlaylist.Visibility = Visibility.Visible;
            }
        }

        private void OnItemMouseDoubleClick(object sender, MouseButtonEventArgs args)
        {

            if (sender is TreeViewItem)
            {
                if (!((TreeViewItem)sender).IsSelected)
                {
                    try
                    {
                        TreeViewItem fileNode = (TreeViewItem)playlistTreeView.SelectedItem;
                        string directoryString = (string)fileNode.DataContext + "\\" + fileNode.Header;
                        if (outputDevice != null && currentAudioFile != null)
                        {
                            outputDevice.Stop();
                            outputDevice.Dispose();
                            if (currentAudioFile != null) currentAudioFile.RemoveAudio();
                            currentAudioFile = null;
                            outputDevice = null;
                        }
                        currentPlaylist = listOfPlaylists.GetPlaylist(System.IO.Path.GetFileName(fileNode.DataContext.ToString()));
                        currentPlaylist.SetCurrentPlaylistFile(directoryString);
                        isPlaylist = true;
                        isPlayingPlaylist = true;
                        InitPlaylist();
                        PlayPlaylist(sender, args);
                        isSelectedPlaylistAudio = false;
                        return;
                    }
                    catch (Exception) { }
                }
            }
        }

        private void OnItemMouseClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem)
            {
                if (!((TreeViewItem)sender).IsSelected)
                {
                    try
                    {
                        TreeViewItem fileNode = (TreeViewItem)playlistTreeView.SelectedItem;
                        if (fileNode != null)
                        {
                            selectedPlaylistAudio = (fileNode.Header).ToString();
                            isSelectedPlaylistAudio = true;
                            selectedFileNode = fileNode;
                            isSelectedPlaylist = false;
                        }
                    }
                    catch (Exception) { }

                    return;
                }
                else
                {
                    try
                    {
                        TreeViewItem parentNode = (TreeViewItem)playlistTreeView.SelectedItem;
                        if (parentNode != null)
                        {
                            selectedPlaylist = (parentNode.Header).ToString();
                            isSelectedPlaylist = true;
                            selectedDirectoryNode = parentNode;
                            isSelectedPlaylistAudio = false;
                        }
                    }
                    catch (Exception) { }
                    return;
                }
            }

        }

        private void DeletePlaylist(object sender, RoutedEventArgs e)
        {

            if (selectedPlaylist != null && isSelectedPlaylist && !isSelectedPlaylistAudio)
            {
                if (listOfPlaylists.IsAnyPlaylist())
                {
                    if (currentPlaylist.GetPlaylistName() == selectedPlaylist)
                    {
                        foreach (var file in listOfPlaylists.GetPlaylist(selectedPlaylist).GetPlaylistDirectory().GetFiles("*.mp3"))
                        {
                            if (currentAudioFile != null)
                            {
                                if (file.Name == currentAudioFile.GetTitle() + ".mp3")
                                {
                                    StopFile(sender, e);
                                    currentAudioFile.RemoveAudio();
                                    currentAudioFile = null;
                                    if (outputDevice != null) outputDevice.Dispose();
                                    GC.Collect();
                                    GC.WaitForPendingFinalizers();
                                    outputDevice = new WaveOut();
                                    isAudio = false;
                                }
                            }
                            listOfPlaylists.GetPlaylist(selectedPlaylist).DeleteFile(file.Name);
                        }

                        listOfPlaylists.DeletePlaylist(selectedPlaylist);
                        if (listOfPlaylists.GetSize() > 1)
                            currentPlaylist = listOfPlaylists.Last();
                        else
                        {
                            if (listOfPlaylists.GetSize() == 0)
                            {
                                currentPlaylist = null;
                                SetBeginning(sender, e);
                            }
                        }
                    }
                    else
                    {
                        listOfPlaylists.DeletePlaylist(selectedPlaylist);
                    }
                    if (listOfPlaylists.GetSize() == 0)
                        listOfPlaylists = null;

                    foreach (var item in playlistTreeView.Items)
                    {
                        TreeViewItem dirNode = (TreeViewItem)item;
                        if ((string)dirNode.Header == selectedPlaylist)
                        {
                            playlistTreeView.Items.Remove(dirNode);
                            break;
                        }
                    }
                    isSelectedPlaylist = false;
                }
            }
        }

        private void SetBeginning(object sender, RoutedEventArgs e)
        {
            BitmapImage bitmap = new BitmapImage(new Uri("/images/queen.jpg", UriKind.Relative));
            albumImage.Source = bitmap;
            title.Text = "Title";
            author.Text = "Author";
            album.Text = "Album";
            textTotalTime.Text = "00:03:56";
            textCurrentTime.Text = "00:00:00";
            durationSlider.Value = 0;
            StopFile(sender, e);
            if (outputDevice != null) outputDevice.Dispose();
            outputDevice = null;
            if (currentAudioFile != null) currentAudioFile.RemoveAudio();
            currentAudioFile = null;
            isAudio = false;
            isPlay = false;
            isPlayingPlaylist = false;
            isPlaylist = false;
        }

        private void RenamePlaylist(object sender, RoutedEventArgs e)
        {
            if (selectedPlaylist != null && isSelectedPlaylist && !isSelectedPlaylistAudio)
            {
                if (listOfPlaylists.IsAnyPlaylist())
                {
                    RenameWindow rename = new RenameWindow();
                    rename.Left = this.Left + 350;
                    rename.Top = this.Top + 200;
                    rename.ShowDialog();
                    rename.Check += value => folderName = value;
                    if (folderName != null)
                    {
                        try
                        {
                            string deletedName = listOfPlaylists.GetPlaylist(selectedPlaylist).GetPlaylistDirectory().FullName;
                            string newName = deletedName.Replace(selectedPlaylist, folderName);

                            File.SetAttributes(deletedName, FileAttributes.Normal);
                            Directory.CreateDirectory(newName);
                            DirectoryInfo directoryInfo = new DirectoryInfo(newName);
                            if (listOfPlaylists.GetPlaylist(currentPlaylist.GetPlaylistName()).GetIsShuffle())
                            {
                                listOfPlaylists.GetPlaylist(currentPlaylist.GetPlaylistName()).SetIsShuffle();
                                shuffleButton.Background = Brushes.Transparent;
                            }

                            foreach (var file in listOfPlaylists.GetPlaylist(selectedPlaylist).GetPlaylistDirectory().GetFiles("*.mp3"))
                            {
                                if (currentAudioFile != null)
                                {
                                    if (file.Name == currentAudioFile.GetTitle() + ".mp3")
                                    {
                                        StopFile(sender, e);
                                        currentAudioFile.RemoveAudio();
                                        currentAudioFile = null;
                                        if (outputDevice != null) outputDevice.Dispose();
                                        GC.Collect();
                                        GC.WaitForPendingFinalizers();
                                        outputDevice = new WaveOut();
                                        isAudio = false;
                                    }
                                }
                                listOfPlaylists.GetPlaylist(selectedPlaylist).DeleteFile(file.Name);
                                File.SetAttributes(file.FullName, FileAttributes.Normal);
                                File.Copy(file.FullName, newName + "\\" + file.Name, true);
                                File.Delete(file.FullName);
                            }
                            listOfPlaylists.DeletePlaylist(selectedPlaylist);
                            Directory.Delete(deletedName);
                            MakePlaylist(directoryInfo);
                            playlistTreeView.Items.Remove(selectedDirectoryNode);
                            listOfPlaylists.GetPlaylist(folderName).UpdatePlaylist();
                            isPlaylist = true;
                            if (listOfPlaylists.GetSize() == 1)
                            {
                                currentPlaylist = listOfPlaylists.GetPlaylist(folderName);
                            }
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Podana nazwa już istnieje");
                        }


                    }
                }
            }
            isSelectedPlaylist = false;
        }



        private void AddFile(object sender, RoutedEventArgs e)
        {
            if (selectedPlaylist != null && isSelectedPlaylist && !isSelectedPlaylistAudio)
            {
                if (listOfPlaylists.IsAnyPlaylist())
                {
                    List<String> pathsList = new List<String>();
                    if (listOfPlaylists.GetPlaylist(currentPlaylist.GetPlaylistName()).GetIsShuffle())
                    {
                        listOfPlaylists.GetPlaylist(currentPlaylist.GetPlaylistName()).SetIsShuffle();
                        shuffleButton.Background = Brushes.Transparent;
                    }
                    pathsList = listOfPlaylists.GetPlaylist(selectedPlaylist).AddPlaylistFile();
                    string DestinationPath = listOfPlaylists.GetPlaylist(selectedPlaylist).GetPlaylistDirectory().FullName;
                    foreach (String file in pathsList)
                    {
                        string SourcePath = file;
                        string SourceName = Path.GetFileName(SourcePath);
                        if (!File.Exists(Path.Combine(DestinationPath, SourceName)))
                        {
                            try
                            {
                                File.Copy(SourcePath, DestinationPath + "\\" + SourceName, true);

                            }
                            catch (IOException iox)
                            {
                                Console.WriteLine(iox.Message);
                            }
                        }
                    }
                    listOfPlaylists.GetPlaylist(selectedPlaylist).SetFirst();
                    TreeViewItem dirNode = GetItemFromTreeView(listOfPlaylists.GetPlaylist(selectedPlaylist).GetPlaylistDirectory().FullName);
                    playlistTreeView.Items.Remove(dirNode);

                    playlistTreeView.Items.Add(CreateDirectoryNode(listOfPlaylists.GetPlaylist(selectedPlaylist).GetPlaylistDirectory()));
                    isPlaylist = true;
                    listOfPlaylists.GetPlaylist(selectedPlaylist).UpdatePlaylist();

                    if (currentAudioFile != null)
                    {
                        currentFileNode = GetItemFromDirectoryNode(currentAudioFile.GetTitle() + ".mp3", currentPlaylist.GetPlaylistDirectory().Name);
                        currentFileNode.Foreground = Brushes.SteelBlue;
                    }


                }
                isSelectedPlaylist = false;
            }
        }

        private void DeleteFile(object sender, RoutedEventArgs e)
        {

            if (selectedPlaylistAudio != null && isSelectedPlaylistAudio && !isSelectedPlaylist)
            {


                TreeViewItem fileNode = selectedFileNode;
                TreeViewItem directoryNode = GetItemFromTreeView((String)fileNode.DataContext);
                Playlist playlist = listOfPlaylists.GetPlaylist((String)directoryNode.Header);

                try
                {
                    if (listOfPlaylists.GetPlaylist(currentPlaylist.GetPlaylistName()).GetIsShuffle())
                    {
                        listOfPlaylists.GetPlaylist(currentPlaylist.GetPlaylistName()).SetIsShuffle();
                        shuffleButton.Background = Brushes.Transparent;
                    }

                    if (File.Exists(playlist.GetPlaylistDirectory().ToString() + "\\" + selectedPlaylistAudio))
                    {
                        File.SetAttributes(playlist.GetPlaylistDirectory().ToString() + "\\" + selectedPlaylistAudio, FileAttributes.Normal);


                        if (currentAudioFile != null)
                        {
                            if (selectedPlaylistAudio == currentAudioFile.GetTitle() + ".mp3" && currentAudioFile.GetPath() == fileNode.DataContext + "\\" + fileNode.Header)
                            {

                                StopFile(sender, e);
                                if (currentAudioFile != null) currentAudioFile.RemoveAudio();
                                currentAudioFile = null;
                                if (outputDevice != null) outputDevice.Dispose();
                                GC.Collect();
                                GC.WaitForPendingFinalizers();
                                outputDevice = new WaveOut();
                                isAudio = false;
                                if (currentPlaylist.GetPlaylistCount() > 0)
                                {
                                    NextSong(sender, e);
                                }
                            }
                        }
                        playlist.DeleteFile(selectedPlaylistAudio);
                        File.Delete(playlist.GetPlaylistDirectory().ToString() + "\\" + selectedPlaylistAudio);

                        if (currentPlaylist.GetPlaylistCount() == 0)
                        {
                            SetBeginning(sender, e);
                        }
                    }
                }
                catch (IOException ioExp)
                {
                    Console.WriteLine(ioExp.Message);
                }


                playlistTreeView.Items.Remove(directoryNode);
                playlistTreeView.Items.Add(CreateDirectoryNode(playlist.GetPlaylistDirectory()));
                playlist.UpdatePlaylist();

                if (currentAudioFile != null)
                {
                    currentFileNode = GetItemFromDirectoryNode(currentAudioFile.GetTitle() + ".mp3", currentPlaylist.GetPlaylistDirectory().Name);
                    currentFileNode.Foreground = Brushes.SteelBlue;
                }

            }

        }

        private void ShufflePlaylist(object sender, RoutedEventArgs e)
        {
            if (isPlaylist && listOfPlaylists.IsAnyPlaylist() && currentPlaylist.GetPlaylistCount() > 0)
            {
                listOfPlaylists.GetPlaylist(currentPlaylist.GetPlaylistName()).SetIsShuffle();
                if (listOfPlaylists.GetPlaylist(currentPlaylist.GetPlaylistName()).GetIsShuffle())
                {
                    shuffleButton.Background = Brushes.LightSteelBlue;
                    listOfPlaylists.GetPlaylist(currentPlaylist.GetPlaylistName()).Shuffle();
                }
                else
                {
                    shuffleButton.Background = Brushes.Transparent;
                }
            }
        }

        private void SavePlaylists(object sender, RoutedEventArgs e)
        {

            string path = "savePlaylists.txt";
            if (listOfPlaylists != null)
            {
                using (StreamWriter streamWriter = File.CreateText(path))
                {
                    foreach (var list in listOfPlaylists.GetAllPlaylists())
                    {
                        streamWriter.WriteLine(list.GetPlaylistDirectory().FullName);
                    }
                }
            }
            else
            {
                using (StreamWriter streamWriter = File.CreateText(path))
                {
                    streamWriter.Close();
                }
            }
        }

        private void OpenAtBeginning()
        {
            string path = "savePlaylists.txt";
            string pathOfPlaylists;

            if (File.Exists(path))
            {
                using (StreamReader sr = File.OpenText(path))
                {
                    while ((pathOfPlaylists = sr.ReadLine()) != null)
                    {
                        DirectoryInfo directory = new DirectoryInfo(pathOfPlaylists);
                        if (Directory.Exists(directory.FullName))
                            MakePlaylist(directory);
                    }
                    sr.Close();
                }
            }
        }
    }
}






