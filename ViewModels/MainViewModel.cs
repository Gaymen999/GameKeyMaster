using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using GameKeyMaster.Models;
using GameKeyMaster.Services;

namespace GameKeyMaster.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly DataService _dataService;
        private AppProfile _profile;
        private GameProfile? _selectedGame;
        private string _statusText = "Hazır";

        public ObservableCollection<GameProfile> Games { get; set; }

        public GameProfile? SelectedGame
        {
            get => _selectedGame;
            set
            {
                _selectedGame = value;
                OnPropertyChanged();
            }
        }

        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            _dataService = new DataService();
            _profile = _dataService.LoadProfile();
            Games = new ObservableCollection<GameProfile>(_profile.Games);
        }

        public void AddGame(string name, string executable)
        {
            var newGame = new GameProfile { Name = name, ExecutableName = executable };
            _profile.Games.Add(newGame);
            Games.Add(newGame);
            Save();
        }

        public void Save()
        {
            _dataService.SaveProfile(_profile);
            StatusText = "Ayarlar kaydedildi.";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
