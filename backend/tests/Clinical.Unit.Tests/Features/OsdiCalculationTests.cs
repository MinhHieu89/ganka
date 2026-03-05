using Clinical.Application.Features;
using Clinical.Domain.Enums;
using FluentAssertions;

namespace Clinical.Unit.Tests.Features;

public class OsdiCalculationTests
{
    [Fact]
    public void Calculate_AllFours_ReturnsMaxScore()
    {
        // All 12 questions answered with 4 -> score = (48*100)/(12*4) = 100.0
        var answers = new int?[] { 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 };
        var result = OsdiCalculator.Calculate(answers);

        result.Should().NotBeNull();
        result!.Score.Should().Be(100.0m);
        result.Severity.Should().Be(OsdiSeverity.Severe);
    }

    [Fact]
    public void Calculate_AllZeros_ReturnsMinScore()
    {
        // All 12 questions answered with 0 -> score = (0*100)/(12*4) = 0.0
        var answers = new int?[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        var result = OsdiCalculator.Calculate(answers);

        result.Should().NotBeNull();
        result!.Score.Should().Be(0.0m);
        result.Severity.Should().Be(OsdiSeverity.Normal);
    }

    [Fact]
    public void Calculate_MixedWithNulls_CalculatesCorrectly()
    {
        // [4,4,4,4,4, 0,0,0,0, null,null,null] -> answered=9, sum=20, score=(20*100)/(9*4)=55.56
        var answers = new int?[] { 4, 4, 4, 4, 4, 0, 0, 0, 0, null, null, null };
        var result = OsdiCalculator.Calculate(answers);

        result.Should().NotBeNull();
        result!.Score.Should().BeApproximately(55.56m, 0.01m);
        result.Severity.Should().Be(OsdiSeverity.Severe);
    }

    [Fact]
    public void Calculate_Score15_ReturnsMild()
    {
        // Need sum/answered*4 to get 15 -> e.g. sum=9, answered=12 -> score=(9*100)/48=18.75 (Mild)
        // Better: sum=9, answered=15 -> need exactly 15
        // Let's use: [1,1,1,1,1, 1,1,0,0, null,null,null] -> answered=9, sum=7, score=(7*100)/36=19.44 (Mild)
        // Actually let's target exactly 15: (sum*100)/(answered*4)=15 -> sum*100=15*answered*4 -> sum=0.6*answered
        // answered=5, sum=3 -> 3*100/20=15
        var answers = new int?[] { 1, 1, 1, 0, 0, null, null, null, null, null, null, null };
        var result = OsdiCalculator.Calculate(answers);

        result.Should().NotBeNull();
        result!.Score.Should().Be(15.0m);
        result.Severity.Should().Be(OsdiSeverity.Mild);
    }

    [Fact]
    public void Calculate_Score25_ReturnsModerate()
    {
        // (sum*100)/(answered*4)=25 -> sum*100=25*answered*4 -> sum=answered
        // answered=4, sum=4 -> (4*100)/(4*4)=25
        var answers = new int?[] { 1, 1, 1, 1, null, null, null, null, null, null, null, null };
        var result = OsdiCalculator.Calculate(answers);

        result.Should().NotBeNull();
        result!.Score.Should().Be(25.0m);
        result.Severity.Should().Be(OsdiSeverity.Moderate);
    }

    [Fact]
    public void Calculate_ZeroAnswered_ReturnsNull()
    {
        // All null -> no questions answered -> guard returns null
        var answers = new int?[] { null, null, null, null, null, null, null, null, null, null, null, null };
        var result = OsdiCalculator.Calculate(answers);

        result.Should().BeNull();
    }

    [Fact]
    public void Calculate_EmptyArray_ReturnsNull()
    {
        var result = OsdiCalculator.Calculate([]);
        result.Should().BeNull();
    }

    [Fact]
    public void Calculate_SingleAnswer_CalculatesCorrectly()
    {
        // Single question answered (value=2) -> score = (2*100)/(1*4) = 50.0
        var answers = new int?[] { 2, null, null, null, null, null, null, null, null, null, null, null };
        var result = OsdiCalculator.Calculate(answers);

        result.Should().NotBeNull();
        result!.Score.Should().Be(50.0m);
        result.Severity.Should().Be(OsdiSeverity.Severe);
    }

    [Fact]
    public void Calculate_BoundaryNormal12_ReturnsNormal()
    {
        // score=12 is boundary of Normal
        // (sum*100)/(answered*4)=12 -> sum=12*answered*4/100=0.48*answered
        // answered=25, sum=12 -> too many. answered=1, sum=0.48 -> not integer
        // Use: answered=5, sum=5*4*12/100=2.4 -> not integer
        // Score exactly 12: sum*100 = 12*answered*4. sum = 0.48*answered. answered=25 -> sum=12
        // Actually answered max 12. Try answered=12, sum = 0.48*12 = 5.76 -> floor
        // Not exact. Score<=12 boundary test. Let's aim for 12.5 which is Mild.
        // Test boundary: score=12 should be Normal
        // (sum*100)/(answered*4)=12 is hard with integers. Test with score below and above.
        // answered=2, sum=1 -> (1*100)/(2*4)=12.5 -> Mild
        // answered=5, sum=2 -> (2*100)/(5*4)=10.0 -> Normal
        var answers = new int?[] { 1, 1, 0, 0, 0, null, null, null, null, null, null, null };
        var result = OsdiCalculator.Calculate(answers);

        result.Should().NotBeNull();
        result!.Score.Should().Be(10.0m);
        result.Severity.Should().Be(OsdiSeverity.Normal);
    }

    [Fact]
    public void Calculate_Boundary13_ReturnsMild()
    {
        // answered=2, sum=1 -> (1*100)/(2*4) = 12.5 -> above 12 -> Mild
        var answers = new int?[] { 1, 0, null, null, null, null, null, null, null, null, null, null };
        var result = OsdiCalculator.Calculate(answers);

        result.Should().NotBeNull();
        result!.Score.Should().Be(12.5m);
        result.Severity.Should().Be(OsdiSeverity.Mild);
    }

    [Fact]
    public void Calculate_Boundary33_ReturnsSevere()
    {
        // answered=3, sum=4 -> (4*100)/(3*4) = 33.33 -> Severe
        var answers = new int?[] { 2, 1, 1, null, null, null, null, null, null, null, null, null };
        var result = OsdiCalculator.Calculate(answers);

        result.Should().NotBeNull();
        result!.Score.Should().BeApproximately(33.33m, 0.01m);
        result.Severity.Should().Be(OsdiSeverity.Severe);
    }
}
