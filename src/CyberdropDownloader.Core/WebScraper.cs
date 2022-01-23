﻿using CyberdropDownloader.Core.DataModels;
using CyberdropDownloader.Core.Exceptions;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CyberdropDownloader.Core
{
    public class WebScraper
    {
        private Album _album;
        private bool _successful;

        public Album Album => _album;
        public bool Successful => _successful;

        public async Task LoadAlbumAsync(string url)
        {
            await Task.Run(async () =>
            {
                // Load webpage
                HtmlDocument htmlDocument = await new HtmlWeb().LoadFromWebAsync(url);

                if(htmlDocument != null)
                {
                    (string title, string size, Queue<AlbumFile> files) albumData = FetchAlbumData(htmlDocument);

                    _album = new Album(albumData.title, albumData.size, albumData.files);
                    _successful = true;
                }
            });
        }

        #region Load Album
        private (string title, string size, Queue<AlbumFile> files) FetchAlbumData(HtmlDocument htmlDocument)
        {
            return (FetchAlbumTitle(htmlDocument), FetchAlbumSize(htmlDocument), FetchAlbumFiles(htmlDocument));
        }

        private string FetchAlbumTitle(HtmlDocument htmlDocument)
        {
            string title = htmlDocument.DocumentNode.SelectNodes("//div/h1[@id='title']").First().Attributes["title"].Value;

            return title ?? throw new NullAlbumTitleException();
        }

        private string FetchAlbumSize(HtmlDocument htmlDocument)
        {
            string size = htmlDocument.DocumentNode.SelectNodes("//div/p[@class='title']")[1].InnerHtml;

            return size ?? throw new NullAlbumFilesException();
        }

        private Queue<AlbumFile> FetchAlbumFiles(HtmlDocument htmlDocument)
        {
            Queue<AlbumFile> urls = new Queue<AlbumFile>();

            HtmlNodeCollection files = htmlDocument.DocumentNode.SelectNodes("//a[@class='image'][@href]");

            if(files == null)
            {
                throw new NullAlbumFilesException();
            }

            foreach(HtmlNode link in files)
            {
                urls.Enqueue(new AlbumFile()
                {
                    Name = link.Attributes["title"].Value,
                    Url = link.Attributes["href"].Value
                });
            }

            return urls;
        }
        #endregion
    }
}
