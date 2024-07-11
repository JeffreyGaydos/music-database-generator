﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MusicDatabaseGenerator.Generators
{
    public class AlbumArtGenerator : AGenerator, IGenerator
    {
        private Bitmap _imgFile;
        private string _imgFileName;
        public AlbumArtGenerator(Bitmap imgFile, string imgFileName, MusicLibraryTrack track)
        {
            _imgFile = imgFile;
            _data = track;
            _imgFileName = imgFileName;
        }

        public void Generate()
        {
            UnhideAlbumArtImages(_imgFileName);

            _data.albumArt = new AlbumArt()
            {
                AlbumArtPath = PVU.PrevalidateStringTruncate(Path.GetFullPath(_imgFileName), 260, nameof(AlbumArt.AlbumArtPath)),
                PrimaryColor = ColorToHexString(GetPrimaryColor())
            };
        }

        private Color GetPrimaryColor()
        {
            GraphicsUnit unit = GraphicsUnit.Pixel;
            RectangleF bounds = _imgFile.GetBounds(ref unit);

            Dictionary<Color, int> maxColorDictionary = new Dictionary<Color, int>();

            int divisble = 25;
            int divisbleW = bounds.Width < divisble ? (int)bounds.Width : (int)(bounds.Width / divisble);
            int divisbleH = bounds.Height < divisble ? (int)bounds.Height : (int)(bounds.Height / divisble);

            for (int i = 0; i < bounds.Width; i += divisbleW)
            {
                for(int j = 0; j < bounds.Height; j += divisbleH)
                {
                    Color pixelColor = _imgFile.GetPixel(i, j);
                    if(maxColorDictionary.TryGetValue(pixelColor, out int count))
                    {
                        maxColorDictionary[pixelColor] = count + 1;
                    } else
                    {
                        maxColorDictionary.Add(pixelColor, 0);
                    }
                }
            }

            int maxCount = maxColorDictionary.Max(kvp => kvp.Value);

            foreach(KeyValuePair<Color, int> kvp in maxColorDictionary)
            {
                if(kvp.Value == maxCount)
                {
                    return kvp.Key;
                }
            }

            return maxColorDictionary.FirstOrDefault().Key;
        }

        private string ColorToHexString(Color color)
        {
            string r = color.R.ToString("X2");
            string g = color.G.ToString("X2");
            string b = color.B.ToString("X2");
            return $"#{r}{g}{b}";
        }

        private void UnhideAlbumArtImages(string path)
        {
            if (File.GetAttributes(path).HasFlag(FileAttributes.Hidden))
            {
                File.SetAttributes(path, FileAttributes.Normal);
            }
        }

        /*private void TEMP_ExportImages(string path)
        {
            string sanitizedPath = path.Replace('\\', '/');
            string relativePath = sanitizedPath.Substring(sanitizedPath.IndexOf("/Music/"));
            string fullPath = $"F:/MyMusicImageData_TEMP{relativePath}";
            string folder = fullPath.Substring(0, fullPath.LastIndexOf("/"));
            if(!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            File.Copy(path, fullPath);
        }*/
    }
}
