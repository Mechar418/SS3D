using System.Collections.Generic;
using System.Linq;

namespace SS3D.Systems.Tile
{
    /// <summary>
    /// Represent a Tile location able to contain up to 4 tile objects, in each cardinal directions.
    /// </summary>
    public class CardinalTileLocation : ITileLocation
    {
        private TileLayer _layer;
        private int _x;
        private int _y;

        /// <summary>
        /// The four potential placed tile objects. 0 is for north, 1 is for east, 2 for south, 3 for west.
        /// </summary>
        private PlacedTileObject[] _cardinalPlacedTileObject = new PlacedTileObject[4];
        
        public CardinalTileLocation(TileLayer layer, int x, int y)
        {
            _layer = layer;
            _x = x;
            _y = y;
        }

        /// <summary>
        /// The layer this location is on.
        /// </summary>
        public TileLayer Layer => _layer;

        public void ClearAllPlacedObject()
        {
            for (int i = 0; i < 4; i++)
            {
                TryClearPlacedObject(IndexToDir(i));
            }
        }

        public bool IsEmpty(Direction direction = Direction.North)
        {
            if (!TileHelper.IsCardinalDirection(direction))
            {
                return false;
            }
            
            return !_cardinalPlacedTileObject[DirToIndex(direction)];
        }

        public bool IsFullyEmpty()
        {
            return !_cardinalPlacedTileObject.Any(x => x);
        }

        public ISavedTileLocation Save()
        {
            List<SavedPlacedTileObject> savedTileObjects = new();
            foreach (PlacedTileObject tileObject in _cardinalPlacedTileObject.Where(x => x))
            {
                savedTileObjects.Add(tileObject.Save());
            }

            return new SavedTileCardinalLocation(savedTileObjects, new(_x, _y), _layer);
        }

        public bool TryClearPlacedObject(Direction direction = Direction.North)
        {
            if (!TileHelper.IsCardinalDirection(direction))
            {
                return false;
            }

            PlacedTileObject placedObject = _cardinalPlacedTileObject[DirToIndex(direction)];
            if (placedObject)
            {
                placedObject.DestroySelf();
                return true;
            }
            
            return false;
        }

        public bool TryGetPlacedObject(out PlacedTileObject placedObject, Direction direction = Direction.North)
        {
            if (!TileHelper.IsCardinalDirection(direction))
            {
                placedObject = null;
                return false;
            }

            PlacedTileObject currentPlacedObject = _cardinalPlacedTileObject[DirToIndex(direction)];
            if (currentPlacedObject)
            {
                placedObject = currentPlacedObject;
                return true;
            }
            
            placedObject = null;
            return false;
        }

        public void AddPlacedObject(PlacedTileObject tileObject, Direction direction = Direction.North)
        {
            if (TileHelper.CardinalDirections().Contains(direction))
            {
                _cardinalPlacedTileObject[DirToIndex(direction)] = tileObject;
            }
        }
        
        public List<PlacedTileObject> GetAllPlacedObject()
        {
            return _cardinalPlacedTileObject.Where(x => x).ToList();
        }

        /// <summary>
        /// Tie an index array to each cardinal direction.
        /// </summary>
        private int DirToIndex(Direction direction)
        {
            return (int)direction / 2;
        }

        /// <summary>
        /// Tie an index array to each cardinal direction.
        /// </summary>
        private Direction IndexToDir(int i)
        {
            return (Direction)(i * 2);
        }
    }
}
