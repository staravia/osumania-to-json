using System;
using System.Collections.Generic;
using System.Linq;
using Json;
using Json.Net;
using osu.Shared;
using osu_database_reader.BinaryFiles;

namespace MGFileGrabber
{
    class Program
    {
        // Replace these fields with the location to your osu!.db file and an output json file.
        private const string OSU_PATH = "D:/osu!/osu!.db";
        private const string OUTPUT_PATH = "C:/users/admin/desktop/song-data.json";
        
        /// <summary>
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            var songs = ParseFiles(OSU_PATH);
            var serializer = new JsonSerializer();
            serializer.Initialize();
            serializer.Serialize(songs);
            
            System.IO.File.WriteAllLines(OUTPUT_PATH, serializer.Builder.ToString().Split(Environment.NewLine.ToCharArray()));
        }

        /// <summary>
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private static List<Song> ParseFiles(string path)
        {
            var output = new List<Song>();
            var db = OsuDb.Read(OSU_PATH);
            var maps = db.Beatmaps.Where(x => x.GameMode == GameMode.Mania && ( x.CircleSize == 4 || x.CircleSize == 7 )).ToList();
            var prev = -1;
            
            foreach (var map in maps)
            {
                var title = map.Title.ToLower();
                var artist = map.Artist.ToLower();

                // Ignore prev mapset
                if (map.BeatmapSetId == prev)
                    continue;
                
                // Ignore non Ranked or loved maps
                if (!(map.RankedStatus == SubmissionStatus.Ranked || map.RankedStatus == SubmissionStatus.Loved))
                    continue;
                
                // Ignore packs/mapset/memes/compilations
                if ( title.Contains("pack") || title.Contains("mapset") || title.Contains("collection") || title.Contains("meme") || title.Contains("compilation"))
                    continue;
                
                // Ignore Various Artists or unknown
                if (artist.Contains("various") || artist.Contains("unknown") || artist.Length <= 1 || artist == "zen" || artist == "zzzzz")
                    continue;
                
                // Ignore weird ID
                if (map.BeatmapId == 0 || map.BeatmapId == -1)
                    continue;
                
                prev = map.BeatmapSetId;
                var song = new Song()
                {
                    Artist = map.Artist,
                    Title = map.Title,
                    Id = map.BeatmapSetId.ToString()
                };
                
                output.Add(song);
                Console.WriteLine($"{song.Artist} - {song.Title} [{song.Id}]");
            }

            return output;
        }
    }
}
