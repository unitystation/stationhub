using UnitystationLauncher.Models.Api.Changelog;

namespace UnitystationLauncher.ViewModels;

public class ChangeViewModel : ViewModelBase
{
    private string Type { get; }
    private string Description { get; }
    private string AuthorAndPr { get; }

    public ChangeViewModel(Change change)
    {
        Type = $"[{change.Category}]";
        Description = change.Description ?? "null";
        AuthorAndPr = $"{change.AuthorUsername ?? "null"} | PR #{change.PrNumber}";
    }
}