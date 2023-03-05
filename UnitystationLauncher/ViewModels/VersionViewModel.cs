using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnitystationLauncher.Models.Api.Changelog;

namespace UnitystationLauncher.ViewModels;

public class VersionViewModel : ViewModelBase
{
    private string Version { get; }
    private string Date { get; }
    private ObservableCollection<ChangeViewModel> Changes { get; }

    public VersionViewModel(GameVersion version)
    {
        Version = version.VersionNumber ?? "null";
        Date = version.DateCreated.ToShortDateString();
        Changes = new();

        if (version.Changes != null)
        {
            ParseChanges(version.Changes);
        }
    }

    private void ParseChanges(List<Change> changes)
    {
        foreach (Change change in changes)
        {
            Changes.Add(new(change));
        }
    }
}