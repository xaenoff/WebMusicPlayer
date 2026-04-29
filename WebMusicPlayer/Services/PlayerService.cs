using Microsoft.JSInterop;
using WebMusicPlayer.Data.Models;

namespace WebMusicPlayer.Services
{
    public class PlayerService
    {
        public Track? CurrentTrack { get; private set; }
        public bool IsPlaying { get; private set; }
        public double CurrentTime { get; private set; }
        public double Duration { get; private set; }
        public int Volume { get; set; } = 10;

        public List<Track> TrackQueue { get; set; } = new();
        public int CurrentTrackIndex { get; set; } = -1;

        // События для обновления UI
        public event Action? StateChanged;
        public event Action? TimeUpdated;

        public void LoadTrack(Track track)
        {
            CurrentTrack = track;
            CurrentTime = 0;
            Duration = 0;
            IsPlaying = true; 
            StateChanged?.Invoke();
        }

        public void UpdateProgress(double time, double duration)
        {
            CurrentTime = time;
            Duration = duration;
            TimeUpdated?.Invoke();
        }

        public void SetPlaying(bool playing)
        {
            IsPlaying = playing;
            StateChanged?.Invoke();
        }

        public void TogglePlay()
        {
            IsPlaying = !IsPlaying;
            StateChanged?.Invoke();
        }



        public async Task PlayTrackAsync(Track track, IJSRuntime js)
        {
            LoadTrack(track);
            await js.InvokeVoidAsync("audioManager.setSource", track.FilePath);
            await js.InvokeVoidAsync("audioManager.play");
        }

        public async Task TogglePlayAsync(IJSRuntime js)
        {
            if (CurrentTrack == null) return;

            IsPlaying = !IsPlaying;

            if (IsPlaying)
            {
                await js.InvokeVoidAsync("audioManager.play");
            }
            else
            {
                await js.InvokeVoidAsync("audioManager.pause");
            }

            StateChanged?.Invoke();
        }

        public void SetQueue(List<Track> tracks, Track currentTrack)
        {
            TrackQueue = new List<Track>(tracks);
            CurrentTrackIndex = tracks.IndexOf(currentTrack);
            Console.WriteLine($"🎵 [Player] Queue set: {tracks.Count} tracks, current index: {CurrentTrackIndex}");
            LoadTrack(currentTrack);
        }

        public Track? GetNextTrack()
        {
            if (TrackQueue.Count == 0 || CurrentTrackIndex < 0)
            {
                Console.WriteLine(" [Player] No next track: queue empty or invalid index");
                return null;
            }

            var nextIndex = (CurrentTrackIndex + 1) % TrackQueue.Count;
            Console.WriteLine($" [Player] Next track: {CurrentTrackIndex} → {nextIndex}");
            CurrentTrackIndex = nextIndex;
            return TrackQueue[nextIndex];
        }

        public Track? GetPreviousTrack()
        {
            if (TrackQueue.Count == 0 || CurrentTrackIndex < 0)
            {
                Console.WriteLine(" [Player] No previous track: queue empty or invalid index");
                return null;
            }

            var prevIndex = (CurrentTrackIndex - 1 + TrackQueue.Count) % TrackQueue.Count;
            Console.WriteLine($" [Player] Previous track: {CurrentTrackIndex} → {prevIndex}");
            CurrentTrackIndex = prevIndex;
            return TrackQueue[prevIndex];
        }
    }
}