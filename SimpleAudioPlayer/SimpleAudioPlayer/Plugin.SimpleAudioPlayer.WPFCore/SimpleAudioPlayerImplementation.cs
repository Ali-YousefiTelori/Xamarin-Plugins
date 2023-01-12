﻿using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Plugin.SimpleAudioPlayer
{
    /// <summary>
    /// Implementation for Feature
    /// </summary>
    public class SimpleAudioPlayerImplementation : ISimpleAudioPlayer
    {
        public Func<Task> PlaybackEnded { get; set; }

        private static int index;

        private MediaPlayer player;


        MediaPlayer Player()
        {
            try
            {
                return player != null && player.CanPause ? player : null;
            }
            catch
            {
                return null;
            }
        }

        ///<Summary>
        /// Length of audio in seconds
        ///</Summary>
        public double Duration => Player()?.NaturalDuration.TimeSpan.TotalSeconds ?? 0;

        ///<Summary>
        /// Current position of audio in seconds
        ///</Summary>
        public double CurrentPosition => Player()?.Position.TotalSeconds ?? 0;

        ///<Summary>
        /// Playback volume (0 to 1)
        ///</Summary>
        public double Volume
        {
            get => Player()?.Volume ?? 0;
            set => SetVolume(value, Balance);
        }

        ///<Summary>
        /// Balance left/right: -1 is 100% left : 0% right, 1 is 100% right : 0% left, 0 is equal volume left/right
        ///</Summary>
        public double Balance
        {
            get => Player()?.Balance ?? 0;
            set => SetVolume(Volume, value);
        }

        ///<Summary>
        /// Indicates if the currently loaded audio file is playing
        ///</Summary>
        public bool IsPlaying
        {
            get
            {
                if (Player() == null)
                    return false;
                return _isPlaying;
            }
        }
        private bool _isPlaying;

        ///<Summary>
        /// Continously repeats the currently playing sound
        ///</Summary>
        public bool Loop { get; set; }

        ///<Summary>
        /// Indicates if the position of the loaded audio file can be updated
        ///</Summary>
        public bool CanSeek => Player() != null;

        ///<Summary>
        /// Load wave or mp3 audio file from a stream
        ///</Summary>
        public bool Load(Stream audioStream)
        {
            DeletePlayer();

            player = GetPlayer();

            if (player != null)
            {
                var fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyMusic), $"{++index}.wav");
                using (var fileStream = File.Create(fileName)) audioStream.CopyTo(fileStream);

                player.Open(new Uri(fileName));
                player.MediaEnded += OnPlaybackEnded;
            }

            return player != null && player.Source != null;
        }

        ///<Summary>
        /// Load wave or mp3 audio file from assets folder in the UWP project
        ///</Summary>
        public bool Load(string fileName)
        {
            DeletePlayer();

            player = GetPlayer();

            if (player != null)
            {
                player.Open(new Uri(@"Assets\" + fileName, UriKind.Relative));
                player.MediaEnded += OnPlaybackEnded;
            }

            return player != null && player.Source != null;
        }

        void DeletePlayer()
        {
            Stop();

            if (player != null)
            {
                player.MediaEnded -= OnPlaybackEnded;
                player = null;
            }
        }

        private void OnPlaybackEnded(object sender, EventArgs args)
        {
            if (_isPlaying && Loop)
            {
                Play();
            }

            PlaybackEnded?.Invoke();
        }

        ///<Summary>
        /// Begin playback or resume if paused
        ///</Summary>
        public void Play()
        {
            if (player == null || player.Source == null)
                return;

            if (IsPlaying)
            {
                Pause();
                Seek(0);
            }

            _isPlaying = true;
            player.Play();
        }

        ///<Summary>
        /// Pause playback if playing (does not resume)
        ///</Summary>
        public void Pause()
        {
            _isPlaying = false;
            Player()?.Pause();
        }

        ///<Summary>
        /// Stop playack and set the current position to the beginning
        ///</Summary>
        public void Stop()
        {
            Pause();
            Seek(0);
            PlaybackEnded?.Invoke();
        }

        ///<Summary>
        /// Seek a position in seconds in the currently loaded sound file 
        ///</Summary>
        public void Seek(double position)
        {
            if (Player() == null) return;
            player.Position = TimeSpan.FromSeconds(position);
        }

        private void SetVolume(double volume, double balance)
        {
            if (Player() == null || _isDisposed) return;

            player.Volume = Math.Min(1, Math.Max(0, volume));
            player.Balance = Math.Min(1, Math.Max(-1, balance));
        }

        private static MediaPlayer GetPlayer()
        {
            return new MediaPlayer();
        }

        private bool _isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed || Player() == null)
                return;

            if (disposing)
                DeletePlayer();

            _isDisposed = true;
        }

        ~SimpleAudioPlayerImplementation()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        public void SetSpeed(double speed)
        {
            if (Player() != null)
                player.SpeedRatio = speed;
        }
    }
}