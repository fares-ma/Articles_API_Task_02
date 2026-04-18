# PowerShell script to test the Articles API - Get Article by Title endpoint
# This script demonstrates how to authenticate and call the protected endpoint

param(
    [string]$BaseUrl = "http://localhost:5212",
    [string]$Username = "admin",
    [string]$Password = "password123",
    [string]$ArticleTitle = "test"
)

Write-Host "=== Articles API Test - Get Article by Title ===" -ForegroundColor Cyan
Write-Host "Base URL: $BaseUrl" -ForegroundColor Yellow
Write-Host "Username: $Username" -ForegroundColor Yellow
Write-Host "Article Title: $ArticleTitle" -ForegroundColor Yellow
Write-Host ""

# Step 1: Test API connection
Write-Host "Step 1: Testing API connection..." -ForegroundColor Green
try {
    $healthResponse = Invoke-RestMethod -Uri "$BaseUrl/api/Test/health" -Method Get -TimeoutSec 10
    Write-Host "✓ API is running. Response: $healthResponse" -ForegroundColor Green
} catch {
    Write-Host "✗ Failed to connect to API: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Make sure the API is running on $BaseUrl" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Step 2: Login to get JWT token
Write-Host "Step 2: Logging in to get JWT token..." -ForegroundColor Green
$loginBody = @{
    username = $Username
    password = $Password
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$BaseUrl/api/Auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.token
    Write-Host "✓ Login successful!" -ForegroundColor Green
    Write-Host "  User: $($loginResponse.username)" -ForegroundColor White
    Write-Host "  Token expires at: $($loginResponse.expiresAt)" -ForegroundColor White
    Write-Host "  Token: $($token.Substring(0, 50))..." -ForegroundColor Gray
} catch {
    Write-Host "✗ Login failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "Check your username and password" -ForegroundColor Yellow
    }
    exit 1
}

Write-Host ""

# Step 3: Test the protected endpoint - Get Article by Title
Write-Host "Step 3: Getting article by title (protected endpoint)..." -ForegroundColor Green
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

try {
    $articleResponse = Invoke-RestMethod -Uri "$BaseUrl/api/Articles/title/$([System.Web.HttpUtility]::UrlEncode($ArticleTitle))" -Method Get -Headers $headers
    Write-Host "✓ Article found successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Article Details:" -ForegroundColor Cyan
    Write-Host "  ID: $($articleResponse.id)" -ForegroundColor White
    Write-Host "  Title: $($articleResponse.title)" -ForegroundColor White
    Write-Host "  Author: $($articleResponse.author)" -ForegroundColor White
    Write-Host "  Created: $($articleResponse.createdAt)" -ForegroundColor White
    Write-Host "  Published: $($articleResponse.isPublished)" -ForegroundColor White
    Write-Host "  View Count: $($articleResponse.viewCount)" -ForegroundColor White
    if ($articleResponse.newspaperName) {
        Write-Host "  Newspaper: $($articleResponse.newspaperName)" -ForegroundColor White
    }
    Write-Host ""
    Write-Host "Description:" -ForegroundColor Cyan
    Write-Host "  $($articleResponse.description)" -ForegroundColor White
} catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    Write-Host "✗ Failed to get article: $($_.Exception.Message)" -ForegroundColor Red
    
    if ($statusCode -eq 401) {
        Write-Host "  Unauthorized - Token might be invalid or expired" -ForegroundColor Yellow
    } elseif ($statusCode -eq 404) {
        Write-Host "  Article not found with title: '$ArticleTitle'" -ForegroundColor Yellow
        Write-Host "  Try a different article title" -ForegroundColor Yellow
    } else {
        Write-Host "  HTTP Status Code: $statusCode" -ForegroundColor Yellow
    }
}

Write-Host ""

# Step 4: Test getting all articles (no auth required)
Write-Host "Step 4: Getting all articles (no authentication required)..." -ForegroundColor Green
try {
    $allArticlesResponse = Invoke-RestMethod -Uri "$BaseUrl/api/Articles?pageNumber=1&pageSize=5" -Method Get
    Write-Host "✓ Articles retrieved successfully!" -ForegroundColor Green
    Write-Host "  Total Articles: $($allArticlesResponse.totalCount)" -ForegroundColor White
    Write-Host "  Page: $($allArticlesResponse.pageNumber) of $($allArticlesResponse.totalPages)" -ForegroundColor White
    Write-Host ""
    Write-Host "Available Articles:" -ForegroundColor Cyan
    foreach ($article in $allArticlesResponse.items) {
        Write-Host "  - $($article.title) (ID: $($article.id))" -ForegroundColor White
    }
} catch {
    Write-Host "✗ Failed to get articles: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Test Complete ===" -ForegroundColor Cyan

# Usage examples:
Write-Host ""
Write-Host "Usage Examples:" -ForegroundColor Yellow
Write-Host "  .\test-article-by-title.ps1" -ForegroundColor Gray
Write-Host "  .\test-article-by-title.ps1 -ArticleTitle 'My Article'" -ForegroundColor Gray
Write-Host "  .\test-article-by-title.ps1 -BaseUrl 'http://localhost:5000' -ArticleTitle 'Test'" -ForegroundColor Gray