using System;

namespace crowlr.contracts
{
    public interface IPageDownloader
    {
        IPage GetPage(string url);

        IPage GetPage(Uri uri);
    }
}
