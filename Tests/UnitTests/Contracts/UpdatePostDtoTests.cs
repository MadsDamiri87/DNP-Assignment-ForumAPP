using System.Text.Json;
using ApiContracts;
using Xunit;

namespace Tests.UnitTests.Contracts;

// Unit tests af UpdatePostDto. PUT er en fuld erstatning, så subForumId skal med - ellers ville posten
// tavst miste sit subforum. Testnavne: Should<Resultat>_When<Betingelse>.
public class UpdatePostDtoTests
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    // Regressionstest: en udeladt subForumId nulstillede tidligere postens subforum uden en fejl.
    [Fact]
    public void ShouldThrow_WhenSubForumIdIsMissing()
    {
        // Arrange
        string json = """{"title":"T","body":"B"}""";

        // Act + Assert
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<UpdatePostDto>(json, Options));
    }

    [Fact]
    public void ShouldAccept_WhenSubForumIdIsExplicitlyNull()
    {
        // Arrange
        string json = """{"title":"T","body":"B","subForumId":null}""";

        // Act
        UpdatePostDto? dto = JsonSerializer.Deserialize<UpdatePostDto>(json, Options);

        // Assert
        Assert.Null(dto!.SubForumId);
    }

    [Fact]
    public void ShouldReadTheId_WhenSubForumIdIsGiven()
    {
        // Arrange
        string json = """{"title":"T","body":"B","subForumId":3}""";

        // Act
        UpdatePostDto? dto = JsonSerializer.Deserialize<UpdatePostDto>(json, Options);

        // Assert
        Assert.Equal(3, dto!.SubForumId);
    }
}
