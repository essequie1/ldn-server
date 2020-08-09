using System.Collections.Generic;

namespace LanPlayServer
{
    public class Game
    {
        public ulong ID;
        public string Name;

        public Game(ulong id, string name)
        {
            ID = id;
            Name = name;
        }
    }

    public static class GameList
    {
        // TODO: populate using some online list or something.
        // We still need to be able to provide game specific info, as we might want to parse AdvertiseData or do special things for different games.
        // (eg. a mode to regulate framerate to keep sync in Mario Kart would be nice)

        private static Dictionary<ulong, Game> _games = new Dictionary<ulong, Game>()
        {
            { 0x0100152000022000, new Game(0x0100152000022000, "Mario Kart 8 Deluxe") }
        };

        public static Game GetGameById(ulong id)
        {
            Game result;

            _games.TryGetValue(id, out result);

            return result;
        }
    }
}
