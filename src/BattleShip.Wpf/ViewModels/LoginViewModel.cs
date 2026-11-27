using System.Windows.Input;
using BattleShip.Wpf.Commands;
using BattleShip.Wpf.Helpers;
using BattleShip.Wpf.Services;

namespace BattleShip.Wpf.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly IPasswordRecoveryService _passwordRecovery = new PendingPasswordRecoveryService();
        private string _username = string.Empty;
        private string _password = string.Empty;
        private bool _rememberMe;
        private string _message = string.Empty;
        private bool _isError;

        /// <param name="notice">Thông báo hiện sẵn, ví dụ sau khi đăng ký hoặc đổi mật khẩu thành công.</param>
        public LoginViewModel(NavigationStore navigationStore, string? notice = null)
        {
            _navigationStore = navigationStore;

            var rememberedUsername = RememberedUsernameStore.Load();
            if (!string.IsNullOrEmpty(rememberedUsername))
            {
                _username = rememberedUsername;
                _rememberMe = true;
            }

            LoginCommand = new RelayCommand(_ => Login());
            ToggleRememberCommand = new RelayCommand(_ => RememberMe = !RememberMe);
            ForgotPasswordCommand = new RelayCommand(_ =>
                _navigationStore.CurrentViewModel = new ForgotPasswordViewModel(_navigationStore, _passwordRecovery));
            SignUpCommand = new RelayCommand(_ => _navigationStore.CurrentViewModel = new SignUpViewModel(_navigationStore));

            if (!string.IsNullOrEmpty(notice))
            {
                Show(notice, isError: false);
            }
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

        /// <summary>Có ghi nhớ tên đăng nhập trên máy này không (không ghi nhớ mật khẩu).</summary>
        public bool RememberMe
        {
            get => _rememberMe;
            set => SetField(ref _rememberMe, value);
        }

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

        public ICommand ToggleRememberCommand { get; }

        public ICommand ForgotPasswordCommand { get; }

        public ICommand SignUpCommand { get; }

        private void Login()
        {
            var result = AppServices.Auth.Login(Username, Password);
            if (!result.Success)
            {
                Show(result.Message, isError: true);
                return;
            }

            if (RememberMe)
            {
                RememberedUsernameStore.Save(Username.Trim());
            }
            else
            {
                RememberedUsernameStore.Clear();
            }

            _navigationStore.CurrentViewModel = new MainMenuViewModel(_navigationStore);
        }

        private void Show(string message, bool isError)
        {
            IsError = isError;
            Message = message;
        }
    }
}
