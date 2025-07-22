using Core.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Services
{
    public interface IS3ArticleProvider
    {
        Task<Article?> GetArticleByIdAsync(int id);
        Task<Article?> GetArticleByTitleAsync(string title);
        Task<List<Article>> GetAllArticlesAsync();
    }
} 