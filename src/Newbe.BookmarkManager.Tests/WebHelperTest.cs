using FluentAssertions;
using Newbe.BookmarkManager.Services;
using Newbe.BookmarkManager.Services.Common;
using NUnit.Framework;

namespace Newbe.BookmarkManager.Tests
{
    public class WebHelperTest
    {
        public const string nullToken = null;
        public const string validToken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpYXQiOjE2Mjg1ODYzNTgsImp0aSI6Ijg5ZWJlMzdkLTE3YWEtNDY5ZC04Njk0LThmMGNkNzRmZGM1ZCIsInN1YiI6IjczOGZhOGI2LWIzZmUtNDA5Ni04ODU1LTViN2ViNzZmMzBiYyIsIm5iZiI6MTYyODQ5OTk1OCwiZXhwIjoxNjMxMTc4MzU4LCJpc3MiOiJ1c2VyLm5ld2JlLnBybyIsImF1ZCI6ImFwaS5uZXdiZS5wcm8ifQ.hjOiqYkpoCJnQ5zFmshUpZA_Mturd_QBanZy2e_fmX-X4L5WqSLN11LwddVzVwKMe201oJCAmtAVtFEBaoIYD-YKKzBnyu5U32SizTD3FZv_iZy_nD6tmBtioxR8RRPiYreOciBll1KzZSX0YAdHEpDehlVetfFYW2hYAedER67obQMnh74gdRDdgwOMj10OzHcAvDQTSndEG8kOtKJaFxpVwcLld45Gm4HpH8SOv0sDmFdtwIucuma6Wsj5sj-M6G4BMk6kEEb297V3fHURHsdihdPZTbjHsos1bteojuFpaJ17Z3GEUjtMLKpqKXpb4MnPSZgf5SqKV6m70qtWeQ";
        public const string invalidToken = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpYXQiOjE2Mjg1ODYzNTgsImp0aSI6Ijg5ZWJlMzdkLTE3YWEtNDY5ZC04Njk0LThmMGNkNzRmZGM1ZCIsInN1YiI6IjczOGZhOGI2LWIzZmUtNDA5Ni04ODU1LTViN2V";

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        [TestCase(validToken)]
        public void GetJwtExp_ValidToken_Test(string token)
        {
            var exp = WebHelper.GetJwtExp(token);
            exp.Should().NotBeNull();
        }

        [Test]
        [TestCase(nullToken)]
        public void GetJwtExp_NullToken_Test(string token)
        {
            Assert.Throws<AccessTokenInvalidException>(() => WebHelper.GetJwtExp(token));
        }

        [Test]
        [TestCase(invalidToken)]
        public void GetJwtExp_InvalidToken_Test(string token)
        {
            Assert.Throws<AccessTokenInvalidException>(() => WebHelper.GetJwtExp(token));
        }
    }
}