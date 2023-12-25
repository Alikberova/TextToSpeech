using BookToAudio.Infa;
using BookToAudio.Tests.TestData;

namespace BookToAudio.Tests;

[TestClass]
public class UnitTest1
{
    private readonly AppDbContext _dbContext;

    public UnitTest1()
    {
        _dbContext = new AppDbContext();
    }

    [TestMethod]
    public void GenerateTestData()
    {
        var userFactory = new UserFactory();
        var fakeUsers = userFactory.Generate(5);

        _dbContext.AddRange(fakeUsers);
        _dbContext.SaveChanges();
    }
}