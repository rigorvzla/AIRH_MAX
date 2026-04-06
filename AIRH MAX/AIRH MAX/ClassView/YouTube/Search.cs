using YoutubeExplode;
using YoutubeExplode.Common;

namespace AIRH_MAX.ClassView.YouTube
{
    public class Search
    {
        /// <summary>
        /// Busca resultados asociados a la peticion de busqueda
        /// </summary>
        /// <param name="busqueda">peticion del video</param>
        /// <returns>URL del video, pedido en busqueda</returns>
        public static async Task<string> SearchYouTubeVideo(string busqueda)
        {
            var youtube = new YoutubeClient();
            var searchResults = await youtube.Search.GetVideosAsync(busqueda).CollectAsync(1);
            return searchResults[0].Url;
        }

        /// <summary>
        /// Busca resultados asociados a la peticion de busqueda
        /// </summary>
        /// <param name="busqueda">peticion del video</param>
        /// <param name="cantidad">numero de resultados obtenidos</param>
        /// <returns>Lista de URL de videos, pedidos en busqueda</returns>
        public static async Task<List<string>> SearchYouTubeVideo(string busqueda, int cantidad)
        {
            List<string> list = new List<string>();

            var youtube = new YoutubeClient();
            var searchResults = await youtube.Search.GetVideosAsync(busqueda).CollectAsync(cantidad);

            foreach (var item in searchResults)
            {
                list.Add(item.Url);
            }
            return list;
        }
    }
}
