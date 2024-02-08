using System.Diagnostics;
using UnitystationLauncher.Models.Api.Changelog;

namespace UnitystationLauncher.ViewModels;

public class ChangeViewModel : ViewModelBase
{
    public string Type { get; }
    public string Description { get; }
    public string AuthorAndPr { get; }
    public string? PrUrl { get; }

    public ChangeViewModel(Change change)
    {
        Type = $"[{change.Category}]";
        Description = change.Description ?? "null";
        AuthorAndPr = $"{change.AuthorUsername ?? "null"} | PR #{change.PrNumber}";
        PrUrl = change.PrUrl;
    }

    public bool OnClick()
    {
        if (PrUrl != null)
        {
            ProcessStartInfo psi = new()
            {
                FileName = PrUrl,
                UseShellExecute = true
            };

            Process.Start(psi);
            return true;
        }

        return false;
    }

    public override void Refresh()
    {
        // Do nothing
    }
}