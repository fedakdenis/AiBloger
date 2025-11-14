using AiBloger.Core.Entities;

namespace AiBloger.Core.Interfaces;

public interface IAuthorService
{
    Task<PostInfo> ProcessUrlAsync(string url, string model);
    Task<SelectedNews> SelectBestTitlesAsync(List<NewsTitle> titles, int top, string model);
}
