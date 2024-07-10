using HtmlAgilityPack;
using IMDB.Core.Data;
using IMDB.Core.Entities;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;

namespace IMDB.Services
{
    public class IMDbArtorScrapService
    {
        private readonly ILogger<IMDbArtorScrapService> _logger;
        private readonly ApplicationDbContext _context;
        public IMDbArtorScrapService(ILogger<IMDbArtorScrapService> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        //public async Task<List<Actor>> ScrapeArtorData(string url)
        //{
        //    var actorList = new List<Actor>();

        //    try
        //    {
        //        var web = new HtmlWeb();
        //        var doc = await web.LoadFromWebAsync(url);

        //        var nodes = doc.DocumentNode.SelectNodes("//li[contains(@class, 'ipc-metadata-list-summary-item')]");
        //        if (nodes != null)
        //        {
        //            foreach (var node in nodes)
        //            {
        //                try
        //                {

        //                    var nameNode = node.SelectSingleNode(".//h3[@class='ipc-title__text']");
        //                    var rankNode = nameNode?.InnerText.Split('.').FirstOrDefault();
        //                    var fullName = nameNode?.InnerText.Substring(nameNode.InnerText.IndexOf('.') + 1).Trim();
        //                    var typeNode = node.SelectSingleNode(".//ul[contains(@class, 'ipc-inline-list')]")?.InnerText;

        //                    var descriptionNode = node.SelectSingleNode(".//div[contains(@class, 'ipc-bq__b-base')]");
        //                    if (fullName != null && rankNode != null)
        //                    {
        //                        var description = descriptionNode != null ? descriptionNode.InnerText.Trim() : "";
        //                        var type = typeNode != null ? typeNode : "";
        //                        actorList.Add(new Actor
        //                        {
        //                            Rank = int.Parse(rankNode.Trim()),
        //                            Name = fullName.Trim(),
        //                            Details = description,
        //                            Type = type,
        //                            Source = "IMDb"
        //                        });
        //                        Task.Delay(100);

        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    _logger.LogError(ex, ex.Message);

        //                }
        //            }
        //        }
        //        else
        //        {
        //            _logger.LogWarning("No nodes found in the HTML document.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, ex.Message);
        //    }
        //    return  actorList;

        //}

        //using Selenium
        public async Task<List<Actor>> ScrapeArtorData(string url)
        {
            var actorList = new List<Actor>();

            try
            {
                var options = new ChromeOptions();
                //options.AddArgument("--headless");
                options.AddArgument("--disable-gpu");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");

                using (var driver = new ChromeDriver(options))
                {
                    driver.Navigate().GoToUrl(url);


                    var js = (IJavaScriptExecutor)driver;
                    var lastHeight = js.ExecuteScript("return document.body.scrollHeight");

                    while (true)
                    {
                        js.ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
                        await Task.Delay(2000);

                        var newHeight = js.ExecuteScript("return document.body.scrollHeight");
                        if (newHeight.Equals(lastHeight))
                        {
                            break;
                        }
                        lastHeight = newHeight;
                    }

                    var pageSource = driver.PageSource;
                    var doc = new HtmlDocument();
                    doc.LoadHtml(pageSource);

                    var nodes = doc.DocumentNode.SelectNodes("//li[contains(@class, 'ipc-metadata-list-summary-item')]");
                    if (nodes != null)
                    {
                        foreach (var node in nodes)
                        {
                            try
                            {

                                var nameNode = node.SelectSingleNode(".//h3[@class='ipc-title__text']");
                                var rankNode = nameNode?.InnerText.Split('.').FirstOrDefault();
                                var fullName = nameNode?.InnerText.Substring(nameNode.InnerText.IndexOf('.') + 1).Trim();
                                var typeNode = node.SelectSingleNode(".//ul[contains(@class, 'ipc-inline-list')]")?.InnerText;

                                var descriptionNode = node.SelectSingleNode(".//div[contains(@class, 'ipc-bq__b-base')]");
                                if (fullName != null && rankNode != null)
                                {
                                    var description = descriptionNode != null ? descriptionNode.InnerText.Trim() : "";
                                    var type = typeNode != null ? typeNode : "";
                                    var actor = _context.Actors.Any(x => x.Name == fullName.Trim() && x.Rank == int.Parse(rankNode.Trim()) && x.Source == "IMDb");
                                    if (!actor)
                                    {

                                        actorList.Add(new Actor
                                        {
                                            Rank = int.Parse(rankNode.Trim()),
                                            Name = fullName.Trim(),
                                            Details = description,
                                            Type = type,
                                            Source = "IMDb"
                                        });
                                    }
                                    Task.Delay(100);

                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, ex.Message);

                            }
                        }
                    }
                    else
                    {
                        _logger.LogWarning("No nodes found in the HTML document.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return actorList;

        }

    }
}
