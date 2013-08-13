using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Input;
using DotNetUtilitiesApp.Properties;
using DotNetUtilitiesApp.WpfUtils;
using TinyGithub;

namespace DotNetUtilitiesApp
{
    internal class GitHubSettingViewModel : ViewModel
    {
        private Github _gitHub;
        private string _personalApiAccessToken;
        private GitHubSetting _window;

        private bool _canApply;

        public ICommand ApplyGitHubSetting { get; private set; }
        public ICommand CancelGitHubSetting { get; private set; }
        public ICommand ValidGitHubSetting { get; private set; }

        public string PersonalApiAccessToken
        {
            get { return _personalApiAccessToken; }
            set
            {
                if( value != _personalApiAccessToken )
                {
                    _personalApiAccessToken = value;
                    if( IsPersonalApiTokenValid( value ) )
                    {
                        _canApply = true;
                        _gitHub.SetApiToken( value );
                    }
                    RaisePropertyChanged();
                }
            }
        }

        internal GitHubSettingViewModel( GitHubSetting window)
        {
            _gitHub = new Github();
            _window = window;

            LoadSettings();

            PrepareCommands();

            _canApply = false;
        }

        private void PrepareCommands()
        {
            ApplyGitHubSetting = new RelayCommand( ExecuteApplyGitHubSetting, CanExecuteApplyGitHubSetting );
            CancelGitHubSetting = new RelayCommand( ExecuteCancelGitHubSetting );
            ValidGitHubSetting = new RelayCommand( ExecuteValidGitHubSetting );
        }

        private void ExecuteValidGitHubSetting( object obj )
        {
            SaveSettings();
            _window.Close();
        }

        private void ExecuteCancelGitHubSetting( object obj )
        {
            _window.Close();
        }

        private void ExecuteApplyGitHubSetting( object obj )
        {
            SaveSettings();
            _canApply = false;
        }

        private bool CanExecuteApplyGitHubSetting(object obj)
        {
            return _canApply;
        }

        private void LoadSettings()
        {
            PersonalApiAccessToken = Settings.Default.PersonalApiToken;
        }

        private void SaveSettings()
        {
            Settings.Default.PersonalApiToken = PersonalApiAccessToken;

            Settings.Default.Save();
        }

        private static bool IsPersonalApiTokenValid( string apiToken )
        {
            return Regex.Match( apiToken, "^[a-fA-F0-9]{40}$" ).Success;
        }
    }
}
