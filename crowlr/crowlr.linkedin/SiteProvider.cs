using crowlr.contracts;
using crowlr.core;
using System;
using System.Collections.Generic;

namespace crowlr.linkedin
{
    public class SiteProvider : ISiteProvider
    {
        private IPageDownloader Downloader { get; set; }
        private IDictionary<string, string> SessionParams { get; set; } = new Dictionary<string, string>();

        public string GetSessionParams(string param)
        {
            if (!SessionParams.ContainsKey(param))
                return null;

            return SessionParams[param];
        }

        public SiteProvider(IPageDownloader downloader)
        {
            Downloader = downloader;
        }

        public IPage ExecutePage<T>(string pageName, T @params)
        {
            switch(pageName)
            {
                case Pages.PostLogin:
                    return this.Login((@params as string[,]).ToDictionary());

                default:
                    throw new ArgumentOutOfRangeException($@"Not recognized page name. Current page name = [{pageName}]");
            }
        }

        private IPage ToLoginPage()
        {
            var loginPage = Downloader.Get("https://www.linkedin.com/uas/login");

            SessionParams.Add(SessionParameters.Csrf, loginPage.GetNodeById(SessionParameters.Csrf)?.Value);
            SessionParams.Add(SessionParameters.Alias, loginPage.GetNodeById(SessionParameters.Alias)?.Value);

            return loginPage;
        }

        private IPage Login(IDictionary<string, string> parameters)
        {
            var loginPage = this.ToLoginPage();
            
            var defaultParameters = new Dictionary<string, string>
                {
                    { "isJsEnabled", "false" },
                    { "loginCsrfParam", GetSessionParams(SessionParameters.Csrf) },
                    { "sourceAlias", GetSessionParams(SessionParameters.Alias) },
                    { "submit", "Войти" }
                };

            parameters.AddRange(defaultParameters);

            return Downloader.Post("https://www.linkedin.com/uas/login-submit", parameters);
        }

        public void Dispose()
        {
            Downloader?.Dispose();
        }
    }
}
