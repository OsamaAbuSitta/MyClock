using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using MyClock.Core.Models;
using MyClock.Infrastructure.Services;
using ReactiveUI;

namespace MyClock.App.ViewModels;

public class SessionSetEditorViewModel : ViewModelBase
{
    private readonly SettingsService _settingsService;

    public event Action? CloseRequested;
    public bool Saved { get; private set; }

    public ObservableCollection<SessionSetViewModel> Sets { get; } = new();

    private SessionSetViewModel? _selectedSet;
    public SessionSetViewModel? SelectedSet
    {
        get => _selectedSet;
        set => this.RaiseAndSetIfChanged(ref _selectedSet, value);
    }

    public ReactiveCommand<Unit, Unit> NewSetCommand { get; }
    public ReactiveCommand<Unit, Unit> DeleteSetCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveCommand { get; }
    public ReactiveCommand<Unit, Unit> CancelCommand { get; }

    public SessionSetEditorViewModel(SettingsService settingsService)
    {
        _settingsService = settingsService;

        foreach (var set in settingsService.Current.SessionSets)
            Sets.Add(new SessionSetViewModel(set, settingsService));

        SelectedSet = Sets.FirstOrDefault();

        NewSetCommand = ReactiveCommand.Create(() =>
        {
            var newSet = new SessionSet { Name = "New Set" };
            settingsService.Current.SessionSets.Add(newSet);
            var vm = new SessionSetViewModel(newSet, settingsService);
            Sets.Add(vm);
            SelectedSet = vm;
        });

        DeleteSetCommand = ReactiveCommand.Create(() =>
        {
            if (SelectedSet is null || SelectedSet.IsBuiltIn) return;
            settingsService.Current.SessionSets.RemoveAll(s => s.Id == SelectedSet.Id);
            Sets.Remove(SelectedSet);
            SelectedSet = Sets.FirstOrDefault();
        });

        SaveCommand   = ReactiveCommand.Create(ExecuteSave);
        CancelCommand = ReactiveCommand.Create(ExecuteCancel);
    }

    private void ExecuteSave()
    {
        // Flush all view model edits back to the model objects
        foreach (var setVm in Sets)
            setVm.FlushToModel();

        _settingsService.Save();
        Saved = true;
        CloseRequested?.Invoke();
    }

    private void ExecuteCancel()
    {
        // Discard in-memory changes by reloading from disk on next open
        Saved = false;
        CloseRequested?.Invoke();
    }
}

// ── Per-set view model ────────────────────────────────────────────────────────

public class SessionSetViewModel : ViewModelBase
{
    private readonly SessionSet _model;
    private readonly SettingsService _settingsService;

    public string Id => _model.Id;
    public bool IsBuiltIn => _model.IsBuiltIn;

    private string _name;
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    public ObservableCollection<SessionItemViewModel> Sessions { get; } = new();

    public ReactiveCommand<Unit, Unit> AddSessionCommand { get; }
    public ReactiveCommand<SessionItemViewModel, Unit> RemoveSessionCommand { get; }
    public ReactiveCommand<SessionItemViewModel, Unit> MoveUpCommand { get; }
    public ReactiveCommand<SessionItemViewModel, Unit> MoveDownCommand { get; }
    public ReactiveCommand<Unit, Unit> RestoreCommand { get; }

    public SessionSetViewModel(SessionSet model, SettingsService settingsService)
    {
        _model = model;
        _settingsService = settingsService;
        _name = model.Name;

        foreach (var item in model.Sessions.OrderBy(s => s.Order))
            Sessions.Add(new SessionItemViewModel(item));

        AddSessionCommand = ReactiveCommand.Create(() =>
        {
            var item = new SessionItem { Name = "Session", DurationMinutes = 25, Order = Sessions.Count };
            Sessions.Add(new SessionItemViewModel(item));
        });

        RemoveSessionCommand = ReactiveCommand.Create<SessionItemViewModel>(item =>
        {
            Sessions.Remove(item);
        });

        MoveUpCommand = ReactiveCommand.Create<SessionItemViewModel>(item =>
        {
            var idx = Sessions.IndexOf(item);
            if (idx > 0) Sessions.Move(idx, idx - 1);
        });

        MoveDownCommand = ReactiveCommand.Create<SessionItemViewModel>(item =>
        {
            var idx = Sessions.IndexOf(item);
            if (idx < Sessions.Count - 1) Sessions.Move(idx, idx + 1);
        });

        RestoreCommand = ReactiveCommand.Create(() =>
        {
            if (!IsBuiltIn) return;
            settingsService.RestoreDefaultSet(Id);
            var restored = settingsService.Current.SessionSets.First(s => s.Id == Id);
            Name = restored.Name;
            Sessions.Clear();
            foreach (var s in restored.Sessions.OrderBy(x => x.Order))
                Sessions.Add(new SessionItemViewModel(s));
        });
    }

    // Writes UI state back into the underlying SessionSet model
    public void FlushToModel()
    {
        _model.Name = Name;
        _model.Sessions.Clear();
        for (int i = 0; i < Sessions.Count; i++)
        {
            var s = Sessions[i];
            _model.Sessions.Add(new SessionItem
            {
                Id = s.Id,
                Name = s.Name,
                DurationMinutes = (int)s.DurationMinutes,
                Order = i
            });
        }
    }
}

// ── Per-session-item view model ───────────────────────────────────────────────

public class SessionItemViewModel : ViewModelBase
{
    public string Id { get; }

    private string _name;
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    private decimal _durationMinutes;
    public decimal DurationMinutes
    {
        get => _durationMinutes;
        set => this.RaiseAndSetIfChanged(ref _durationMinutes, value);
    }

    public SessionItemViewModel(SessionItem model)
    {
        Id               = model.Id;
        _name            = model.Name;
        _durationMinutes = model.DurationMinutes;
    }
}
