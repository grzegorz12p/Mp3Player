using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projekt
{
    class ListPlaylists
    {
        private LinkedList<Playlist> playlists = new LinkedList<Playlist>();
        private Playlist currentPlaylist = new Playlist();
        private bool isAnyPlaylist = false;

        public void AddPlaylist(Playlist playlist)
        {
            playlists.AddLast(playlist);
            SetCurrentPlaylist(playlists.Last());
            isAnyPlaylist = true;
        }

        public LinkedList<Playlist> GetAllPlaylists()
        {
            return this.playlists;
        }

        public int GetSize()
        {
            return playlists.Count;
        }

        public bool Contains(string name)
        {
            foreach (var playlist in playlists)
            {
                if (playlist.GetPlaylistName() == name)
                {
                    return true;
                }
            }
            return false;
        }

        public Playlist GetPlaylist(string name)
        {
            Playlist returnPlaylist = new Playlist();
            if (isAnyPlaylist)
            {

                foreach (var playlist in playlists)
                {
                    if (playlist.GetPlaylistName() == name)
                    {
                        returnPlaylist = playlist;
                    }
                }
                return returnPlaylist;
            }
            else
            {
                return returnPlaylist;
            }
        }

        public void DeletePlaylist(string name)
        {
            if (playlists.Count != 0 && isAnyPlaylist)
            {
                if (playlists.Count == 1 && playlists.First().GetPlaylistName() == name)
                {
                    currentPlaylist = null;
                    isAnyPlaylist = false;
                    playlists.Remove(playlists.First());
                }
                else
                {
                    foreach (var playlist in playlists)
                    {
                        if (playlist.GetPlaylistName() == name)
                        {
                            playlists.Remove(playlist);
                            break;
                        }
                    }
                    currentPlaylist = playlists.Last();
                }
            }
        }

        public Playlist Last()
        {
            return playlists.Last();
            ;
        }

        public bool IsAnyPlaylist()
        {
            return isAnyPlaylist;
        }

        public void RenamePlaylist(string PlaylistName, string NewName)
        {
            if (playlists.Count != 0)
            {
                foreach (var playlist in playlists)
                {
                    if (playlist.GetPlaylistName() == PlaylistName)
                    {
                        playlist.SetPlaylistName(NewName);
                    }
                }
            }
        }

        public Playlist GetCurrentPlaylist()
        {
            return currentPlaylist;
        }
        public void SetCurrentPlaylist(Playlist playlist)
        {
            currentPlaylist = playlist;
        }
    }
}
