using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Assets.Astrolabe.Scripts.Tools;

namespace Assets.Scripts.Tools.Extensions
{
    /// <summary>
    /// Extension du type Dictionary, conversion en liste ou en chaîne
    /// </summary>
    public static class DictionaryExtension
    {
        /// <summary>
        /// Convertit en string
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dico"></param>
        /// <param name="separator">string - chaîne séparant la clé et la valeur</param>
        /// <returns>String - chaque ligne correspond à "Clé + Séparateur + Valeur"</returns>
        public static string ToString<TKey, TValue>(this Dictionary<TKey, TValue> dico, string separator)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var item in dico.Keys)
            {
                sb.Append(item).Append(separator).AppendLine(dico[item].ToString());
            }
            return sb.ToString();
        }

        /// <summary>
        /// Convertit en liste de String
        /// </summary>
        /// <param name="dico"></param>
        /// <param name="separator">string - chaîne séparant la clé et la valeur</param>
        /// <returns>List[String] - chaque ligne correspond à "Clé + Séparateur + Valeur"</returns>
        public static List<string> ToStringList<TValue, TKey>(this Dictionary<TValue, TKey> dico, string separator = ";")
        {
            List<string> retour = new List<string>();
            foreach (var key in dico.Keys)
            {
                retour.Add(key + separator + dico[key].ToString());
            }


            return retour;
        }

        #region ToCSV

        /// <summary>
        /// Enregistre ce dictionnaire au format CSV
        /// </summary>
        /// <typeparam name="TKey">String</typeparam>
        /// <typeparam name="TValue">int</typeparam>
        /// <param name="dico"></param>
        /// <param name="fullPath">String - chemin complet du fichier (dossier + fichier)</param>
        /// <param name="separator">string - séparateur</param>
        /// <returns>Renvoi false en cas d'erreur</returns>
        public static async Task<bool> ToCsv<TKey, TValue>(this Dictionary<TKey, TValue> dico, string fullPath, bool overwriteIfExist = true, string separator = ";")
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                foreach (var item in dico.Keys)
                {
                    sb.Append(item).Append(separator).AppendLine(dico[item].ToString());
                }
                await FileHelper.WriteTextAsync(sb.ToString(), fullPath, overwriteIfExist);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Enregistre ce dictionnaire au format CSV
        /// </summary>
        /// <typeparam name="TKey">int</typeparam>
        /// <typeparam name="TValue">int</typeparam>
        /// <param name="dico"></param>
        /// <param name="filename">String - nom voulu pour le fichier</param>
        /// <param name="path">String - chemin du fichier</param>
        /// <param name="separator">string - séparateur</param>
        /// <returns>Renvoi false en cas d'erreur</returns>
        public static async Task<bool> ToCsv<TKey, TValue>(this Dictionary<TKey, TValue> dico, string filename, string path, bool overwriteIfExist = true, string separator = ";")
        {
            return await dico.ToCsv(Path.Combine(path, filename), overwriteIfExist);
        }

        /// <summary>
        /// Enregistre ce dictionnaire au format CSV
        /// </summary>
        /// <typeparam name="TKey">String</typeparam>
        /// <typeparam name="TValue">int</typeparam>
        /// <param name="dico"></param>
        /// <param name="filename">String - nom voulu pour le fichier</param>
        /// <param name="storage">StorageType - Emplacement de stockage du fichier</param>
        /// <param name="subFolder">String - sous-dossier</param>
        /// <param name="separator">string - séparateur</param>
        /// <returns>Renvoi false en cas d'erreur</returns>
        public static async Task<bool> ToCsv<TKey, TValue>(this Dictionary<TKey, TValue> dico, string filename, StorageType storage, string subFolder = "", bool overwriteIfExist = true, string separator = ";")
        {
            return await dico.ToCsv(Path.Combine(storage.GetPath(), subFolder, filename), overwriteIfExist);
        }

        #endregion ToCSV
    }
}