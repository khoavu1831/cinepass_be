# CinePass - MySQL Vector Search Strategy

## 📌 Tổng Quan

Tài liệu này mô tả chiến lược lưu trữ và tìm kiếm vector embeddings trong MySQL, thay thế cho PostgreSQL's pgvector.

---

## 🎯 Lựa Chọn Chiến Lược

### Option 1: MySQL Vector Search (Recommended for simplicity)

**Yêu cầu**: MySQL 8.0.32+ với hỗ trợ Vector Search

#### Schema
```sql
-- ReviewEmbeddings với MySQL Vector type
CREATE TABLE `ReviewEmbeddings` (
    `Id` INT PRIMARY KEY AUTO_INCREMENT,
    `ReviewId` INT NOT NULL UNIQUE,
    `MovieId` INT NOT NULL,
    `MovieDescriptionVector` VECTOR NOT NULL,  -- JSON format: [0.123, 0.456, ...]
    `ReviewContentVector` VECTOR NOT NULL,
    `CombinedVector` VECTOR NOT NULL,
    `EmbeddingModel` VARCHAR(100) DEFAULT 'text-embedding-3-small',
    `CreatedAt` DATETIME,
    `UpdatedAt` DATETIME,
    VECTOR INDEX `idx_movie_desc_vector` (`MovieDescriptionVector`),
    VECTOR INDEX `idx_review_content_vector` (`ReviewContentVector`),
    FOREIGN KEY (`ReviewId`) REFERENCES `Reviews`(`Id`) ON DELETE CASCADE,
    FOREIGN KEY (`MovieId`) REFERENCES `Movies`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB;
```

#### Usage in C#/.NET
```csharp
// EF Core configuration
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<ReviewEmbedding>()
        .Property(e => e.MovieDescriptionVector)
        .HasColumnType("VECTOR")
        .HasPrecision(1536);  // 1536 dimensions for OpenAI embeddings
        
    modelBuilder.Entity<ReviewEmbedding>()
        .HasIndex(e => e.MovieDescriptionVector)
        .HasMethod("IVF");  // Index type
}

// Query with similarity
var queryEmbedding = new float[] { /* 1536 values */ };

var results = await _context.ReviewEmbeddings
    .Select(e => new 
    { 
        Embedding = e,
        Similarity = EF.Functions.VectorDistance(
            e.MovieDescriptionVector, 
            queryEmbedding
        )
    })
    .Where(x => x.Similarity < 0.5)  // Threshold
    .OrderBy(x => x.Similarity)
    .Take(10)
    .ToListAsync();
```

**Ưu điểm**:
✅ Native MySQL, không cần external service
✅ Transactions, ACID compliance
✅ Dễ deploy, manage
✅ Cost efficient

**Nhược điểm**:
❌ MySQL 8.0.32+ bắt buộc
❌ Performance không tốt như specialized vector DB ở large scale
❌ Limited vector operations

---

### Option 2: External Vector Database (Best for scale)

**Sử dụng**:
- **Qdrant**: Vector search engine (open-source, cloud)
- **Weaviate**: Vector DB with ML integration
- **Pinecone**: Managed vector database (SaaS)
- **Milvus**: Open-source, high-performance

#### Architecture

```
MySQL (Metadata)          External Vector DB (Embeddings)
├── ReviewEmbeddings      ├── Movie embeddings → IVF index
│   ├── Id                │   ├── Cosine distance search
│   ├── ReviewId          │   └── Fast retrieval
│   ├── MovieId           │
│   └── EmbeddingModel    └── Review embeddings
│                             ├── HNSW index
│                             └── K-NN search

┌─────────────────────────────────────────────────────────┐
│ Search Flow:                                             │
│ 1. User query → OpenAI embedding (1536 dims)            │
│ 2. Query Vector DB for similar vectors                  │
│ 3. Get IDs from result                                  │
│ 4. Join with MySQL metadata → Full results              │
└─────────────────────────────────────────────────────────┘
```

#### Qdrant Example

##### Schema in Qdrant
```json
// Collection: "movie_embeddings"
{
  "size": 1536,
  "distance": "Cosine",
  "vectors": {
    "size": 1536,
    "distance": "Cosine"
  },
  "payload_schema": {
    "properties": {
      "review_id": {"type": "integer"},
      "movie_id": {"type": "integer"},
      "type": {"type": "text"}  // "movie" or "review"
    }
  }
}
```

##### C# Client
```csharp
using Qdrant.Client;

var client = new QdrantClient("localhost:6334");

// Insert embeddings
var points = new List<PointStruct>
{
    new PointStruct 
    { 
        Id = 1,
        Vectors = new float[] { /* 1536 values */ },
        Payload = new Dictionary<string, Value>
        {
            { "review_id", new Value { IntegerValue = 123 } },
            { "movie_id", new Value { IntegerValue = 456 } },
            { "type", new Value { StringValue = "movie" } }
        }
    }
};

await client.UpsertAsync("movie_embeddings", points);

// Search
var searchResponse = await client.SearchAsync(
    "movie_embeddings",
    new float[] { /* query embedding */ },
    limit: 10,
    confidence: 0.7f
);

foreach (var result in searchResponse.Result)
{
    var reviewId = result.Payload["review_id"].IntegerValue;
    // Join with MySQL to get full data
}
```

##### Docker Compose
```yaml
version: '3'
services:
  qdrant:
    image: qdrant/qdrant:latest
    ports:
      - "6334:6334"
    volumes:
      - ./qdrant_storage:/qdrant/storage
    environment:
      - QDRANT_API_KEY=your_api_key
```

**Ưu điểm**:
✅ Production-grade vector search
✅ Highly scalable
✅ Advanced indexing (HNSW, IVF)
✅ Fast vector operations
✅ Supports multi-tenancy (Qdrant)

**Nhược điểm**:
❌ Extra infrastructure to maintain
❌ Network latency between services
❌ More complex deployment
❌ Data sync challenges (eventual consistency)

---

### Option 3: In-Memory Similarity (Simple MVP)

**Lưu trữ**: Embeddings as LONGBLOB in MySQL

```csharp
public class VectorSearchService
{
    private readonly AppDbContext _context;
    
    public async Task<List<(int ReviewId, double Similarity)>> SearchSimilarAsync(
        float[] queryVector, 
        int limit = 10)
    {
        // Load all embeddings (for MVP - not production)
        var embeddings = await _context.ReviewEmbeddings
            .Select(e => new 
            { 
                e.Id,
                e.ReviewId,
                Vector = e.MovieDescriptionVector
            })
            .ToListAsync();
        
        // Compute cosine similarity in memory
        var results = embeddings
            .Select(e => new 
            {
                e.ReviewId,
                Similarity = CosineSimilarity(queryVector, Deserialize(e.Vector))
            })
            .OrderByDescending(x => x.Similarity)
            .Where(x => x.Similarity > 0.5)  // Threshold
            .Take(limit)
            .ToList();
            
        return results.Select(x => (x.ReviewId, x.Similarity)).ToList();
    }
    
    private static double CosineSimilarity(float[] vec1, float[] vec2)
    {
        double dotProduct = 0;
        double normA = 0;
        double normB = 0;
        
        for (int i = 0; i < vec1.Length; i++)
        {
            dotProduct += vec1[i] * vec2[i];
            normA += vec1[i] * vec1[i];
            normB += vec2[i] * vec2[i];
        }
        
        return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }
    
    private static float[] Deserialize(byte[] data)
    {
        var floats = new float[data.Length / 4];
        Buffer.BlockCopy(data, 0, floats, 0, data.Length);
        return floats;
    }
}
```

**Ưu điểm**:
✅ Simple, no external services
✅ Works with any database
✅ Good for MVP/POC

**Nhược điểm**:
❌ Slow for large datasets (O(n) complexity)
❌ Memory intensive
❌ Not suitable for production
❌ Can't scale beyond memory limit

---

## 📊 So Sánh

| Tiêu Chí | MySQL Vector | Qdrant | In-Memory |
|----------|--------|--------|-----------|
| **Độ Phức Tạp** | ⭐⭐ | ⭐⭐⭐⭐ | ⭐ |
| **Performance** | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐ |
| **Scalability** | ⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐ |
| **Cost** | Low | Medium | Low |
| **Setup Time** | ⭐⭐ | ⭐⭐⭐ | ⭐ |
| **Maintenance** | Easy | Medium | Easy |
| **Production Ready** | Yes (8.0.32+) | Yes | No (MVP) |

---

## 🚀 Khuyến Nghị

### Giai Đoạn 1: MVP (Weeks 1-3)
```
├─ Dùng Option 3: In-Memory similarity
└─ Nhanh để validate business logic
```

### Giai Đoạn 2: Tối ưu hóa (Weeks 4-6)
```
├─ Nếu MySQL ≥ 8.0.32:
│  └─ Migrate to Option 1 (MySQL Vector)
└─ Nếu cần high performance:
   └─ Migrate to Option 2 (Qdrant)
```

### Giai Đoạn 3: Production (Week 7+)
```
Dữ liệu < 1M vectors:
└─ MySQL Vector Search là đủ

Dữ liệu > 1M vectors hoặc cần advanced features:
└─ Dùng Qdrant/Weaviate + MySQL hybrid
```

---

## 🔧 Implementation Guide

### Step 1: Storage Layer

```csharp
// ReviewEmbedding Model
public class ReviewEmbedding
{
    public int Id { get; set; }
    public int ReviewId { get; set; }
    public int MovieId { get; set; }
    
    // Store as LONGBLOB (binary format)
    public byte[] MovieDescriptionVector { get; set; }
    public byte[] ReviewContentVector { get; set; }
    public byte[] CombinedVector { get; set; }
    
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

### Step 2: Conversion Utilities

```csharp
public static class VectorConversions
{
    // Float array → LONGBLOB (binary)
    public static byte[] ToBlob(float[] vector)
    {
        var bytes = new byte[vector.Length * 4];
        Buffer.BlockCopy(vector, 0, bytes, 0, bytes.Length);
        return bytes;
    }
    
    // LONGBLOB → Float array
    public static float[] FromBlob(byte[] blob)
    {
        var floats = new float[blob.Length / 4];
        Buffer.BlockCopy(blob, 0, floats, 0, blob.Length);
        return floats;
    }
    
    // Float array → JSON (for MySQL Vector)
    public static string ToJson(float[] vector)
    {
        return JsonSerializer.Serialize(vector);
    }
}
```

### Step 3: Service Layer

```csharp
public class EmbeddingService
{
    private readonly IOpenAIService _openAI;
    private readonly AppDbContext _context;
    private readonly IVectorSearchService _vectorSearch;
    
    public async Task GenerateAndStoreEmbeddingAsync(Review review)
    {
        // Get movie description
        var movie = await _context.Movies.FindAsync(review.MovieId);
        
        // Call OpenAI
        var movieVector = await _openAI.EmbedTextAsync(movie.Description);
        var reviewVector = await _openAI.EmbedTextAsync(review.Content);
        
        // Combine vectors (weighted average)
        var combined = CombineVectors(movieVector, reviewVector, 0.6, 0.4);
        
        // Store in database
        var embedding = new ReviewEmbedding
        {
            ReviewId = review.Id,
            MovieId = review.MovieId,
            MovieDescriptionVector = VectorConversions.ToBlob(movieVector),
            ReviewContentVector = VectorConversions.ToBlob(reviewVector),
            CombinedVector = VectorConversions.ToBlob(combined),
            CreatedAt = DateTime.UtcNow
        };
        
        _context.ReviewEmbeddings.Add(embedding);
        await _context.SaveChangesAsync();
    }
    
    public async Task<List<ReviewSearchResult>> SemanticSearchAsync(string query)
    {
        // Convert query to embedding
        var queryVector = await _openAI.EmbedTextAsync(query);
        
        // Delegate to appropriate vector search
        return await _vectorSearch.SearchAsync(queryVector);
    }
}
```

### Step 4: Vector Search Interface

```csharp
public interface IVectorSearchService
{
    Task<List<ReviewSearchResult>> SearchAsync(float[] queryVector);
}

// MySQL Vector implementation
public class MySqlVectorSearchService : IVectorSearchService
{
    private readonly AppDbContext _context;
    
    public async Task<List<ReviewSearchResult>> SearchAsync(float[] queryVector)
    {
        // Use MySQL Vector functions
        var results = await _context.ReviewEmbeddings
            .Select(e => new ReviewSearchResult
            {
                ReviewId = e.ReviewId,
                MovieId = e.MovieId,
                // MySQL 8.0.32+ vector_distance function
                Similarity = 1 - EF.Functions.CustomFunction(
                    "vector_distance", 
                    e.MovieDescriptionVector, 
                    queryVector
                )
            })
            .Where(r => r.Similarity > 0.5)
            .OrderByDescending(r => r.Similarity)
            .Take(10)
            .ToListAsync();
            
        return results;
    }
}

// Qdrant implementation
public class QdrantVectorSearchService : IVectorSearchService
{
    private readonly QdrantClient _client;
    
    public async Task<List<ReviewSearchResult>> SearchAsync(float[] queryVector)
    {
        var searchResponse = await _client.SearchAsync(
            "movie_embeddings",
            queryVector,
            limit: 10
        );
        
        return searchResponse.Result
            .Select(r => new ReviewSearchResult
            {
                ReviewId = (int)r.Payload["review_id"].IntegerValue,
                MovieId = (int)r.Payload["movie_id"].IntegerValue,
                Similarity = r.Score
            })
            .ToList();
    }
}
```

---

## 📋 Migration Checklist

### MySQL Vector Setup

```bash
# 1. Verify MySQL version
mysql -u root -p -e "SELECT VERSION();"
# Output should be >= 8.0.32

# 2. Create vector index
mysql -u root -p << EOF
CREATE TABLE review_embeddings (
    id INT PRIMARY KEY AUTO_INCREMENT,
    review_id INT NOT NULL UNIQUE,
    movie_description_vector VECTOR NOT NULL,
    review_content_vector VECTOR NOT NULL,
    combined_vector VECTOR NOT NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    VECTOR INDEX idx_movie (movie_description_vector),
    VECTOR INDEX idx_review (review_content_vector)
);
EOF

# 3. Test similarity query
mysql -u root -p << EOF
SELECT id, VECTOR_DISTANCE(movie_description_vector, JSON_ARRAY([0.1, 0.2, ...]))
FROM review_embeddings
ORDER BY VECTOR_DISTANCE(movie_description_vector, JSON_ARRAY([0.1, 0.2, ...]))
LIMIT 10;
EOF
```

### Qdrant Setup

```bash
# 1. Start Qdrant with Docker
docker-compose up -d

# 2. Create collection
curl -X PUT 'http://localhost:6334/collections/movie_embeddings' \
  -H 'Content-Type: application/json' \
  -d '{
    "vectors": {
      "size": 1536,
      "distance": "Cosine"
    }
  }'

# 3. Verify
curl 'http://localhost:6334/collections/movie_embeddings'
```

---

## ⚙️ Performance Tuning

### MySQL Vector
```sql
-- Optimize index
ANALYZE TABLE review_embeddings;
OPTIMIZE TABLE review_embeddings;

-- Monitor query performance
EXPLAIN SELECT * FROM review_embeddings
WHERE VECTOR_DISTANCE(...) < 0.5
LIMIT 10;
```

### Qdrant
```json
// Tune IVF index
{
  "vector_size": 1536,
  "distance": "Cosine",
  "hnsw_config": {
    "m": 16,
    "ef_construct": 200,
    "full_scan_threshold": 10000
  }
}
```

---

## 🎓 References

- [MySQL Vector Search Docs](https://dev.mysql.com/doc/mysql-vector-search/en/)
- [Qdrant Docs](https://qdrant.tech/documentation/)
- [Vector Distance Algorithms](https://en.wikipedia.org/wiki/Cosine_similarity)
- [OpenAI Embeddings](https://platform.openai.com/docs/guides/embeddings)

---

**MySQL Vector Strategy Guide v1.0**
