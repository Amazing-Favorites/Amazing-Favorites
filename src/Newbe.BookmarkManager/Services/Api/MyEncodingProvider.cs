using System.Text;

namespace Newbe.BookmarkManager.Services
{
    sealed class MyEncodingProvider : EncodingProvider
    {
        public override Encoding GetEncoding(int codepage)
        {
            return CodePagesEncodingProvider.Instance.GetEncoding(codepage);
        }

        public override Encoding GetEncoding(string name)
        {
            if (name == "utf8")
            {
                return Encoding.UTF8;
            }
            else
            {
                return CodePagesEncodingProvider.Instance.GetEncoding(name);
            }
        }
    }
}