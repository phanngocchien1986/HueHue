﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SharpDX.DirectInput;
using System.Windows.Threading;
using HueHue.Helpers;
using MaterialDesignThemes.Wpf;
using System.Collections.ObjectModel;

namespace HueHue.Views
{
    /// <summary>
    /// Interaction logic for JoystickMode.xaml
    /// </summary>
    public partial class JoystickMode : UserControl
    {
        DispatcherTimer timer;
        ObservableCollection<ButtonColor> buttonsToColors;
        List<Guid> guids;
        Joystick joystick;
        JoystickHelper joystickHelper;

        public JoystickMode()
        {
            InitializeComponent();

            //GetInput();

            timer = new DispatcherTimer()
            {
                Interval = TimeSpan.FromMilliseconds(App.settings.Speed)
            };
            timer.Tick += Timer_Tick;

            joystickHelper = new JoystickHelper();

            guids = joystickHelper.GetGuids();
            combo_joysticks.ItemsSource = joystickHelper.GetJoystickNames(guids);
            buttonsToColors = new ObservableCollection<ButtonColor>();

            if (combo_joysticks.Items.Count > 0)
            {
                combo_joysticks.SelectedIndex = 0;
            }

            for (int i = 0; i < 5; i++)
            {
                buttonsToColors.Add(new ButtonColor() { Button = JoystickOffset.Buttons0, Color = new LEDBulb() });
            }

            foreach (var item in buttonsToColors)
            {
                StackColors.Children.Add(new ButtonToColor(item));
            }

            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (joystick == null)
            {
                return;
            }

            joystick.Poll();

            joystick.GetCurrentState();
            var datas = joystick.GetBufferedData();

            foreach (var state in datas)
            {
                ButtonColor PressedColor = (ButtonColor)buttonsToColors.Select(x => x.Button == state.Offset);
                if (PressedColor != null)
                {
                    Effects.Colors[0] = PressedColor.Color;
                }
            }

            Effects.FixedColor();
        }

        private void combo_joysticks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            timer.Stop();

            if (joystick != null)
            {
                joystick.Unacquire();
                joystick.Dispose();
            }

            timer.Start();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            guids = joystickHelper.GetGuids();
            combo_joysticks.ItemsSource = joystickHelper.GetJoystickNames(guids);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            joystick = joystickHelper.HookJoystick(guids[combo_joysticks.SelectedIndex]);
        }

        private async void Button_AddButtonColor_Click(object sender, RoutedEventArgs e)
        {
            var newButton = await DialogHost.Show(new AddButton(guids[combo_joysticks.SelectedIndex], joystickHelper, dialogHost));
            buttonsToColors.Add((ButtonColor)newButton);
            StackColors.Children.Add(new ButtonToColor(buttonsToColors[buttonsToColors.Count - 1]));
        }

        private void dialogHost_DialogClosing(object sender, MaterialDesignThemes.Wpf.DialogClosingEventArgs eventArgs)
        {

        }
    }
}
