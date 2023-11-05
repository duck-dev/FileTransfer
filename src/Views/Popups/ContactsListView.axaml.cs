using System;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FileTransfer.Models;
using FileTransfer.UtilityCollection;
using FileTransfer.ViewModels;

namespace FileTransfer.Views;

public class ContactsListView : UserControl
{
    public ContactsListView()
    {
        InitializeComponent();
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void ShowContactFlyout(Control button)
    {
        if (this.DataContext is not ContactsListViewModel viewModel)
            return;
        ICommand[] commands = { viewModel.WriteMessageCommand, viewModel.EditContactCommand, viewModel.RemoveContactCommand };
        ShowFlyout(button, commands);
    }
    
    private static void ShowFlyout(Control button, ICommand[] commands)
    {
        if (commands.Length < 3)
            throw new ArgumentException($"The '{nameof(commands)}' param doesn't contain enough elements (is: {commands.Length}, should be: 3).", nameof(commands));
        if (button.DataContext is not User user)
            throw new ArgumentException($"The DataContext of the '{nameof(Button)}' param is not of type {nameof(User)}");

        bool isSendingEnabled = user.IsOnline;
        var items = new MenuItem[]
        {
            new() { Header = "Send message", Command = commands[0], CommandParameter = button.DataContext, IsEnabled = isSendingEnabled},
            new() { Header = "Edit contact", Command = commands[1], CommandParameter = button.DataContext },
            new() { Header = "Remove contact", Command = commands[2], CommandParameter = button.DataContext }
        };
        Utilities.ShowFlyout(button, items, FlyoutPlacementMode.BottomEdgeAlignedLeft);
    }
}