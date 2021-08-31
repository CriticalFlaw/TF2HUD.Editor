using HUDEditor.Classes;
using log4net;
using Moq;
using NUnit.Framework;
using System;
using System.Windows;

namespace HUDEditor.Tests
{
    public class The_Notifier
    {
        protected Mock<ILog> _logger;
        protected Mock<IMessageBox> _messageBox;
        protected Notifier _notifier;

        protected MessageBoxImage _image;
        protected string _message;

        [SetUp]
        public virtual void Setup()
        {
            _logger = new Mock<ILog>();
            _messageBox = new Mock<IMessageBox>();
            _notifier = new Notifier(_messageBox.Object, _logger.Object);
        }

        protected MessageBoxResult Act()
        {
            return _notifier.ShowMessageBox(_image, _message);
        }

        [Test]
        public void It_Displays_A_MessageBox()
        {
            _messageBox.Setup(m => m.Show(_message, string.Empty, MessageBoxButton.OK, _image)).Verifiable();

            var result = Act();

            _messageBox.Verify(m => m.Show(_message, string.Empty, MessageBoxButton.OK, _image), Times.Once());
        }

        [TestFixture]
        public class Given_an_error_image_type : The_Notifier
        {
            public override void Setup()
            {
                base.Setup();
                _image = MessageBoxImage.Error;
            }

            [Test]
            public void It_Logs_An_Error()
            {
                _logger.Setup(l => l.Error(_message)).Verifiable();

                var result = Act();

                _logger.Verify(l => l.Error(_message), Times.Once());
            }
        }

        [TestFixture]
        public class Given_a_warning_image_type : The_Notifier
        {
            public override void Setup()
            {
                base.Setup();
                _image = MessageBoxImage.Warning;
            }

            [Test]
            public void It_Logs_A_Warning()
            {
                _logger.Setup(l => l.Warn(_message)).Verifiable();

                var result = Act();

                _logger.Verify(l => l.Warn(_message), Times.Once());
            }
        }
    }
}