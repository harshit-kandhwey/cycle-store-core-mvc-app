# Cloud Readiness Configuration Guide

## AWS Services Required

### 1. Amazon ElastiCache for Redis
The application uses Redis for distributed caching to replace static collections.

**Configuration:**
- Set the Redis connection string via environment variable or appsettings.json
- Environment Variable: `REDIS_CONNECTION_STRING`
- Format: `hostname:port` (e.g., `my-redis-cluster.abc123.0001.use1.cache.amazonaws.com:6379`)
- For production, use ElastiCache Redis cluster endpoint
- For local development, defaults to `localhost:6379`

**appsettings.json example:**
```json
{
  "Redis": {
    "ConnectionString": "your-elasticache-endpoint:6379"
  }
}
```

### 2. AWS Secrets Manager
The application retrieves secrets from AWS Secrets Manager at runtime.

**Required Secrets:**
1. **AdventureWorks/CartSessionKey**
   - Type: String
   - Value: A unique session key for cart storage (e.g., "cart" or any custom value)
   - Example: `"cs-cart-session"`

**IAM Permissions Required:**
The application's IAM role must have the following permissions:
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "secretsmanager:GetSecretValue"
      ],
      "Resource": [
        "arn:aws:secretsmanager:REGION:ACCOUNT_ID:secret:AdventureWorks/*"
      ]
    }
  ]
}
```

### 3. Database Connection String
The database connection string should be provided via environment variable:
- Environment Variable: `ConnectionStrings__DefaultConnection`
- Format: Standard SQL Server connection string

## Environment Variables Summary

| Variable | Required | Description | Example |
|----------|----------|-------------|---------|
| `REDIS_CONNECTION_STRING` | Yes | Redis endpoint for distributed caching | `my-redis.cache.amazonaws.com:6379` |
| `ConnectionStrings__DefaultConnection` | Yes | SQL Server connection string | `Server=...;Database=...;` |
| `AWS_REGION` | Yes | AWS region for Secrets Manager | `us-east-1` |

## AWS Setup Steps

### Step 1: Create ElastiCache Redis Cluster
```bash
aws elasticache create-cache-cluster \
  --cache-cluster-id adventureworks-redis \
  --engine redis \
  --cache-node-type cache.t3.micro \
  --num-cache-nodes 1 \
  --region us-east-1
```

### Step 2: Create Secrets in AWS Secrets Manager
```bash
# Create cart session key secret
aws secretsmanager create-secret \
  --name AdventureWorks/CartSessionKey \
  --secret-string "cs-cart-session" \
  --region us-east-1
```

### Step 3: Configure IAM Role
Attach the IAM policy (shown above) to the EC2 instance role or ECS task role running the application.

## Local Development

For local development without AWS services:
1. Run Redis locally: `docker run -d -p 6379:6379 redis:latest`
2. Set environment variable: `REDIS_CONNECTION_STRING=localhost:6379`
3. The application will fall back to local Redis

## Deployment Checklist

- [ ] ElastiCache Redis cluster created and accessible
- [ ] Secrets created in AWS Secrets Manager
- [ ] IAM role configured with Secrets Manager permissions
- [ ] Environment variables configured in deployment environment
- [ ] Security groups allow application to access Redis (port 6379)
- [ ] Application can resolve Redis endpoint DNS

## Monitoring

Monitor the following CloudWatch metrics:
- ElastiCache: `CacheHits`, `CacheMisses`, `CPUUtilization`
- Application logs for Redis connection errors
- Secrets Manager API call metrics

## Troubleshooting

**Redis Connection Issues:**
- Verify security group allows inbound traffic on port 6379
- Check Redis endpoint DNS resolution
- Verify `REDIS_CONNECTION_STRING` environment variable is set correctly

**Secrets Manager Issues:**
- Verify IAM role has `secretsmanager:GetSecretValue` permission
- Check secret name matches exactly: `AdventureWorks/CartSessionKey`
- Verify AWS region is set correctly
