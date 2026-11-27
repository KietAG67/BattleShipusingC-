using System.Windows.Input;
using BattleShip.Infrastructure;
using BattleShip.Wpf.Commands;
using BattleShip.Wpf.Helpers;
using BattleShip.Wpf.Services;

namespace BattleShip.Wpf.ViewModels
{
    public class SignUpViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private string _username = string.Empty;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private bool _agreeTerms;
        private string _message = string.Empty;
        private bool _isError;

        public SignUpViewModel(NavigationStore navigationStore)
        {
            _navigationStore = navigationStore;

            SignUpCommand = new RelayCommand(_ => SignUp());
            ToggleTermsCommand = new RelayCommand(_ => AgreeTerms = !AgreeTerms);
            LoginCommand = new RelayCommand(_ => _navigationStore.CurrentViewModel = new LoginViewModel(_navigationStore));
        }

        public string Username
        {
            get => _username;
            set => SetField(ref _username, value ?? string.Empty);
        }

        /// <summary>Hiện tại chỉ kiểm tra định dạng. Database chưa có cột Email (xem GHI_CHU_CHO_THANH_VIEN.md).</summary>
        public string Email
        {
            get => _email;
            set => SetField(ref _email, value ?? string.Empty);
        }

        /// <summary>Được PasswordBoxAssist đồng bộ từ PasswordBox.</summary>
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

        /// <summary>Được PasswordBoxAssist đồng bộ từ PasswordBox.</summary>
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                if (SetField(ref _confirmPassword, value ?? string.Empty))
                {
                    OnPropertyChanged(nameof(HasConfirmPassword));
                }
            }
        }

        public bool HasConfirmPassword => _confirmPassword.Length > 0;

        public bool AgreeTerms
        {
            get => _agreeTerms;
            set => SetField(ref _agreeTerms, value);
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

        public ICommand SignUpCommand { get; }

        public ICommand ToggleTermsCommand { get; }

        public ICommand LoginCommand { get; }

        private void SignUp()
        {
            var validationError = Validate();
            if (validationError is not null)
            {
                Show(validationError, isError: true);
                return;
            }

            var result = AppServices.Auth.Register(Username.Trim(), Password);
            if (!result.Success)
            {
                Show(result.Message, isError: true);
                return;
            }

            _navigationStore.CurrentViewModel = new LoginViewModel(_navigationStore, "Account created! Please log in.");
        }

        private string? Validate()
        {
            var username = Username.Trim();
            if (username.Length < UserRepository.MinUsernameLength || username.Length > UserRepository.MaxUsernameLength)
            {
                return $"Username must be {UserRepository.MinUsernameLength}-{UserRepository.MaxUsernameLength} characters.";
            }

            if (!EmailValidator.IsValid(Email))
            {
                return "Please enter a valid email address.";
            }

            if (Password.Length < UserRepository.MinPasswordLength)
            {
                return $"Password must be at least {UserRepository.MinPasswordLength} characters.";
            }

            if (Password != ConfirmPassword)
            {
                return "Passwords do not match.";
            }

            if (!AgreeTerms)
            {
                return "Please accept the Terms & Conditions.";
            }

            return null;
        }

        private void Show(string message, bool isError)
        {
            IsError = isError;
            Message = message;
        }
    }
}
