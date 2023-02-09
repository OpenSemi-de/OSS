﻿namespace ACDCs.ApplicationLogic.Components.Components;

using System.Reflection;
using ACDCs.Data.ACDCs.Components.BJT;
using ACDCs.Data.ACDCs.Interfaces;
using IO.Spice;
using ModelEditor;
using ModelSelection;
using Sharp.UI;

public class ComponentsView : Grid
{
    public List<ComponentViewModel> dataSource = new();
    private readonly string _category = string.Empty;
    private readonly ListView _componentsGrid;
    private readonly Entry _keywordEntry;
    private readonly Window.Window _window;
    private List<ComponentViewModel> _baseData = new();
    private object? _lastSelectedItem;

    public ComponentsView(Window.Window window)
    {
        this.Margin(4);
        _window = window;

        ColumnDefinitionCollection columnDefinitions = new() { new Microsoft.Maui.Controls.ColumnDefinition() };
        this.ColumnDefinitions(columnDefinitions);

        RowDefinitionCollection rowDefinitions = new()
        {
            new Microsoft.Maui.Controls.RowDefinition(40),
            new Microsoft.Maui.Controls.RowDefinition(34), new Microsoft.Maui.Controls.RowDefinition()
        };

        this.RowDefinitions(rowDefinitions);

        _keywordEntry = new Entry()
            .OnTextChanged(KeywordEntry_OnTextChanged)
            .HorizontalOptions(LayoutOptions.Fill);

        StackLayout inputlayout = new() { new Label("Search text:"), _keywordEntry };
        inputlayout.Row(1).Orientation(StackOrientation.Horizontal);
        Add(inputlayout);

        _componentsGrid = new ListView()
            .OnItemTapped(ComponentsGrid_ItemTapped)
            .ItemTemplate(new ComponentsGridTemplate())
            .Row(2);

        Add(_componentsGrid);

        Loaded += OnLoaded;
    }

    public async void ImportSpiceModels(string fileName)
    {
        string jsonData = await File.ReadAllTextAsync(fileName);

        SpiceReader spiceReader = new();
        List<IElectronicComponent> components = spiceReader.ReadComponents(jsonData);
        if (spiceReader.HasErrors && spiceReader.Errors != null && API.MainPage != null)
        {
            await API.MainPage.DisplayAlert("Model import failed",
                string.Join(':', spiceReader.Errors), "ok");
            // return;
        }
        LoadFromSource(components);
    }

    public void LoadFromSource(List<IElectronicComponent> components)
    {
        dataSource.Clear();
        components = components.OrderBy(c => c.Name).ThenBy(c => c.Type).ToList();
        int row = 0;
        foreach (IElectronicComponent component in components)
        {
            ComponentViewModel modelLine = new()
            {
                Name = component.Name,
                Type = component.GetType().Name,
                Row = row,
                Model = component
            };

            switch (component)
            {
                case Bjt bjt:
                    modelLine.Value = bjt.TypeName;
                    modelLine.Model = bjt;
                    break;
            }

            dataSource.Add(modelLine);
            row++;
        }

        _baseData = dataSource.ToList();
        Reload();
    }

    // ReSharper disable once UnusedMember.Global
    public bool OnClose()
    {
        return true;
        /*
                ComponentsGrid.BatchBegin();
                dataSource.Clear();
                _baseData?.Clear();
                ComponentsGrid.ItemsSource = new List<string>();
                ComponentsGrid.BatchCommit();
        */
    }

    private static bool ReflectedSearch(ComponentViewModel componentViewModel, string text)
    {
        Type? modelType = componentViewModel.Model?.GetType();
        text = text.ToLower();
        if (modelType == null)
        {
            return false;
        }

        foreach (PropertyInfo propertyInfo in modelType.GetProperties())
        {
            string? value = Convert.ToString(propertyInfo.GetValue(componentViewModel.Model));
            if (value != null)
            {
                value = value.ToLower();
                if (value.Contains(text))
                    return true;
            }
            else
            {
                return false;
            }
        }

        return false;
    }

    private void ComponentsGrid_ItemTapped(object? sender, ItemTappedEventArgs e)
    {
        if (e.Item == _lastSelectedItem)
        {
            ShowModelEditor(e.Item);
            _lastSelectedItem = null;
        }
        _lastSelectedItem = e.Item;
    }

    private void KeywordEntry_OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        IEnumerable<ComponentViewModel> query = _baseData;
        if (_category != "")
        {
            query = query.Where(d => d.Type.ToLower().Contains(_category.ToLower()));
        }

        string keyword = _keywordEntry.Text.ToLower();

        if (keyword != "")
        {
            query = query.Where(d => d.Name != null && (ReflectedSearch(d, _keywordEntry.Text) ||
                                                        d.Type.ToLower().Contains(keyword) ||
                                                        d.Name.ToLower().Contains(keyword)));
        }

        dataSource.Clear();
        foreach (ComponentViewModel model in query)
        {
            dataSource.Add(model);
        }

        Reload();
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
    }

    private void OnModelEdited(IElectronicComponent obj)
    {
    }

    private void Reload()
    {
        _componentsGrid.ItemsSource = null;
        _componentsGrid.ItemsSource = dataSource;
    }

    private void ShowModelEditor(object selectedItem)
    {
        if (selectedItem is ComponentViewModel viewModel)
        {
            ModelEditorWindow modelEditor = new(API.MainContainer)
            {
                OnModelEdited = OnModelEdited
            };
            modelEditor.GetProperties(viewModel.Model);
        }
    }
}
