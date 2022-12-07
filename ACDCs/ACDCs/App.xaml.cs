﻿using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ACDCs;

public partial class App : Application
{
    private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, object?>> _comValues = new();

    public App()
    {
        InitializeComponent();
        this.UserAppTheme = AppTheme.Dark;
        MainPage = new AppShell();
    }

    public static event ResetEvent? Reset;

    public static async Task Call(Func<Task> action, bool disableReset = false)
    {
        if (!disableReset)
        {
            try
            {
                OnReset(null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        try
        {
            await action().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);

            await Shell.Current.CurrentPage.DisplayAlert("Internal exception", ex.Message.ToString(), "ok");
            //_log?.Invoke(ex.ToString());
        }
    }

    public static T? Com<T>(string name, string property, object? value = null)
    {
        if (value != null)
        {
            if (!_comValues.ContainsKey(name))
                _comValues.GetOrAdd(name, new ConcurrentDictionary<string, object?>());
            if (!_comValues[name].ContainsKey(property))
            {
                _comValues[name].GetOrAdd(property, value);
            }
            else
            {
                _comValues[name][property] = value;
            }
        }

        if (_comValues.ContainsKey(name) && _comValues[name].ContainsKey(property))
        {
            return (T)_comValues[name][property]!;
        }

        return default;
    }

    public static async Task<string> LoadMauiAssetAsString(string name)
    {
        await using var stream = await FileSystem.OpenAppPackageFileAsync(name);
        using var reader = new StreamReader(stream);

        var contents = await reader.ReadToEndAsync();
        return contents;
    }

    private static void OnReset(ResetEventArgs args)
    {
        Reset?.Invoke(null, args);
    }
}

public class ResetEventArgs
{
}

public delegate void ResetEvent(object sender, ResetEventArgs args);