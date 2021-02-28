using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using NAudio.Wave;


namespace Projekt
{
    public class Audio
    {
        private AudioFileReader audioFile;
        private string totalTime;
        private string title;
        private long maxPosition;
        private string path;

        public Audio(string fileName)
        {
            this.path = fileName;
            this.audioFile = new AudioFileReader(fileName);
            this.title = Path.GetFileNameWithoutExtension(audioFile.FileName);
            this.totalTime = string.Format("{0:hh\\:mm\\:ss}", audioFile.TotalTime);
            this.audioFile.Volume = 0.5f;
            this.maxPosition = audioFile.Length;
        }

        public Audio(DirectoryInfo directoryInfo, string name)
        {
            this.path = name;
            this.audioFile = new AudioFileReader(name);
            this.title = Path.GetFileNameWithoutExtension(name);
            this.totalTime = string.Format("{0:hh\\:mm\\:ss}", audioFile.TotalTime);
            this.audioFile.Volume = 0.5f;
            this.maxPosition = audioFile.Length;
        }

        public string GetPath()
        {
            return this.path;
        }

        public void RemoveAudio()
        {
            this.audioFile.Close();
            this.audioFile.Dispose();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        public AudioFileReader GetAudioFile()
        {
            return audioFile;
        }

        public String GetTitle()
        {
            return title;
        }

        public String GetTotalTime()
        {
            return totalTime;
        }

        public long GetCurrentPosition()
        {
            return audioFile.Position;
        }

        public void SetVolume(float volume)
        {
            audioFile.Volume = volume;
        }

        public long GetMaxPosition()
        {
            return maxPosition;
        }
    }
}
