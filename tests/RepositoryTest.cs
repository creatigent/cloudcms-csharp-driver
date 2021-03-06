using Xunit;
using System.Collections.Generic;
using CloudCMS;

namespace CloudCMS.Tests
{
    public class RepositoryTest : AbstractTest<PlatformFixture>
    {
        public RepositoryTest(PlatformFixture platformFixture) : base(platformFixture)
        {

        }

        [Fact]
        public async void TestRepositories()
        {
            IPlatform platform = Fixture.Platform;
            List<IRepository> repositories = await platform.ListRepositoriesAsync();
            Assert.True(repositories.Count > 0);

            IRepository repository = await platform.CreateRepositoryAsync();
            Assert.Equal("/repositories/" + repository.Id, repository.URI);

            string expectedRef = "repository://" + repository.PlatformId + "/" + repository.Id;
            Assert.Equal(expectedRef, repository.Ref.Ref);

            IRepository repositoryRead = await platform.ReadRepositoryAsync(repository.Id);
            Assert.Equal(repository.Data, repositoryRead.Data);

            await repository.DeleteAsync();
            repositoryRead = await platform.ReadRepositoryAsync(repository.Id);
            Assert.Null(repositoryRead);

            repository = await platform.ReadRepositoryAsync("I'm not real");
            Assert.Null(repository);
        }
    }
}