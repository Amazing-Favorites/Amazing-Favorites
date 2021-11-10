using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.Moq;
using Microsoft.Extensions.Options;
using Moq;
using System.Threading.Tasks;
using AutoBogus;
using FluentAssertions;
using Microsoft.Toolkit.HighPerformance;
using NUnit.Framework;
using Newbe.BookmarkManager.Services;

public class IndexedBkSearcherTest
{


    private List<Bk> _bkList;

    private List<BkTag> _bkTagList;

    public IndexedBkSearcherTest()
    {
        _bkTagList = new BkTagGenerator().Generate(5);
        _bkList = new BkGenerator(_bkTagList).Generate(10);
    }
    [Test]
    public async Task DefaultSort_Search()
    {
        using var mocker = AutoMock.GetLoose();
        mocker.Mock<IIndexedDbRepo<Bk, string>>()
        .Setup(x => x.GetAllAsync()).ReturnsAsync(_bkList);
        
        mocker.Mock<IIndexedDbRepo<BkTag, string>>()
            .Setup(x => x.GetAllAsync()).ReturnsAsync(_bkTagList);

        var service = mocker.Create<IndexedBkSearcher>();
        var result =  await service.Search(string.Empty, 100);

        result.Count().Should().Be(_bkList.Count());
    }

    public class BkGenerator : AutoFaker<Bk>
    {
        public BkGenerator(List<BkTag> tagList)
        {
            RuleFor(x => x.Id, f => f.Internet.Url());
            RuleFor(x => x.Title, f => f.Random.Word());
            RuleFor(x => x.Url, f => f.Random.Word());
            RuleFor(x => x.Tags, f => tagList.Select(x => x.Tag).Where(x=>f.Random.Bool(.5f)).ToList());
        }
    }

    public class BkTagGenerator : AutoFaker<BkTag>
    {
        public BkTagGenerator()
        {
            RuleFor(x => x.Id, f => f.Random.Guid().ToString());
            RuleFor(x => x.Tag, f => f.Random.Word());
        }
    }
    
}