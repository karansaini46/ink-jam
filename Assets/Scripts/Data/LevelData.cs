using System;
using InkJam.Core;

namespace InkJam.Data
{
    [Serializable]
    public class LevelData
    {
        public int formatVersion;
        public int width;
        public int height;
        public TileData[] tiles;
        public ExitFrameData[] exitFrames;
        public ObstacleData[] obstacles;
    }

    [Serializable]
    public class TileData
    {
        public int id;
        public TileColor color;
        public int startX;
        public int startY;
    }

    [Serializable]
    public class ExitFrameData
    {
        public int x;
        public int y;
        public Direction edge;
        public TileColor acceptedColor;
    }

    [Serializable]
    public class ObstacleData
    {
        public string type;
        public int x;
        public int y;
        public int targetTileId;
        // Allows for flexible JSON object deserialization of varied obstacle properties
        public string serializedProperties; 
    }
}
