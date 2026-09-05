using Entities;
using RepositoryContracts;

namespace InMemoryRepositories;

public class PostInMemoryRepository : RepositoryBase<Post>, IPostRepository
{
    private List<Post> posts;
    
    public Task<Post> AddAsync(Post post)
    {
        post.Id = posts.Any()
            ? posts.Max(p => p.Id) + 1
            : 1;
        posts.Add(post);
        return Task.FromResult(post);
    }

    public Task UpdateAsync(Post post)
    {
        Post? existingPost = posts.SingleOrDefault(p => p.Id == post.Id);
        if (existingPost is null)
        {
            throw new InvalidOperationException(
                $"The post with id {post.Id} was not found.");
            
        }
        posts.Remove(existingPost);
        posts.Add(post);
        
        return Task.CompletedTask;
    }

    public Task DeleteAsync(int id)
    {
        Post? postToRemove = posts.SingleOrDefault(p => p.Id == id);
        if (postToRemove is null)
        {
            throw new InvalidOperationException(
                $"The post with id {id} was not found.");
        }
        posts.Remove(postToRemove);
        return Task.CompletedTask;
    }

    public Task<Post> GetSingleAsync(int id)
    {
        Post? existingPost = posts.SingleOrDefault(p => p.Id == id);
        if (existingPost is null)
        {
            throw new InvalidOperationException(
                $"Post with id {id} was not found.");
        }
        return Task.FromResult(existingPost);
    }

    public IQueryable<Post> GetManyAsync()
    {
        return posts.AsQueryable();
    }
}