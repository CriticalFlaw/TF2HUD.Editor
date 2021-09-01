using HUDEditor.Classes;
using log4net;
using Moq;
using NUnit.Framework;
using System;
using System.Windows;
using System.Windows.Forms;

namespace HUDEditor.Tests
{
    public class The_Hud_Directory
    {
        protected Mock<ILog> _logger;
        protected Mock<INotifier> _notifier;
        protected Mock<IUtilities> _utilities;
        protected Mock<IAppSettings> _settings;
        protected Mock<IApplication> _application;
        protected Mock<ILocalization> _localization;
        protected Mock<IServiceProvider> _serviceProvider;
        protected Mock<IFolderBrowserDialog> _dialog;
        protected string _currentHudPath;
        protected bool _userInitiated;
        protected HudDirectory _hudDirectory;

        [SetUp]
        public virtual void Setup()
        {
            _logger = new Mock<ILog>();
            _notifier = new Mock<INotifier>();
            _utilities = new Mock<IUtilities>();
            _settings = new Mock<IAppSettings>();
            _application = new Mock<IApplication>();
            _localization = new Mock<ILocalization>();
            _localization.SetupGet(l => l.InfoPathInvalid).Returns("invalid path");
            _localization.SetupGet(l => l.ErrorAppDirectory).Returns("error app directory");
            _dialog = new Mock<IFolderBrowserDialog>();
            _serviceProvider = new Mock<IServiceProvider>();
            _serviceProvider.Setup(s => s.GetService(typeof(IFolderBrowserDialog))).Returns(_dialog.Object);
            _hudDirectory = new HudDirectory(_logger.Object, _utilities.Object, _notifier.Object, _settings.Object, _application.Object, _localization.Object, _serviceProvider.Object);
        }

        protected void Act()
        {
            _hudDirectory.Setup(_currentHudPath, _userInitiated);
        }

        [TestFixture]
        public class Given_the_method_is_not_initiated_by_the_user : The_Hud_Directory
        {
            public override void Setup()
            {
                base.Setup();
                _userInitiated = false;
            }

            [TestFixture]
            public class Given_the_directory_is_already_setup : Given_the_method_is_not_initiated_by_the_user
            {
                public override void Setup()
                {
                    base.Setup();
                    _utilities.Setup(u => u.CheckUserPath(_currentHudPath)).Returns(true);
                    _utilities.Setup(u => u.SearchRegistry()).Returns(true);
                }

                [Test]
                public void It_does_not_prompt_the_user()
                {
                    Act();

                    _serviceProvider.Verify(s => s.GetService(typeof(IFolderBrowserDialog)), Times.Never);
                }
            }
        }

        [TestFixture]
        public class Given_the_method_is_initiated_by_the_user : The_Hud_Directory
        {
            public override void Setup()
            {
                base.Setup();
                _userInitiated = true;
            }

            [Test]
            public void It_logs_a_message()
            {
                Act();

                _logger.Verify(l => l.Info("tf/custom directory is not set. Asking the user..."), Times.Once);
            }

            [TestFixture]
            public class Given_the_user_cancels : Given_the_method_is_initiated_by_the_user
            {
                public override void Setup()
                {
                    base.Setup();
                    _dialog.Setup(d => d.ShowDialog()).Returns(DialogResult.Cancel);
                }

                [Test]
                public void It_does_not_save_the_directory()
                {
                    Act();

                    _settings.Verify(s => s.Save(), Times.Never);
                }

                [TestFixture]
                public class Given_the_directory_is_not_set : Given_the_user_cancels
                {
                    public override void Setup()
                    {
                        base.Setup();
                        _settings.SetupGet(s => s.HudDirectory).Returns("invalid\\path");
                        _utilities.Setup(u => u.CheckUserPath("invalid\\path")).Returns(false);
                    }

                    [Test]
                    public void It_logs_a_message()
                    {
                        Act();

                        _logger.Verify(l => l.Info("tf/custom directory still not set. Exiting..."), Times.Once);
                    }

                    [Test]
                    public void It_displays_a_warning()
                    {
                        _utilities.Setup(u => u.GetLocalizedString("error app directory")).Returns("localized error message");
                        
                        Act();

                        _notifier.Verify(n => n.ShowMessageBox(MessageBoxImage.Warning, "localized error message", MessageBoxButton.OK), Times.Once);
                    }

                    [Test]
                    public void It_quits_the_application()
                    {
                        Act();

                        _application.Verify(a => a.Shutdown(), Times.Once);
                    }
                }
            }

            [TestFixture]
            public class Given_the_user_hits_ok : Given_the_method_is_initiated_by_the_user
            {
                public override void Setup()
                {
                    base.Setup();
                    _dialog.Setup(d => d.ShowDialog()).Returns(DialogResult.OK);
                }

                [TestFixture]
                public class Given_the_directory_is_always_invalid : Given_the_user_hits_ok
                {
                    public override void Setup()
                    {
                        base.Setup();
                        _dialog.SetupGet(d => d.SelectedPath).Returns("tf\\cfg");
                    }

                    [Test]
                    public void It_displays_an_error_message()
                    {
                        Act();

                        _notifier.Verify(n => n.ShowMessageBox(MessageBoxImage.Error, "invalid path", MessageBoxButton.OK), Times.AtLeastOnce);
                    }

                    [Test]
                    public void It_does_not_save_the_directory()
                    {
                        Act();

                        _settings.Verify(s => s.Save(), Times.Never);
                    }
                }

                [TestFixture]
                public class Given_the_directory_is_invalid_and_then_valid : Given_the_user_hits_ok
                {
                    public override void Setup()
                    {
                        base.Setup();
                        _dialog.SetupSequence(d => d.SelectedPath)
                            .Returns("")
                            .Returns("tf\\cfg")
                            .Returns("tf\\custom");
                    }

                    [Test]
                    public void It_displays_an_error_message()
                    {
                        Act();

                        _notifier.Verify(n => n.ShowMessageBox(MessageBoxImage.Error, "invalid path", MessageBoxButton.OK), Times.AtLeastOnce);
                    }

                    [Test]
                    public void It_does_not_save_the_directory()
                    {
                        Act();

                        _settings.Verify(s => s.Save(), Times.Never);
                    }
                }

                [TestFixture]
                public class Given_the_directory_is_valid : Given_the_user_hits_ok
                {
                    public override void Setup()
                    {
                        base.Setup();
                        _dialog.SetupGet(d => d.SelectedPath).Returns("tf\\custom");
                    }

                    [Test]
                    public void It_does_not_display_an_error_message()
                    {
                        Act();

                        _notifier.Verify(n => n.ShowMessageBox(MessageBoxImage.Error, "invalid path", MessageBoxButton.OK), Times.Never);
                    }

                    [Test]
                    public void It_updates_the_hud_directory()
                    {
                        string updatedHudDirectory = null;
                        _settings.SetupSet(s => s.HudDirectory = It.IsAny<string>()).Callback<string>(value => updatedHudDirectory = value);
                        
                        Act();

                        Assert.AreEqual("tf\\custom", updatedHudDirectory);
                    }

                    [Test]
                    public void It_saves_changes()
                    {
                        _settings.Setup(s => s.Save()).Verifiable();

                        Act();

                        _settings.Verify(s => s.Save(), Times.Once);
                    }

                    [Test]
                    public void It_logs_a_message()
                    {
                        _settings.SetupGet(s => s.HudDirectory).Returns("tf\\custom");

                        Act();

                        _logger.Verify(l => l.Info($"tf/custom directory is set to: tf\\custom"), Times.Once);
                    }
                }
            }
        }
    }
}