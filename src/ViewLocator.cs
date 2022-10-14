using System;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using FileTransfer.ViewModels;

namespace FileTransfer;

public class ViewLocator : IDataTemplate
{
    public IControl Build(object data)
    {
        string name = data.GetType().FullName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);

        if (type is null || Activator.CreateInstance(type) is not Control view)
            return new TextBlock {Text = "Not Found: " + name};
            
        view.DataContext = data;
        return view;
    }

    public bool Match(object data) => data is ViewModelBase;
}