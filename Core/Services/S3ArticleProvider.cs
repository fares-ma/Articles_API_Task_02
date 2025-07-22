using Amazon.S3;
using Amazon.S3.Model;
using AutoMapper;
using Core.Domain.Models;
using Newtonsoft.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Shared.DTOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Services
{
    public class S3ArticleProvider : IS3ArticleProvider
    {
        private readonly IAmazonS3 _s3Client;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly string _bucketName;
        private readonly string _articlesKey;
        private const string S3_ARTICLES_CACHE_KEY = "S3_ALL_ARTICLES";
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(15);

        public S3ArticleProvider(IAmazonS3 s3Client, IMapper mapper, IMemoryCache cache, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _mapper = mapper;
            _cache = cache;
            _bucketName = configuration["AWS:BucketName"];
            _articlesKey = configuration["AWS:ArticlesKey"] ?? "articles/all-articles.json";
        }

        public async Task<Article?> GetArticleByIdAsync(int id)
        {
            var articles = await GetAllArticlesAsync();
            return articles.FirstOrDefault(a => a.Id == id);
        }

        public async Task<Article?> GetArticleByTitleAsync(string title)
        {
            if (string.IsNullOrEmpty(title))
                return null;

            string cacheKey = $"{S3_ARTICLES_CACHE_KEY}_TITLE_{title.ToLower()}";
            if (_cache.TryGetValue(cacheKey, out Article cachedArticle))
            {
                return cachedArticle;
            }

            var articles = await GetAllArticlesAsync();
            var article = articles.FirstOrDefault(a => a.Title.Equals(title, StringComparison.OrdinalIgnoreCase));

            if (article != null)
            {
                _cache.Set(cacheKey, article, _cacheExpiration);
            }

            return article;
        }

        public async Task<List<Article>> GetAllArticlesAsync()
        {
            if (_cache.TryGetValue(S3_ARTICLES_CACHE_KEY, out List<Article> cachedArticles))
            {
                return cachedArticles;
            }

            try
            {
                if (_s3Client == null)
                {
                    throw new InvalidOperationException("S3 client is not configured properly");
                }

                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = _articlesKey
                };

                using var response = await _s3Client.GetObjectAsync(request);
                using var reader = new StreamReader(response.ResponseStream);
                var json = await reader.ReadToEndAsync();

                var articleDtos = JsonConvert.DeserializeObject<List<ArticleDto>>(json);
                var articles = articleDtos?.Select(dto => _mapper.Map<Article>(dto)).ToList() ?? new List<Article>();

                _cache.Set(S3_ARTICLES_CACHE_KEY, articles, _cacheExpiration);
                return articles;
            }
            catch (AmazonS3Exception ex)
            {
                throw new InvalidOperationException($"S3 service error: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error reading from S3: {ex.Message}");
            }
        }
    }
} 