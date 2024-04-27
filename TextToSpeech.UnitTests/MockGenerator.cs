using Xunit;

namespace TextToSpeech.UnitTests;

public class MockGenerator : IClassFixture<DbContextFixture>
{
    private readonly DbContextFixture _dbContext;

    public MockGenerator(DbContextFixture dbContext)
    {
        _dbContext = dbContext;
    }

    public void GenerateTestData()
    {
        //var userFactory = new UserFactory();
        //var fakeUsers = userFactory.Generate(5);

        //_dbContext.DbContext.AddRange(fakeUsers);
        //_dbContext.DbContext.SaveChanges();
    }
}