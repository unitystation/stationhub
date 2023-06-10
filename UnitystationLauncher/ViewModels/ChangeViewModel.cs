using System.Diagnostics;
using UnitystationLauncher.Models.Api.Changelog;

namespace UnitystationLauncher.ViewModels;

public class ChangeViewModel : ViewModelBase
{
    private string Type { get; }
    private string Description { get; }
    private string AuthorAndPr { get; }
    private string? PrUrl { get; }

    public ChangeViewModel(Change change)
    {
        Type = $"[{change.Category}]";
        Description = change.Description ?? "null";
        AuthorAndPr = $"{change.AuthorUsername ?? "null"} | PR #{change.PrNumber}";
        PrUrl = change.PrUrl;
    }

    private void OnClick()
    {
        if (PrUrl != null)
        {
            ProcessStartInfo psi = new()
            {
                FileName = PrUrl,
                UseShellExecute = true
            };

            Process.Start(psi);
        }
    }

    public override void Refresh()
    {
        // Do nothing
    }
}