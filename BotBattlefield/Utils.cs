using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColoryrSDK
{
    class Utils
    {
        /// <summary>
        /// 去掉字符串中的数字
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string RemoveNumber(string key)
        {
            return System.Text.RegularExpressions.Regex.Replace(key, @"\d", "");
        }

        public static string GetString(string a, string b, string c = null)
        {
            int x = a.IndexOf(b) + b.Length;
            int y;
            if (c != null)
            {
                y = a.IndexOf(c, x);
                if (a[y - 1] == '"')
                {
                    y = a.IndexOf(c, y + 1);
                }
                if (y - x <= 0)
                    return a;
                else
                    return a.Substring(x, y - x);
            }
            else
                return a.Substring(x);
        }

        public static Image ZoomImage(Image bitmap, int destHeight, int destWidth)
        {
            try
            {
                int width = 0, height = 0;
                //按比例缩放             
                int sourWidth = bitmap.Width;
                int sourHeight = bitmap.Height;
                if (sourHeight > destHeight || sourWidth > destWidth)
                {
                    if (sourWidth * destHeight > sourHeight * destWidth)
                    {
                        width = destWidth;
                        height = destWidth * sourHeight / sourWidth;
                    }
                    else
                    {
                        height = destHeight;
                        width = sourWidth * destHeight / sourHeight;
                    }
                }
                else
                {
                    width = sourWidth;
                    height = sourHeight;
                }
                bitmap.Mutate((a) =>
                {
                    a.Resize(width, height);
                });
                return bitmap;
            }
            catch
            {
                return bitmap;
            }
        }
    }
}
