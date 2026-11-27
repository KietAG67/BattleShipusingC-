using System.Windows.Input;
using BattleShip.Infrastructure;
using BattleShip.Wpf.Commands;
using BattleShip.Wpf.Services;

namespace BattleShip.Wpf.ViewModels
{
    public class ResetPasswordViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly IPasswordRecoveryService _passwordRecovery;
        private readonly string _email;
        private string _verificationCode = string.Empty;
        private string _newPassword = string.Empty;
        private string _message = string.Empty;
        private bool _isError;

        public ResetPasswordViewModel(NavigationStore navigationStore, IPasswordRecoveryService passwordRecovery, string email)
        {
            _navigationStore = navigationStore;
            _passwordRecovery = passwordRecovery;
            _email = email;

            ConfirmCommand = new RelayCommand(_ => Confirm());
            LoginCommand = new RelayCommand(_ => _navigationStore.CurrentViewModel = new LoginViewModel(_navigationStore));
        }

        public string VerificationCode
        {
            get => _verificationCode;
            set => SetField(ref _verificationCode, value ?? string.Empty);
        }

        /// <summary>Được PasswordBoxAssist đồng bộ từ PasswordBox.</summary>
        public string NewPassword
        {
            get => _newPassword;
            set
            {
                if (SetField(ref _newPassword, value ?? string.Empty))
                {
                    OnPropertyChanged(nameof(HasNewPassword));
                }
            }
        }

        public bool HasNewPassword => _newPassword.Length > 0;

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

        public ICommand ConfirmCommand { get; }

        public ICommand LoginCommand { get; }

        private void Confirm()
        {
            var code = VerificationCode.Trim();
            if (code.Length == 0)
            {
                Show("Please enter the verification code from your email.", isError: true);
                return;
            }

            if (NewPassword.Length < UserRepository.MinPasswordLength)
            {
                Show($"Password must be at least {UserRepository.MinPasswordLength} characters.", isError: true);
                return;
            }

            var result = _passwordRecovery.ResetPassword(_email, code, NewPassword);
            if (!result.Success)
            {
                Show(result.Message, isError: true);
                return;
            }

            _navigationStore.CurrentViewModel = new LoginViewModel(
                _navigationStore,
                "Password changed. Please log in with your new password.");
        }

        private void Show(string message, bool isError)
        {
            IsError = isError;
            Message = message;
        }
    }
}
