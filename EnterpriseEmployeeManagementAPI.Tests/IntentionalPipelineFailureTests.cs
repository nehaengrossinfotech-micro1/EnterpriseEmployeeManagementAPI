namespace EnterpriseEmployeeManagementAPI.Tests;

public sealed class IntentionalPipelineFailureTests
{
    [Fact]
    public void Review_fixture_keeps_test_pipeline_red_until_removed()
    {
        // Intentional failure for the code-review assistant lifecycle exercise.
        Assert.Equal("pipeline-passed", "intentional-code-review-fixture");
    }
}
