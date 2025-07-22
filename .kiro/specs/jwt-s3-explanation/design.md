# تصميم شرح تطبيق JWT و Amazon S3

## نظرة عامة

هذا المستند يوضح التصميم الشامل لشرح كيفية تطبيق JWT Authentication و Amazon S3 Integration في مشروع Articles API. التصميم يركز على توضيح المفاهيم النظرية والتطبيق العملي بطريقة يمكن شرحها بوضوح.

## البنية المعمارية

### 1. JWT Authentication Architecture

```
Client Request → AuthController → JwtService → Token Generation
     ↓
Protected Endpoint → JWT Middleware → Token Validation → Access Granted/Denied
```

#### المكونات الأساسية:
- **JwtService**: مسؤول عن إنشاء وتوليد JWT tokens
- **AuthController**: يتعامل مع طلبات تسجيل الدخول
- **JWT Middleware**: يتحقق من صحة التوكن في الطلبات المحمية
- **Configuration**: إعدادات JWT في appsettings.json

### 2. Amazon S3 Integration Architecture

```
ArticleService → S3ArticleProvider → Amazon S3 → JSON Data
     ↓
Memory Cache ← Data Processing ← JSON Deserialization
```

#### المكونات الأساسية:
- **S3ArticleProvider**: يتعامل مع قراءة البيانات من S3
- **ArticleService**: يحدد مصدر البيانات (Database أو S3)
- **Memory Cache**: يحسن الأداء بتخزين البيانات مؤقتاً
- **AWS Configuration**: إعدادات الاتصال بـ S3

## المكونات والواجهات

### 1. JWT Components

#### JwtService Class
```csharp
public class JwtService
{
    - IConfiguration _configuration
    + GenerateToken(string username) : TokenDto
    + ValidateCredentials(string username, string password) : bool
}
```

**الوظائف الأساسية:**
- إنشاء JWT token مع Claims
- التحقق من بيانات المستخدم
- تحديد مدة انتهاء التوكن

#### AuthController Class
```csharp
public class AuthController
{
    - JwtService _jwtService
    + Login(LoginDto loginDto) : ActionResult<TokenDto>
    + ValidateToken(string authorization) : ActionResult
}
```

#### JWT Configuration
```json
{
  "JwtSettings": {
    "SecretKey": "your-super-secret-key-with-at-least-32-characters",
    "Issuer": "ArticlesAPI",
    "Audience": "ArticlesAPIUsers",
    "ExpirationInMinutes": 60
  }
}
```

### 2. S3 Integration Components

#### IS3ArticleProvider Interface
```csharp
public interface IS3ArticleProvider
{
    + GetArticleByIdAsync(int id) : Task<Article?>
    + GetArticleByTitleAsync(string title) : Task<Article?>
    + GetAllArticlesAsync() : Task<List<Article>>
}
```

#### S3ArticleProvider Class
```csharp
public class S3ArticleProvider : IS3ArticleProvider
{
    - IAmazonS3 _s3Client
    - IMapper _mapper
    - IMemoryCache _cache
    - string _bucketName
    + GetAllArticlesAsync() : Task<List<Article>>
    + GetArticleByIdAsync(int id) : Task<Article?>
    + GetArticleByTitleAsync(string title) : Task<Article?>
}
```

#### ArticleService Integration
```csharp
public class ArticleService
{
    - bool _useS3
    - IS3ArticleProvider _s3ArticleProvider
    - IArticleRepository _articleRepository
    
    + GetArticleByTitleAsync(string title) : Task<Article?>
    // يستخدم S3 أو Database حسب التكوين
}
```

## نماذج البيانات

### JWT Data Models

#### TokenDto
```csharp
public class TokenDto
{
    public string Token { get; set; }
    public string Username { get; set; }
    public DateTime ExpiresAt { get; set; }
}
```

#### LoginDto
```csharp
public class LoginDto
{
    public string Username { get; set; }
    public string Password { get; set; }
}
```

### S3 Configuration Model
```json
{
  "AWS": {
    "AccessKey": "your-access-key",
    "SecretKey": "your-secret-key",
    "Region": "us-east-1",
    "BucketName": "articles-bucket"
  },
  "UseS3": false
}
```

## معالجة الأخطاء

### JWT Error Handling
1. **Invalid Credentials**: إرجاع 401 Unauthorized
2. **Missing Token**: إرجاع 401 Unauthorized
3. **Expired Token**: إرجاع 401 Unauthorized
4. **Invalid Token Format**: إرجاع 400 Bad Request

### S3 Error Handling
1. **S3 Connection Error**: إرجاع 503 Service Unavailable
2. **Bucket Not Found**: إرجاع InvalidOperationException
3. **File Not Found**: إرجاع empty list أو null
4. **JSON Parse Error**: إرجاع InvalidOperationException

## استراتيجية الاختبار

### JWT Testing Strategy
1. **Unit Tests**:
   - اختبار إنشاء التوكن
   - اختبار التحقق من البيانات
   - اختبار انتهاء صلاحية التوكن

2. **Integration Tests**:
   - اختبار Login endpoint
   - اختبار Protected endpoints
   - اختبار Token validation

### S3 Testing Strategy
1. **Unit Tests**:
   - اختبار S3ArticleProvider methods
   - اختبار Memory Cache functionality
   - اختبار Error handling

2. **Integration Tests**:
   - اختبار S3 connection
   - اختبار Data retrieval
   - اختبار Fallback to database

## التكامل بين JWT و S3

### Protected S3 Endpoints
```csharp
[HttpGet("title/{title}")]
[Authorize] // JWT Protection
public async Task<ActionResult<ArticleDto>> GetArticleByTitle(string title)
{
    // يستخدم S3ArticleProvider إذا كان UseS3 = true
    var article = await _articleService.GetArticleByTitleAsync(title);
    return Ok(article);
}
```

### Flow Diagram
```
1. Client sends Login request
2. AuthController validates credentials
3. JwtService generates token
4. Client receives token
5. Client sends request with Authorization header
6. JWT Middleware validates token
7. ArticlesController processes request
8. ArticleService checks UseS3 flag
9. S3ArticleProvider fetches data from S3
10. Memory Cache stores result
11. Response sent to client
```

## أفضل الممارسات المطبقة

### JWT Best Practices
1. **Secure Secret Key**: استخدام مفتاح قوي (32+ characters)
2. **Token Expiration**: تحديد مدة انتهاء مناسبة
3. **Claims Validation**: التحقق من Issuer و Audience
4. **HTTPS Only**: استخدام HTTPS في الإنتاج

### S3 Best Practices
1. **Caching Strategy**: استخدام Memory Cache لتحسين الأداء
2. **Error Handling**: معالجة شاملة للأخطاء
3. **Configuration Flag**: إمكانية التبديل بين S3 و Database
4. **Resource Management**: استخدام using statements للموارد

### Integration Best Practices
1. **Separation of Concerns**: فصل JWT logic عن S3 logic
2. **Dependency Injection**: استخدام DI للمرونة
3. **Interface Segregation**: استخدام interfaces للتجريد
4. **Configuration Management**: إدارة مركزية للإعدادات