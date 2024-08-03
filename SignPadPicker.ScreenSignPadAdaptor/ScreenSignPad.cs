﻿using Microsoft.Win32.SafeHandles;
using SignPadPicker.Exceptions;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Screen = System.Windows.Forms.Screen;
using Size = System.Windows.Size;

namespace SignPadPicker.Adaptor
{
    public class ScreenSignPad : ISignPadPlugin
    {
        public string Name => "ScreenSignPad";

        public string Description => "ScreenSignPad Plugin";

        public bool IsPhysicalDevice => false;

        public bool IsAvailable => true;

        public string Activate(Window owner = null)
        {
            SignPadConfig config = new SignPadConfig
            {
                ScreenSizeWidth = double.NaN,
                ScreenSizeHeight = double.NaN,
                ScreenPositionLeft = double.NaN,
                ScreenPositionTop = double.NaN,
                ScreenIsMaximized = false,
            };

            return Activate(config, owner);
        }

        public string Activate(SignPadConfig config, Window owner = null)
        {
            Window win = CreateWindow(config, owner);

            _ = win.ShowDialog();

            if (win.Content is ScreenSignPadUserControl uc &&
                uc.DataContext is ScreenSignPadViewModel vm)
            {
                if (vm.Result?.Exception != null)
                {
                    throw vm.Result.Exception;
                }

                if (!string.IsNullOrEmpty(vm.Result?.SignImgFilePath))
                {
                    return vm.Result.SignImgFilePath;
                }
            }

            throw new SignCancelException();
        }

        private Window CreateWindow(SignPadConfig config, Window owner = null)
        {
            Size winSize = new Size(
                width: double.IsNaN(config.ScreenSizeWidth) ? 500 : config.ScreenSizeWidth,
                height: double.IsNaN(config.ScreenSizeHeight) ? 380 : config.ScreenSizeHeight);

            Window win = new Window
            {
                Owner = owner ?? Application.Current.MainWindow,
                ShowInTaskbar = false,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Left = double.IsNaN(config.ScreenPositionLeft)
                    ? (Screen.PrimaryScreen.WorkingArea.Left + Screen.PrimaryScreen.WorkingArea.Width / 2 - winSize.Width / 2)
                    : config.ScreenPositionLeft,
                Top = double.IsNaN(config.ScreenPositionTop)
                    ? (Screen.PrimaryScreen.WorkingArea.Top + Screen.PrimaryScreen.WorkingArea.Height / 2 - winSize.Height / 2)
                    : config.ScreenPositionTop,
                Width = winSize.Width,
                Height = winSize.Height,
                WindowStyle = WindowStyle.None,
                WindowState = config.ScreenIsMaximized ? WindowState.Maximized : WindowState.Normal,
            };

            ScreenSignPadUserControl uc = new ScreenSignPadUserControl();

            if (uc.DataContext is ScreenSignPadViewModel viewModel)
            {
                viewModel.Owner = win;
                viewModel.SetBrushSize(config.PenBrushSize);
            }

            win.Content = uc;

            return win;
        }
    }
}
