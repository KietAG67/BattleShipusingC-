using System.Windows.Input;
using BattleShip.Wpf.Commands;
using BattleShip.Wpf.Services;

namespace BattleShip.Wpf.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _message = string.Empty;
        private bool _isError;

        public LoginViewModel(NavigationStore navigationStore)
        {
            _navigationStore = navigationStore;

            LoginCommand = new RelayCommand(_ => Login());
            RegisterCommand = new RelayCommand(_ => Register());
        }

        public string Username
        {
            get => _username;
            set => SetField(ref _username, value ?? string.Empty);
        }

        /// <summary>Được PasswordBoxAssist đồng bộ từ PasswordBox. Không bao giờ hiển thị lại ra giao diện.</summary>
        public string Password
        {
            get => _password;
            set
            {
                if (SetField(ref _password, value ?? string.Empty))
                {
                    OnPropertyChanged(nameof(HasPassword));
                }
            }
        }

        public bool HasPassword => _password.Length > 0;

        public string Message
        {
            get => _message;
            private set => SetField(ref _message, value);
        }

        public bool IsError
        {
            get => _isError;
            private set => SetField(ref _isError, value);
        }

        public ICommand LoginCommand { get; }

        public ICommand RegisterCommand { get; }

        private void Login()
        {
            var result = AppServices.Auth.Login(Username, Password);
            if (!result.Success)
            {
                Show(result.Message, isError: true);
                return;
            }

            _navigationStore.CurrentViewModel = new MainMenuViewModel(_navigationStore);
        }

        private void Register()
        {
            var result = AppServices.Auth.Register(Username, Password);
            Show(result.Message, isError: !result.Success);
        }

        private void Show(string message, bool isError)
        {
            IsError = isError;
            Message = message;
        }
    }
}
