using Entities;
using Tests.TestHelpers;
using Xunit;

namespace Tests.IntegrationTests;

// Integration tests: the whole CLI - menus -> views -> real in-memory repositories filled by DataSeeder -
// driven through the console like a real user. Black-box: designed from the Assignment 2 requirements.
// One nested class per part of the application. Test names: Should<Result>_When<Condition>.
public class CliAppTests
{
    private static readonly TimeSpan RunTimeout = TimeSpan.FromSeconds(5);

    // Runs the app on a background thread and throws if it has not stopped in time, so an endless loop
    // fails the test instead of hanging the whole test run.
    private static async Task RunAsync(TestForumApp forum)
    {
        Task run = Task.Run(() => forum.App.StartAsync());
        Task finished = await Task.WhenAny(run, Task.Delay(RunTimeout));
        if (finished != run)
        {
            throw new TimeoutException($"CliApp did not stop within {RunTimeout.TotalSeconds} seconds.");
        }

        await run;
    }

    // Main menu - valid choices are 1..3:
    //   Partition (EP)   | Representative  | BVA values | Expected
    //   invalid choice   | "x", "99", ""   | "0", "4"   | "Invalid choice. Try again."
    //   1 = Manage Users |                 | "1"        | the users menu opens
    //   2 = Manage Posts | "2"             |            | the posts menu opens
    //   3 = Exit         |                 | "3"        | the application stops
    [Collection(ConsoleCollection.Name)]
    public class MainMenu
    {
        [Theory]
        [InlineData("0")]  // BVA: just below the lowest choice
        [InlineData("4")]  // BVA: just above the highest choice
        [InlineData("x")]  // EP: not a number
        [InlineData("99")] // EP: number far outside the menu
        [InlineData("")]   // EP: nothing entered
        public async Task ShouldSayTheChoiceIsInvalid_WhenChoiceIsOutsideOneToThree(string choice)
        {
            // Arrange
            TestForumApp forum = await TestForumApp.CreateSeededAsync();
            using var console = new ConsoleSession(choice, "3");
            string expectedMessage = "Invalid choice. Try again.";

            // Act
            await RunAsync(forum);

            // Assert
            Assert.Contains(expectedMessage, console.Output);
        }

        [Fact]
        public async Task ShouldOpenTheUsersMenu_WhenChoiceIsOne()
        {
            // Arrange - BVA: the lowest valid choice
            TestForumApp forum = await TestForumApp.CreateSeededAsync();
            using var console = new ConsoleSession("1", "3", "3");
            string usersMenuOption = "1. Create user";

            // Act
            await RunAsync(forum);

            // Assert
            Assert.Contains(usersMenuOption, console.Output);
        }

        [Fact]
        public async Task ShouldOpenThePostsMenu_WhenChoiceIsTwo()
        {
            // Arrange
            TestForumApp forum = await TestForumApp.CreateSeededAsync();
            using var console = new ConsoleSession("2", "4", "3");
            string postsMenuOption = "1. Create Post";

            // Act
            await RunAsync(forum);

            // Assert
            Assert.Contains(postsMenuOption, console.Output);
        }

        [Fact]
        public async Task ShouldExit_WhenChoiceIsThree()
        {
            // Arrange - BVA: the highest valid choice
            TestForumApp forum = await TestForumApp.CreateSeededAsync();
            using var console = new ConsoleSession("3");
            string expectedMessage = "You have chosen to exit.";

            // Act
            await RunAsync(forum);

            // Assert
            Assert.Contains(expectedMessage, console.Output);
        }
    }

    // Posts menu - valid choices are 1..4:
    //   Partition (EP)   | Representative | BVA values | Expected
    //   invalid choice   | "x"            | "0", "5"   | "Invalid choice"
    //   4 = Back         |                | "4"        | back to the main menu
    [Collection(ConsoleCollection.Name)]
    public class PostsMenu
    {
        [Theory]
        [InlineData("0")] // BVA: just below the lowest choice
        [InlineData("5")] // BVA: just above the highest choice
        [InlineData("x")] // EP: not a number
        public async Task ShouldSayTheChoiceIsInvalid_WhenChoiceIsOutsideOneToFour(string choice)
        {
            // Arrange
            TestForumApp forum = await TestForumApp.CreateSeededAsync();
            using var console = new ConsoleSession("2", choice, "4", "3");
            // The line ends right after "Invalid choice" - the main menu's message continues with ". Try again."
            string expectedMessage = "Invalid choice" + Environment.NewLine;

            // Act
            await RunAsync(forum);

            // Assert
            Assert.Contains(expectedMessage, console.Output);
        }

        [Fact]
        public async Task ShouldReturnToTheMainMenu_WhenChoiceIsFour()
        {
            // Arrange - BVA: the highest valid choice. "3" only exits if we are back in the main menu.
            TestForumApp forum = await TestForumApp.CreateSeededAsync();
            using var console = new ConsoleSession("2", "4", "3");
            string expectedMessage = "You have chosen to exit.";

            // Act
            await RunAsync(forum);

            // Assert
            Assert.Contains(expectedMessage, console.Output);
        }
    }

    // The must-have requirements of Assignment 2, end to end.
    [Collection(ConsoleCollection.Name)]
    public class UseCases
    {
        [Fact]
        public async Task ShouldListTheNewUser_WhenUserIsCreatedAndUsersAreListed()
        {
            // Arrange
            TestForumApp forum = await TestForumApp.CreateSeededAsync();
            using var console = new ConsoleSession("1", "1", "mads", "secret", "mads@x.dk", "2", "3", "3");
            string expectedEntry = "UserName: mads";

            // Act
            await RunAsync(forum);

            // Assert
            Assert.Contains(expectedEntry, console.Output);
        }

        [Fact]
        public async Task ShouldShowTheNewPost_WhenPostIsCreatedAndThenViewed()
        {
            // Arrange
            TestForumApp forum = await TestForumApp.CreateSeededAsync();
            string newPostId = (forum.Posts.GetManyAsync().Max(p => p.Id) + 1).ToString();
            using var console = new ConsoleSession(
                "2", "1", "Integration title", "Integration body", "1",
                "3", newPostId,
                "4", "3");
            string expectedTitle = "Title: Integration title";

            // Act
            await RunAsync(forum);

            // Assert
            Assert.Contains(expectedTitle, console.Output);
        }

        [Fact]
        public async Task ShouldListEverySeededPost_WhenPostsOverviewIsOpened()
        {
            // Arrange
            TestForumApp forum = await TestForumApp.CreateSeededAsync();
            List<string> expectedTitles = forum.Posts.GetManyAsync().Select(p => $"Title: {p.Title}").ToList();
            using var console = new ConsoleSession("2", "2", "4", "3");

            // Act
            await RunAsync(forum);

            // Assert
            Assert.All(expectedTitles, title => Assert.Contains(title, console.Output));
        }

        [Fact]
        public async Task ShouldShowItsComment_WhenSeededPostIsViewed()
        {
            // Arrange
            TestForumApp forum = await TestForumApp.CreateSeededAsync();
            Comment comment = forum.Comments.GetManyAsync().First(c => c.PostId == 1);
            using var console = new ConsoleSession("2", "3", "1", "4", "3");
            string expectedComment = $"- {comment.Body}";

            // Act
            await RunAsync(forum);

            // Assert
            Assert.Contains(expectedComment, console.Output);
        }

        [Fact]
        public async Task ShouldNotStoreThePost_WhenUserIdHasNoUser()
        {
            // Arrange
            TestForumApp forum = await TestForumApp.CreateSeededAsync();
            int expectedPostCount = forum.Posts.GetManyAsync().Count();
            using var console = new ConsoleSession("2", "1", "Title", "Body", "99", "4", "3");

            // Act
            await RunAsync(forum);

            // Assert
            Assert.Equal(expectedPostCount, forum.Posts.GetManyAsync().Count());
        }

        [Fact]
        public async Task ShouldKeepRunning_WhenUnknownPostIsViewed()
        {
            // Arrange - the app used to crash here; reaching "exit" proves it survived
            TestForumApp forum = await TestForumApp.CreateSeededAsync();
            using var console = new ConsoleSession("2", "3", "99", "4", "3");
            string expectedMessage = "You have chosen to exit.";

            // Act
            await RunAsync(forum);

            // Assert
            Assert.Contains(expectedMessage, console.Output);
        }
    }

    // White-box: every menu loop has a branch for Console.ReadLine() returning null (the input has ended).
    // These tests exist to execute that branch in each menu - they are designed from the code.
    [Collection(ConsoleCollection.Name)]
    public class InputEnds
    {
        [Fact]
        public async Task ShouldStop_WhenThereIsNoInputAtAll()
        {
            // Arrange
            TestForumApp forum = await TestForumApp.CreateSeededAsync();
            using var console = new ConsoleSession();

            // Act
            Exception? exception = await Record.ExceptionAsync(() => RunAsync(forum));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task ShouldStop_WhenInputEndsInsideTheUsersMenu()
        {
            // Arrange
            TestForumApp forum = await TestForumApp.CreateSeededAsync();
            using var console = new ConsoleSession("1");

            // Act
            Exception? exception = await Record.ExceptionAsync(() => RunAsync(forum));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public async Task ShouldStop_WhenInputEndsInsideThePostsMenu()
        {
            // Arrange
            TestForumApp forum = await TestForumApp.CreateSeededAsync();
            using var console = new ConsoleSession("2");

            // Act
            Exception? exception = await Record.ExceptionAsync(() => RunAsync(forum));

            // Assert
            Assert.Null(exception);
        }
    }
}
