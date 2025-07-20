using System.Text;
using Benchy.Output;

namespace Benchy.Tests.Unit.Output;

public class FormattedTextTests
{
    [Fact]
    public void PlainText_WriteTo_WritesStringContent()
    {
        // Arrange
        var writer = new StringWriter();
        FormattedText text = "Hello World";

        // Act
        text.WriteTo(writer, interactive: false);

        // Assert
        writer.ToString().Should().Be("Hello World");
    }

    [Fact]
    public void PlainText_WriteTo_BehavesIdenticallyInBothModes()
    {
        // Arrange
        var interactiveWriter = new StringWriter();
        var nonInteractiveWriter = new StringWriter();
        FormattedText text = "Test Content";

        // Act
        text.WriteTo(interactiveWriter, interactive: true);
        text.WriteTo(nonInteractiveWriter, interactive: false);

        // Assert
        interactiveWriter.ToString().Should().Be(nonInteractiveWriter.ToString());
        interactiveWriter.ToString().Should().Be("Test Content");
    }

    [Fact]
    public void ImplicitStringConversion_CreatesPlainText()
    {
        // Arrange & Act
        FormattedText text = "Implicit conversion";
        var writer = new StringWriter();
        text.WriteTo(writer, interactive: false);

        // Assert
        writer.ToString().Should().Be("Implicit conversion");
    }

    [Fact]
    public void Concatenation_WithPlus_CombinesTexts()
    {
        // Arrange
        FormattedText left = "Hello ";
        FormattedText right = "World";
        var writer = new StringWriter();

        // Act
        var combined = left + right;
        combined.WriteTo(writer, interactive: false);

        // Assert
        writer.ToString().Should().Be("Hello World");
    }

    [Fact]
    public void Join_MultipleParts_CombinesAllTexts()
    {
        // Arrange
        var parts = new FormattedText[] { "Part1", " ", "Part2", " ", "Part3" };
        var writer = new StringWriter();

        // Act
        var joined = FormattedText.Join(parts);
        joined.WriteTo(writer, interactive: false);

        // Assert
        writer.ToString().Should().Be("Part1 Part2 Part3");
    }

    [Fact]
    public void DecorationText_NonInteractiveMode_WritesNothing()
    {
        // Arrange
        var decoratedText = FormattedText.Decor("Decorated Content");
        var writer = new StringWriter();

        // Act
        decoratedText.WriteTo(writer, interactive: false);

        // Assert
        writer.ToString().Should().BeEmpty();
    }

    [Fact]
    public void DecorationText_InteractiveMode_WritesContent()
    {
        // Arrange
        var decoratedText = FormattedText.Decor("Decorated Content");
        var writer = new StringWriter();

        // Act
        decoratedText.WriteTo(writer, interactive: true);

        // Assert
        writer.ToString().Should().Be("Decorated Content");
    }

    [Fact]
    public void ColoredText_NonInteractiveMode_WritesContentWithoutColorChange()
    {
        // Arrange
        var originalColor = Console.ForegroundColor;
        var coloredText = FormattedText.Colored("Colored Text", ConsoleColor.Red);
        var writer = new StringWriter();

        // Act
        coloredText.WriteTo(writer, interactive: false);

        // Assert
        writer.ToString().Should().Be("Colored Text");
        Console.ForegroundColor.Should().Be(originalColor); // Color should remain unchanged
    }

    [Fact]
    public void ColoredText_InteractiveMode_WritesContentAndRestoresColor()
    {
        // Arrange
        var originalColor = Console.ForegroundColor;
        var coloredText = FormattedText.Colored("Colored Text", ConsoleColor.Green);
        var writer = new StringWriter();

        // Act
        coloredText.WriteTo(writer, interactive: true);

        // Assert
        writer.ToString().Should().Be("Colored Text");
        Console.ForegroundColor.Should().Be(originalColor); // Color should be restored
    }

    [Fact]
    public void Em_CreatesTextWithCyanColor()
    {
        // Arrange
        var emText = FormattedText.Em("Emphasized");
        var writer = new StringWriter();

        // Act
        emText.WriteTo(writer, interactive: false);

        // Assert
        writer.ToString().Should().Be("Emphasized");
    }

    [Fact]
    public void Dim_CreatesTextWithGrayColor()
    {
        // Arrange
        var dimText = FormattedText.Dim("Dimmed");
        var writer = new StringWriter();

        // Act
        dimText.WriteTo(writer, interactive: false);

        // Assert
        writer.ToString().Should().Be("Dimmed");
    }

    [Fact]
    public void ComplexCombination_MixesDecorationColorAndConcatenation()
    {
        // Arrange
        var complex =
            FormattedText.Em("Important: ")
            + FormattedText.Decor("[DEBUG] ")
            + "Regular text "
            + FormattedText.Dim("(note)");

        var interactiveWriter = new StringWriter();
        var nonInteractiveWriter = new StringWriter();

        // Act
        complex.WriteTo(interactiveWriter, interactive: true);
        complex.WriteTo(nonInteractiveWriter, interactive: false);

        // Assert
        interactiveWriter.ToString().Should().Be("Important: [DEBUG] Regular text (note)");
        nonInteractiveWriter.ToString().Should().Be("Important: Regular text (note)"); // Decoration is hidden
    }

    [Fact]
    public void NestedConcatenation_HandlesMultipleLevels()
    {
        // Arrange
        var part1 = FormattedText.Join("A", "B");
        var part2 = FormattedText.Join("C", "D");
        var combined = part1 + part2;
        var writer = new StringWriter();

        // Act
        combined.WriteTo(writer, interactive: false);

        // Assert
        writer.ToString().Should().Be("ABCD");
    }

    [Fact]
    public void EmptyStringHandling_WorksCorrectly()
    {
        // Arrange
        FormattedText empty = "";
        var writer = new StringWriter();

        // Act
        empty.WriteTo(writer, interactive: false);

        // Assert
        writer.ToString().Should().BeEmpty();
    }

    [Fact]
    public void ColoredText_NestedWithOtherFormatting_CombinesCorrectly()
    {
        // Arrange
        var nestedFormatting = FormattedText.Colored(
            FormattedText.Decor("Hidden") + "Visible",
            ConsoleColor.Yellow
        );

        var interactiveWriter = new StringWriter();
        var nonInteractiveWriter = new StringWriter();

        // Act
        nestedFormatting.WriteTo(interactiveWriter, interactive: true);
        nestedFormatting.WriteTo(nonInteractiveWriter, interactive: false);

        // Assert
        interactiveWriter.ToString().Should().Be("HiddenVisible");
        nonInteractiveWriter.ToString().Should().Be("Visible"); // Decoration hidden in non-interactive
    }
}
