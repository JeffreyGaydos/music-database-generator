using System;
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
            _data.albumArt = new AlbumArt()
            {
                AlbumArtPath = Path.GetFullPath(_imgFileName),
                PrimaryColor = ColorToHexString(GetPrimaryColor())
            };
        }

        private Color GetPrimaryColor()
        {
            GraphicsUnit unit = GraphicsUnit.Pixel;
            RectangleF bounds = _imgFile.GetBounds(ref unit);

            Dictionary<Color, int> maxColorDictionary = new Dictionary<Color, int>();

            for(int i = 0; i < bounds.Width; i += (int)(bounds.Width / 25))
            {
                for(int j = 0; j < bounds.Height; j += (int)(bounds.Width / 25))
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
    }
}
