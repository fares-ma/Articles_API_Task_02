/**
 * Articles API Test Suite
 * اختبارات شاملة لـ Articles API مع التركيز على Get Article by Title
 */

class ArticlesAPITester {
    constructor(baseUrl = 'http://localhost:5212/api') {
        this.baseUrl = baseUrl;
        this.authToken = null;
        this.testResults = [];
    }

    // Utility method to make HTTP requests
    async makeRequest(endpoint, options = {}) {
        const url = `${this.baseUrl}${endpoint}`;
        const defaultOptions = {
            headers: {
                'Content-Type': 'application/json',
                ...(this.authToken && { 'Authorization': `Bearer ${this.authToken}` })
            }
        };

        const finalOptions = {
            ...defaultOptions,
            ...options,
            headers: {
                ...defaultOptions.headers,
                ...options.headers
            }
        };

        try {
            const response = await fetch(url, finalOptions);
            const data = await response.json().catch(() => null);
            
            return {
                success: response.ok,
                status: response.status,
                statusText: response.statusText,
                data: data,
                response: response
            };
        } catch (error) {
            return {
                success: false,
                error: error.message,
                status: 0
            };
        }
    }

    // Test API Connection
    async testConnection() {
        console.log('🔗 Testing API Connection...');
        
        const result = await this.makeRequest('/articles?pageNumber=1&pageSize=1');
        
        if (result.success) {
            console.log('✅ API Connection successful');
            return true;
        } else {
            console.log('❌ API Connection failed:', result.error || result.statusText);
            return false;
        }
    }

    // Login and get JWT token
    async login(username = 'admin', password = 'admin123') {
        console.log('🔐 Attempting login...');
        
        const result = await this.makeRequest('/auth/login', {
            method: 'POST',
            body: JSON.stringify({ username, password })
        });

        if (result.success && result.data?.token) {
            this.authToken = result.data.token;
            console.log('✅ Login successful');
            console.log('🎫 Token:', result.data.token.substring(0, 50) + '...');
            return true;
        } else {
            console.log('❌ Login failed:', result.data?.message || result.error);
            return false;
        }
    }

    // Test Get Article by Title - Main functionality
    async testGetArticleByTitle(title) {
        console.log(`🔍 Testing Get Article by Title: "${title}"`);
        
        if (!this.authToken) {
            console.log('❌ No auth token available. Please login first.');
            return false;
        }

        const encodedTitle = encodeURIComponent(title);
        const result = await this.makeRequest(`/articles/title/${encodedTitle}`);

        if (result.success) {
            console.log('✅ Article found successfully');
            console.log('📄 Article details:', {
                id: result.data.id,
                title: result.data.title,
                author: result.data.author,
                isPublished: result.data.isPublished,
                createdAt: result.data.createdAt
            });
            return result.data;
        } else if (result.status === 404) {
            console.log(`❌ Article not found: "${title}"`);
            return null;
        } else if (result.status === 401) {
            console.log('❌ Unauthorized - Token may be expired');
            return false;
        } else {
            console.log('❌ Error getting article:', result.data?.error || result.statusText);
            return false;
        }
    }

    // Get all articles to see available titles
    async getAllArticles(pageSize = 10) {
        console.log('📚 Getting all articles...');
        
        const result = await this.makeRequest(`/articles?pageNumber=1&pageSize=${pageSize}`);

        if (result.success) {
            console.log(`✅ Found ${result.data.totalCount} articles`);
            console.log('📝 Available titles:');
            result.data.items.forEach((article, index) => {
                console.log(`   ${index + 1}. "${article.title}" (ID: ${article.id})`);
            });
            return result.data.items;
        } else {
            console.log('❌ Error getting articles:', result.error || result.statusText);
            return [];
        }
    }

    // Test various scenarios for Get Article by Title
    async runComprehensiveTests() {
        console.log('🚀 Starting Comprehensive Tests for Get Article by Title');
        console.log('=' .repeat(60));

        // Test 1: Connection
        const connectionOk = await this.testConnection();
        if (!connectionOk) {
            console.log('❌ Cannot proceed - API connection failed');
            return;
        }

        // Test 2: Login
        const loginOk = await this.login();
        if (!loginOk) {
            console.log('❌ Cannot proceed - Login failed');
            return;
        }

        // Test 3: Get all articles to see what's available
        const articles = await this.getAllArticles();
        
        if (articles.length === 0) {
            console.log('❌ No articles available for testing');
            return;
        }

        console.log('\n🧪 Running specific tests...\n');

        // Test 4: Valid article title (using first available article)
        const firstArticle = articles[0];
        console.log(`Test 1: Valid title - "${firstArticle.title}"`);
        await this.testGetArticleByTitle(firstArticle.title);

        // Test 5: Non-existent article
        console.log('\nTest 2: Non-existent title');
        await this.testGetArticleByTitle('Non Existent Article Title 12345');

        // Test 6: Title with special characters
        console.log('\nTest 3: Title with special characters');
        await this.testGetArticleByTitle('Test & Special Characters!');

        // Test 7: Empty title
        console.log('\nTest 4: Empty title');
        await this.testGetArticleByTitle('');

        // Test 8: Title with Arabic characters (if any)
        console.log('\nTest 5: Arabic title');
        await this.testGetArticleByTitle('مقال تجريبي');

        // Test 9: Case sensitivity test
        if (articles.length > 0) {
            const testTitle = firstArticle.title.toLowerCase();
            console.log(`\nTest 6: Case sensitivity - "${testTitle}"`);
            await this.testGetArticleByTitle(testTitle);
        }

        // Test 10: Partial title match
        if (articles.length > 0) {
            const partialTitle = firstArticle.title.split(' ')[0];
            console.log(`\nTest 7: Partial title - "${partialTitle}"`);
            await this.testGetArticleByTitle(partialTitle);
        }

        console.log('\n' + '='.repeat(60));
        console.log('🏁 Comprehensive tests completed!');
    }

    // Test without authentication
    async testWithoutAuth() {
        console.log('🔒 Testing Get Article by Title without authentication...');
        
        // Temporarily remove token
        const originalToken = this.authToken;
        this.authToken = null;

        const result = await this.makeRequest('/articles/title/test');
        
        if (result.status === 401) {
            console.log('✅ Correctly rejected unauthorized request');
        } else {
            console.log('❌ Should have rejected unauthorized request');
        }

        // Restore token
        this.authToken = originalToken;
    }

    // Performance test
    async performanceTest(title, iterations = 5) {
        console.log(`⚡ Performance test: ${iterations} requests for "${title}"`);
        
        const times = [];
        
        for (let i = 0; i < iterations; i++) {
            const startTime = Date.now();
            await this.testGetArticleByTitle(title);
            const endTime = Date.now();
            times.push(endTime - startTime);
        }

        const avgTime = times.reduce((a, b) => a + b, 0) / times.length;
        const minTime = Math.min(...times);
        const maxTime = Math.max(...times);

        console.log(`📊 Performance Results:`);
        console.log(`   Average: ${avgTime.toFixed(2)}ms`);
        console.log(`   Min: ${minTime}ms`);
        console.log(`   Max: ${maxTime}ms`);
    }

    // Create a test article for testing
    async createTestArticle() {
        console.log('📝 Creating test article...');
        
        const testArticle = {
            title: `Test Article ${Date.now()}`,
            description: 'This is a test article created for API testing',
            content: 'This article was created automatically for testing the Get Article by Title functionality.',
            tags: 'test,api,automation',
            author: 'API Tester',
            isPublished: true,
            newspaperId: 1
        };

        const result = await this.makeRequest('/articles', {
            method: 'POST',
            body: JSON.stringify(testArticle)
        });

        if (result.success) {
            console.log('✅ Test article created successfully');
            console.log('📄 Article ID:', result.data.id);
            return result.data;
        } else {
            console.log('❌ Failed to create test article:', result.data?.error || result.statusText);
            return null;
        }
    }
}

// Usage Examples and Test Runner
async function runTests() {
    const tester = new ArticlesAPITester();
    
    // Run comprehensive tests
    await tester.runComprehensiveTests();
    
    // Test without authentication
    await tester.testWithoutAuth();
    
    // Performance test (if we have articles)
    const articles = await tester.getAllArticles(1);
    if (articles.length > 0) {
        await tester.performanceTest(articles[0].title, 3);
    }
}

// Export for use in browser console or Node.js
if (typeof window !== 'undefined') {
    // Browser environment
    window.ArticlesAPITester = ArticlesAPITester;
    window.runTests = runTests;
    
    console.log('🎯 Articles API Tester loaded!');
    console.log('💡 Usage:');
    console.log('   const tester = new ArticlesAPITester();');
    console.log('   await tester.runComprehensiveTests();');
    console.log('   OR simply run: await runTests();');
} else {
    // Node.js environment
    module.exports = { ArticlesAPITester, runTests };
}

// Auto-run tests if this script is executed directly
if (typeof window !== 'undefined' && window.location.search.includes('autotest=true')) {
    document.addEventListener('DOMContentLoaded', () => {
        setTimeout(runTests, 1000);
    });
}