using Entities;
using Tests.TestHelpers;
using Xunit;

namespace Tests.IntegrationTests;

// Integrationstests: hele CLI'en - menuer -> views -> rigtige in-memory repositories fyldt af DataSeeder -
// styret via konsollen ligesom en rigtig bruger. Black-box: lavet ud fra kravene i Assignment 2.
// Én nested klasse pr. del af programmet. Testnavne: Should<Resultat>_When<Betingelse>.
public class CliAppTests
{
    private static readonly TimeSpan RunTimeout = TimeSpan.FromSeconds(5);

    // Kører appen på en baggrundstråd og kaster en fejl, hvis den ikke er stoppet i tide, så en uendelig løkke
    // får testen til at fejle i stedet for at hænge hele testkørslen.
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

    // Hovedmenu - gyldige valg er 1..3:
    //   Partition (EP)   | Repræsentant     | BVA-værdi  | Forventet
    //   ugyldigt valg    | "x", "99", ""   | "0", "4"   | "Invalid choice. Try again."
    //   1 = Manage Users |                 | "1"        | brugermenuen åbner
    //   2 = Manage Posts | "2"             |            | postmenuen åbner
    //   3 = Exit         |                 | "3"        | programmet stopper
    [Collection(ConsoleCollection.Name)]
    public class MainMenu
    {
        [Theory]
        [InlineData("0")]  // BVA: lige under laveste valg
        [InlineData("4")]  // BVA: lige over højeste valg
        [InlineData("x")]  // EP: ikke et tal
        [InlineData("99")] // EP: tal langt uden for menuen
        [InlineData("")]   // EP: ingenting indtastet
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
            // Arrange - BVA: laveste gyldige valg
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
            // Arrange - BVA: højeste gyldige valg
            TestForumApp forum = await TestForumApp.CreateSeededAsync();
            using var console = new ConsoleSession("3");
            string expectedMessage = "You have chosen to exit.";

            // Act
            await RunAsync(forum);

            // Assert
            Assert.Contains(expectedMessage, console.Output);
        }
    }

    // Postmenu - gyldige valg er 1..4:
    //   Partition (EP)   | Repræsentant   | BVA-værdi  | Forventet
    //   ugyldigt valg    | "x"            | "0", "5"   | "Invalid choice"
    //   4 = Back         |                | "4"        | tilbage til hovedmenuen
    [Collection(ConsoleCollection.Name)]
    public class PostsMenu
    {
        [Theory]
        [InlineData("0")] // BVA: lige under laveste valg
        [InlineData("5")] // BVA: lige over højeste valg
        [InlineData("x")] // EP: ikke et tal
        public async Task ShouldSayTheChoiceIsInvalid_WhenChoiceIsOutsideOneToFour(string choice)
        {
            // Arrange
            TestForumApp forum = await TestForumApp.CreateSeededAsync();
            using var console = new ConsoleSession("2", choice, "4", "3");
            // Linjen slutter lige efter "Invalid choice" - hovedmenuens besked fortsætter med ". Try again."
            string expectedMessage = "Invalid choice" + Environment.NewLine;

            // Act
            await RunAsync(forum);

            // Assert
            Assert.Contains(expectedMessage, console.Output);
        }

        [Fact]
        public async Task ShouldReturnToTheMainMenu_WhenChoiceIsFour()
        {
            // Arrange - BVA: højeste gyldige valg. "3" afslutter kun, hvis vi er tilbage i hovedmenuen.
            TestForumApp forum = await TestForumApp.CreateSeededAsync();
            using var console = new ConsoleSession("2", "4", "3");
            string expectedMessage = "You have chosen to exit.";

            // Act
            await RunAsync(forum);

            // Assert
            Assert.Contains(expectedMessage, console.Output);
        }
    }

    // Must-have-kravene fra Assignment 2, hele vejen igennem.
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
            // Arrange - appen plejede at crashe her; at vi når til "exit" viser, at den overlever
            TestForumApp forum = await TestForumApp.CreateSeededAsync();
            using var console = new ConsoleSession("2", "3", "99", "4", "3");
            string expectedMessage = "You have chosen to exit.";

            // Act
            await RunAsync(forum);

            // Assert
            Assert.Contains(expectedMessage, console.Output);
        }
    }

    // White-box: hver menuløkke har en gren til, at Console.ReadLine() returnerer null (inputtet er slut).
    // Testene er der for at køre den gren i hver menu - de er lavet ud fra koden.
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
