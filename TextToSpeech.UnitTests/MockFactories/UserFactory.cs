using Bogus;
using TextToSpeech.Core.Entities;

namespace TextToSpeech.UnitTests.TestData;

internal class UserFactory : Faker<User>
{
    public UserFactory()
    {
        RuleFor(u => u.Id, f => f.Random.Guid().ToString());
        RuleFor(u => u.FirstName, f => f.Person.FirstName);
        RuleFor(u => u.LastName, f => f.Person.LastName);
        RuleFor(u => u.Email, f => f.Person.Email);
        RuleFor(u => u.PhoneNumber, f => f.Person.Phone);
        RuleFor(u => u.UserName, f => f.Person.UserName);
        RuleFor(u => u.Password, f => f.Internet.Password());
    }
}
