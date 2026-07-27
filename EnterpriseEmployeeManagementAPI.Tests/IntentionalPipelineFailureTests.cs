namespace EnterpriseEmployeeManagementAPI.Tests;

public sealed class IntentionalPipelineFailureTests
{
    [Fact]
    public void ReviewFixtureKeepsTestPipelineRedUntilRemoved()
    {
        // Intentional failure for the code-review assistant lifecycle exercise.
        Assert.Equal("pipeline-passed", "intentional-code-review-fixture");
    }
}
