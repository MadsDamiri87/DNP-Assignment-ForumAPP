using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace WebAPI.Seeding;

public class DemoDataSeeder
{
    public const string DemoPassword = "demo1234";

    private readonly IUserRepository userRepository;
    private readonly ISubForumRepository subForumRepository;
    private readonly IPostRepository postRepository;
    private readonly ICommentRepository commentRepository;

    public DemoDataSeeder(
        IUserRepository userRepository,
        ISubForumRepository subForumRepository,
        IPostRepository postRepository,
        ICommentRepository commentRepository)
    {
        this.userRepository = userRepository;
        this.subForumRepository = subForumRepository;
        this.postRepository = postRepository;
        this.commentRepository = commentRepository;
    }

    public async Task SeedIfEmptyAsync()
    {
        if (await postRepository.GetMany().AnyAsync())
        {
            return;
        }

        DateTime now = DateTime.UtcNow;

        User mads = await AddUserAsync("mads", "mads@via.dk", "Admin", now.AddDays(-40));
        User alice = await AddUserAsync("alice", "alice@via.dk", "User", now.AddDays(-32));
        User bob = await AddUserAsync("bob", "bob@via.dk", "User", now.AddDays(-25));
        User sofia = await AddUserAsync("sofia", "sofia@via.dk", "User", now.AddDays(-18));

        SubForum dotnet = await AddSubForumAsync(
            "C# and .NET",
            "Language features, Entity Framework and the DNP assignments",
            mads, now.AddDays(-39));

        SubForum distributed = await AddSubForumAsync(
            "Distributed systems",
            "gRPC, message queues and everything that talks over a network",
            alice, now.AddDays(-30));

        SubForum databases = await AddSubForumAsync(
            "Databases",
            "Modelling, migrations and the queries that turned out slower than expected",
            bob, now.AddDays(-24));

        SubForum studyLife = await AddSubForumAsync(
            "Study life",
            "Group work, deadlines and where the good coffee is",
            sofia, now.AddDays(-17));

        Post cascade = await AddPostAsync(
            "Deleting a user wiped half the forum, and the database was right",
            "I gave my entities navigation properties and generated the migration without reading it. "
            + "Four of the five foreign keys came out as Cascade, all of them pointing at Users. "
            + "Deleting one account would take that user's posts with it, and every comment on them, "
            + "including comments written by other people. The fix was not to change the constraint but to stop "
            + "deleting the row at all: the account gets a flag, the content stays, and the author shows up as gone.",
            mads, dotnet, now.AddDays(-6));

        Post queryProvider = await AddPostAsync(
            "ToListAsync on a file repository throws, and that is not a bug",
            "My services call ToListAsync on whatever IQueryable the repository hands them. That works against "
            + "Entity Framework and nothing else. LINQ-to-Objects has no async query provider, so the moment you swap "
            + "the implementation back, it falls over. Either the services depend on EF on purpose, or the async calls "
            + "have to move behind the repository contract. Pretending both are true is the thing that bites.",
            alice, dotnet, now.AddDays(-5));

        Post relativePath = await AddPostAsync(
            "no such table: Users, and an app.db of exactly zero bytes",
            "The connection string said Data Source=app.db. SQLite resolves that against the working directory, and "
            + "dotnet run starts from the project folder, not from wherever you ran the migration. So it cheerfully "
            + "created a second, empty database next to the Web API. No error, no warning, just an empty file.",
            bob, databases, now.AddDays(-4));

        Post nullableFk = await AddPostAsync(
            "A nullable foreign key is the whole difference between 1 and 0..1",
            "A post does not have to belong to a sub-forum, so the key is nullable. That one question mark decides "
            + "the multiplicity in the domain model, and it decides what EF does when you delete the sub-forum. "
            + "Required means Cascade. Optional means ClientSetNull. Same model, two very different Tuesdays.",
            alice, databases, now.AddDays(-3));

        Post swagger = await AddPostAsync(
            "Swagger says Undocumented because the controller never said otherwise",
            "Swagger reads your signatures, not your method bodies. An ActionResult of string tells it that 200 "
            + "returns a string, and that is all it can see. The Unauthorized call three lines down is a runtime "
            + "decision. If you want the 401 in the docs, you have to declare it with ProducesResponseType.",
            mads, dotnet, now.AddDays(-2));

        Post grpc = await AddPostAsync(
            "Proto files are a contract, so treat them like one",
            "We spent an afternoon on a mismatch that turned out to be two copies of the same proto file drifting "
            + "apart. One source of truth, generated on build, and the problem stops existing.",
            sofia, distributed, now.AddDays(-2).AddHours(-6));

        Post messageQueue = await AddPostAsync(
            "At-least-once delivery means your handler will run twice",
            "It is not a theoretical edge case. Make the handler idempotent, or keep a record of what you have "
            + "already processed. Finding out in production is a bad way to learn this one.",
            bob, distributed, now.AddDays(-1).AddHours(-8));

        Post groupWork = await AddPostAsync(
            "Migrations in a group: let one person generate them",
            "Two people adding migrations against the same model produces a snapshot conflict that nobody enjoys "
            + "resolving. Agree on who owns them before the first one is created.",
            sofia, studyLife, now.AddDays(-1));

        Post oralExam = await AddPostAsync(
            "The oral exam asks why, not what",
            "Nobody is going to ask you to recite what AddScoped does. They will ask why your service layer exists "
            + "when the assignment did not require one. Have an answer for every choice you cannot point at an "
            + "assignment for.",
            mads, studyLife, now.AddHours(-14));

        await AddCommentAsync("This is the one that caught me too. The migration file is worth reading line by line.",
            alice, cascade, now.AddDays(-6).AddHours(2));
        await AddCommentAsync("Does the soft delete block the username from being reused?",
            bob, cascade, now.AddDays(-6).AddHours(5));
        await AddCommentAsync("It does, and that is deliberate. Otherwise someone could take the name and inherit the old attribution.",
            mads, cascade, now.AddDays(-6).AddHours(6));
        await AddCommentAsync("Worth saying out loud at the exam: the constraint forced a design decision.",
            sofia, cascade, now.AddDays(-5).AddHours(-3));
        await AddCommentAsync("We went the other way and made the key nullable. It rippled through everything.",
            alice, cascade, now.AddDays(-5));

        await AddCommentAsync("The fakes hide this, because they return LINQ-to-Objects and the tests still pass.",
            bob, queryProvider, now.AddDays(-5).AddHours(3));
        await AddCommentAsync("Right. Green tests and a broken swap is the worst combination.",
            alice, queryProvider, now.AddDays(-4).AddHours(-6));
        await AddCommentAsync("Assignment 7 asks for exactly this, so I would just document the limitation.",
            mads, queryProvider, now.AddDays(-4));

        await AddCommentAsync("A path one level up works from both the API and the migration folder, since they are siblings.",
            mads, relativePath, now.AddDays(-4).AddHours(4));
        await AddCommentAsync("Until you run it from bin and it breaks again. Configuration is the real answer.",
            sofia, relativePath, now.AddDays(-3).AddHours(-2));

        await AddCommentAsync("ClientSetNull also means EF does it in memory, not the database. Worth knowing which one acts.",
            mads, nullableFk, now.AddDays(-2).AddHours(-4));
        await AddCommentAsync("So if the dependents are not loaded, the delete just fails?",
            sofia, nullableFk, now.AddDays(-2).AddHours(-1));

        await AddCommentAsync("Added them to all five controllers. Took twenty minutes and the docs finally match.",
            alice, swagger, now.AddDays(-1).AddHours(-5));

        await AddCommentAsync("We generate from a shared proto in CI now. No more drift.",
            bob, grpc, now.AddDays(-1).AddHours(-2));

        await AddCommentAsync("A processed-message table is boring and it works.",
            alice, messageQueue, now.AddHours(-20));

        await AddCommentAsync("Learned this the hard way in SEP3.",
            mads, groupWork, now.AddHours(-16));

        await AddCommentAsync("This is the best advice in here.",
            bob, oralExam, now.AddHours(-9));
        await AddCommentAsync("Agreed. Write down every decision the assignment did not make for you.",
            alice, oralExam, now.AddHours(-4));
    }

    private async Task<User> AddUserAsync(string userName, string email, string role, DateTime createdAt)
    {
        User? existing = await userRepository.GetMany()
            .FirstOrDefaultAsync(user => user.UserName == userName);

        if (existing is not null)
        {
            return existing;
        }

        return await userRepository.AddAsync(new User
        {
            UserName = userName,
            Password = DemoPassword,
            Email = email,
            Role = role,
            CreatedAt = createdAt
        });
    }

    private async Task<SubForum> AddSubForumAsync(string name, string description, User creator, DateTime createdAt)
    {
        SubForum? existing = await subForumRepository.GetMany()
            .FirstOrDefaultAsync(subForum => subForum.Name == name);

        if (existing is not null)
        {
            return existing;
        }

        return await subForumRepository.AddAsync(new SubForum
        {
            Name = name,
            Description = description,
            CreatorUserId = creator.Id,
            CreatedAt = createdAt
        });
    }

    private async Task<Post> AddPostAsync(string title, string body, User author, SubForum subForum, DateTime createdAt) =>
        await postRepository.AddAsync(new Post
        {
            Title = title,
            Body = body,
            UserId = author.Id,
            SubForumId = subForum.Id,
            CreatedAt = createdAt
        });

    private async Task AddCommentAsync(string body, User author, Post post, DateTime createdAt) =>
        await commentRepository.AddAsync(new Comment
        {
            Body = body,
            PostId = post.Id,
            UserId = author.Id,
            CreatedAt = createdAt
        });
}
