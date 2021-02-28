using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Projekt
{
    public class Playlist
    {
        private LinkedList<Audio> playlist = new LinkedList<Audio>();
        private List<Audio> temporaryList = new List<Audio>();
        public bool isShuffle;
        private Audio currentPlaylistFile;
        private string name;
        private int temporaryListCounter;
        private DirectoryInfo playlistDirectory;

        public void UpdatePlaylist()
        {
            playlist = GetAllFiles(playlistDirectory);
            if (playlist.Count == 0)
                currentPlaylistFile = null;

        }

        public Playlist(DirectoryInfo directoryInfo)
        {
            playlistDirectory = directoryInfo;
            playlist = GetAllFiles(directoryInfo);
            if (playlist.Count != 0)
                currentPlaylistFile = playlist.First();
            name = directoryInfo.Name;
            isShuffle = false;
        }
        public Playlist()
        {

        }

        public void SetIsShuffle()
        {
            isShuffle = !isShuffle;
            if (!isShuffle)
            {
                temporaryList.Clear();
            }
        }
        public bool GetIsShuffle()
        {
            return isShuffle;
        }

        public void Shuffle()
        {
            temporaryListCounter = 0;
            Random rng = new Random();
            int playlistCount = playlist.Count;
            temporaryList = playlist.ToList();
            while (playlistCount > 1)
            {
                playlistCount--;
                int k = rng.Next(playlistCount + 1);
                Audio audio = temporaryList[k];
                temporaryList[k] = temporaryList[playlistCount];
                temporaryList[playlistCount] = audio;
            }
            int i = 0;
            foreach (var audio in temporaryList)
            {
                if (audio == currentPlaylistFile)
                    temporaryListCounter = i;
                i++;
            }
        }

        public LinkedList<Audio> GetAllFiles(DirectoryInfo directoryInfo)
        {
            LinkedList<Audio> namesSongs = new LinkedList<Audio>();
            foreach (var file in directoryInfo.GetFiles("*.mp3"))
            {
                namesSongs.AddLast(new Audio(directoryInfo, file.FullName));
            }
            return namesSongs;
        }

        public int GetPlaylistCount()
        {
            return playlist.Count;
        }

        public void SetFirst()
        {
            if (currentPlaylistFile != null) currentPlaylistFile.RemoveAudio();
            currentPlaylistFile = playlist.First();
        }

        public Audio GetCurrentPlaylistFile()
        {
            return currentPlaylistFile;
        }

        public void SetCurrentPlaylistFile(Audio audio)
        {
            if (currentPlaylistFile != null) currentPlaylistFile.RemoveAudio();
            currentPlaylistFile = audio;
        }

        public void SetCurrentPlaylistFile(string path)
        {
            if (currentPlaylistFile != null) currentPlaylistFile.RemoveAudio();
            currentPlaylistFile = GetPath(path);
        }

        public int GetCurrentAudioIndex()
        {
            int i = 0;
            foreach (var audio in playlist)
            {
                if (audio == currentPlaylistFile)
                    return i;
                i++;
            }
            return i;
        }

        public Audio GetPath(string path)
        {
            Audio correctAudio = new Audio(path);
            foreach (var audio in playlist)
            {
                if (audio.GetPath() == path)
                {
                    correctAudio = audio;
                }
            }
            return correctAudio;
        }

        public void SetNextAudio()
        {
            if (isShuffle == false)
            {
                int i = 0;
                int index = 0;
                foreach (var audio in playlist)
                {
                    if (currentPlaylistFile != null)
                    {
                        if (audio.GetTitle() == currentPlaylistFile.GetTitle())
                            index = i;
                    }

                    i++;
                }
                if (currentPlaylistFile != null) currentPlaylistFile.RemoveAudio();
                if (index + 1 < playlist.Count)
                {
                    currentPlaylistFile = playlist.ToArray()[index + 1];
                }
                else
                {
                    currentPlaylistFile = playlist.First();
                }
            }
            else
            {
                temporaryListCounter++;
                if (temporaryListCounter == temporaryList.Count)
                {
                    temporaryListCounter = 0;
                }
                if (currentPlaylistFile != null) currentPlaylistFile.RemoveAudio();
                if(File.Exists(temporaryList[temporaryListCounter].GetPath()))
                currentPlaylistFile = temporaryList[temporaryListCounter];
                else
                {
                    while (!File.Exists(temporaryList[temporaryListCounter].GetPath()))
                    {
                        temporaryListCounter++;
                        if (temporaryListCounter == temporaryList.Count)
                        {
                            temporaryListCounter = 0;
                        }
                    }
                    currentPlaylistFile = temporaryList[temporaryListCounter];
                }
            }
        }

        public DirectoryInfo GetPlaylistDirectory()
        {
            return playlistDirectory;
        }

        public void SetPlaylistDirectory(DirectoryInfo setDirectoryInfo)
        {
            playlistDirectory = setDirectoryInfo;
            UpdatePlaylist();
        }

        public void SetPreviousAudio()
        {
            if (isShuffle == false)
            {
                int i = 0;
                int index = 0;
                foreach (var audio in playlist)
                {
                    if (audio.GetTitle() == currentPlaylistFile.GetTitle())
                        index = i;
                    i++;
                }
                if (currentPlaylistFile != null) currentPlaylistFile.RemoveAudio();
                if (i > 1)
                {
                    currentPlaylistFile = playlist.ToArray()[index - 1];
                }
                else
                {
                    currentPlaylistFile = playlist.Last();
                }
            }
            else
            {
                temporaryListCounter--;
                if (temporaryListCounter < 0)
                {
                    temporaryListCounter = temporaryList.Count - 1;
                }
                if (currentPlaylistFile != null) currentPlaylistFile.RemoveAudio();
                if (File.Exists(temporaryList[temporaryListCounter].GetPath()))
                    currentPlaylistFile = temporaryList[temporaryListCounter];
                else
                {
                    while (!File.Exists(temporaryList[temporaryListCounter].GetPath()))
                    {
                        temporaryListCounter--;
                        if (temporaryListCounter < 0)
                        {
                            temporaryListCounter = temporaryList.Count - 1;
                        }
                    }
                    currentPlaylistFile = temporaryList[temporaryListCounter];
                }
            }
        }

        public string GetPlaylistName()
        {
            return name;
        }

        public void SetPlaylistName(string name)
        {
            this.name = name;
        }

        public LinkedList<Audio> GetPlaylist()
        {
            return playlist;
        }

        public List<String> AddPlaylistFile()
        {
            List<String> pathsList = new List<String>();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Media files(*.mp3)|*.mp3";
            openFileDialog.Multiselect = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                foreach (String file in openFileDialog.FileNames)
                {
                    pathsList.Add(file);
                    Audio audio = new Audio(file);
                    if (!playlist.Contains(audio))
                        playlist.AddLast(audio);
                }


            }
            return pathsList;
        }


        public void DeleteFile(string name)
        {
            if (playlist.Count != 0)
            {
                foreach (var audio in playlist)
                {
                    if (audio.GetTitle() + ".mp3" == name)
                    {
                        if (currentPlaylistFile != null)
                        {
                            if (audio.GetPath() == currentPlaylistFile.GetPath())
                            {
                                currentPlaylistFile.RemoveAudio();
                                currentPlaylistFile = null;
                            }
                        }
                        playlist.Remove(audio);
                        audio.RemoveAudio();
                        break;
                    }
                }
            }

        }
    }
}