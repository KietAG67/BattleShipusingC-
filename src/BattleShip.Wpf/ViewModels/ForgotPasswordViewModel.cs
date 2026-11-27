using System.Windows.Input;
using BattleShip.Wpf.Commands;
using BattleShip.Wpf.Helpers;
using BattleShip.Wpf.Services;

namespace BattleShip.Wpf.ViewModels
{
    public class ForgotPasswordViewModel : ViewModelBase
    {
        private readonly NavigationStore _navigationStore;
        private readonly IPasswordRecoveryService _passwordRecovery;
        private string _email = string.Empty;
        private string _message = string.Empty;
        private bool _isError;

        public ForgotPasswordViewModel(NavigationStore navigationStore, IPasswordRecoveryService passwordRecovery)
        {
            _navigationStore = navigationStore;
            _passwordRecovery = passwordRecovery;

            SendResetLinkCommand = new RelayCommand(_ => SendResetLink());
            LoginCommand = new RelayCommand(_ => _navigationStore.CurrentViewModel = new LoginViewModel(_navigationStore));
        }

        public string Email
        {
            get => _email;
            set => SetField(ref _email, value ?? string.Empty);
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

        public ICommand SendResetLinkCommand { get; }

        public ICommand LoginCommand { get; }

        private void SendResetLink()
        {
            var email = Email.Trim();
            if (!EmailValidator.IsValid(email))
            {
                Show("Please enter a valid email address.", isError: true);
                return;
            }

            var result = _passwordRecovery.SendVerificationCode(email);
            if (!result.Success)
            {
                Show(result.Message, isError: true);
                return;
            }

            _navigationStore.CurrentViewModel = new ResetPasswordViewModel(_navigationStore, _passwordRecovery, email);
        }

        private void Show(string message, bool isError)
        {
            IsError = isError;
            Message = message;
        }
    }
}
