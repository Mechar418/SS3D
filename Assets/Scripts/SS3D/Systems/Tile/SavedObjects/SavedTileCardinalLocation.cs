using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SS3D.Systems.Tile
{
    /// <summary>
    /// Implementation of ISavedTileLocation for saving cardinal tile locations
    /// </summary>
    [Serializable]
    public class SavedTileCardinalLocation : ISavedTileLocation
    {
        [SerializeField]
        public List<SavedPlacedTileObject> _placedSaveObjects;

        [SerializeField]
        public int x;

        [SerializeField]
        public int y;

        public Vector2Int Location
        {
            get => new(x, y);
            set => (x, y) = (value.x, value.y);
        }

        public TileLayer Layer
        {
            get;
            set;
        }

        public SavedTileCardinalLocation(List<SavedPlacedTileObject> placedSaveObjects, Vector2Int location, TileLayer layer)
        {
            _placedSaveObjects = placedSaveObjects;
            Location = location;
            Layer = layer;
        }
        
        public List<SavedPlacedTileObject> GetPlacedObjects()
        {
            return _placedSaveObjects;
        }
    }
}
