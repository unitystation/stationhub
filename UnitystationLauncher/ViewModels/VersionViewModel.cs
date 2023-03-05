using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnitystationLauncher.Models.Api.Changelog;

namespace UnitystationLauncher.ViewModels;

public class VersionViewModel : ViewModelBase
{
    private string Version { get; }
    private string Date { get; }
    private bool NoRegisteredChanges { get; }
    private ObservableCollection<ChangeViewModel> Changes { get; }

    public VersionViewModel(GameVersion version)
    {
        Version = version.VersionNumber ?? "null";
        Date = version.DateCreated.ToShortDateString();
        Changes = new();

        if (version.Changes != null && version.Changes.Count != 0)
        {
            ParseChanges(version.Changes);
            NoRegisteredChanges = false;
        }
        else
        {
            NoRegisteredChanges = true;
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