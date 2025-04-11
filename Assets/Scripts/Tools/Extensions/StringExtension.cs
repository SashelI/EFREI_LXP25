using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
#if WINDOWS_UWP
using Windows.UI;
using Windows.UI.Xaml.Media;
#endif

namespace Assets.Scripts.Tools.Extensions
{
    public static class StringExtension
    {
        /// <summary>
        /// Remove diacritics from string. Origin value can be null and String.Empty.
        /// </summary>
        /// <param name="value">Origin string value. </param>
        /// <returns>New string value without diacritics.</returns>
        public static string RemoveDiacritics(this string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                string stFormD = value.Normalize(NormalizationForm.FormD);
                int len = stFormD.Length;
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < len; i++)
                {
                    UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[i]);
                    if (uc != UnicodeCategory.NonSpacingMark)
                    {
                        sb.Append(stFormD[i]);
                    }
                }

                return (sb.ToString().Normalize(NormalizationForm.FormC));
            }
            else
            {
                return value;
            }
        }

        /// <summary>
        /// Formate un texte en Upper Camel Case
        /// </summary>
        /// <param name="value">string - texte a formatter</param>
        /// <returns>string - texte formaté en Upper Camel Case</returns>
        public static string ToUpperCamelCase(this string value)
        {
            string retour = string.Empty;
            if (!String.IsNullOrWhiteSpace(value))
            {
                string[] words = value.Split(' ');
                foreach (string word in words)
                {
                    if (!String.IsNullOrWhiteSpace(word))
                    {
                        char firstChar = char.ToUpper(word[0]);
                        retour += (firstChar + word.Substring(1).ToLower());
                    }
                }
            }

            return retour;
        }

        /// <summary>
        /// Formate un texte en Lower Camel Case
        /// </summary>
        /// <param name="value">string - texte a formatter</param>
        /// <returns>string - texte formaté en Lower Camel Case</returns>
        public static string ToLowerCamelCase(this string value)
        {
            string retour = ToUpperCamelCase(value);
            if (retour.Length < 1)
            {
                return retour.ToLower();
            }
            else
            {
                return char.ToLower(retour[0]) + retour.Substring(1);
            }
        }

        /// <summary>
        /// Mettre en majuscule seulement le premier caractère
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ToUpperOnlyFirstLetter(this string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            if (text.Length == 1)
                return text.ToUpper();

            return text[0].ToString().ToUpper() + text.Substring(1).ToLower();
        }

        /// <summary>
        /// Appliquer les sauts de ligne indiqués dans le string d'origine
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ApplyLineBreakChars(this string value)
        {
            var newString = value.Replace(@"\n", "\n");
            newString = newString.Replace(@"\""", "\"");

            return newString;
        }

        /// <summary>
        /// Obtenir les n derniers caractères d'un string
        /// </summary>
        /// <param name="value"></param>
        /// <param name="numberCharacters"></param>
        /// <returns></returns>
        public static string Last(this string value, int numberCharacters)
        {
            if (numberCharacters == 0)
                return value;

            if (numberCharacters >= value.Length)
                return value;

            return value.Substring(value.Length - numberCharacters);
        }

#if WINDOWS_UWP
        /*
        /// <summary>
        /// Obtenir la couleur d'une chaîne
        /// </summary>
        /// <param name="name"></param>
        /// <returns>SolidColorBrush</returns>
        public static SolidColorBrush ToSolidColorBrush(this string name)
        {
            return new SolidColorBrush(name.ToWindowsUIColor());
        }

        /// <summary>
        /// Obtenir la couleur d'une chaîne
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Windows.UI.Color</returns>
        public static Windows.UI.Color ToWindowsUIColor(this string name)
        {
            return name.ToUnityColor().ToWindowsUIColor() ;
        }*/
#endif

        /// <summary>
        /// Obtenir la couleur d'une chaîne
        /// </summary>
        /// <param name="name"></param>
        /// <returns>UnityEngine.Color</returns>
        public static UnityEngine.Color ToUnityColor(this string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return UnityEngine.Color.clear;
            }

            string rgbString = null;

            if (name.StartsWith("#"))
            {
                rgbString = name.Substring(1);
            }
            else
            {
                rgbString = name;
            }

            if (rgbString.Length < 6)
            {
                return UnityEngine.Color.clear;
            }

            string rString = rgbString.Substring(0, 2);
            string gString = rgbString.Substring(2, 2);
            string bString = rgbString.Substring(4, 2);

            int r;
            if (int.TryParse(rString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out r) == false)
            {
                return UnityEngine.Color.clear;
            }

            int g;
            if (int.TryParse(gString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out g) == false)
            {
                return UnityEngine.Color.clear;
            }

            int b;
            if (int.TryParse(bString, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out b) == false)
            {
                return UnityEngine.Color.clear;
            }

            UnityEngine.Color brush = new UnityEngine.Color(r / 255f, g / 255f, b / 255f, 0xFF);

            return brush;
        }

        /// <summary>
        /// Wrapper un texte avec des mots entiers
        /// </summary>
        /// <param name="text"></param>
        /// <param name="lineLength"></param>
        /// <returns></returns>
        public static string[] Wrap(this string text, int lineLength)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            var lines = text.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            string previousWord = String.Empty;
            List<string> returnedLines = new List<string>();
            foreach (String word in lines)
            {
                if ((previousWord.Length + word.Length + 1) <= lineLength)
                {
                    returnedLines.Remove(previousWord);
                    previousWord += " " + word;
                }
                else
                {
                    previousWord = word;
                }
                returnedLines.Add(previousWord);
            }

            return returnedLines.ToArray();
            /*//Le résultats donnés par la formule complexe n'étant pas toujours bon, j'ai préféré reproduire l'effet voulu avec du code moins sophistiqué
            return lines.GroupBy(w => (charCount += w.Length + 1) / (lineLength + 2))
                        .Select(g => string.Join(" ", g.ToArray()))
                        .ToArray();
            */
        }

        /// <summary>
        /// Wrapper un texte avec des mots entiers séparés par un caractère de breakline
        /// </summary>
        /// <param name="text"></param>
        /// <param name="lineLength"></param>
        /// <returns></returns>
        public static string WrapWithBreakLine(this string text, int lineLength)
        {
            var textArray = text.Wrap(lineLength);

            if (textArray == null || textArray.Length == 0)
                return null;

            return string.Join("\n", textArray);
        }
    }
}