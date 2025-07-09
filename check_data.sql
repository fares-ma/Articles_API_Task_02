-- Check if Articles table exists and has data
USE ArticlesDB;

-- Check table structure
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Articles'
ORDER BY ORDINAL_POSITION;

-- Check if data exists
SELECT COUNT(*) as TotalArticles FROM Articles;

-- Show sample articles
SELECT TOP 5 
    Id,
    Title,
    Author,
    IsPublished,
    ViewCount,
    CreatedAt
FROM Articles
ORDER BY CreatedAt DESC; 