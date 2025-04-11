using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Tools.Extensions
{
    /// <summary>
    /// Extension de différents type de collection
    /// </summary>
    public static class CollectionExtender
    {
        /// <summary>
        /// Teste si cet IEnumerable est null ou vide
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <returns></returns>
        public static bool IsNullOrCountZero<T>(this IEnumerable<T> collection)
        {
            return IsNullOrCountInferiorEqualTo(collection, 0);
        }

        /// <summary>
        /// Teste si cet IEnumerable est null ou contient moins d’éléments qu’un nombre donné
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="count">int - limite du nombre d’éléments (non comprise)</param>
        /// <returns></returns>
        public static bool IsNullOrCountInferiorEqualTo<T>(this IEnumerable<T> collection, int count)
        {
            return !(collection != null && collection.Count() > count);
        }

        /// <summary>
        /// Recherche si un element existe dans cette liste
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="predicate">predicat - critère de recherche</param>
        /// <returns>true si l’élément est présent dans la liste</returns>
        public static bool Contains<T>(this IEnumerable<T> list, Predicate<T> predicate)
        {
            if (list == null)
            {
                return false;
            }
            foreach (var elt in list)
            {
                if (predicate(elt))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Ajoute un ensemble de valeurs
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="values">ICollection - ensemble de valeurs a ajouter</param>
        public static void AddRange<T>(this ICollection<T> list, ICollection<T> values)
        {
            if (list == null)
                return;

            if (values.IsNullOrCountZero())
                return;

            foreach (var value in values)
            {
                list.Add(value);
            }
        }

        /// <summary>
        /// Convertit en ObservableCollection
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> list)
        {
            if (list == null)
            {
                return new ObservableCollection<T>();
            }

            return new ObservableCollection<T>(list);
        }

        /// <summary>
        /// Supprimer tout selon un prédicat
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="coll"></param>
        /// <param name="condition">predicat - critère de recherche</param>
        /// <returns>int - nombre d’éléments supprimés</returns>
        public static int RemoveAll<T>(this ObservableCollection<T> coll, Func<T, bool> condition)
        {
            var itemsToRemove = coll.Where(condition).ToList();

            if (itemsToRemove != null && itemsToRemove.Count > 0)
            {
                foreach (var itemToRemove in itemsToRemove)
                {
                    coll.Remove(itemToRemove);
                }
            }
            else
            {
                return 0;
            }

            return itemsToRemove.Count ;
        }

        public static string ToCharacterSeparatedString(this List<String> list, string separator = null)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string item in list)
            {
                if (separator == null)
                {
                    sb.AppendLine(item);
                }
                else
                {
                    sb.Append(item).Append(separator);
                }

            }
            return sb.ToString();
        }

    }
}